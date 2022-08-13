using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Settings;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Util.Capture;

public abstract class ScreenCapture : BaseCapture, IScreenCapture
{
    #region Variables

    private BlockingCollection<RecordingEvent> _eventConsumer;
    private Task _eventConsumerTask;

    private FileStream _fileStream;
    private BufferedStream _bufferedStream;
    protected DeflateStream CompressStream;

    private FileStream _eventsFileStream;
    private BufferedStream _eventsBufferedStream;

    #endregion

    #region Properties

    public int Left { get; set; }

    public int Top { get; set; }

    /// <summary>
    /// The name of the monitor device where the recording is supposed to happen.
    /// </summary>
    public string DeviceName { get; set; }

    public bool WasEventCaptureStarted { get; set; }

    /// <summary>
    /// True if the capture system is expecting new events (mouse, keyboard).
    /// </summary>
    public bool IsAcceptingEvents { get; set; }
    
    #endregion

    public virtual void Start(bool isAutomatic, int delay, int left, int top, int width, int height, double scale, RecordingProject project)
    {
        base.Start(isAutomatic, delay, width, height, scale, project);

        Left = left;
        Top = top;

        //Frame cache on memory/disk.
        _fileStream = new FileStream(project.FramesCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        _bufferedStream = new BufferedStream(_fileStream, UserSettings.All.MemoryCacheSize * 1_048_576); //Each 1 MB has 1_048_576 bytes.
        CompressStream = new DeflateStream(_bufferedStream, UserSettings.All.CaptureCompression, false);
        
        //Events (cursor, key presses) cache on memory/disk.
        _eventsFileStream = new FileStream(project.EventsCachePath, FileMode.Create, FileAccess.Write, FileShare.None);
        _eventsBufferedStream = new BufferedStream(_eventsFileStream, 10 * 1_048_576); //Each 1 MB has 1_048_576 bytes.

        ConfigureEventConsumer();

        WasEventCaptureStarted = true;
        IsAcceptingEvents = IsAutomatic;
    }

    private void ConfigureEventConsumer()
    {
        _eventConsumer ??= new BlockingCollection<RecordingEvent>();

        //Spin up a Task to consume the events generated by the recorder.
        _eventConsumerTask = Task.Factory.StartNew(() =>
        {
            try
            {
                while (true)
                {
                    var recordingEvent = _eventConsumer.Take();

                    switch (recordingEvent.EventType)
                    {
                        case RecordingEvents.Cursor:
                            SaveEvent((CursorEvent)recordingEvent);
                            break;
                        case RecordingEvents.Key:
                            SaveEvent((KeyEvent)recordingEvent);
                            break;
                        default:
                            SaveEvent((CursorDataEvent)recordingEvent);
                            break;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                //It means that Take() was called on a completed collection.
            }
            catch (Exception e)
            {
                Application.Current.Dispatcher.Invoke(() => OnError?.Invoke(e));
            }
        });
    }

    #region Capture

    public abstract int CaptureWithCursor(RecordingFrame frame);

    public async Task<int> CaptureWithCursorAsync(RecordingFrame frame)
    {
        return await Task.Factory.StartNew(() => CaptureWithCursor(frame));
    }

    public virtual int ManualCapture(RecordingFrame frame, bool showCursor = false)
    {
        return showCursor ? CaptureWithCursor(frame) : Capture(frame);
    }

    public virtual Task<int> ManualCaptureAsync(RecordingFrame frame, bool showCursor = false)
    {
        return showCursor ? CaptureWithCursorAsync(frame) : CaptureAsync(frame);
    }

    #endregion

    #region Events

    public void RegisterCursorEvent(int x, int y, MouseButtonState left, MouseButtonState right, MouseButtonState middle, MouseButtonState firstExtra, MouseButtonState secondExtra, short mouseDelta = 0)
    {
        if (!IsAcceptingEvents || !Stopwatch.IsRunning)
            return;

        var recordingEvent = new CursorEvent
        {
            TimeStampInTicks = Stopwatch.GetElapsedTicks(),
            Left = x - Left,
            Top = y - Top,
            LeftButton = left,
            RightButton = right,
            MiddleButton = middle,
            FirstExtraButton = firstExtra,
            SecondExtraButton = secondExtra,
            MouseDelta = mouseDelta
        };

        Project.Events.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }

    public void RegisterCursorDataEvent(int type, byte[] pixels, int width, int height, int left, int top, int xHotspot, int yHotspot)
    {
        if (!IsAcceptingEvents || !Stopwatch.IsRunning)
            return;

        var recordingEvent = new CursorDataEvent
        {
            TimeStampInTicks = Stopwatch.GetElapsedTicks(),
            CursorType = type,
            Left = left,
            Top = top,
            Width = width,
            Height = height,
            XHotspot = xHotspot,
            YHotspot = yHotspot,
            Data = pixels
        };

        Project.Events.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }
    
    public void RegisterKeyEvent(Key key, ModifierKeys modifiers, bool isUppercase, bool wasInjected)
    {
        if (!IsAcceptingEvents || !Stopwatch.IsRunning)
            return;

        var recordingEvent = new KeyEvent
        {
            TimeStampInTicks = Stopwatch.GetElapsedTicks(),
            Key = key,
            Modifiers = modifiers,
            IsUppercase = isUppercase,
            WasInjected = wasInjected
        };

        Project.Events.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }

    public virtual void SaveEvent(CursorEvent cursorEvent)
    {
        cursorEvent.StreamPosition = _eventsBufferedStream.Position;

        _eventsBufferedStream.WriteByte((byte)RecordingEvents.Cursor); //1 byte.
        _eventsBufferedStream.WriteUInt64((ulong)cursorEvent.TimeStampInTicks); //8 bytes.
        _eventsBufferedStream.WriteInt32(cursorEvent.Left); //4 bytes.
        _eventsBufferedStream.WriteInt32(cursorEvent.Top); //4 bytes.
        _eventsBufferedStream.WriteBoolean(cursorEvent.LeftButton == MouseButtonState.Pressed); //1 byte.
        _eventsBufferedStream.WriteBoolean(cursorEvent.RightButton == MouseButtonState.Pressed); //1 byte.
        _eventsBufferedStream.WriteBoolean(cursorEvent.MiddleButton == MouseButtonState.Pressed); //1 byte.
        _eventsBufferedStream.WriteBoolean(cursorEvent.FirstExtraButton == MouseButtonState.Pressed); //1 byte.
        _eventsBufferedStream.WriteBoolean(cursorEvent.SecondExtraButton == MouseButtonState.Pressed); //1 byte.
        _eventsBufferedStream.WriteInt16(cursorEvent.MouseDelta); //2 bytes.
    }

    public virtual void SaveEvent(CursorDataEvent cursorEvent)
    {
        cursorEvent.StreamPosition = _eventsBufferedStream.Position;

        _eventsBufferedStream.WriteByte((byte)RecordingEvents.CursorData); //1 byte.
        _eventsBufferedStream.WriteUInt64((ulong)cursorEvent.TimeStampInTicks); //8 bytes.
        _eventsBufferedStream.WriteByte((byte)cursorEvent.CursorType); //1 byte.

        _eventsBufferedStream.WriteInt32(cursorEvent.Left); //4 bytes.
        _eventsBufferedStream.WriteInt32(cursorEvent.Top); //4 bytes.
        _eventsBufferedStream.WriteUInt32((uint)cursorEvent.Width); //4 bytes.
        _eventsBufferedStream.WriteUInt32((uint)cursorEvent.Height); //4 bytes.
        _eventsBufferedStream.WriteUInt32((uint)cursorEvent.XHotspot); //4 bytes.
        _eventsBufferedStream.WriteUInt32((uint)cursorEvent.YHotspot); //4 bytes.
        _eventsBufferedStream.WriteUInt64((ulong)cursorEvent.Data.Length); //8 bytes.

        if (cursorEvent.Data?.Length > 0)
        {
            _eventsBufferedStream.WriteBytes(cursorEvent.Data);

            cursorEvent.PixelsLength = cursorEvent.Data.Length;
            cursorEvent.Data = null;
        }
    }

    public virtual void SaveEvent(KeyEvent keyEvent)
    {
        keyEvent.StreamPosition = _eventsBufferedStream.Position;

        _eventsBufferedStream.WriteByte((byte)RecordingEvents.Key); //Key event type.
        _eventsBufferedStream.WriteUInt64((ulong)keyEvent.TimeStampInTicks); //TimeStamp since capture start.
        _eventsBufferedStream.WriteUInt32((uint)keyEvent.Key);
        _eventsBufferedStream.WriteByte((byte)keyEvent.Modifiers);
        _eventsBufferedStream.WriteByte(keyEvent.IsUppercase ? (byte) 1 : (byte) 0);
        _eventsBufferedStream.WriteByte(keyEvent.WasInjected ? (byte) 1 : (byte) 0);
    }

    #endregion

    public override async Task Stop()
    {
        await base.Stop();

        if (!WasEventCaptureStarted)
            return;

        IsAcceptingEvents = false;

        //Stop the consumer thread.
        _eventConsumer.CompleteAdding();

        await _eventConsumerTask;

        //Finishing writing the events to the cache.
        await CompressStream.FlushAsync();
        await CompressStream.DisposeAsync();
        //await _bufferedStream.FlushAsync();
        await _bufferedStream.DisposeAsync();
        await _fileStream.DisposeAsync();
        
        await _eventsBufferedStream.DisposeAsync();
        await _eventsFileStream.DisposeAsync();

        WasEventCaptureStarted = false;
    }

    internal override async Task DisposeInternal()
    {
        await base.DisposeInternal();

        _eventConsumerTask?.Dispose();
        _eventConsumerTask = null;

        _eventConsumer?.Dispose();
        _eventConsumer = null;
    }
}
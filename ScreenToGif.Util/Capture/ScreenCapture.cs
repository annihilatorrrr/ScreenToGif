using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Domain.Models.Project.Recording.Events;
using ScreenToGif.Util.Settings;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Util.Capture;

public abstract class ScreenCapture : BaseCapture, IScreenCapture
{
    #region Variables

    private BlockingCollection<RecordingEvent> _eventConsumer;
    private Task _eventConsumerTask;

    protected FileStream FramesFileStream;
    protected ExBinaryWriter FramesBinaryWriter;

    private FileStream _mouseEventsFileStream;
    private ExBinaryWriter _mouseEventsBinaryWriter;

    private FileStream _keyboardEventsFileStream;
    private ExBinaryWriter _keyboardEventsBinaryWriter;

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

    public long StreamPosition => FramesFileStream.Position;

    #endregion

    public virtual void Start(bool isAutomatic, int delay, int left, int top, int width, int height, double scale, RecordingProject project)
    {
        base.Start(isAutomatic, delay, width, height, scale, project);

        Left = left;
        Top = top;

        //Frame cache on memory/disk.
        FramesFileStream = new FileStream(project.FramesCachePath, FileMode.Create, FileAccess.Write, FileShare.None, UserSettings.All.MemoryCacheSize * 1_048_576);  //Each 1 MB has 1_048_576 bytes.
        FramesBinaryWriter = new ExBinaryWriter(FramesFileStream);
        
        //Mouse events cache on memory/disk.
        _mouseEventsFileStream = new FileStream(project.MouseEventsCachePath, FileMode.Create, FileAccess.Write, FileShare.None, 5 * 1_048_576); //Each 1 MB has 1_048_576 bytes.
        _mouseEventsBinaryWriter = new ExBinaryWriter(_mouseEventsFileStream);

        //Keyboard events cache on memory/disk.
        _keyboardEventsFileStream = new FileStream(project.KeyboardEventsCachePath, FileMode.Create, FileAccess.Write, FileShare.None, 1 * 1_048_576); //Each 1 MB has 1_048_576 bytes.
        _keyboardEventsBinaryWriter = new ExBinaryWriter(_keyboardEventsFileStream);

        ConfigureEventConsumer();
        WriteFrameSequenceHeader();

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

    private void WriteFrameSequenceHeader()
    {
        //Sequence details.
        FramesBinaryWriter.Write((ushort)1); //2 bytes, ID.
        FramesBinaryWriter.Write((byte)SequenceTypes.Frame); //1 bytes.
        FramesBinaryWriter.Write(0); //8 bytes, start time in ticks.
        FramesBinaryWriter.Write(0); //8 bytes, end time in ticks (unknown for now).
        FramesBinaryWriter.Write(BitConverter.GetBytes(1F)); //4 bytes, opacity.
        FramesBinaryWriter.Write(0); //4 bytes, no background.

        //Sequence effects.
        FramesBinaryWriter.Write((byte)0); //Effect count, 1 bytes.

        //Rect sequence.
        FramesBinaryWriter.Write(0); //4 bytes, left/X.
        FramesBinaryWriter.Write(0); //4 bytes, top/Y.
        FramesBinaryWriter.Write((ushort) Width); //2 bytes.
        FramesBinaryWriter.Write((ushort) Height); //2 bytes.
        FramesBinaryWriter.Write(BitConverter.GetBytes(0F)); //4 bytes, angle.
        FramesBinaryWriter.WriteTwice(BitConverter.GetBytes(Convert.ToSingle(Project.Dpi))); //4+4 bytes.
        
        //Raster sequence. Should it be type of raster?
        FramesBinaryWriter.Write((byte)RasterSequenceSources.Screen); //1 byte.
        FramesBinaryWriter.Write((ushort)Width); //2 bytes.
        FramesBinaryWriter.Write((ushort)Height); //2 bytes.
        FramesBinaryWriter.WriteTwice(BitConverter.GetBytes(Convert.ToSingle(Project.Dpi))); //4+4 bytes.
        FramesBinaryWriter.Write(Project.ChannelCount); //1 byte.
        FramesBinaryWriter.Write(Project.BitsPerChannel); //1 byte.
        FramesBinaryWriter.Write((uint)0); //4 byte, frame count (unknown for now).
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
            TimeStampInTicks = Stopwatch.GetElapsedTicks(true),
            Left = x - Left,
            Top = y - Top,
            LeftButton = left,
            RightButton = right,
            MiddleButton = middle,
            FirstExtraButton = firstExtra,
            SecondExtraButton = secondExtra,
            MouseDelta = mouseDelta
        };

        Project.MouseEvents.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }

    public void RegisterCursorDataEvent(int type, byte[] pixels, int width, int height, int left, int top, int xHotspot, int yHotspot, bool force = false)
    {
        if (!IsAcceptingEvents || (!Stopwatch.IsRunning && !force))
            return;

        var recordingEvent = new CursorDataEvent
        {
            TimeStampInTicks = Stopwatch.GetElapsedTicks(true),
            CursorType = type,
            Left = left,
            Top = top,
            Width = width,
            Height = height,
            XHotspot = xHotspot,
            YHotspot = yHotspot,
            Data = pixels
        };

        Project.MouseEvents.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }
    
    public void RegisterKeyEvent(Key key, ModifierKeys modifiers, bool isUppercase, bool wasInjected)
    {
        if (!IsAcceptingEvents || !Stopwatch.IsRunning)
            return;

        var recordingEvent = new KeyEvent
        {
            TimeStampInTicks = Stopwatch.GetElapsedTicks(true),
            Key = key,
            Modifiers = modifiers,
            IsUppercase = isUppercase,
            WasInjected = wasInjected
        };

        Project.KeyboardEvents.Add(recordingEvent);
        _eventConsumer.Add(recordingEvent);
    }

    public virtual void SaveEvent(CursorEvent cursorEvent)
    {
        cursorEvent.StreamPosition = _mouseEventsBinaryWriter.Position;

        _mouseEventsBinaryWriter.Write((byte)RecordingEvents.Cursor); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.TimeStampInTicks); //8 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.Left); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.Top); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.LeftButton == MouseButtonState.Pressed); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.RightButton == MouseButtonState.Pressed); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.MiddleButton == MouseButtonState.Pressed); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.FirstExtraButton == MouseButtonState.Pressed); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.SecondExtraButton == MouseButtonState.Pressed); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.MouseDelta); //2 bytes.
    }

    public virtual void SaveEvent(CursorDataEvent cursorEvent)
    {
        cursorEvent.StreamPosition = _mouseEventsBinaryWriter.Position;

        _mouseEventsBinaryWriter.Write((byte)RecordingEvents.CursorData); //1 byte.
        _mouseEventsBinaryWriter.Write(cursorEvent.TimeStampInTicks); //8 bytes.
        _mouseEventsBinaryWriter.Write((byte)cursorEvent.CursorType); //1 byte.

        _mouseEventsBinaryWriter.Write(cursorEvent.Left); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.Top); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.Width); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.Height); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.XHotspot); //4 bytes.
        _mouseEventsBinaryWriter.Write(cursorEvent.YHotspot); //4 bytes.
        _mouseEventsBinaryWriter.Write((long)cursorEvent.Data.Length); //8 bytes.

        if (cursorEvent.Data?.Length > 0)
        {
            _mouseEventsBinaryWriter.Write(cursorEvent.Data);

            cursorEvent.PixelsLength = cursorEvent.Data.Length;
            cursorEvent.Data = null;
        }
    }

    public virtual void SaveEvent(KeyEvent keyEvent)
    {
        keyEvent.StreamPosition = _keyboardEventsBinaryWriter.Position;

        _keyboardEventsBinaryWriter.Write((byte)RecordingEvents.Key); //Key event type.
        _keyboardEventsBinaryWriter.Write(keyEvent.TimeStampInTicks); //TimeStamp since capture start.
        _keyboardEventsBinaryWriter.Write((int)keyEvent.Key);
        _keyboardEventsBinaryWriter.Write((byte)keyEvent.Modifiers);
        _keyboardEventsBinaryWriter.Write(keyEvent.IsUppercase);
        _keyboardEventsBinaryWriter.Write(keyEvent.WasInjected);
    }

    #endregion

    public override async Task Stop()
    {
        IsAcceptingEvents = false;

        await base.Stop();

        if (!WasEventCaptureStarted)
            return;

        //Stop the consumer thread.
        _eventConsumer.CompleteAdding();

        await _eventConsumerTask;

        //Finishing writing the events to the cache.
        //await _compressStream.FlushAsync();
        //await CompressStream.DisposeAsync();
        //await _framesBufferedStream.FlushAsync();
        //await _bufferedStream.DisposeAsync();
        await FramesFileStream.DisposeAsync();
        
        //await _mouseEventsBufferedStream.DisposeAsync();
        await _mouseEventsFileStream.DisposeAsync();
        //await _keyboardEventsBufferedStream.DisposeAsync();
        await _keyboardEventsFileStream.DisposeAsync();

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
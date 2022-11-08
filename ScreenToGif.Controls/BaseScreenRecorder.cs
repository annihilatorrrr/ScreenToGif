using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Capture;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenToGif.Controls;

public class BaseScreenRecorder : BaseRecorder, IDisposable
{
    #region Variables

    /// <summary>
    /// The token in use to control the execution of the capture.
    /// </summary>
    private CancellationTokenSource _captureToken;

    /// <summary>
    /// Deals with all screen capture methods.
    /// </summary>
    protected IScreenCapture Capture;

    /// <summary>
    /// Timer responsible for the forced clean up of the objects in memory.
    /// </summary>
    protected readonly System.Timers.Timer GarbageTimer = new();

    #endregion

    public BaseScreenRecorder()
    {
        LoadOptions();

        GarbageTimer.Interval = 3000;
        GarbageTimer.Elapsed += GarbageTimer_Tick;
    }

    private void LoadOptions()
    {
        ViewModel.UseDesktopDuplication = UserSettings.All.UseDesktopDuplication;
        ViewModel.CaptureFrequency = UserSettings.All.CaptureFrequency;
        ViewModel.Framerate = UserSettings.All.LatestFps;
        ViewModel.ShowDiscardDuringCapturing = UserSettings.All.RecorderDisplayDiscard;
        ViewModel.Monitors = MonitorHelper.AllMonitors;
        //ViewModel.CurrentMonitor = //TODO: Maybe I need to detect the current monitor, based on the position.

        //Selection.
        if (Arguments.Region != Rect.Empty)
        {
            ViewModel.Selection = Arguments.Region;
            ViewModel.RegionWasForceSelected = true;
            Arguments.Region = Rect.Empty;
        }
        else
        {
            ViewModel.Selection = UserSettings.All.SelectionBehavior switch
            {
                RecorderSelectionBehaviors.AlwaysAsk => Rect.Empty,
                RecorderSelectionBehaviors.RememberSize => UserSettings.All.SelectedRegion with { X = double.NaN, Y = double.NaN },
                _ => UserSettings.All.SelectedRegion
            };
        }

        //?
    }

    protected void PersistOptions()
    {
        UserSettings.All.UseDesktopDuplication = ViewModel.UseDesktopDuplication;
        UserSettings.All.CaptureFrequency = ViewModel.CaptureFrequency;
        UserSettings.All.LatestFps = ViewModel.Framerate;

        UserSettings.All.SelectedRegion = UserSettings.All.SelectionBehavior switch
        {
            RecorderSelectionBehaviors.RememberSize => ViewModel.Selection with { X = double.NaN, Y = double.NaN },
            RecorderSelectionBehaviors.RememberSizeAndPosition => ViewModel.Selection,
            _ => UserSettings.All.SelectedRegion
        };

        //?
    }

    private void GarbageTimer_Tick(object sender, EventArgs e)
    {
        GC.Collect(2);
    }

    protected bool HasFixedDelay()
    {
        return ViewModel.CaptureFrequency != CaptureFrequencies.PerSecond || UserSettings.All.FixedFrameRate;
    }

    protected int GetFixedDelay()
    {
        return ViewModel.CaptureFrequency switch
        {
            CaptureFrequencies.Manual => UserSettings.All.PlaybackDelayManual,
            CaptureFrequencies.Interaction => UserSettings.All.PlaybackDelayInteraction,
            CaptureFrequencies.PerMinute => UserSettings.All.PlaybackDelayMinute,
            CaptureFrequencies.PerHour => UserSettings.All.PlaybackDelayHour,

            //When the capture is 'PerSecond', the fixed delay is set to use the current framerate.
            _ => 1000 / ViewModel.Framerate
        };
    }

    protected int GetTriggerDelay()
    {
        switch (ViewModel.CaptureFrequency)
        {
            case CaptureFrequencies.Interaction:
                return UserSettings.All.TriggerDelayInteraction;
            case CaptureFrequencies.Manual:
                return UserSettings.All.TriggerDelayManual;
            default:
                return 0;
        }
    }

    protected int GetCaptureInterval()
    {
        return ViewModel.CaptureFrequency switch
        {
            //15 frames per hour = 240,000 ms (240 sec, 4 min).
            CaptureFrequencies.PerHour => (1000 * 60 * 60) / ViewModel.Framerate,

            //15 frames per minute = 4,000 ms (4 sec).
            CaptureFrequencies.PerMinute => (1000 * 60) / ViewModel.Framerate,

            //15 frames per second = 66.66 ms
            _ => 1000 / ViewModel.Framerate
        };
    }

    protected bool IsAutomaticCapture()
    {
        return ViewModel.CaptureFrequency is not (CaptureFrequencies.Manual or CaptureFrequencies.Interaction);
    }

    protected IScreenCapture GetDirectCapture()
    {
        return UserSettings.All.OnlyCaptureChanges ? new DirectChangedCapture() : new DirectCapture();
    }

    public virtual void StartCapture()
    {
        Capture.StartStopwatch(HasFixedDelay(), GetFixedDelay());
        ViewModel.HasImpreciseCapture = false;

        if (UserSettings.All.ForceGarbageCollection)
            GarbageTimer.Start();

        lock (UserSettings.Lock)
        {
            //Starts the capture.
            _captureToken = new CancellationTokenSource();

            Task.Run(() => PrepareCaptureLoop(GetCaptureInterval()), _captureToken.Token);
        }
    }

    public virtual void PauseCapture()
    {
        Capture.StopStopwatch();

        StopInternalCapture();
    }

    public virtual async Task StopCapture()
    {
        StopInternalCapture();

        Capture?.StopStopwatch();

        if (Capture != null)
            await Capture.Stop();

        GarbageTimer.Stop();
    }

    private void PrepareCaptureLoop(int interval)
    {
        using (var resolution = new TimerResolution(1))
        {
            if (!resolution.SuccessfullySetTargetResolution)
            {
                LogWriter.Log($"Imprecise timer resolution... Target: {resolution.TargetResolution}, Current: {resolution.CurrentResolution}");
                ViewModel.HasImpreciseCapture = true;
            }

            if (UserSettings.All.ShowCursor)
                CaptureWithCursor(interval);
            else
                CaptureWithoutCursor(interval);

            ViewModel.HasImpreciseCapture = false;
        }
    }

    private void CaptureWithCursor(int interval)
    {
        var sw = new Stopwatch();

        while (_captureToken != null && !_captureToken.IsCancellationRequested)
        {
            sw.Restart();

            //Capture frame.
            var frame = new RecordingFrame();

            var frameCount = Capture.CaptureWithCursor(frame);
            ViewModel.FrameCount = frameCount;

            //If behind wait time, wait before capturing new frame.
            if (sw.ElapsedMilliseconds >= interval)
                continue;

            while (sw.Elapsed.TotalMilliseconds < interval)
                Thread.Sleep(1);

            //SpinWait.SpinUntil(() => sw.ElapsedMilliseconds >= interval);
        }

        sw.Stop();
    }

    private void CaptureWithoutCursor(int interval)
    {
        var sw = new Stopwatch();

        while (_captureToken != null && !_captureToken.IsCancellationRequested)
        {
            sw.Restart();

            //Capture frame.
            var frame = new RecordingFrame();

            var frameCount = Capture.Capture(frame);
            ViewModel.FrameCount = frameCount;

            //If behind wait time, wait before capturing new frame.
            if (sw.ElapsedMilliseconds >= interval)
                continue;

            while (sw.Elapsed.TotalMilliseconds < interval)
                Thread.Sleep(1);

            //SpinWait.SpinUntil(() => sw.ElapsedMilliseconds >= interval);
        }

        sw.Stop();
    }

    private void StopInternalCapture()
    {
        if (_captureToken == null)
            return;

        _captureToken.Cancel();
        _captureToken.Dispose();
        _captureToken = null;
    }

    public void Dispose()
    {
        StopInternalCapture();

        GarbageTimer?.Dispose();
    }
}

using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Dialogs;
using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Events;
using ScreenToGif.Domain.Exceptions;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models;
using ScreenToGif.Domain.Models.Project.Recording;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Capture;
using ScreenToGif.Util.Extensions;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Project;
using ScreenToGif.Util.Settings;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenToGif.Views.Recorders;

public partial class ScreenRecorder : BaseScreenRecorder
{
    #region Variables

    private static readonly object Lock = new();

    private readonly Timer _preStartTimer = new();
    private readonly Timer _followTimer = new();
    private readonly Timer _showBorderTimer = new(500);
    private readonly Timer _limitTimer = new();

    /// <summary>
    /// Keyboard and mouse hooks helper.
    /// </summary>
    private readonly InputHook _actHook;

    /// <summary>
    /// The amount of seconds of the pre start delay, plus 1 (1+1=2);
    /// </summary>
    private int _preStartCount = 1;

    #region Mouse cursor follow up

    /// <summary>
    /// The previous position of the cursor in the X axis.
    /// </summary>
    private int _prevPosX = 0;

    /// <summary>
    /// The previous position of the cursor in the Y axis.
    /// </summary>
    private int _prevPosY = 0;

    /// <summary>
    /// The latest position of the cursor in the X axis.
    /// </summary>
    private int _posX = 0;

    /// <summary>
    /// The latest position of the cursor in the Y axis.
    /// </summary>
    private int _posY = 0;

    /// <summary>
    /// The offset in pixels. Used for moving the recorder around the X axis.
    /// </summary>
    private double _offsetX = 0;

    /// <summary>
    /// The offset in pixels. Used for moving the recorder around the Y axis.
    /// </summary>
    private double _offsetY = 0;

    #endregion

    #endregion

    private readonly CaptureRegion _captureRegion = new();

    public ScreenRecorder()
    {
        InitializeComponent();

        RegisterCommands();
        RegisterTimers();

        #region Global hook

        try
        {
            _actHook = new InputHook(true, true); //true for the mouse, true for the keyboard.
            _actHook.KeyDown += KeyHookTarget;
            _actHook.OnMouseActivity += MouseHookTarget;
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to initialize the user activity hook.");
        }

        #endregion

        //Define events for capture region.
        _captureRegion.PositionChanged += CaptureRegion_PositionChanged;
        _captureRegion.DragStarted += CaptureRegion_DragStarted;
        _captureRegion.DragEnded += CaptureRegion_DragEnded;

        SystemEvents.PowerModeChanged += System_PowerModeChanged;
        SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
    }
    
    [SuppressMessage("ReSharper", "AsyncVoidLambda")]
    private void RegisterCommands()
    {
        CommandBindings.Clear();
        CommandBindings.AddRange(new CommandBindingCollection
        {
            //new CommandBinding(ViewModel.CloseCommand, (_, _) => Close(),
            //    (_, args) => args.CanExecute = Stage == RecorderStages.Stopped || (ViewModel.CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction && (Project == null || !Project.Any))),

            new CommandBinding(ViewModel.OptionsCommand, ShowOptions, (_, args) => args.CanExecute = ViewModel.CanOpenOptions),
            new CommandBinding(ViewModel.SwitchFrequencyCommand, SwitchFrequency, (_, args) => args.CanExecute = (args.Parameter != null && !args.Parameter.Equals("Switch")) || ViewModel.CanSwitchFrequency),
            new CommandBinding(ViewModel.SelectRegionCommand, async (_, _) => await SelectRegion(), (_, args) => args.CanExecute = ViewModel.CanSelectRegion),

            new CommandBinding(ViewModel.RecordCommand, Record, (_, args) => args.CanExecute = ViewModel.CanRecord),
            new CommandBinding(ViewModel.PauseCommand, Pause, (_, args) => args.CanExecute = ViewModel.CanPause),
            new CommandBinding(ViewModel.SnapCommand, Snap, (_, args) => args.CanExecute = ViewModel.CanSnap),

            new CommandBinding(ViewModel.StopLargeCommand, Stop, (_, args) => args.CanExecute = ViewModel.CanStopLarge),
            new CommandBinding(ViewModel.StopCommand, Stop, (_, args) => args.CanExecute = ViewModel.CanStop),

            new CommandBinding(ViewModel.DiscardCommand, Discard, (_, args) => args.CanExecute = ViewModel.CanDiscard)
        });

        ViewModel.RefreshKeyGestures();
    }

    private void RegisterTimers()
    {
        _showBorderTimer.Elapsed += ShowBorderTimer_Elapsed;
        _followTimer.Elapsed += FollowTimer_Tick;

        _preStartTimer.Elapsed += PreStart_Elapsed;
        _preStartTimer.Interval = 1000;
    }

    private async Task RequestRegionSelection()
    {
        var (mode, monitor, selection) = await SelectRegionInternal();

        if (selection != Rect.Empty)
        {
            ViewModel.SelectionMode = mode;
            ViewModel.CurrentMonitor = monitor;
            ViewModel.Selection = selection;
        }
    }

    private void AdjustSelectionToScreen()
    {
        //Nothing to adjust, if there's no selection.
        if (ViewModel.Selection.IsEmpty)
            return;

        //TODO: Being called multiple times without need. Because of with/height box changes;

        //If position not defined, center on main screen (if possible).
        if (double.IsNaN(ViewModel.Selection.Y) || double.IsNaN(ViewModel.Selection.X))
        {
            if (ViewModel.MainMonitor != null)
            {
                //Center the selection on the main screen.
                var left = ViewModel.MainMonitor.Bounds.Left + ViewModel.MainMonitor.Bounds.Width / 2d - ViewModel.Selection.Width / 2d;
                var top = ViewModel.MainMonitor.Bounds.Top + ViewModel.MainMonitor.Bounds.Height / 2d - ViewModel.Selection.Height / 2d;

                ViewModel.CurrentMonitor = ViewModel.MainMonitor;
                ViewModel.Selection = ViewModel.Selection with { X = left, Y = top };
                ViewModel.SelectionScale = ViewModel.MainMonitor.Scale;
            }
            else
            {
                //If it was not possible to detect the primary screen, simply clear the selection.
                ViewModel.CurrentMonitor = null;
                ViewModel.Selection = Rect.Empty;
                ViewModel.SelectionScale = 1;
            }

            return;
        }

        //TODO: Get the biggest intersection.
        //Check if the selection can be positioned inside of a screen.
        var monitor = ViewModel.Monitors.FirstOrDefault(f => f.NativeBounds.IntersectsWith(ViewModel.Selection.Scale(ViewModel.CurrentMonitor.Scale)));

        if (monitor != null)
        {
            ViewModel.CurrentMonitor = monitor;
            ViewModel.SelectionScale = monitor.Scale;
        }
        else
        {
            //For a fullscreen selection, offset 1px, just to be within bounds.
            monitor = ViewModel.Monitors.FirstOrDefault(f => f.Bounds == ViewModel.Selection.Offset(1));

            if (monitor != null)
            {
                ViewModel.CurrentMonitor = monitor;
                ViewModel.SelectionScale = monitor.Scale;
            }
            else
            {
                ViewModel.CurrentMonitor = null;
                ViewModel.SelectionScale = 1;
                ViewModel.Selection = Rect.Empty;
            }
        }
    }

    private void RepositionCaptureRegion()
    {
        if (ViewModel.Selection.IsEmpty)
        {
            if (_captureRegion.IsVisible)
                _captureRegion.Hide();

            return;
        }

        _captureRegion.Select(ViewModel.SelectionMode, ViewModel.Selection, ViewModel.CurrentMonitor);
    }

    /// <summary>
    /// Repositions the capture controls near the selected region, in order to stay away from the capture.
    /// If no space available on the nearest screen, try others.
    /// <param name="ignoreCenter">If there's no space left, don't move the panel to the middle.</param>
    /// </summary>
    private void RepositionCaptureControls(bool ignoreCenter = false)
    {
        if (ViewModel.Selection.Width < 20 || ViewModel.Selection.Height < 20)
        {
            var screen = ViewModel.Monitors.FirstOrDefault(x => x.Bounds.Contains(Util.Native.Other.GetMousePosition(1, Left, Top))) ?? ViewModel.MainMonitor;

            if (screen == null)
                throw new Exception("It was not possible to get a list of known screens.");

            MoveCaptureControlsToPosition(screen, screen.WorkingArea.Left + screen.WorkingArea.Width / 2 - RecorderWindow.ActualWidth / 2, screen.WorkingArea.Top + screen.WorkingArea.Height / 2 - RecorderWindow.ActualHeight / 2);

            return;
        }

        #region Calculate the available spaces for all four sides

        //If the selected region is passing the bottom edge of the display, it means that there are no space available on the bottom.
        //If the selected region is inside (bottom is below the top most part), it means that there are space available.
        //If none above, it means that the region is not located inside the screen.

        var bottomSpace = ViewModel.Selection.Bottom > ViewModel.CurrentMonitor.Bounds.Bottom ? 0 :
            ViewModel.Selection.Bottom > ViewModel.CurrentMonitor.Bounds.Top ? ViewModel.CurrentMonitor.Bounds.Bottom - ViewModel.Selection.Bottom :
            ViewModel.CurrentMonitor.Bounds.Height;

        var topSpace = ViewModel.Selection.Top < ViewModel.CurrentMonitor.Bounds.Top ? 0 :
            ViewModel.Selection.Top < ViewModel.CurrentMonitor.Bounds.Bottom ? ViewModel.Selection.Top - ViewModel.CurrentMonitor.Bounds.Top :
            ViewModel.CurrentMonitor.Bounds.Height;

        var leftSpace = ViewModel.Selection.Left < ViewModel.CurrentMonitor.Bounds.Left ? 0 :
            ViewModel.Selection.Left < ViewModel.CurrentMonitor.Bounds.Right ? ViewModel.Selection.Left - ViewModel.CurrentMonitor.Bounds.Left :
            ViewModel.CurrentMonitor.Bounds.Width;

        var rightSpace = ViewModel.Selection.Right > ViewModel.CurrentMonitor.Bounds.Right ? 0 :
            ViewModel.Selection.Right > ViewModel.CurrentMonitor.Bounds.Left ? ViewModel.CurrentMonitor.Bounds.Right - ViewModel.Selection.Right :
            ViewModel.CurrentMonitor.Bounds.Width;

        #endregion

        //Bottom.
        if (bottomSpace > (ActualHeight + 20))
        {
            MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, (ViewModel.Selection.Left + ViewModel.Selection.Width / 2 - (ActualWidth / 2))
                .Clamp(ViewModel.CurrentMonitor.Bounds.Left, ViewModel.CurrentMonitor.Bounds.Right - ActualWidth), ViewModel.Selection.Bottom + 10);
            return;
        }

        //Top.
        if (topSpace > ActualHeight + 20)
        {
            MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, (ViewModel.Selection.Left + ViewModel.Selection.Width / 2 - ActualWidth / 2)
                .Clamp(ViewModel.CurrentMonitor.Bounds.Left, ViewModel.CurrentMonitor.Bounds.Right - ActualWidth), ViewModel.Selection.Top - ActualHeight - 10);
            return;
        }

        //Left.
        if (leftSpace > ActualWidth + 20)
        {
            MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, ViewModel.Selection.Left - ActualWidth - 10, ViewModel.Selection.Top + ViewModel.Selection.Height / 2 - ActualHeight / 2);
            return;
        }

        //Right.
        if (rightSpace > ActualWidth + 20)
        {
            MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, ViewModel.Selection.Right + 10, ViewModel.Selection.Top + ViewModel.Selection.Height / 2 - ActualHeight / 2);
            return;
        }
        
        if (ignoreCenter)
        {
            //If no space left, move the control more to the left (if there's more space available to the left).
            //This is useful when the command panel is to the left of the recording, but there's no enough space.
            if (leftSpace > rightSpace && leftSpace > (ActualWidth * 0.6))
                MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, ViewModel.Selection.Left - ActualWidth - 10, ViewModel.Selection.Top + ViewModel.Selection.Height / 2 - ActualHeight / 2);

            return;
        }

        //No space available, simply center on the selected region.
        MoveCaptureControlsToPosition(ViewModel.CurrentMonitor, ViewModel.Selection.Left + ViewModel.Selection.Width / 2 - ActualWidth / 2, ViewModel.Selection.Top + ViewModel.Selection.Height / 2 - ActualHeight / 2);
    }

    private void MoveCaptureControlsToPosition(IMonitor monitor, double left, double top)
    {
        if (ViewModel.CurrentMonitor == null || !ViewModel.CurrentMonitor.Equals(monitor) || !ViewModel.CurrentMonitor.Scale.NearlyEquals(monitor.Scale))
            this.MoveToScreen(monitor);
        
        //Move the command window to the final place.
        Left = left / (this.GetVisualScale() / monitor.Scale);
        Top = top / (this.GetVisualScale() / monitor.Scale);
    }

    private void PreStart_Elapsed(object sender, EventArgs e)
    {
        if (_preStartCount >= 1)
        {
            Dispatcher.Invoke(() =>
            {
                Title = "ScreenToGif - " + LocalizationHelper.Get("S.Recorder.PreStarting");
                MainDisplayTimer.SetElapsed(-_preStartCount);
                Splash.SetTime(-_preStartCount);
            });

            _preStartCount--;
            return;
        }

        _preStartTimer.Stop();

        Dispatcher.Invoke(() =>
        {
            if (Splash.IsBeingDisplayed())
                Splash.Dismiss();

            if (IsRegionIntersected())
                WindowState = WindowState.Minimized;

            Title = "ScreenToGif";

            StartCapture();
        });
        
        if (Arguments.StartCapture && Arguments.Limit > TimeSpan.Zero)
            _limitTimer.Start();

        ViewModel.Stage = RecorderStages.Recording;
    }

    private void ShowBorderTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _showBorderTimer.Stop();

        DetectMonitorChanges();
        AdjustSelectionToScreen();
        RepositionCaptureControls();

        Visibility = Visibility.Visible;
    }

    private void FollowTimer_Tick(object sender, ElapsedEventArgs e)
    {
        if (ViewModel.Selection.IsEmpty || _prevPosX == _posX && _prevPosY == _posY || ViewModel.Stage is RecorderStages.Paused or RecorderStages.Stopped or RecorderStages.Discarding ||
            (Keyboard.Modifiers != ModifierKeys.None && Keyboard.Modifiers == UserSettings.All.DisableFollowModifiers))
            return;

        _prevPosX = _posX;
        _prevPosY = _posY;

        //Only move to the left if 'Mouse.X < Rect.L' and only move to the right if 'Mouse.X > Rect.R'
        _offsetX = _posX - UserSettings.All.FollowBuffer < ViewModel.Selection.X ? _posX - ViewModel.Selection.X - UserSettings.All.FollowBuffer :
            _posX + UserSettings.All.FollowBuffer > ViewModel.Selection.Right ? _posX - ViewModel.Selection.Right + UserSettings.All.FollowBuffer : 0;

        _offsetY = _posY - UserSettings.All.FollowBuffer < ViewModel.Selection.Y ? _posY - ViewModel.Selection.Y - UserSettings.All.FollowBuffer :
            _posY + UserSettings.All.FollowBuffer > ViewModel.Selection.Bottom ? _posY - ViewModel.Selection.Bottom + UserSettings.All.FollowBuffer : 0;

        //Hide the UI when moving.
        if (_posX - UserSettings.All.FollowBuffer - UserSettings.All.FollowBufferInvisible < ViewModel.Selection.X || _posX + UserSettings.All.FollowBuffer + UserSettings.All.FollowBufferInvisible > ViewModel.Selection.Right ||
            _posY - UserSettings.All.FollowBuffer - UserSettings.All.FollowBufferInvisible < ViewModel.Selection.Y || _posY + UserSettings.All.FollowBuffer + UserSettings.All.FollowBufferInvisible > ViewModel.Selection.Bottom)
        {
            _showBorderTimer.Stop();

            Visibility = Visibility.Hidden;
            _captureRegion.Hide();

            _showBorderTimer.Start();
        }

        //Limit to the current screen (only if in DirectX mode).
        //_viewModel.Region = new Rect(new Point((_viewModel.Region.X + _offsetX).Clamp(_viewModel.MaximumBounds.Left - 1, _viewModel.MaximumBounds.Width - _viewModel.Region.Width + 1),
        //    (_viewModel.Region.Y + _offsetY).Clamp(_viewModel.MaximumBounds.Top - 1, _viewModel.MaximumBounds.Height - _viewModel.Region.Height + 1)), _viewModel.Region.Size);

        //Limit to the current screen.
        ViewModel.Selection = new Rect(new Point((ViewModel.Selection.X + _offsetX).Clamp(ViewModel.CurrentMonitor.Bounds.Left - 1, ViewModel.CurrentMonitor.Bounds.Width - ViewModel.Selection.Width + 1),
            (ViewModel.Selection.Y + _offsetY).Clamp(ViewModel.CurrentMonitor.Bounds.Top - 1, ViewModel.CurrentMonitor.Bounds.Height - ViewModel.Selection.Height + 1)), ViewModel.Selection.Size);

        //Tell the capture helper that the position changed.
        if (Capture == null)
            return;

        Capture.Left = (int)ViewModel.SelectionScaled.Left;
        Capture.Top = (int)ViewModel.SelectionScaled.Top;
    }

    private void Limit_Elapsed(object sender, EventArgs e)
    {
        _limitTimer.Stop();

        if (!IsLoaded || (ViewModel.Stage != RecorderStages.Recording && ViewModel.Stage == RecorderStages.PreStarting))
            return;

        ViewModel.StopCommand.Execute(null, this);
    }

    private void ShowOptions(object sender, ExecutedRoutedEventArgs e)
    {
        Topmost = false;
        _captureRegion.Topmost = false;

        var options = new Options(Options.RecorderIndex);
        options.ShowDialog();

        //TODO: Should it reposition the capture controls?

        DetectCaptureFrequency();
        RegisterCommands();
        AdjustSelectionToScreen();
        RepositionCaptureControls();

        //TODO: Reload settings?

        //If not recording (or recording in manual/interactive mode, but with no frames captured yet), adjust the maximum bounds for the recorder.
        if (ViewModel.Stage == RecorderStages.Stopped || (ViewModel.CaptureFrequency is CaptureFrequencies.Manual or CaptureFrequencies.Interaction && ViewModel.Stage == RecorderStages.Recording && ViewModel.FrameCount == 0))
            ViewModel.IsDirectMode = UserSettings.All.UseDesktopDuplication;

        Topmost = true;
        _captureRegion.Topmost = true;
    }

    private void SwitchFrequency(object sender, ExecutedRoutedEventArgs e)
    {
        switch (ViewModel.CaptureFrequency)
        {
            case CaptureFrequencies.Manual:
                ViewModel.CaptureFrequency = CaptureFrequencies.Interaction;
                break;

            case CaptureFrequencies.Interaction:
                ViewModel.CaptureFrequency = CaptureFrequencies.PerSecond;
                break;

            case CaptureFrequencies.PerSecond:
                ViewModel.CaptureFrequency = CaptureFrequencies.PerMinute;
                break;

            case CaptureFrequencies.PerMinute:
                ViewModel.CaptureFrequency = CaptureFrequencies.PerHour;
                break;

            default: //PerHour.
                ViewModel.CaptureFrequency = CaptureFrequencies.Manual;
                break;
        }

        //When event is fired when the frequency is picked from the context menu, just switch the labels.
        DetectCaptureFrequency();
    }

    private void DetectCaptureFrequency()
    {
        switch (ViewModel.CaptureFrequency)
        {
            case CaptureFrequencies.Manual:
                AdjustToManual();
                break;
            case CaptureFrequencies.Interaction:
                AdjustToInteraction();
                break;
            case CaptureFrequencies.PerSecond:
                FrequencyTextBlock.SetResourceReference(TextBlock.TextProperty, "S.Recorder.Fps.Short");
                FramerateGrid.SetResourceReference(ToolTipProperty, "S.Recorder.Fps");
                AdjustToAutomatic();
                break;

            case CaptureFrequencies.PerMinute:
                FrequencyTextBlock.SetResourceReference(TextBlock.TextProperty, "S.Recorder.Fpm.Short");
                FramerateGrid.SetResourceReference(ToolTipProperty, "S.Recorder.Fpm");
                AdjustToAutomatic();
                break;

            default: //PerHour.
                FrequencyTextBlock.SetResourceReference(TextBlock.TextProperty, "S.Recorder.Fph.Short");
                FramerateGrid.SetResourceReference(ToolTipProperty, "S.Recorder.Fph");
                AdjustToAutomatic();
                break;
        }

        CommandManager.InvalidateRequerySuggested();
    }

    private void AdjustToManual()
    {
        ViewModel.Stage = RecorderStages.Recording;
        Title = "ScreenToGif";
        Capture?.StartStopwatch(HasFixedDelay(), GetFixedDelay());

        _captureRegion.DisplayGuidelines();
    }

    private void AdjustToInteraction()
    {
        ViewModel.Stage = Project?.Frames?.Count > 0 ? RecorderStages.Paused : RecorderStages.Stopped;
        Title = "ScreenToGif";
        Capture?.StartStopwatch(HasFixedDelay(), GetFixedDelay());

        _captureRegion.DisplayGuidelines();
    }

    private void AdjustToAutomatic()
    {
        ViewModel.Stage = Project?.Frames?.Count > 0 ? RecorderStages.Paused : RecorderStages.Stopped;
        Title = "ScreenToGif";
        Capture?.StopStopwatch();

        _captureRegion.DisplayGuidelines();
    }

    private async Task SelectRegion()
    {
        await RequestRegionSelection();

        AdjustSelectionToScreen();
        RepositionCaptureRegion();
        RepositionCaptureControls();
    }

    private async void Record(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            switch (ViewModel.Stage)
            {
                case RecorderStages.Stopped:
                {
                    #region If region not yet selected

                    if (ViewModel.Selection.IsEmpty)
                    {
                        await SelectRegion();

                        if (ViewModel.Selection.IsEmpty)
                            return;
                    }

                    #endregion

                    #region If interaction mode

                    if (ViewModel.CaptureFrequency == CaptureFrequencies.Interaction)
                    {
                        ViewModel.Stage = RecorderStages.Recording;
                        SetTaskbarButtonOverlay();
                        Hide();
                        return;
                    }

                    #endregion

                    #region To record

                    ViewModel.Project = RecordingProjectHelper.Create(ProjectSources.ScreenRecorder);
                    ViewModel.FrameCount = 0;

                    await PrepareCapture();

                    //FrequencyIntegerUpDown.IsEnabled = false;

                    _captureRegion.HideGuidelines();
                    Topmost = true;

                    //Tries to move the command panel away from the recording area.
                    RepositionCaptureControls(true);

                    //Detects a possible intersection of capture region and capture controls.
                    var isIntersecting = IsRegionIntersected();

                    if (isIntersecting)
                    {
                        Topmost = false;
                        Splash.Display(LocalizationHelper.GetWithFormat("S.Recorder.Splash.Title", "Press {0} to stop the recording", Util.Native.Other.GetSelectKeyText(UserSettings.All.StopShortcut, UserSettings.All.StopModifiers)),
                            LocalizationHelper.GetWithFormat("S.Recorder.Splash.Subtitle", "The recorder window will be minimized,&#10;restore it or press {0} to pause the capture", Util.Native.Other.GetSelectKeyText(UserSettings.All.StartPauseShortcut, UserSettings.All.StartPauseModifiers)));
                        Splash.SetTime(-UserSettings.All.PreStartValue);
                    }

                    #region Start

                    if (isIntersecting || UserSettings.All.UsePreStart)
                    {
                        ViewModel.Stage = RecorderStages.PreStarting;

                        Title = "ScreenToGif - " + LocalizationHelper.Get("S.Recorder.PreStarting");
                        MainDisplayTimer.SetElapsed(-UserSettings.All.PreStartValue);

                        _preStartCount = UserSettings.All.PreStartValue - 1;
                        _preStartTimer.Start();
                        return;
                    }

                    Hide();
                    StartCapture();

                    ViewModel.Stage = RecorderStages.Recording;
                    SetTaskbarButtonOverlay();

                    if (Arguments.StartCapture && Arguments.Limit > TimeSpan.Zero)
                        _limitTimer.Start();

                    #endregion

                    #endregion

                    break;
                }

                case RecorderStages.Paused:
                {
                    #region To record again

                    ViewModel.Stage = RecorderStages.Recording;
                    Title = "ScreenToGif";
                    _captureRegion.HideGuidelines();
                    SetTaskbarButtonOverlay();

                    //Tries to move the command panel away from the recording area.
                    RepositionCaptureControls(true);

                    //If it's interaction mode, the capture is done via Snap().
                    if (ViewModel.CaptureFrequency == CaptureFrequencies.Interaction)
                        return;

                    await PrepareCapture(false);

                    //Detects a possible intersection of capture region and capture controls.
                    if (IsRegionIntersected())
                        WindowState = WindowState.Minimized;

                    //FrequencyIntegerUpDown.IsEnabled = false;

                    StartCapture();

                    #endregion

                    break;
                }
            }
        }
        catch (GraphicsConfigurationException g)
        {
            LogWriter.Log(g, "Impossible to start the recording due to wrong graphics adapter.");
            GraphicsConfigurationDialog.Show(g, ViewModel.CurrentMonitor);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to start the recording.");
            ErrorDialog.ShowStatic(LocalizationHelper.Get("S.Recorder.Warning.StartPauseNotPossible"), ex.Message, ex);
        }
        finally
        {
            Arguments.ClearAutomationArgs();

            //Wait a bit, then refresh the commands. Some of the commands are dependent of the FrameCount property.
            await Task.Delay(TimeSpan.FromMilliseconds(300));

            CommandManager.InvalidateRequerySuggested();

            RepositionCaptureControls(true);
            Show();
        }
    }

    private async void Snap(object sender, ExecutedRoutedEventArgs e)
    {
        var snapTriggerDelay = GetTriggerDelay();

        if (snapTriggerDelay != 0)
            await Task.Delay(snapTriggerDelay);

        #region If region not yet selected

        if (ViewModel.Selection.IsEmpty)
        {
            await SelectRegion();

            if (ViewModel.Selection.IsEmpty)
                return;
        }

        #endregion

        _captureRegion.HideGuidelines();

        if (ViewModel.Project == null || ViewModel.Project.Frames.Count == 0)
        {
            try
            {
                ViewModel.Project = RecordingProjectHelper.Create(ProjectSources.ScreenRecorder);

                await PrepareCapture();

                Capture?.StartStopwatch(HasFixedDelay(), GetFixedDelay());
            }
            catch (Exception ex)
            {
                LogWriter.Log(ex, "Impossible to start the screencasting.");
                ErrorDialog.ShowStatic(LocalizationHelper.Get("S.Recorder.Warning.CaptureNotPossible"), ex.Message, ex);
                return;
            }
        }

        #region Take the screenshot

        try
        {
            var limit = 0;
            do
            {
                Capture.IsAcceptingEvents = true;
                ViewModel.FrameCount = await Capture.ManualCaptureAsync(new RecordingFrame(), UserSettings.All.ShowCursor);
                Capture.IsAcceptingEvents = false;

                if (limit > 5)
                    throw new Exception("Impossible to capture the manual screenshot.");

                limit++;
            }
            while (ViewModel.FrameCount == 0);

            //Displays that a frame was manually captured.
            MainDisplayTimer.ManuallyCapturedCount++;
            CommandManager.InvalidateRequerySuggested();
        }
        catch (GraphicsConfigurationException g)
        {
            LogWriter.Log(g, "Impossible to take a snap due to wrong graphics adapter.");
            GraphicsConfigurationDialog.Show(g, ViewModel.CurrentMonitor);
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to capture the manual screenshot.");
            ErrorDialog.Show("S.Recorder.Warning.CaptureNotPossible", "S.Recorder.Warning.CaptureNotPossible.Info", ex);
        }

        #endregion
    }

    private void Pause(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            if (ViewModel.Stage != RecorderStages.Recording || ViewModel.CaptureFrequency is CaptureFrequencies.Manual)
                return;

            ViewModel.Stage = RecorderStages.Paused;
            Title = "ScreenToGif";

            if (ViewModel.CaptureFrequency == CaptureFrequencies.Interaction)
                return;

            _limitTimer.Stop();
            PauseCapture();

            _captureRegion.DisplayGuidelines();
            SetTaskbarButtonOverlay();
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Impossible to pause the recording.");
            ErrorDialog.ShowStatic(LocalizationHelper.Get("S.Recorder.Warning.StartPauseNotPossible"), ex.Message, ex);
        }
    }

    private async void Stop(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            //RecordControlsGrid.IsEnabled = false;
            Title = "ScreenToGif - " + LocalizationHelper.Get("S.Recorder.Stopping");
            Cursor = Cursors.AppStarting;

            _limitTimer.Stop();
            await StopCapture();

            if (ViewModel.Stage is RecorderStages.Recording or RecorderStages.Paused && Project?.Any == true)
            {
                #region Finishes if it's recording and it has any frames

                await Task.Delay(100);

                Close();
                return;

                #endregion
            }

            #region Stops if it is not recording, or has no frames

            //Stop the pre-start timer to kill pre-start warming up.
            if (ViewModel.Stage == RecorderStages.PreStarting)
                _preStartTimer.Stop();

            Splash.Dismiss();
            ViewModel.Stage = RecorderStages.Stopped;
            Topmost = true;

            _captureRegion.DisplayGuidelines();
            SetTaskbarButtonOverlay();

            #endregion
        }
        catch (NullReferenceException nll)
        {
            LogWriter.Log(nll, "NullPointer on the Stop function");

            ErrorDialog.ShowStatic("Error while stopping", nll.Message, nll);  //TODO: Localize
        }
        catch (Exception ex)
        {
            LogWriter.Log(ex, "Error on the Stop function");

            ErrorDialog.ShowStatic("Error while stopping", ex.Message, ex); //TODO: Localize
        }
        finally
        {
            if (IsLoaded)
            {
                Title = "ScreenToGif";
                Cursor = Cursors.Arrow;
                //RecordControlsGrid.IsEnabled = true;

                //Wait a bit, then refresh the commands.
                await Task.Delay(TimeSpan.FromMilliseconds(200));

                CommandManager.InvalidateRequerySuggested();
                RepositionCaptureControls(true);
            }
        }
    }

    private async void Discard(object sender, ExecutedRoutedEventArgs e)
    {
        Pause(sender, e);

        if (UserSettings.All.NotifyRecordingDiscard && !Dialog.Ask("S.Recorder.Discard.Instruction", "S.Recorder.Discard.Message", "S.Imperative.Discard", "S.Imperative.Keep"))
            return;

        await StopCapture();

        ViewModel.FrameCount = 0;
        ViewModel.Stage = RecorderStages.Discarding;
        //RecordControlsGrid.IsEnabled = false;
        Cursor = Cursors.AppStarting;
        SetTaskbarButtonOverlay();

        await Task.Run(() =>
        {
            ViewModel.Project.Discard();
            ViewModel.Project = null;
        });

        //Enables the controls that are disabled while recording;
        //FrequencyIntegerUpDown.IsEnabled = true;
        //RecordControlsGrid.IsEnabled = true;

        Title = "ScreenToGif";
        Cursor = Cursors.Arrow;
        
        DetectCaptureFrequency();
        SetTaskbarButtonOverlay();

        //Wait a bit, then refresh the commands.
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        CommandManager.InvalidateRequerySuggested();
        RepositionCaptureControls(true);
    }
    
    private async Task PrepareCapture(bool isNew = true)
    {
        if (isNew && Capture != null)
        {
            await Capture.DisposeAsync();
            Capture = null;
        }

        //If the capture helper was initialized already, ignore this.
        if (Capture != null)
            return;

        if (UserSettings.All.UseDesktopDuplication)
        {
            //Check if Windows 8 or newer.
            if (!OperationalSystemHelper.IsWin8OrHigher())
                throw new Exception(LocalizationHelper.Get("S.Recorder.Warning.Windows8"));

            Capture = GetDirectCapture();
            Capture.DeviceName = ViewModel.CurrentMonitor.Name;

            ViewModel.IsDirectMode = true;
        }
        else
        {
            //Capture with BitBlt.
            Capture = new GdiCapture();

            ViewModel.IsDirectMode = false;
        }

        Capture.OnError += exception =>
        {
            //Pause the recording and show the error.
            ViewModel.PauseCommand.Execute(null, null);

            if (exception is GraphicsConfigurationException)
                GraphicsConfigurationDialog.Show(exception, ViewModel.CurrentMonitor);
            else
                ErrorDialog.ShowStatic(LocalizationHelper.Get("S.Recorder.Warning.CaptureNotPossible"), exception.Message, exception);

            Capture.Dispose();
            Capture = null;
        };

        Capture.Start(IsAutomaticCapture(), GetCaptureInterval(), (int)ViewModel.SelectionScaled.X, (int)ViewModel.SelectionScaled.Y, (int)ViewModel.SelectionScaled.Width, (int)ViewModel.SelectionScaled.Height, ViewModel.SelectionScale, Project);
    }

    public override void StartCapture()
    {
        MainDisplayTimer.Start();

        base.StartCapture();
    }

    public override void PauseCapture()
    {
        MainDisplayTimer.Pause();

        base.PauseCapture();
    }

    public override async Task StopCapture()
    {
        MainDisplayTimer.Stop();

        await base.StopCapture();
    }

    /// <summary>
    /// True if the capture controls are intersecting with the capture region.
    /// </summary>
    /// <returns></returns>
    private bool IsRegionIntersected()
    {
        return IsVisible && ViewModel.Selection.IntersectsWith(new Rect(Left, Top, Width, Height).Scale(ViewModel.SelectionScale));
    }

    private void Follow()
    {
        if (ViewModel.IsFollowing && UserSettings.All.FollowShortcut != Key.None)
        {
            _followTimer.Interval = (1000d / ViewModel.Framerate) / 2d;
            _followTimer.Start();
            return;
        }

        _followTimer.Stop();
    }

    private void DetectMonitorChanges()
    {
        if (ViewModel.CurrentMonitor != null && ViewModel.CurrentMonitor.Handle != ViewModel.PreviousMonitor?.Handle)
        {
            if (ViewModel.PreviousMonitor != null && ViewModel.Stage == RecorderStages.Recording && ViewModel.Project?.Any == true)
            {
                Pause(null, null);

                Capture.DeviceName = ViewModel.CurrentMonitor.Name;
                Capture?.ResetConfiguration();

                ViewModel.RecordCommand.Execute(null, this);
            }

            ViewModel.PreviousMonitor = ViewModel.CurrentMonitor;
        }
    }

    private void MoveWindow(int left, int top, int right, int bottom)
    {
        //Limit to this screen in directX capture mode.
        var x = left > 0 ? Math.Max(ViewModel.Selection.Left - left, ViewModel.MaximumBounds.Left - 1) : right > 0 ? Math.Min(ViewModel.Selection.Left + right, ViewModel.MaximumBounds.Right - ViewModel.Selection.Width + 1) : ViewModel.Selection.Left;
        var y = top > 0 ? Math.Max(ViewModel.Selection.Top - top, ViewModel.MaximumBounds.Top - 1) : bottom > 0 ? Math.Min(ViewModel.Selection.Top + bottom, ViewModel.MaximumBounds.Bottom - ViewModel.Selection.Height + 1) : ViewModel.Selection.Top;

        ViewModel.Selection = ViewModel.Selection with { X = x, Y = y };

        DetectMonitorChanges();
        AdjustSelectionToScreen();
        RepositionCaptureControls();
    }

    private void ResizeWindow(int left, int top, int right, int bottom)
    {
        //Resize to top left increases height/width when reaching the limit.

        var newLeft = left < 0 ? Math.Max(ViewModel.Selection.Left + left, ViewModel.MaximumBounds.Left - 1) : left > 0 ? ViewModel.Selection.Left + left : ViewModel.Selection.Left;
        var newTop = top < 0 ? Math.Max(ViewModel.Selection.Top + top, ViewModel.MaximumBounds.Top - 1) : top > 0 ? ViewModel.Selection.Top + top : ViewModel.Selection.Top;
        var width = (right > 0 ? Math.Min(ViewModel.Selection.Width + right, ViewModel.MaximumBounds.Right - ViewModel.Selection.Left + 1) - left : right < 0 ? ViewModel.Selection.Width + right + (left > 0 ? -left : 0) : ViewModel.Selection.Width - left);
        var height = (bottom > 0 ? Math.Min(ViewModel.Selection.Height + bottom, ViewModel.MaximumBounds.Bottom - ViewModel.Selection.Top + 1) - top : bottom < 0 ? ViewModel.Selection.Height + bottom + (top > 0 ? -top : 0) : ViewModel.Selection.Height - top);

        //Ignore input if the new size will be smaller than the minimum.
        if ((height < 25 && (top > 0 || bottom < 0)) || (width < 25 && (left > 0 || right < 0)))
            return;

        ViewModel.Selection = new Rect(newLeft, newTop, width, height);

        DetectMonitorChanges();
        AdjustSelectionToScreen();
        RepositionCaptureControls();
    }

    private void SetTaskbarButtonOverlay()
    {
        try
        {
            switch (ViewModel.Stage)
            {
                case RecorderStages.Stopped:
                    TaskbarItemInfo.Overlay = null;
                    return;

                case RecorderStages.Recording:
                    if (ViewModel.CaptureFrequency != CaptureFrequencies.Manual)
                        TaskbarItemInfo.Overlay = RecordThumbInfo.ImageSource;
                    else
                        TaskbarItemInfo.Overlay = null;
                    return;

                case RecorderStages.Paused:
                    TaskbarItemInfo.Overlay = PauseThumbInfo.ImageSource;
                    return;

                case RecorderStages.Discarding:
                    TaskbarItemInfo.Overlay = DiscardThumbInfo.ImageSource;
                    return;
            }
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Impossible to set the taskbar button overlay");
        }
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //If the selection was not forced (via arguments) and it's empty or it should always be asked.
        if (!ViewModel.RegionWasForceSelected && (ViewModel.Selection == Rect.Empty || UserSettings.All.SelectionBehavior == RecorderSelectionBehaviors.AlwaysAsk))
            await RequestRegionSelection();

        AdjustSelectionToScreen();
        RepositionCaptureRegion();
        RepositionCaptureControls();
        DetectCaptureFrequency();

        this.HideFromCapture();

        if (UserSettings.All.CursorFollowing)
            Follow();
        else
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            CommandManager.InvalidateRequerySuggested();
            RepositionCaptureControls();
        }

        //Automation arguments were passed by command line.
        if (Arguments.Open)
        {
            if (Arguments.FrequencyType.HasValue)
            {
                ViewModel.CaptureFrequency = Arguments.FrequencyType.Value;
                ViewModel.Framerate = Arguments.Frequency;
                DetectCaptureFrequency();

                Arguments.FrequencyType = null;
            }

            if (Arguments.StartCapture && ViewModel.CaptureFrequency >= CaptureFrequencies.PerSecond)
            {
                if (Arguments.Limit > TimeSpan.Zero)
                {
                    _limitTimer.Elapsed += Limit_Elapsed;
                    _limitTimer.Interval = (int)Math.Min(int.MaxValue, Arguments.Limit.TotalMilliseconds);
                }

                ViewModel.RecordCommand.Execute(null, this);
            }
            else
            {
                Arguments.ClearAutomationArgs();
            }
        }
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        lock (Lock)
        {
            if (_captureRegion.IsEnabled && _captureRegion.WindowState == WindowState.Minimized)
                _captureRegion.WindowState = WindowState.Normal;

            ViewModel.IsFollowing = UserSettings.All.CursorFollowing;

            if (!ViewModel.IsFollowing || UserSettings.All.FollowShortcut != Key.None)
                return;

            UserSettings.All.CursorFollowing = ViewModel.IsFollowing = false;
            Follow();

            Dialog.Ok("S.Options.Warning.Follow.Header", "S.Options.Warning.Follow.Message");
        }
    }

    private void CaptureRegion_DragStarted(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void CaptureRegion_PositionChanged(object sender, RegionSelectionChangedEventArgs e)
    {
        DetectMonitorChanges();

        ViewModel.SelectionScale = e.Scale;
        ViewModel.Selection = e.NewSelection;

        if (Capture != null)
        {
            Capture.Left = (int)ViewModel.SelectionScaled.Left;
            Capture.Top = (int)ViewModel.SelectionScaled.Top;
        }
    }

    private void CaptureRegion_DragEnded(object sender, RoutedEventArgs e)
    {
        DetectMonitorChanges();
        RepositionCaptureControls();
        Show();
    }
    
    private void Size_ValueChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        AdjustSelectionToScreen();
        RepositionCaptureRegion();
        RepositionCaptureControls();
    }

    private void SizeIntegerBox_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        CursorHelper.SetToPosition(sender as FrameworkElement, true);
    }

    private void SplitButton_SelectedIndexChanged(object sender, RoutedEventArgs e)
    {
        DetectCaptureFrequency();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            _captureRegion.Hide();
            return;
        }

        if (ViewModel.Stage == RecorderStages.Recording && IsRegionIntersected())
        {
            ViewModel.PauseCommand.Execute(null, null);
            Topmost = true;
        }

        RepositionCaptureRegion();
        //ForceUpdate();
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var step = (Keyboard.Modifiers & ModifierKeys.Alt) != 0 ? 5 : 1;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (ViewModel.Stage == RecorderStages.Stopped)
        {
            //Control + Shift: Expand both ways.
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                switch (key)
                {
                    case Key.Up:
                        ResizeWindow(0, -step, 0, step);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        ResizeWindow(0, step, 0, -step);
                        e.Handled = true;
                        break;
                    case Key.Left:
                        ResizeWindow(step, 0, -step, 0);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        ResizeWindow(-step, 0, step, 0);
                        e.Handled = true;
                        break;
                }

                return;
            }

            //If the Shift key is pressed, the sizing mode is enabled (bottom right).
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                switch (key)
                {
                    case Key.Left:
                        ResizeWindow(0, 0, -step, 0);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        ResizeWindow(0, 0, 0, -step);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        ResizeWindow(0, 0, step, 0);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        ResizeWindow(0, 0, 0, step);
                        e.Handled = true;
                        break;
                }

                return;
            }

            //If the Control key is pressed, the sizing mode is enabled (top left).
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                switch (key)
                {
                    case Key.Left:
                        ResizeWindow(-step, 0, 0, 0);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        ResizeWindow(0, -step, 0, 0);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        ResizeWindow(step, 0, 0, 0);
                        e.Handled = true;
                        break;
                    case Key.Down:
                        ResizeWindow(0, step, 0, 0);
                        e.Handled = true;
                        break;
                }

                return;
            }
        }

        //If no other key is pressed, move the region.
        switch (key)
        {
            case Key.Left:
                MoveWindow(step, 0, 0, 0);
                e.Handled = true;
                break;
            case Key.Up:
                MoveWindow(0, step, 0, 0);
                e.Handled = true;
                break;
            case Key.Right:
                MoveWindow(0, 0, step, 0);
                e.Handled = true;
                break;
            case Key.Down:
                MoveWindow(0, 0, 0, step);
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// MouseHook event method, detects the mouse clicks.
    /// </summary>
    private void MouseHookTarget(object sender, SimpleMouseGesture args)
    {
        try
        {
            if (IsSelecting || ViewModel.Stage == RecorderStages.Discarding)
                return;

            _posX = (int)Math.Round(args.PosX / ViewModel.SelectionScale, MidpointRounding.AwayFromZero);
            _posY = (int)Math.Round(args.PosY / ViewModel.SelectionScale, MidpointRounding.AwayFromZero);

            Capture?.RegisterCursorEvent(_posX, _posY, args.LeftButton, args.RightButton, args.MiddleButton, args.FirstExtraButton, args.SecondExtraButton, args.MouseDelta);

            if (ViewModel.Stage == RecorderStages.Recording && args.IsInteraction && ViewModel.CaptureFrequency == CaptureFrequencies.Interaction)
            {
                if (UserSettings.All.IgnoreMouseWheelInteraction && !args.IsClicked)
                    return;

                var controlHit = VisualTreeHelper.HitTest(this, Mouse.GetPosition(this));
                var selectionHit = _captureRegion.IsVisible && _captureRegion.Opacity > 0 ? VisualTreeHelper.HitTest(_captureRegion, Mouse.GetPosition(_captureRegion)) : null;

                if (controlHit == null && selectionHit == null)
                    ViewModel.SnapCommand.Execute(null, this);
            }
        }
        catch (Exception e)
        {
            LogWriter.Log(e, "Error in mouse hook target.");
        }
    }

    /// <summary>
    /// KeyHook event method. This fires when the user press a key.
    /// When using commands when the current window has no focus, pass an IInputElement as the target to make it work.
    /// </summary>
    private void KeyHookTarget(object sender, CustomKeyEventArgs e)
    {
        if (IsSelecting || ViewModel.Stage == RecorderStages.Discarding)
            return;

        //Capture when an user interactions happens.
        if (ViewModel.Stage == RecorderStages.Recording && ViewModel.CaptureFrequency == CaptureFrequencies.Interaction && !IsKeyboardFocusWithin)
            ViewModel.SnapCommand.Execute(null, this);

        //Record/snap or pause.
        if (Keyboard.Modifiers.HasFlag(UserSettings.All.StartPauseModifiers) && e.Key == UserSettings.All.StartPauseShortcut)
        {
            if (ViewModel.CaptureFrequency == CaptureFrequencies.Manual)
            {
                ViewModel.SnapCommand.Execute(null, this);
                return;
            }

            if (ViewModel.Stage == RecorderStages.Recording)
                ViewModel.PauseCommand.Execute(null, this);
            else
            {
                if (ViewModel.Selection.IsEmpty && WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                ViewModel.RecordCommand.Execute(null, this);
            }

            return;
        }

        //TODO: Maybe replace some logic with CanPause/CanRecord etc.

        if (Keyboard.Modifiers.HasFlag(UserSettings.All.StopModifiers) && e.Key == UserSettings.All.StopShortcut && ViewModel.Stage is RecorderStages.Recording or RecorderStages.Paused or RecorderStages.PreStarting)
            ViewModel.StopCommand.Execute(null, this);
        else if (Keyboard.Modifiers.HasFlag(UserSettings.All.DiscardModifiers) && e.Key == UserSettings.All.DiscardShortcut)
            ViewModel.DiscardCommand.Execute(null, this);
        else if (Keyboard.Modifiers.HasFlag(UserSettings.All.FollowModifiers) && e.Key == UserSettings.All.FollowShortcut)
        {
            UserSettings.All.CursorFollowing = ViewModel.IsFollowing = !ViewModel.IsFollowing;
            Follow();
        }
        else
            Capture?.RegisterKeyEvent(e.Key, Keyboard.Modifiers, e.IsUppercase, e.IsInjected);
    }

    private void System_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
        {
            if (ViewModel.Stage == RecorderStages.Recording)
                ViewModel.PauseCommand.Execute(sender, null);
            else if (ViewModel.Stage == RecorderStages.PreStarting)
                ViewModel.StopCommand.Execute(sender, null);

            GC.Collect();
        }
    }

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs eventArgs)
    {
        ViewModel.Monitors = MonitorHelper.AllMonitorsGranular();

        AdjustSelectionToScreen();

        if (WindowState == WindowState.Minimized && _captureRegion != null)
            _captureRegion.WindowState = WindowState.Minimized;
    }

    private async void Window_Closing(object sender, CancelEventArgs e)
    {
        //Close the selecting rectangle.
        _captureRegion?.Close();

        PersistOptions();

        #region Remove Hooks

        try
        {
            if (_actHook != null)
            {
                _actHook.OnMouseActivity -= MouseHookTarget;
                _actHook.KeyDown -= KeyHookTarget;
                _actHook.Stop(); //Stop the user activity watcher.
            }
        }
        catch (Exception) { }

        #endregion

        SystemEvents.PowerModeChanged -= System_PowerModeChanged;
        SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

        #region Stops the timers

        if (ViewModel.Stage != RecorderStages.Stopped)
        {
            _preStartTimer.Stop();
            _preStartTimer.Dispose();

            await StopCapture();
        }

        GarbageTimer?.Stop();
        _followTimer?.Stop();
        _limitTimer?.Stop();

        #endregion

        //Clean all capture resources.
        if (Capture != null)
            await Capture.DisposeAsync();

        GC.Collect();
    }
}
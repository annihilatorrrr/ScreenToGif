using ScreenToGif.Domain.Enums;
using System.Collections;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenToGif.Util.Settings;

public partial class UserSettings
{
    #region Options • Application

    private const bool IsFirstTimeOriginal = true;
    public bool IsFirstTime
    {
        get => (bool)GetValue(IsFirstTimeOriginal);
        set => SetValue(value, IsFirstTimeOriginal);
    }

    private const bool SingleInstanceOriginal = true;
    public bool SingleInstance
    {
        get => (bool)GetValue(SingleInstanceOriginal);
        set => SetValue(value, SingleInstanceOriginal);
    }

    private const bool StartMinimizedOriginal = false;
    public bool StartMinimized
    {
        get => (bool)GetValue(StartMinimizedOriginal);
        set => SetValue(value, StartMinimizedOriginal);
    }

    private const StartupWindows StartupWindowOriginal = StartupWindows.Welcome;
    public StartupWindows StartupWindow
    {
        get => (StartupWindows)GetValue(StartupWindowOriginal);
        set => SetValue(value, StartupWindowOriginal);
    }

    private const AfterRecordingWindows WindowAfterRecordingOriginal = AfterRecordingWindows.SaveDialog;
    public AfterRecordingWindows WindowAfterRecording
    {
        get => (AfterRecordingWindows)GetValue(WindowAfterRecordingOriginal);
        set => SetValue(value, WindowAfterRecordingOriginal);
    }

    private const AppThemes MainThemeOriginal = AppThemes.FollowSystem;
    public AppThemes MainTheme
    {
        get => (AppThemes)GetValue(MainThemeOriginal);
        set => SetValue(value, MainThemeOriginal);
    }

    //Notification icon actions

    private const NotificationIconActions LeftClickActionOriginal = NotificationIconActions.OpenWindow;
    public NotificationIconActions LeftClickAction
    {
        get => (NotificationIconActions)GetValue(LeftClickActionOriginal);
        set => SetValue(value, LeftClickActionOriginal);
    }

    private const StartupWindows LeftOpenWindowOriginal = StartupWindows.ScreenRecorder;
    public StartupWindows LeftOpenWindow
    {
        get => (StartupWindows)GetValue(LeftOpenWindowOriginal);
        set => SetValue(value, LeftOpenWindowOriginal);
    }

    private const NotificationIconActions DoubleLeftClickActionOriginal = NotificationIconActions.ToggleWindows;
    public NotificationIconActions DoubleLeftClickAction
    {
        get => (NotificationIconActions)GetValue(DoubleLeftClickActionOriginal);
        set => SetValue(value, LeftClickActionOriginal);
    }

    private const StartupWindows DoubleLeftOpenWindowOriginal = StartupWindows.None;
    public StartupWindows DoubleLeftOpenWindow
    {
        get => (StartupWindows)GetValue(DoubleLeftOpenWindowOriginal);
        set => SetValue(value, DoubleLeftOpenWindowOriginal);
    }

    private const NotificationIconActions MiddleClickActionOriginal = NotificationIconActions.ToggleWindows;
    public NotificationIconActions MiddleClickAction
    {
        get => (NotificationIconActions)GetValue(MiddleClickActionOriginal);
        set => SetValue(value, MiddleClickActionOriginal);
    }

    private const StartupWindows MiddleOpenWindowOriginal = StartupWindows.None;
    public StartupWindows MiddleOpenWindow
    {
        get => (StartupWindows)GetValue(MiddleOpenWindowOriginal);
        set => SetValue(value, MiddleOpenWindowOriginal);
    }

    //Updates

    private const bool CheckForUpdatesOriginal = true;
    public bool CheckForUpdates
    {
        get => (bool)GetValue(CheckForUpdatesOriginal);
        set => SetValue(value, CheckForUpdatesOriginal);
    }

    private const bool PortableUpdateOriginal = false;
    public bool PortableUpdate
    {
        get => (bool)GetValue(PortableUpdateOriginal);
        set => SetValue(value, PortableUpdateOriginal);
    }

    private const bool ForceUpdateAsAdminOriginal = false;
    public bool ForceUpdateAsAdmin
    {
        get => (bool)GetValue(ForceUpdateAsAdminOriginal);
        set => SetValue(value, ForceUpdateAsAdminOriginal);
    }

    private const bool InstallUpdatesOriginal = false;
    public bool InstallUpdates
    {
        get => (bool)GetValue(InstallUpdatesOriginal);
        set => SetValue(value, InstallUpdatesOriginal);
    }

    private const bool PromptToInstallOriginal = true;
    public bool PromptToInstall
    {
        get => (bool)GetValue(PromptToInstallOriginal);
        set => SetValue(value, PromptToInstallOriginal);
    }

    #endregion

    #region Options • Screen Recorder

    private const RecorderSelectionBehaviors SelectionBehaviorOriginal = RecorderSelectionBehaviors.AlwaysAsk;
    public RecorderSelectionBehaviors SelectionBehavior
    {
        get => (RecorderSelectionBehaviors)GetValue(SelectionBehaviorOriginal);
        set => SetValue(value, SelectionBehaviorOriginal);
    }

    //TODO: UseProjectResolutionFromEditor

    private const bool RememberScreenSelectionModeOriginal = true;
    public bool RememberScreenSelectionMode
    {
        get => (bool)GetValue(RememberScreenSelectionModeOriginal);
        set => SetValue(value, RememberScreenSelectionModeOriginal);
    }

    private const bool StartRecordingAfterSelectionOriginal = false;
    public bool StartRecordingAfterSelection
    {
        get => (bool)GetValue(StartRecordingAfterSelectionOriginal);
        set => SetValue(value, StartRecordingAfterSelectionOriginal);
    }

    //Rename: DisplayMagnifier
    private const bool MagnifierOriginal = true;
    public bool Magnifier
    {
        get => (bool)GetValue(MagnifierOriginal);
        set => SetValue(value, MagnifierOriginal);
    }

    //Rename: ImproveSelectionPerformance
    private const bool SelectionImprovementOriginal = false;
    public bool SelectionImprovement
    {
        get => (bool)GetValue(SelectionImprovementOriginal);
        set => SetValue(value, SelectionImprovementOriginal);
    }

    private const bool EnableSelectionPanningOriginal = true;
    public bool EnableSelectionPanning
    {
        get => (bool)GetValue(EnableSelectionPanningOriginal);
        set => SetValue(value, EnableSelectionPanningOriginal);
    }

    private const CaptureFrequencies CaptureFrequencyOriginal = CaptureFrequencies.PerSecond;
    public CaptureFrequencies CaptureFrequency
    {
        get => (CaptureFrequencies)GetValue(CaptureFrequencyOriginal);
        set => SetValue(value, CaptureFrequencyOriginal);
    }

    //Rename: DisplayDiscardDuringCapture
    private const bool RecorderDisplayDiscardOriginal = false;
    public bool RecorderDisplayDiscard
    {
        get => (bool)GetValue(RecorderDisplayDiscardOriginal);
        set => SetValue(value, RecorderDisplayDiscardOriginal);
    }

    private const int PlaybackDelayManualOriginal = 1000;
    /// <summary>
    /// The playback speed of the capture frame, in the "manual" mode.
    /// </summary>
    public int PlaybackDelayManual
    {
        get => (int)GetValue(PlaybackDelayManualOriginal);
        set => SetValue(value, PlaybackDelayManualOriginal);
    }

    private const int TriggerDelayManualOriginal = 0;
    /// <summary>
    /// The synthetic delay after trigger, in the "manual" capture mode.
    /// </summary>
    public int TriggerDelayManual
    {
        get => (int)GetValue(TriggerDelayManualOriginal);
        set => SetValue(value, TriggerDelayManualOriginal);
    }

    private const int PlaybackDelayInteractionOriginal = 500;
    /// <summary>
    /// The playback speed of the capture frame, in the "manual" mode.
    /// </summary>
    public int PlaybackDelayInteraction
    {
        get => (int)GetValue(PlaybackDelayInteractionOriginal);
        set => SetValue(value, PlaybackDelayInteractionOriginal);
    }

    private const int TriggerDelayInteractionOriginal = 0;
    /// <summary>
    /// The synthetic delay after trigger, in the "interaction" capture mode.
    /// </summary>
    public int TriggerDelayInteraction
    {
        get => (int)GetValue(TriggerDelayInteractionOriginal);
        set => SetValue(value, TriggerDelayInteractionOriginal);
    }

    private const bool IgnoreMouseWheelInteractionOriginal = true;
    /// <summary>
    /// ignore mouse wheel events, in the "interaction" capture mode.
    /// </summary>
    public bool IgnoreMouseWheelInteraction
    {
        get => (bool)GetValue(IgnoreMouseWheelInteractionOriginal);
        set => SetValue(value, IgnoreMouseWheelInteractionOriginal);
    }

    private const int PlaybackDelayMinuteOriginal = 66;
    /// <summary>
    /// The playback speed of the capture frame, in the "per minute" mode.
    /// </summary>
    public int PlaybackDelayMinute
    {
        get => (int)GetValue(PlaybackDelayMinuteOriginal);
        set => SetValue(value, PlaybackDelayMinuteOriginal);
    }

    private const int PlaybackDelayHourOriginal = 66;
    /// <summary>
    /// The playback speed of the capture frame, in the "per hour" mode.
    /// </summary>
    public int PlaybackDelayHour
    {
        get => (int)GetValue(PlaybackDelayHourOriginal);
        set => SetValue(value, PlaybackDelayHourOriginal);
    }

    //Rename: UseFixedPlaybackFrameRate
    private const bool FixedFrameRateOriginal = false;
    public bool FixedFrameRate
    {
        get => (bool)GetValue(FixedFrameRateOriginal);
        set => SetValue(value, FixedFrameRateOriginal);
    }

    private const bool UseDesktopDuplicationOriginal = false;
    public bool UseDesktopDuplication
    {
        get => (bool)GetValue(UseDesktopDuplicationOriginal);
        set => SetValue(value, UseDesktopDuplicationOriginal);
    }

    private const bool OnlyCaptureChangesOriginal = false;
    public bool OnlyCaptureChanges
    {
        get => (bool)GetValue(OnlyCaptureChangesOriginal);
        set => SetValue(value, OnlyCaptureChangesOriginal);
    }

    //Rename: CaptureCacheCompression
    private const CompressionLevel CaptureCompressionOriginal = CompressionLevel.Optimal;
    public CompressionLevel CaptureCompression
    {
        get => (CompressionLevel)GetValue(CaptureCompressionOriginal);
        set => SetValue(value, CaptureCompressionOriginal);
    }

    //Rename: CaptureCacheSize
    private const int MemoryCacheSizeOriginal = 250;
    public int MemoryCacheSize
    {
        get => (int)GetValue(MemoryCacheSizeOriginal);
        set => SetValue(value, MemoryCacheSizeOriginal);
    }

    private const bool PreventBlackFramesOriginal = true;
    public bool PreventBlackFrames
    {
        get => (bool)GetValue(PreventBlackFramesOriginal);
        set => SetValue(value, PreventBlackFramesOriginal);
    }

    //Rename: Capture cursor.
    private const bool ShowCursorOriginal = true;
    public bool ShowCursor
    {
        get => (bool)GetValue(ShowCursorOriginal);
        set => SetValue(value, ShowCursorOriginal);
    }

    //Rename: WaitBeforeCapture
    private const bool UsePreStartOriginal = false;
    public bool UsePreStart
    {
        get => (bool)GetValue(UsePreStartOriginal);
        set => SetValue(value, UsePreStartOriginal);
    }

    //Rename: WaitBeforeCaptureDelay
    private const int PreStartValueOriginal = 3;
    public int PreStartValue
    {
        get => (int)GetValue(PreStartValueOriginal);
        set => SetValue(value, PreStartValueOriginal);
    }

    //Rename: RemoteCaptureFix
    private const bool RemoteImprovementOriginal = false;
    public bool RemoteImprovement
    {
        get => (bool)GetValue(RemoteImprovementOriginal);
        set => SetValue(value, RemoteImprovementOriginal);
    }

    private const bool CursorFollowingOriginal = false;
    public bool CursorFollowing
    {
        get => (bool)GetValue(CursorFollowingOriginal);
        set => SetValue(value, CursorFollowingOriginal);
    }

    private const int FollowBufferOriginal = 20;
    public int FollowBuffer
    {
        get => (int)GetValue(FollowBufferOriginal);
        set => SetValue(value, FollowBufferOriginal);
    }

    private const int FollowBufferInvisibleOriginal = 20;
    public int FollowBufferInvisible
    {
        get => (int)GetValue(FollowBufferInvisibleOriginal);
        set => SetValue(value, FollowBufferInvisibleOriginal);
    }

    private const bool NotifyRecordingDiscardOriginal = true;
    public bool NotifyRecordingDiscard
    {
        get => (bool)GetValue(NotifyRecordingDiscardOriginal);
        set => SetValue(value, NotifyRecordingDiscardOriginal);
    }

    private const bool ForceGarbageCollectionOriginal = true;
    public bool ForceGarbageCollection
    {
        get => (bool)GetValue(ForceGarbageCollectionOriginal);
        set => SetValue(value, ForceGarbageCollectionOriginal);
    }
    
    private readonly Color _regionSelectionColorOriginal = Color.FromArgb(255, 171, 171, 171);
    public Color RegionSelectionColor
    {
        get => (Color)GetValue(_regionSelectionColorOriginal);
        set => SetValue(value, _regionSelectionColorOriginal);
    }

    //Guidelines.
    private const bool DisplayThirdsGuidelineOriginal = false;
    public bool DisplayThirdsGuideline
    {
        get => (bool)GetValue(DisplayThirdsGuidelineOriginal);
        set => SetValue(value, DisplayThirdsGuidelineOriginal);
    }

    private const double ThirdsGuidelineThicknessOriginal = 0.5;
    public double ThirdsGuidelineThickness
    {
        get => (double)GetValue(ThirdsGuidelineThicknessOriginal);
        set => SetValue(value, ThirdsGuidelineThicknessOriginal);
    }

    private readonly Color _thirdsGuidelineColorOriginal = Color.FromArgb(200, 157, 157, 157);
    public Color ThirdsGuidelineColor
    {
        get => (Color)GetValue(_thirdsGuidelineColorOriginal);
        set => SetValue(value, _thirdsGuidelineColorOriginal);
    }

    private readonly DoubleCollection _thirdsGuidelineStrokeDashArrayOriginal = new(new List<double> { 1, 0 });
    public DoubleCollection ThirdsGuidelineStrokeDashArray
    {
        get => (DoubleCollection)GetValue(_thirdsGuidelineStrokeDashArrayOriginal);
        set => SetValue(value, _thirdsGuidelineStrokeDashArrayOriginal);
    }

    private const bool DisplayCrosshairGuidelineOriginal = false;
    public bool DisplayCrosshairGuideline
    {
        get => (bool)GetValue(DisplayCrosshairGuidelineOriginal);
        set => SetValue(value, DisplayCrosshairGuidelineOriginal);
    }

    private const double CrosshairGuidelineThicknessOriginal = 1.5;
    public double CrosshairGuidelineThickness
    {
        get => (double)GetValue(CrosshairGuidelineThicknessOriginal);
        set => SetValue(value, CrosshairGuidelineThicknessOriginal);
    }

    private readonly Color _crosshairGuidelineColorOriginal = Color.FromArgb(168, 240, 255, 0);
    public Color CrosshairGuidelineColor
    {
        get => (Color)GetValue(_crosshairGuidelineColorOriginal);
        set => SetValue(value, _crosshairGuidelineColorOriginal);
    }

    private readonly DoubleCollection _crosshairGuidelineStrokeDashArrayOriginal = new(new List<double> { 5, 5 });
    public DoubleCollection CrosshairGuidelineStrokeDashArray
    {
        get => (DoubleCollection)GetValue(_crosshairGuidelineStrokeDashArrayOriginal);
        set => SetValue(value, _crosshairGuidelineStrokeDashArrayOriginal);
    }

    #endregion

    //Options • Webcam Recorder
    //Options • Sketchboard Recorder
    //Options • Editor

    #region Options • Shortcuts

    private const Key RecorderShortcutOriginal = Key.None;
    public Key RecorderShortcut
    {
        get => (Key)GetValue(RecorderShortcutOriginal);
        set => SetValue(value, RecorderShortcutOriginal);
    }

    private const ModifierKeys RecorderModifiersOriginal = ModifierKeys.None;
    public ModifierKeys RecorderModifiers
    {
        get => (ModifierKeys)GetValue(RecorderModifiersOriginal);
        set => SetValue(value, RecorderModifiersOriginal);
    }

    private const Key WebcamRecorderShortcutOriginal = Key.None;
    public Key WebcamRecorderShortcut
    {
        get => (Key)GetValue(WebcamRecorderShortcutOriginal);
        set => SetValue(value, WebcamRecorderShortcutOriginal);
    }

    private const ModifierKeys WebcamRecorderModifiersOriginal = ModifierKeys.None;
    public ModifierKeys WebcamRecorderModifiers
    {
        get => (ModifierKeys)GetValue(WebcamRecorderModifiersOriginal);
        set => SetValue(value, WebcamRecorderModifiersOriginal);
    }

    private const Key BoardRecorderShortcutOriginal = Key.None;
    public Key BoardRecorderShortcut
    {
        get => (Key)GetValue(BoardRecorderShortcutOriginal);
        set => SetValue(value, BoardRecorderShortcutOriginal);
    }

    private const ModifierKeys BoardRecorderModifiersOriginal = ModifierKeys.None;
    public ModifierKeys BoardRecorderModifiers
    {
        get => (ModifierKeys)GetValue(BoardRecorderModifiersOriginal);
        set => SetValue(value, BoardRecorderModifiersOriginal);
    }

    private const Key EditorShortcutOriginal = Key.None;
    public Key EditorShortcut
    {
        get => (Key)GetValue(EditorShortcutOriginal);
        set => SetValue(value, EditorShortcutOriginal);
    }

    private const ModifierKeys EditorModifiersOriginal = ModifierKeys.None;
    public ModifierKeys EditorModifiers
    {
        get => (ModifierKeys)GetValue(EditorModifiersOriginal);
        set => SetValue(value, EditorModifiersOriginal);
    }

    private const Key OptionsShortcutOriginal = Key.None;
    public Key OptionsShortcut
    {
        get => (Key)GetValue(OptionsShortcutOriginal);
        set => SetValue(value, OptionsShortcutOriginal);
    }

    private const ModifierKeys OptionsModifiersOriginal = ModifierKeys.None;
    public ModifierKeys OptionsModifiers
    {
        get => (ModifierKeys)GetValue(OptionsModifiersOriginal);
        set => SetValue(value, OptionsModifiersOriginal);
    }

    private const Key ExitShortcutOriginal = Key.None;
    public Key ExitShortcut
    {
        get => (Key)GetValue(ExitShortcutOriginal);
        set => SetValue(value, ExitShortcutOriginal);
    }

    private const ModifierKeys ExitModifiersOriginal = ModifierKeys.None;
    public ModifierKeys ExitModifiers
    {
        get => (ModifierKeys)GetValue(ExitModifiersOriginal);
        set => SetValue(value, ExitModifiersOriginal);
    }

    private const Key StartPauseShortcutOriginal = Key.F7;
    public Key StartPauseShortcut
    {
        get => (Key)GetValue(StartPauseShortcutOriginal);
        set => SetValue(value, StartPauseShortcutOriginal);
    }

    private const ModifierKeys StartPauseModifiersOriginal = ModifierKeys.None;
    public ModifierKeys StartPauseModifiers
    {
        get => (ModifierKeys)GetValue(StartPauseModifiersOriginal);
        set => SetValue(value, StartPauseModifiersOriginal);
    }

    private const Key StopShortcutOriginal = Key.F8;
    public Key StopShortcut
    {
        get => (Key)GetValue(StopShortcutOriginal);
        set => SetValue(value, StopShortcutOriginal);
    }

    private const ModifierKeys StopModifiersOriginal = ModifierKeys.None;
    public ModifierKeys StopModifiers
    {
        get => (ModifierKeys)GetValue(StopModifiersOriginal);
        set => SetValue(value, StopModifiersOriginal);
    }

    private const Key DiscardShortcutOriginal = Key.F9;
    public Key DiscardShortcut
    {
        get => (Key)GetValue(DiscardShortcutOriginal);
        set => SetValue(value, DiscardShortcutOriginal);
    }

    private const ModifierKeys DiscardModifiersOriginal = ModifierKeys.None;
    public ModifierKeys DiscardModifiers
    {
        get => (ModifierKeys)GetValue(DiscardModifiersOriginal);
        set => SetValue(value, DiscardModifiersOriginal);
    }

    private const Key FollowShortcutOriginal = Key.None;
    public Key FollowShortcut
    {
        get => (Key)GetValue(FollowShortcutOriginal);
        set => SetValue(value, FollowShortcutOriginal);
    }

    private const ModifierKeys FollowModifiersOriginal = ModifierKeys.None;
    public ModifierKeys FollowModifiers
    {
        get => (ModifierKeys)GetValue(FollowModifiersOriginal);
        set => SetValue(value, FollowModifiersOriginal);
    }

    private const ModifierKeys DisableFollowModifiersOriginal = ModifierKeys.None;
    public ModifierKeys DisableFollowModifiers
    {
        get => (ModifierKeys)GetValue(DisableFollowModifiersOriginal);
        set => SetValue(value, DisableFollowModifiersOriginal);
    }

    #endregion

    #region Startup

    private const double StartupTopOriginal = double.NaN;
    public double StartupTop
    {
        get => (double)GetValue(StartupTopOriginal);
        set => SetValue(value, StartupTopOriginal);
    }

    private const double StartupLeftOriginal = double.NaN;
    public double StartupLeft
    {
        get => (double)GetValue(StartupLeftOriginal);
        set => SetValue(value, StartupLeftOriginal);
    }

    private const double StartupHeightOriginal = double.NaN;
    public double StartupHeight
    {
        get => (double)GetValue(StartupHeightOriginal);
        set => SetValue(value, StartupHeightOriginal);
    }

    private const double StartupWidthOriginal = double.NaN;
    public double StartupWidth
    {
        get => (double)GetValue(StartupWidthOriginal);
        set => SetValue(value, StartupWidthOriginal);
    }

    private const WindowState StartupWindowStateOriginal = WindowState.Normal;
    public WindowState StartupWindowState
    {
        get => (WindowState)GetValue(StartupWindowStateOriginal);
        set => SetValue(value, StartupWindowStateOriginal);
    }

    #endregion

    #region Recorder

    private readonly Rect _selectedRegionOriginal = Rect.Empty;
    public Rect SelectedRegion
    {
        get => (Rect)GetValue(_selectedRegionOriginal);
        set => SetValue(value, _selectedRegionOriginal);
    }

    private const double SelectedRegionScaleOriginal = 1d;
    public double SelectedRegionScale
    {
        get => (double)GetValue(SelectedRegionScaleOriginal);
        set => SetValue(value, SelectedRegionScaleOriginal);
    }

    //Rename: LatestPerSecondTiming (maybe divide into multiple props)
    private const int LatestFpsOriginal = 15;
    public int LatestFps
    {
        get => (int)GetValue(LatestFpsOriginal);
        set => SetValue(value, LatestFpsOriginal);
    }

    #endregion

    //Recorder
    //Webcam Recorder
    //Sketchboard Recorder

    #region Exporter

    private const double ExporterTopOriginal = double.NaN;
    public double ExporterTop
    {
        get => (double)GetValue(ExporterTopOriginal);
        set => SetValue(value, ExporterTopOriginal);
    }

    private const double ExporterLeftOriginal = double.NaN;
    public double ExporterLeft
    {
        get => (double)GetValue(ExporterLeftOriginal);
        set => SetValue(value, ExporterLeftOriginal);
    }

    private const double ExporterHeightOriginal = double.NaN;
    public double ExporterHeight
    {
        get => (double)GetValue(ExporterHeightOriginal);
        set => SetValue(value, ExporterHeightOriginal);
    }

    private const double ExporterWidthOriginal = double.NaN;
    public double ExporterWidth
    {
        get => (double)GetValue(ExporterWidthOriginal);
        set => SetValue(value, ExporterWidthOriginal);
    }

    private const WindowState ExporterWindowStateOriginal = WindowState.Normal;
    public WindowState ExporterWindowState
    {
        get => (WindowState)GetValue(ExporterWindowStateOriginal);
        set => SetValue(value, ExporterWindowStateOriginal);
    }

    //Previewer

    private const bool ExporterLoopedPlaybackOriginal = true;
    public bool ExporterLoopedPlayback
    {
        get => (bool)GetValue(ExporterLoopedPlaybackOriginal);
        set => SetValue(value, ExporterLoopedPlaybackOriginal);
    }

    private const int ExporterPlaybackFramerateOriginal = 60;
    public int ExporterPlaybackFramerate
    {
        get => (int)GetValue(ExporterPlaybackFramerateOriginal);
        set => SetValue(value, ExporterPlaybackFramerateOriginal);
    }

    //Exporter

    //TODO: Rename ExportFormat
    private const ExportFormats ExportFormatOriginal = ExportFormats.Gif;
    public ExportFormats SaveType
    {
        get => (ExportFormats)GetValue(ExportFormatOriginal);
        set => SetValue(value, ExportFormatOriginal);
    }

    private readonly ArrayList _exportPresetsOriginal = new();
    public ArrayList ExportPresets
    {
        get => (ArrayList)GetValue(_exportPresetsOriginal);
        set => SetValue(value, _exportPresetsOriginal);
    }

    #endregion

    //Editor

    #region Editor

    private const double EditorTopOriginal = double.NaN;
    public double EditorTop
    {
        get => (double)GetValue(EditorTopOriginal);
        set => SetValue(value, EditorTopOriginal);
    }

    private const double EditorLeftOriginal = double.NaN;
    public double EditorLeft
    {
        get => (double)GetValue(EditorLeftOriginal);
        set => SetValue(value, EditorLeftOriginal);
    }

    private const double EditorHeightOriginal = double.NaN;
    public double EditorHeight
    {
        get => (double)GetValue(EditorHeightOriginal);
        set => SetValue(value, EditorHeightOriginal);
    }

    private const double EditorWidthOriginal = double.NaN;
    public double EditorWidth
    {
        get => (double)GetValue(EditorWidthOriginal);
        set => SetValue(value, EditorWidthOriginal);
    }

    private const WindowState EditorWindowStateOriginal = WindowState.Normal;
    public WindowState EditorWindowState
    {
        get => (WindowState)GetValue(EditorWindowStateOriginal);
        set => SetValue(value, EditorWindowStateOriginal);
    }

    private const double TimelineHeightOriginal = 120;
    public double TimelineHeight
    {
        get => (double)GetValue(TimelineHeightOriginal);
        set => SetValue(value, TimelineHeightOriginal);
    }

    #endregion
}
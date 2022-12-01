using Microsoft.Win32;
using ScreenToGif.Controls;
using ScreenToGif.Dialogs;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ScreenToGif.Views;

public partial class Startup : ExWindow
{
    private readonly StartupViewModel _viewModel;
    
    public Startup()
    {
        InitializeComponent();

        DataContext = _viewModel = new StartupViewModel();

        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.ScreenRecorderCommand, (sender, _) => App.OpenScreenRecorder(sender), (sender, args) => args.CanExecute = App.CanOpenRecorder(sender)),
            new CommandBinding(_viewModel.WebcamRecorderCommand, (sender, _) => App.OpenWebcamRecorder(sender), (sender, args) => args.CanExecute = App.CanOpenRecorder(sender)),
            new CommandBinding(_viewModel.BoardRecorderCommand, (sender, _) => App.OpenBoardRecorder(sender), (sender, args) => args.CanExecute = App.CanOpenRecorder(sender)),
            new CommandBinding(_viewModel.EditorCommand, (sender, _) => App.OpenEditor(sender)),
            new CommandBinding(_viewModel.UpdateCommand, (sender, _) => App.OpenUpdater(sender), (sender, args) => args.CanExecute = App.CanOpenUpdater(sender)),
            new CommandBinding(_viewModel.OptionsCommand, (sender, _) => App.OpenOptions(sender)),

            new CommandBinding(_viewModel.LoadRecentCommand, LoadProjects_Executed),
            new CommandBinding(_viewModel.ExportProjectCommand, ExportProjects_Executed),
            new CommandBinding(_viewModel.EditProjectCommand, EditProjects_Executed),
            new CommandBinding(_viewModel.RemoveProjectCommand, RemoveProjects_Executed)
        });

        _viewModel.LoadRecentCommand.Execute(null, this);

        SystemEvents.DisplaySettingsChanged += System_DisplaySettingsChanged;
    }

    private void Startup_Initialized(object sender, EventArgs e)
    {
        if (!UpdatePositioning())
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void System_DisplaySettingsChanged(object sender, EventArgs e)
    {
        UpdatePositioning(false);
    }

    private async void LoadProjects_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        await _viewModel.LoadProjects();
    }

    private void ExportProjects_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        App.ShowExporter(null, e.Parameter as RecentProjectViewModel);
    }

    private async void EditProjects_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var parameter = e.Parameter as RecentProjectViewModel;

        await App.ShowEditor(null, parameter?.Path);
    }

    private void RemoveProjects_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var parameter = e.Parameter as RecentProjectViewModel;

        //TODO: Custom, message "Are you sure that you want to discard?"

        if (UserSettings.All.NotifyRecordingDiscard && !Dialog.Ask("S.Recorder.Discard.Instruction", "S.Recorder.Discard.Message", "S.Imperative.Discard", "S.Imperative.Keep"))
            return;

        _viewModel.RemoveProject(parameter);
    }

    private void Startup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        SystemEvents.DisplaySettingsChanged -= System_DisplaySettingsChanged;

        //Manually get the position/size of the window, so it's possible opening multiple instances.
        UserSettings.All.StartupTop = Top;
        UserSettings.All.StartupLeft = Left;
        UserSettings.All.StartupWidth = Width;
        UserSettings.All.StartupHeight = Height;
        UserSettings.All.StartupWindowState = WindowState;
        UserSettings.Save();
    }
    
    /// <summary>
    /// Tries to adjust the position/size of the window, centers on screen otherwise.
    /// </summary>
    /// <param name="onLoad">True if called after load.</param>
    /// <returns>True if it was possible to return to last position.</returns>
    private bool UpdatePositioning(bool onLoad = true)
    {
        var top = onLoad ? UserSettings.All.StartupTop : Top;
        var left = onLoad ? UserSettings.All.StartupLeft : Left;
        var width = onLoad ? UserSettings.All.StartupWidth : Width;
        var height = onLoad ? UserSettings.All.StartupHeight : Height;
        var state = onLoad ? UserSettings.All.StartupWindowState : WindowState;

        //If the position was never set, let it center on screen. 
        if (double.IsNaN(top) && double.IsNaN(left))
            return false;

        //The catch here is to get the closest monitor from current Top/Left point. 
        var monitors = MonitorHelper.AllMonitorsScaled(this.GetVisualScale());
        var closest = monitors.FirstOrDefault(x => x.Bounds.Contains(new Point((int)left, (int)top))) ?? monitors.FirstOrDefault(x => x.IsPrimary);

        if (closest == null)
            return false;

        //To much to the Left.
        if (closest.WorkingArea.Left > left + width - 100)
            left = closest.WorkingArea.Left;

        //Too much to the top.
        if (closest.WorkingArea.Top > top + height - 100)
            top = closest.WorkingArea.Top;

        //Too much to the right.
        if (closest.WorkingArea.Right < left + 100)
            left = closest.WorkingArea.Right - width;

        //Too much to the bottom.
        if (closest.WorkingArea.Bottom < top + 100)
            top = closest.WorkingArea.Bottom - height;

        if (top is > int.MaxValue or < int.MinValue || left is > int.MaxValue or < int.MinValue || width is > int.MaxValue or < 0 || height is > int.MaxValue or < 0)
        {
            var desc = $"On load: {onLoad}\nScale: {this.GetVisualScale()}\n\n" +
                       $"Screen: {closest.AdapterName}\nBounds: {closest.Bounds}\n\nTopLeft: {top}x{left}\nWidthHeight: {width}x{height}\n\n" +
                       $"TopLeft Settings: {UserSettings.All.StartupTop}x{UserSettings.All.StartupLeft}\nWidthHeight Settings: {UserSettings.All.StartupWidth}x{UserSettings.All.StartupHeight}";

            LogWriter.Log("Wrong Startup window sizing", desc);
            return false;
        }

        Top = top;
        Left = left;
        Width = width;
        Height = height;
        WindowState = state;

        return true;
    }
}
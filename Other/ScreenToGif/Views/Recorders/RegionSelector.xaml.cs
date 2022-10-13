using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Domain.Models;
using ScreenToGif.Util.Extensions;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ScreenToGif.Views.Recorders;

public partial class RegionSelector : Window
{
    private readonly RegionSelectorViewModel _viewModel;
    private Action<IMonitor, Rect> _selected;
    private Action<IMonitor> _gotHover;
    private Action<RegionSelectionModes> _modeChanged;
    private Action _aborted;

    public RegionSelector()
    {
        InitializeComponent();

        DataContext = _viewModel = new RegionSelectorViewModel();

        CommandBindings.Clear();
        CommandBindings.AddRange(new[]
        {
            new CommandBinding(_viewModel.ChangeModeCommand, ChangeMode_Executed),
            new CommandBinding(_viewModel.CancelCommand, Cancel_Executed),
        });
    }

    public void Select(IMonitor monitor, Action<IMonitor, Rect> selected, Action<IMonitor> gotHover, Action<RegionSelectionModes> modeChanged, Action aborted)
    {
        //Resize to fit given window.
        Left = monitor.Bounds.Left;
        Top = monitor.Bounds.Top;
        Width = monitor.Bounds.Width;
        Height = monitor.Bounds.Height;

        _viewModel.Monitor = monitor;

        _selected = selected;
        _gotHover = gotHover;
        _modeChanged = modeChanged;
        _aborted = aborted;

        SelectElement.Scale = monitor.Scale;
        SelectElement.ParentLeft = Left;
        SelectElement.ParentTop = Top;
        SelectElement.BackgroundImage = _viewModel.CaptureBackground();

        if (UserSettings.All.SelectionImprovement)
        {
            AllowsTransparency = false;
            Background = new ImageBrush(_viewModel.CaptureBackground(false));
        }

        //Get only the windows that are located inside the given screen.
        var win = Util.Native.Windows.EnumerateWindowsByMonitor(monitor);

        //Since each region selector is attached to a single screen, the list of positions must be translated.
        _viewModel.Windows = win.AdjustPosition(monitor.Bounds.Left, monitor.Bounds.Top);
        _viewModel.Monitors = new List<DetectedRegion>
        {
            new(monitor.Handle, new Rect(new Size(monitor.Bounds.Width, monitor.Bounds.Height)), monitor.Name)
        };

        ControlsBorder.MouseEnter += (sender, args) => SelectElement.HideZoom();

        Show();

        this.MoveToScreen(monitor, true);
    }

    public void ClearHoverEffects()
    {
        SelectElement.HideZoom();
    }

    private void ChangeMode_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        switch (_viewModel.SelectionMode)
        {
            case RegionSelectionModes.Window:
            {
                SelectElement.Retry();
                SelectElement.Windows = _viewModel.Windows;
                break;
            }

            case RegionSelectionModes.Fullscreen:
            {
                SelectElement.Retry();
                SelectElement.Windows = _viewModel.Monitors;
                break;
            }

            default:
            {
                SelectElement.Retry();
                break;
            }
        }

        _modeChanged.Invoke(_viewModel.SelectionMode);
    }

    private void Cancel_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        _aborted.Invoke();

        Close();
    }

    //private BitmapSource CaptureBackground(bool addPadding = true)
    //{
    //    //A 7 pixel offset is added to allow the crop by the magnifying glass.
    //    if (addPadding)
    //        return Capture.CaptureScreenAsBitmapSource((int)Math.Round((Width + 14 + 1) * _viewModel.Monitor.Scale), (int)Math.Round((Height + 14 + 1) * _viewModel.Monitor.Scale),
    //            (int)Math.Round((Left - 7) * _viewModel.Monitor.Scale), (int)Math.Round((Top - 7) * _viewModel.Monitor.Scale));

    //    return Capture.CaptureScreenAsBitmapSource((int)Math.Round(Width * _viewModel.Monitor.Scale), (int)Math.Round(Height * _viewModel.Monitor.Scale),
    //        (int)Math.Round(Left * _viewModel.Monitor.Scale), (int)Math.Round(Top * _viewModel.Monitor.Scale));
    //}

    private void SelectElement_SelectionChanged(object sender, RoutedEventArgs e)
    {
        //Hide or show controls based on selection.
        var topMargin = SelectElement.Selected != Rect.Empty ? - 50 : 10;

        ControlsBorder.BeginAnimation(MarginProperty, new ThicknessAnimation(ControlsBorder.Margin, new Thickness(0, topMargin, 0, 0), new Duration(new TimeSpan(0, 0, 0, 0, 100)), FillBehavior.HoldEnd), HandoffBehavior.Compose);
    }

    private void SelectElement_MouseHovering(object sender, RoutedEventArgs e)
    {
        _gotHover.Invoke(_viewModel.Monitor);
    }

    private void SelectElement_SelectionAccepted(object sender, RoutedEventArgs e)
    {
        _selected.Invoke(_viewModel.Monitor, SelectElement.Selected.Translate(_viewModel.Monitor.Bounds.Left, _viewModel.Monitor.Bounds.Top)); //NonExpandedSelection
        Close();
    }
}
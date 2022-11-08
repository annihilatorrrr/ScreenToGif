using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Events;
using ScreenToGif.Domain.Interfaces;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Util;
using ScreenToGif.Util.Extensions;
using ScreenToGif.Util.Native;
using ScreenToGif.Util.Settings;
using ScreenToGif.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ScreenToGif.Views.Recorders;

public partial class CaptureRegion : Window
{
    private IMonitor _monitor;
    private HorizontalAlignment _horizontalAlignment;
    private VerticalAlignment _verticalAlignment;
    private Point _previousPoint;
    private readonly CaptureRegionViewModel _viewModel = new();

    #region Custom events

    public static readonly RoutedEvent DragStartedEvent = EventManager.RegisterRoutedEvent(nameof(DragStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CaptureRegion));
    public static readonly RoutedEvent PositionChangedEvent = EventManager.RegisterRoutedEvent(nameof(PositionChanged), RoutingStrategy.Bubble, typeof(RegionSelectionChangedEventHandler), typeof(CaptureRegion));
    public static readonly RoutedEvent DragEndedEvent = EventManager.RegisterRoutedEvent(nameof(DragEnded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CaptureRegion));
    
    public event RoutedEventHandler DragStarted
    {
        add => AddHandler(DragStartedEvent, value);
        remove => RemoveHandler(DragStartedEvent, value);
    }

    public event RegionSelectionChangedEventHandler PositionChanged
    {
        add => AddHandler(PositionChangedEvent, value);
        remove => RemoveHandler(PositionChangedEvent, value);
    }
    
    public event RoutedEventHandler DragEnded
    {
        add => AddHandler(DragEndedEvent, value);
        remove => RemoveHandler(DragEndedEvent, value);
    }
    
    private void RaiseDragStarted()
    {
        if (DragStartedEvent == null || !IsLoaded)
            return;

        RaiseEvent(new RoutedEventArgs(DragStartedEvent));
    }
    
    private void RaisePositionChanged(Rect selection, double scale)
    {
        if (PositionChangedEvent == null || !IsLoaded)
            return;

        RaiseEvent(new RegionSelectionChangedEventArgs(PositionChangedEvent, selection, scale));
    }

    private void RaiseDragEnded()
    {
        if (DragEndedEvent == null || !IsLoaded)
            return;

        RaiseEvent(new RoutedEventArgs(DragEndedEvent));
    }

    #endregion

    public CaptureRegion()
    {
        InitializeComponent();

        DataContext = _viewModel;

        LoadSettings();
    }

    public void Select(RegionSelectionModes? mode, Rect selection, IMonitor monitor = null)
    {
        #region Reposition on correct monitor

        //When the region switches monitors, move the selection to the new monitor, so that the scale of the UI changes.
        //This solves the issue where the UI would move to the wrong position.
        if (monitor != null)
        {
            //If the new region is in another screen, move the panel to the new screen first, to adjust the UI to the screen DPI.
            if (_monitor != null && (!_monitor.Equals(monitor) || !_monitor.Scale.NearlyEquals(monitor.Scale)))
            {
                if (double.IsNaN(Left) || double.IsNaN(Top))
                    Show();

                this.MoveToScreen(monitor);
            }

            _monitor = monitor;
        }
        else
        {
            //TODO: Maybe get the monitor which intersects the most with the region.
            _monitor = MonitorHelper.FromPoint((int)selection.X, (int)selection.Y);
        }

        #endregion

        _viewModel.Mode = mode ?? _viewModel.Mode;
        _viewModel.Selection = selection;

        DisplaySelection();
        DisplayThumbs();
        Show();
    }

    public void LoadSettings()
    {
        _viewModel.RegionSelectionBrush = new SolidColorBrush(UserSettings.All.RegionSelectionColor);
        _viewModel.ThirdsGuidelineBrush = new SolidColorBrush(UserSettings.All.ThirdsGuidelineColor);
        _viewModel.CrosshairGuidelineBrush = new SolidColorBrush(UserSettings.All.CrosshairGuidelineColor);
        _viewModel.ThirdsGuidelineVisibility = UserSettings.All.DisplayThirdsGuideline ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.CrosshairGuidelineVisibility = UserSettings.All.DisplayCrosshairGuideline ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.DisplayPanner = UserSettings.All.EnableSelectionPanning;
    }

    private void DisplaySelection(bool ignoreThumbs = true)
    {
        Left = (_viewModel.Selection.Left - (ignoreThumbs || _horizontalAlignment == HorizontalAlignment.Right ? 0 : HorizontalBorder.ActualWidth)) / (this.GetVisualScale() / _monitor.Scale);
        Top = (_viewModel.Selection.Top - (ignoreThumbs || _verticalAlignment == VerticalAlignment.Bottom ? 0 : VerticalBorder.ActualHeight)) / (this.GetVisualScale() / _monitor.Scale);

        //SelectionRectangle.Width = Rect.Width;
        //SelectionRectangle.Height = Rect.Height;
    }

    private void DisplayThumbs()
    {
        if (_viewModel.IsStatic)
        {
            HorizontalBorder.Visibility = Visibility.Collapsed;
            CornerBorder.Visibility = Visibility.Collapsed;
            VerticalBorder.Visibility = Visibility.Collapsed;
            return;
        }

        //Detect the space left on all 4 sides.
        var leftSpace = _viewModel.Selection.X - _monitor.Bounds.X;
        var topSpace = _viewModel.Selection.Y - _monitor.Bounds.Y;
        var rightSpace = _monitor.Bounds.Right - _viewModel.Selection.Right;
        var bottomSpace = _monitor.Bounds.Bottom - _viewModel.Selection.Bottom;

        //Display the thumb to the left if there's space on the left and not enough space on the right.
        //Display the thumb to the top if there's space on the top and not enough space on the bottom.
        _horizontalAlignment = rightSpace < 10 && leftSpace > 10 ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        _verticalAlignment = bottomSpace < 10 && topSpace > 10 ? VerticalAlignment.Top : VerticalAlignment.Bottom;

        #region Position the thumbs

        //Visibility as hidden, to have the size available.
        if (_horizontalAlignment != HorizontalAlignment.Right)
        {
            HorizontalBorder.Visibility = Visibility.Hidden;
            HorizontalBorder.Refresh();
        }

        if (_verticalAlignment != VerticalAlignment.Bottom)
        {
            VerticalBorder.Visibility = Visibility.Hidden;
            VerticalBorder.Refresh();
        }

        //Offset.
        Left = (_viewModel.Selection.Left - (_horizontalAlignment == HorizontalAlignment.Right ? 0 : HorizontalBorder.ActualWidth)) / (this.GetVisualScale() / _monitor.Scale);
        Top = (_viewModel.Selection.Top - (_verticalAlignment == VerticalAlignment.Bottom ? 0 : VerticalBorder.ActualHeight)) / (this.GetVisualScale() / _monitor.Scale);

        //Grid positioning.
        Grid.SetRow(HorizontalBorder, 1);
        Grid.SetColumn(HorizontalBorder, _horizontalAlignment == HorizontalAlignment.Right ? 2 : 0);

        Grid.SetRow(CornerBorder, _verticalAlignment == VerticalAlignment.Bottom ? 2 : 0);
        Grid.SetColumn(CornerBorder, _horizontalAlignment == HorizontalAlignment.Right ? 2 : 0);

        Grid.SetRow(VerticalBorder, _verticalAlignment == VerticalAlignment.Bottom ? 2 : 0);
        Grid.SetColumn(VerticalBorder, 1);

        //Alignment.
        VerticalBorder.HorizontalAlignment = _horizontalAlignment;
        HorizontalBorder.VerticalAlignment = _verticalAlignment;

        //Corners.
        HorizontalBorder.CornerRadius = new CornerRadius
        {
            TopLeft = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0,
            TopRight = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0,
            BottomRight = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0,
            BottomLeft = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0
        };
        CornerBorder.CornerRadius = new CornerRadius
        {
            TopLeft = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0,
            TopRight = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0,
            BottomRight = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0,
            BottomLeft = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0
        };
        VerticalBorder.CornerRadius = new CornerRadius
        {
            TopLeft = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0,
            TopRight = _verticalAlignment == VerticalAlignment.Top && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0,
            BottomRight = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Left ? 15 : 0,
            BottomLeft = _verticalAlignment == VerticalAlignment.Bottom && _horizontalAlignment == HorizontalAlignment.Right ? 15 : 0
        };

        //Borders.
        HorizontalBorder.BorderThickness = new Thickness
        {
            Left = _horizontalAlignment == HorizontalAlignment.Left ? 1 : 0,
            Top = _verticalAlignment == VerticalAlignment.Bottom ? 1 : 0,
            Right = _horizontalAlignment == HorizontalAlignment.Right ? 1 : 0,
            Bottom = _verticalAlignment == VerticalAlignment.Top ? 1 : 0
        };
        CornerBorder.BorderThickness = new Thickness
        {
            Left = _horizontalAlignment == HorizontalAlignment.Left ? 1 : 0,
            Top = _verticalAlignment == VerticalAlignment.Top ? 1 : 0,
            Right = _horizontalAlignment == HorizontalAlignment.Right ? 1 : 0,
            Bottom = _verticalAlignment == VerticalAlignment.Bottom ? 1 : 0
        };
        VerticalBorder.BorderThickness = new Thickness
        {
            Left = _horizontalAlignment == HorizontalAlignment.Right ? 1 : 0,
            Top = _verticalAlignment == VerticalAlignment.Top ? 1 : 0,
            Right = _horizontalAlignment == HorizontalAlignment.Left ? 1 : 0,
            Bottom = _verticalAlignment == VerticalAlignment.Bottom ? 1 : 0
        };

        //Tooltips.
        ToolTipService.SetPlacement(HorizontalBorder, _horizontalAlignment == HorizontalAlignment.Right ? PlacementMode.Right : PlacementMode.Left);
        ToolTipService.SetPlacement(CornerBorder, _horizontalAlignment == HorizontalAlignment.Right ? PlacementMode.Right : PlacementMode.Left);
        ToolTipService.SetPlacement(VerticalBorder, _verticalAlignment == VerticalAlignment.Bottom ? PlacementMode.Bottom : PlacementMode.Top);

        //Visibility.
        HorizontalBorder.Visibility = Visibility.Visible;
        CornerBorder.Visibility = Visibility.Visible;
        VerticalBorder.Visibility = Visibility.Visible;

        #endregion
    }
    
    public void DisplayGuidelines()
    {
        GuidelinesGrid.Visibility = Visibility.Visible;
    }

    public void HideGuidelines()
    {
        GuidelinesGrid.Visibility = Visibility.Collapsed;
        Dispatcher.Invoke(new Action(() => { }), DispatcherPriority.ContextIdle, null);
    }

    private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.IsStatic || sender is not Border border)
            return;

        border.CaptureMouse();

        _previousPoint = PointToScreen(e.GetPosition(this));
        RaiseDragStarted();

        e.Handled = true;
    }

    private void Thumb_MouseMove(object sender, MouseEventArgs e)
    {
        if (_viewModel.IsStatic || sender is not Border { IsMouseCaptured: true } || e.LeftButton != MouseButtonState.Pressed)
            return;

        var currentPosition = PointToScreen(e.GetPosition(this));

        //Detect how much the mouse cursor was moved.
        var scale = this.GetVisualScale();
        var x = _viewModel.Selection.X + (currentPosition.X - _previousPoint.X) / scale;
        var y = _viewModel.Selection.Y + (currentPosition.Y - _previousPoint.Y) / scale;

        //Limit the drag to the current screen.
        if (x < _monitor.Bounds.X - 1)
            x = _monitor.Bounds.X - 1;

        if (y < _monitor.Bounds.Y - 1)
            y = _monitor.Bounds.Y - 1;

        if (x + _viewModel.Selection.Width > _monitor.Bounds.Right + 1)
            x = _monitor.Bounds.Right + 1 - _viewModel.Selection.Width;

        if (y + _viewModel.Selection.Height > _monitor.Bounds.Bottom + 1)
            y = _monitor.Bounds.Bottom + 1 - _viewModel.Selection.Height;

        //Is there any way to prevent mouse going towards the edges when the region is already touching it?

        //Move the selection.
        _viewModel.Selection = _viewModel.Selection with { X = x, Y = y };

        DisplaySelection(false);
        RaisePositionChanged(_viewModel.Selection, scale);

        _previousPoint = currentPosition;
        e.Handled = true;
    }

    private void Thumb_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.IsStatic || sender is not Border { IsMouseCaptured: true } border)
            return;

        border.ReleaseMouseCapture();

        DisplayThumbs();
        RaiseDragEnded();
    }
}
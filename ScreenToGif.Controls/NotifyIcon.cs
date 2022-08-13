using ScreenToGif.Domain.Enums;
using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Native.External;
using ScreenToGif.Native.Helpers;
using ScreenToGif.Native.Structs;
using ScreenToGif.Util;
using ScreenToGif.Util.Native;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace ScreenToGif.Controls;

public class NotifyIcon : FrameworkElement, IDisposable
{
    #region Variables

    private Icon _icon;

    /// <summary>
    /// Represents the current icon data.
    /// </summary>
    private NotifyIconData _iconData;

    /// <summary>
    /// Receives messages from the taskbar icon.
    /// </summary>
    private readonly WindowMessageSink _messageSink;

    #endregion

    #region Dependencies

    public static readonly DependencyProperty IconSourceProperty = DependencyProperty.Register(nameof(IconSource), typeof(ImageSource), typeof(NotifyIcon), new FrameworkPropertyMetadata(null, IconSourcePropertyChanged));

    public static readonly DependencyProperty NotifyToolTipProperty = DependencyProperty.Register(nameof(NotifyToolTip), typeof(UIElement), typeof(NotifyIcon), new FrameworkPropertyMetadata(null, ToolTipPropertyChanged));

    public static readonly DependencyProperty NotifyToolTipTextProperty = DependencyProperty.Register(nameof(NotifyToolTipText), typeof(string), typeof(NotifyIcon), new FrameworkPropertyMetadata(string.Empty, ToolTipTextPropertyChanged));

    private static readonly DependencyPropertyKey NotifyToolTipElementPropertyKey = DependencyProperty.RegisterReadOnly(nameof(NotifyToolTipElement), typeof(ToolTip), typeof(NotifyIcon), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty NotifyToolTipElementProperty = NotifyToolTipElementPropertyKey.DependencyProperty;

    private static readonly DependencyProperty LeftClickCommandProperty = DependencyProperty.Register(nameof(LeftClickCommand), typeof(ICommand), typeof(NotifyIcon), new FrameworkPropertyMetadata(null));

    private static readonly DependencyProperty DoubleLeftClickCommandProperty = DependencyProperty.Register(nameof(DoubleLeftClickCommand), typeof(ICommand), typeof(NotifyIcon), new FrameworkPropertyMetadata(null));

    private static readonly DependencyProperty MiddleClickCommandProperty = DependencyProperty.Register(nameof(MiddleClickCommand), typeof(ICommand), typeof(NotifyIcon), new FrameworkPropertyMetadata(null));

    public static readonly RoutedEvent TrayMouseMoveEvent = EventManager.RegisterRoutedEvent(nameof(TrayMouseMove), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayLeftMouseDownEvent = EventManager.RegisterRoutedEvent(nameof(TrayLeftMouseDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayRightMouseDownEvent = EventManager.RegisterRoutedEvent(nameof(TrayRightMouseDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayMiddleMouseDownEvent = EventManager.RegisterRoutedEvent(nameof(TrayMiddleMouseDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayLeftMouseUpEvent = EventManager.RegisterRoutedEvent(nameof(TrayLeftMouseUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayRightMouseUpEvent = EventManager.RegisterRoutedEvent(nameof(TrayRightMouseUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayMiddleMouseUpEvent = EventManager.RegisterRoutedEvent(nameof(TrayMiddleMouseUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayMouseDoubleClickEvent = EventManager.RegisterRoutedEvent(nameof(TrayMouseDoubleClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent PreviewTrayContextMenuOpenEvent = EventManager.RegisterRoutedEvent(nameof(PreviewTrayContextMenuOpen), RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent TrayContextMenuOpenEvent = EventManager.RegisterRoutedEvent(nameof(TrayContextMenuOpen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent PreviewToolTipOpenEvent = EventManager.RegisterRoutedEvent(nameof(PreviewToolTipOpen), RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent ToolTipOpenEvent = EventManager.RegisterRoutedEvent(nameof(ToolTipOpen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent PreviewToolTipCloseEvent = EventManager.RegisterRoutedEvent(nameof(PreviewToolTipClose), RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(NotifyIcon));

    public static readonly RoutedEvent ToolTipCloseEvent = EventManager.RegisterRoutedEvent(nameof(ToolTipClose), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NotifyIcon));

    #endregion

    #region Properties

    /// <summary>
    /// Indicates whether the taskbar icon has been created or not.
    /// </summary>
    public bool IsTaskbarIconCreated { get; private set; }

    /// <summary>
    /// Checks whether a non-tooltip popup is currently opened.
    /// </summary>
    private bool IsPopupOpen => ContextMenu?.IsOpen ?? false;

    public bool IsDisposed { get; private set; }

    [Browsable(false)]
    public Icon Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            _iconData.IconHandle = value == null ? IntPtr.Zero : _icon.Handle;

            NotifyIconHelper.WriteIconData(ref _iconData, NotifyCommands.Modify, IconDataMembers.Icon);
        }
    }

    public ImageSource IconSource
    {
        get => (ImageSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public string NotifyToolTipText
    {
        get => (string)GetValue(NotifyToolTipTextProperty);
        set => SetValue(NotifyToolTipTextProperty, value);
    }

    public UIElement NotifyToolTip
    {
        get => (UIElement)GetValue(NotifyToolTipProperty);
        set => SetValue(NotifyToolTipProperty, value);
    }

    [Bindable(true)]
    public ToolTip NotifyToolTipElement => (ToolTip)GetValue(NotifyToolTipElementProperty);

    public ICommand LeftClickCommand
    {
        get => (ICommand)GetValue(LeftClickCommandProperty);
        set => SetValue(LeftClickCommandProperty, value);
    }

    public ICommand DoubleLeftClickCommand
    {
        get => (ICommand)GetValue(DoubleLeftClickCommandProperty);
        set => SetValue(DoubleLeftClickCommandProperty, value);
    }
                
    public ICommand MiddleClickCommand
    {
        get => (ICommand)GetValue(MiddleClickCommandProperty);
        set => SetValue(MiddleClickCommandProperty, value);
    }

    public event RoutedEventHandler TrayMouseMove
    {
        add => AddHandler(TrayMouseMoveEvent, value);
        remove => RemoveHandler(TrayMouseMoveEvent, value);
    }

    public event RoutedEventHandler TrayLeftMouseDown
    {
        add => AddHandler(TrayLeftMouseDownEvent, value);
        remove => RemoveHandler(TrayLeftMouseDownEvent, value);
    }

    public event RoutedEventHandler TrayRightMouseDown
    {
        add => AddHandler(TrayRightMouseDownEvent, value);
        remove => RemoveHandler(TrayRightMouseDownEvent, value);
    }

    public event RoutedEventHandler TrayMiddleMouseDown
    {
        add => AddHandler(TrayMiddleMouseDownEvent, value);
        remove => RemoveHandler(TrayMiddleMouseDownEvent, value);
    }

    public event RoutedEventHandler TrayLeftMouseUp
    {
        add => AddHandler(TrayLeftMouseUpEvent, value);
        remove => RemoveHandler(TrayLeftMouseUpEvent, value);
    }

    public event RoutedEventHandler TrayRightMouseUp
    {
        add => AddHandler(TrayRightMouseUpEvent, value);
        remove => RemoveHandler(TrayRightMouseUpEvent, value);
    }

    public event RoutedEventHandler TrayMiddleMouseUp
    {
        add => AddHandler(TrayMiddleMouseUpEvent, value);
        remove => RemoveHandler(TrayMiddleMouseUpEvent, value);
    }

    public event RoutedEventHandler TrayMouseDoubleClick
    {
        add => AddHandler(TrayMouseDoubleClickEvent, value);
        remove => RemoveHandler(TrayMouseDoubleClickEvent, value);
    }

    public event RoutedEventHandler PreviewTrayContextMenuOpen
    {
        add => AddHandler(PreviewTrayContextMenuOpenEvent, value);
        remove => RemoveHandler(PreviewTrayContextMenuOpenEvent, value);
    }

    public event RoutedEventHandler TrayContextMenuOpen
    {
        add => AddHandler(TrayContextMenuOpenEvent, value);
        remove => RemoveHandler(TrayContextMenuOpenEvent, value);
    }

    public event RoutedEventHandler PreviewToolTipOpen
    {
        add => AddHandler(PreviewToolTipOpenEvent, value);
        remove => RemoveHandler(PreviewToolTipOpenEvent, value);
    }

    public event RoutedEventHandler ToolTipOpen
    {
        add => AddHandler(ToolTipOpenEvent, value);
        remove => RemoveHandler(ToolTipOpenEvent, value);
    }

    public event RoutedEventHandler PreviewToolTipClose
    {
        add => AddHandler(PreviewToolTipCloseEvent, value);
        remove => RemoveHandler(PreviewToolTipCloseEvent, value);
    }

    public event RoutedEventHandler ToolTipClose
    {
        add => AddHandler(ToolTipCloseEvent, value);
        remove => RemoveHandler(ToolTipCloseEvent, value);
    }

    #endregion

    #region Property Changes

    private static void VisibilityPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var control = o as NotifyIcon;
        var newValue = (Visibility)e.NewValue;

        if (control == null)
            return;

        if (newValue == Visibility.Visible)
            control.CreateTaskbarIcon();
        else
            control.RemoveTaskbarIcon();
    }

    private static void DataContextPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is not NotifyIcon control)
            return;

        control.UpdateDataContext(control.NotifyToolTipElement, e.OldValue, e.NewValue);
        control.UpdateDataContext(control.ContextMenu, e.OldValue, e.NewValue);
    }

    private static void ContextMenuPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var control = o as NotifyIcon;

        if (e.NewValue is not ContextMenu newValue)
            return;

        control?.UpdateDataContext(newValue, null, control.DataContext);
    }

    private static void IconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = d as NotifyIcon;
        var value = (ImageSource)e.NewValue;

        if (owner != null && value != null && !VisualHelper.IsInDesignMode())
            owner.Icon = value.ToIcon();
    }

    private static void ToolTipPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NotifyIcon owner)
            return;

        owner.CreateCustomToolTip();
        owner.WriteToolTipSettings();
    }

    private static void ToolTipTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NotifyIcon owner)
            return;

        if (owner.NotifyToolTip == null)
        {
            //Create or just update the tooltip.
            if (owner.NotifyToolTipElement == null)
                owner.CreateCustomToolTip();
            else
                owner.NotifyToolTipElement.Content = e.NewValue;
        }

        owner.WriteToolTipSettings();
    }

    #endregion

    static NotifyIcon()
    {
        VisibilityProperty.OverrideMetadata(typeof(NotifyIcon), new PropertyMetadata(Visibility.Visible, VisibilityPropertyChanged));
        DataContextProperty.OverrideMetadata(typeof(NotifyIcon), new FrameworkPropertyMetadata(DataContextPropertyChanged));
        ContextMenuProperty.OverrideMetadata(typeof(NotifyIcon), new FrameworkPropertyMetadata(ContextMenuPropertyChanged));
    }

    public NotifyIcon()
    {
        _messageSink = new WindowMessageSink();

        _iconData = NotifyIconData.CreateDefault(_messageSink.MessageWindowHandle);

        _messageSink.MouseEventReceived += OnMouseEvent;
        _messageSink.TaskbarCreated += OnTaskbarCreated;
        _messageSink.ChangeToolTipStateRequest += OnToolTipChange;

        if (Application.Current != null)
            Application.Current.Exit += OnExit;
    }

    #region Methods

    private void CreateTaskbarIcon()
    {
        lock (this)
        {
            if (IsTaskbarIconCreated)
                return;

            //Initial configuration.
            var status = NotifyIconHelper.WriteIconData(ref _iconData, NotifyCommands.Add, IconDataMembers.Message | IconDataMembers.Icon | IconDataMembers.Tip);

            if (!status)
                return;

            _iconData.VersionOrTimeout = (uint)NotifyIconVersions.Vista;

            status = Shell32.Shell_NotifyIcon(NotifyCommands.SetVersion, ref _iconData);

            if (!status)
                return;

            IsTaskbarIconCreated = true;
        }
    }

    private void RemoveTaskbarIcon()
    {
        lock (this)
        {
            if (!IsTaskbarIconCreated)
                return;

            NotifyIconHelper.WriteIconData(ref _iconData, NotifyCommands.Delete, IconDataMembers.Message);
            IsTaskbarIconCreated = false;
        }
    }

    public PointW GetDeviceCoordinates(PointW point)
    {
        var dpi = VisualHelper.GetApplicationScale();

        return new PointW { X = (int)(point.X / dpi), Y = (int)(point.Y / dpi) };
    }

    private void ShowContextMenu(PointW cursorPosition)
    {
        if (IsDisposed)
            return;

        var args = new RoutedEventArgs { RoutedEvent = PreviewTrayContextMenuOpenEvent };
        RaiseEvent(args);
            
        if (args.Handled || ContextMenu == null)
            return;

        ContextMenu.Placement = PlacementMode.AbsolutePoint;
        ContextMenu.HorizontalOffset = cursorPosition.X;
        ContextMenu.VerticalOffset = cursorPosition.Y;
        ContextMenu.IsOpen = true;

        //Gets the handle from the context menu or from the message sink.
        var handle = ((HwndSource)PresentationSource.FromVisual(ContextMenu))?.Handle ?? _messageSink.MessageWindowHandle;

        //This makes sure that the context menu can close if lost focus.
        User32.SetForegroundWindow(handle);

        RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayContextMenuOpenEvent });
    }

    private void UpdateDataContext(FrameworkElement target, object oldDataContextValue, object newDataContextValue)
    {
        if (target == null || target.IsDataContextDataBound())
            return;

        //if the target's data context is the NotifyIcon's old DataContext or the NotifyIcon itself, update it.
        if (ReferenceEquals(this, target.DataContext) || Equals(oldDataContextValue, target.DataContext))
            target.DataContext = newDataContextValue ?? this;
    }

    private void CreateCustomToolTip()
    {
        var tt = NotifyToolTip as ToolTip;

        if (tt == null && NotifyToolTip != null)
        {
            tt = new ToolTip
            {
                Placement = PlacementMode.Mouse,
                HasDropShadow = false,
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                StaysOpen = true,
                Content = NotifyToolTip
            };
        }
        else if (tt == null && !string.IsNullOrEmpty(NotifyToolTipText))
        {
            tt = new ToolTip { Content = NotifyToolTipText };
        }

        if (tt != null)
            UpdateDataContext(tt, null, DataContext);

        //Store a reference to the used tooltip.
        SetValue(NotifyToolTipElementPropertyKey, tt);
    }

    private void WriteToolTipSettings()
    {
        _iconData.ToolTipText = NotifyToolTipText;

        //To get ToolTip events from the taskbar, set a dummy text.            
        if (string.IsNullOrEmpty(_iconData.ToolTipText) && NotifyToolTipElement != null)
            _iconData.ToolTipText = nameof(ToolTip);

        NotifyIconHelper.WriteIconData(ref _iconData, NotifyCommands.Modify, IconDataMembers.Tip);
    }

    public void RefreshVisual()
    {
        if (ContextMenu == null)
            return;

        //For some reason, the context menu of the systray icon is not updating its style.
        NotifyToolTipElement.Background = ContextMenu.Background = TryFindResource("Element.Background") as SolidColorBrush;
            
        foreach (var menuItem in ContextMenu.Items.OfType<ExMenuItem>())
        {
            menuItem.Foreground = TryFindResource("Element.Foreground.Medium") as SolidColorBrush;

            if (menuItem.Name == "ExitButton")
                menuItem.Icon = TryFindResource("Vector.Close") as System.Windows.Media.Brush;
        }
            
        if (NotifyToolTipElement is ToolTip tooltip)
        {
            tooltip.SetValue(TextBlock.ForegroundProperty, TryFindResource("Element.Foreground.Medium") as SolidColorBrush);
            tooltip.InvalidateVisual();
        }
    }

    #endregion

    #region Events

    protected override void OnInitialized(EventArgs e)
    {
        if (Visibility == Visibility.Visible)
            CreateTaskbarIcon();

        base.OnInitialized(e);
    }

    private void OnMouseEvent(MouseEventType type)
    {
        if (IsDisposed)
            return;

        switch (type)
        {
            case MouseEventType.MouseMove:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayMouseMoveEvent });
                return;
            case MouseEventType.IconLeftMouseDown:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayLeftMouseDownEvent });
                break;
            case MouseEventType.IconRightMouseDown:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayRightMouseDownEvent });
                break;
            case MouseEventType.IconMiddleMouseDown:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayMiddleMouseDownEvent });
                break;
            case MouseEventType.IconLeftMouseUp:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayLeftMouseUpEvent });
                LeftClickCommand?.Execute(this);
                break;
            case MouseEventType.IconRightMouseUp:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayRightMouseUpEvent });
                break;
            case MouseEventType.IconMiddleMouseUp:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayMiddleMouseUpEvent });
                MiddleClickCommand?.Execute(this);
                break;
            case MouseEventType.IconLeftDoubleClick:
                RaiseEvent(new RoutedEventArgs { RoutedEvent = TrayMouseDoubleClickEvent });
                DoubleLeftClickCommand?.Execute(this);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Missing handler for mouse event flag: " + type);
        }

        var cursorPosition = new PointW();
        User32.GetPhysicalCursorPos(ref cursorPosition);

        cursorPosition = GetDeviceCoordinates(cursorPosition);

        if (type == MouseEventType.IconRightMouseUp)
            ShowContextMenu(cursorPosition);
    }

    private void OnTaskbarCreated()
    {
        IsTaskbarIconCreated = false;
        CreateTaskbarIcon();
    }

    private void OnToolTipChange(bool visible)
    {
        if (NotifyToolTipElement == null) 
            return;

        if (visible)
        {
            if (IsPopupOpen)
                return;

            var args = new RoutedEventArgs { RoutedEvent = PreviewToolTipOpenEvent };
            RaiseEvent(args);

            if (args.Handled)
                return;

            //TODO: test this.
            NotifyToolTipElement.IsOpen = true;

            NotifyToolTip?.RaiseEvent(new RoutedEventArgs { RoutedEvent = ToolTipOpenEvent });
            RaiseEvent(new RoutedEventArgs { RoutedEvent = ToolTipOpenEvent });
        }
        else
        {
            var args = new RoutedEventArgs { RoutedEvent = PreviewToolTipCloseEvent };
            RaiseEvent(args);

            if (args.Handled) 
                return;

            NotifyToolTip?.RaiseEvent(new RoutedEventArgs { RoutedEvent = ToolTipCloseEvent });

            NotifyToolTipElement.IsOpen = false;

            RaiseEvent(new RoutedEventArgs { RoutedEvent = ToolTipCloseEvent });
        }
    }

    private void OnExit(object sender, EventArgs e)
    {
        Dispose();
    }

    #endregion

    #region Disposing

    public void Dispose()
    {
        Dispose(true);

        //Avoid disposing twice.
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (IsDisposed || !disposing)
            return;

        lock (this)
        {
            IsDisposed = true;

            if (Application.Current != null)
                Application.Current.Exit -= OnExit;

            _messageSink.Dispose();

            RemoveTaskbarIcon();
        }
    }

    #endregion
}
using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

[TemplatePart(Name = SuppressButtonId, Type = typeof(Button))]
public class InfoBar : ContentControl
{
    private const string SuppressButtonId = "SuppressButton";

    private Button _suppressButton;

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(StatusTypes), typeof(InfoBar), new PropertyMetadata(default(StatusTypes)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(InfoBar), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(InfoBar), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty IsLongProperty = DependencyProperty.Register(nameof(IsLong), typeof(bool), typeof(InfoBar), new PropertyMetadata(false));
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(InfoBar), new PropertyMetadata(true));
    public static readonly DependencyProperty IsIconVisibleProperty = DependencyProperty.Register(nameof(IsIconVisible), typeof(bool), typeof(InfoBar), new PropertyMetadata(true));

    public static readonly RoutedEvent DismissedEvent = EventManager.RegisterRoutedEvent(nameof(Dismissed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InfoBar));

    public StatusTypes Type
    {
        get => (StatusTypes)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public bool IsLong
    {
        get => (bool)GetValue(IsLongProperty);
        set => SetValue(IsLongProperty, value);
    }

    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public bool IsIconVisible
    {
        get => (bool)GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
    }

    /// <summary>
    /// Event raised when the InfoBar gets dismissed/suppressed.
    /// </summary>
    public event RoutedEventHandler Dismissed
    {
        add => AddHandler(DismissedEvent, value);
        remove => RemoveHandler(DismissedEvent, value);
    }

    static InfoBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(InfoBar), new FrameworkPropertyMetadata(typeof(InfoBar)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _suppressButton = GetTemplateChild(SuppressButtonId) as Button;

        if (_suppressButton != null)
            _suppressButton.Click += (_, _) => Hide();
    }

    public void Show(StatusTypes type, string title, string description, bool isLong)
    {
        //Collapsed-by-default elements do not apply templates.
        //http://stackoverflow.com/a/2115873/1735672
        //So it's necessary to do this here.
        ApplyTemplate();
        
        Type = type;
        Title = title;
        Description = description;
        IsLong = isLong;
        Visibility = Visibility.Visible;
    }

    public void Update(string title, string description, bool isLong = false)
    {
        Show(StatusTypes.Update, title, description, isLong);
    }

    public void Info(string title, string description, bool isLong = false)
    {
        Show(StatusTypes.Info, title, description, isLong);
    }

    public void Warning(string title, string description, bool isLong = false)
    {
        Show(StatusTypes.Warning, title, description, isLong);
    }

    public void Error(string title, string description, bool isLong = false)
    {
        Show(StatusTypes.Error, title, description, isLong);
    }

    public void Hide()
    {
        if (Visibility == Visibility.Collapsed)
            return;

        Visibility = Visibility.Collapsed;

        RaiseDismissedEvent();
    }

    private void RaiseDismissedEvent()
    {
        if (DismissedEvent == null || !IsLoaded)
            return;

        var newEventArgs = new RoutedEventArgs(DismissedEvent);
        RaiseEvent(newEventArgs);
    }
}
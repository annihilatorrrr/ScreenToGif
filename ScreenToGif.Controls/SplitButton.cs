using ScreenToGif.Domain.Enums;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ScreenToGif.Controls;

public class SplitButton : ItemsControl
{
    private const string ActionButtonId = "ActionButton";
    private const string PopupId = "Popup";
    private ExButton _internalButton;
    private Popup _mainPopup;

    #region Dependency Properties

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(object), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(FluentSymbols), typeof(SplitButton), new PropertyMetadata(FluentSymbols.None));

    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(SplitButton), new FrameworkPropertyMetadata(-1,
        FrameworkPropertyMetadataOptions.AffectsRender, SelectedIndex_Changed));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SplitButton), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SplitButton), new FrameworkPropertyMetadata(null));

    public static readonly RoutedEvent SelectedIndexChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectedIndexChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));

    #endregion

    #region Properties

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public FluentSymbols Icon
    {
        get => (FluentSymbols)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The index of selected item.
    /// </summary>
    [Description("The index of selected item."), Category("Common")]
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetCurrentValue(SelectedIndexProperty, value);
    }

    /// <summary>
    /// Gets or sets the command associated with the menu item.
    /// </summary>
    [Category("Action")]
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="Command"/> property.
    /// </summary>
    [Category("Action")]
    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public event RoutedEventHandler SelectedIndexChanged
    {
        add => AddHandler(SelectedIndexChangedEvent, value);
        remove => RemoveHandler(SelectedIndexChangedEvent, value);
    }

    #endregion

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _internalButton = GetTemplateChild(ActionButtonId) as ExButton;
        _mainPopup = GetTemplateChild(PopupId) as Popup;
        
        //Raises the click event.
        //_internalButton.Click += (sender, args) => _current?.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        foreach (var item in Items.OfType<ExMenuItem>().ToList())
            item.Click += (sender, _) =>
            {
                _mainPopup.IsOpen = false;

                if (sender is not ExMenuItem menu)
                    return;

                var index = Items.OfType<ExMenuItem>().Where(w => w.IsTabStop).ToList().IndexOf(menu);

                if (index > -1)
                    SelectedIndex = index;
            };
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        SelectItem(this);
    }

    private static void SelectedIndex_Changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        if (o is not SplitButton { IsLoaded: true } split)
            return;

        split.SelectItem(split);
    }

    private void SelectItem(SplitButton split)
    {
        if (split.SelectedIndex < 0)
            return;

        var list = split.Items.OfType<ExMenuItem>().Where(w => w.IsTabStop).ToList();

        if (split.SelectedIndex > list.Count - 1)
            split.SelectedIndex = list.Count - 1;
        
        split.Header = list[split.SelectedIndex].Header;
        split.Icon = list[split.SelectedIndex].Icon;

        for (var i = 0; i < list.Count; i++)
            list[i].IsChecked = i == split.SelectedIndex;

        RaiseSelectedIndexChanged();
    }

    private void RaiseSelectedIndexChanged()
    {
        if (SelectedIndexChangedEvent == null || !IsLoaded)
            return;

        RaiseEvent(new RoutedEventArgs(SelectedIndexChangedEvent));
    }
}
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenToGif.Controls;

public class IntegerBox : ExTextBox
{
    private static bool _ignore;

    #region Dependency Property

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(int.MaxValue, OnMaximumPropertyChanged));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(0, OnValuePropertyChanged));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(0, OnMinimumPropertyChanged));

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(StepValue), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(1));

    public static readonly DependencyProperty UpdateOnInputProperty = DependencyProperty.Register(nameof(UpdateOnInput), typeof(bool), typeof(IntegerBox),
        new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty DefaultValueIfEmptyProperty = DependencyProperty.Register(nameof(DefaultValueIfEmpty), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty EmptyIfValueEmptyProperty = DependencyProperty.Register(nameof(EmptyIfValue), typeof(int), typeof(IntegerBox),
        new FrameworkPropertyMetadata(int.MinValue));

    public static readonly DependencyProperty PropagateWheelEventProperty = DependencyProperty.Register(nameof(PropagateWheelEvent), typeof(bool), typeof(IntegerBox), new PropertyMetadata(default(bool)));

    /// <summary>
    /// Create a custom routed event by first registering a RoutedEventID, this event uses the bubbling routing strategy.
    /// </summary>
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(IntegerBox));
    
    #endregion

    #region Property Accessor

    [Bindable(true), Category("Common")]
    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    [Bindable(true), Category("Common")]
    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    [Bindable(true), Category("Common")]
    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// The Increment/Decrement value.
    /// </summary>
    [Description("The Increment/Decrement value.")]
    public int StepValue
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    [Bindable(true), Category("Common")]
    public bool UpdateOnInput
    {
        get => (bool)GetValue(UpdateOnInputProperty);
        set => SetValue(UpdateOnInputProperty, value);
    }

    [Bindable(true), Category("Common")]
    public int DefaultValueIfEmpty
    {
        get => (int)GetValue(DefaultValueIfEmptyProperty);
        set => SetValue(DefaultValueIfEmptyProperty, value);
    }

    [Bindable(true), Category("Common")]
    public int EmptyIfValue
    {
        get => (int)GetValue(EmptyIfValueEmptyProperty);
        set => SetValue(EmptyIfValueEmptyProperty, value);
    }

    /// <summary>
    /// True if the wheel events should not be set as handled.
    /// </summary>
    [Bindable(true), Category("Behavior")]
    public bool PropagateWheelEvent
    {
        get => (bool)GetValue(PropagateWheelEventProperty);
        set => SetValue(PropagateWheelEventProperty, value);
    }

    /// <summary>
    /// Event raised when the numeric value is changed.
    /// </summary>
    public event RoutedEventHandler ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    #endregion

    #region Properties Changed

    private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var intBox = d as IntegerBox;

        if (intBox?.Value > intBox?.Maximum)
            intBox.Value = intBox.Maximum;
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not IntegerBox box || _ignore)
            return;

        _ignore = true;

        if (box.Value > box.Maximum)
        {
            //box.UseTemporary = false;
            //box.Temporary = (box.Maximum / box.Scale);
            box.Value = box.Maximum;
        }

        if (box.Value < box.Minimum)
        {
            //box.UseTemporary = false;
            //box.Temporary = (box.Minimum / box.Scale);
            box.Value = box.Minimum;
        }

        _ignore = false;

        var stringValue = box.Value == box.EmptyIfValue ? "" : box.Value.ToString();

        if (!string.Equals(box.Text, stringValue))
            box.Text = stringValue;

        box.RaiseValueChangedEvent();
    }

    private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var intBox = d as IntegerBox;

        if (intBox?.Value < intBox?.Minimum)
            intBox.Value = intBox.Minimum;
    }
    
    #endregion

    static IntegerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerBox), new FrameworkPropertyMetadata(typeof(IntegerBox)));
    }

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Text = Value == EmptyIfValue ? "" : Value.ToString();
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.Source is IntegerBox)
            SelectAll();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        //Only sets the focus if not clicking on the Up/Down buttons of a IntegerUpDown.
        if (e.OriginalSource is TextBlock or Border)
            return;

        if (!IsKeyboardFocusWithin)
        {
            e.Handled = true;
            Focus();
        }
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            e.Handled = true;
            return;
        }

        if (!IsEntryAllowed(e.Text))
        {
            e.Handled = true;
            return;
        }

        base.OnPreviewTextInput(e);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        if (!UpdateOnInput || string.IsNullOrEmpty(Text) || !IsTextAllowed(Text))
            return;

        Value = Convert.ToInt32(Text, CultureInfo.CurrentUICulture);

        base.OnTextChanged(e);
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);

        if (!UpdateOnInput)
        {
            if (string.IsNullOrEmpty(Text) || !IsTextAllowed(Text))
            {
                Value = DefaultValueIfEmpty;
                return;
            }

            Value = Convert.ToInt32(Text, CultureInfo.CurrentUICulture);
            return;
        }

        //The offset value dictates the value being displayed.
        //For example, The value 600 and the Offset 20 should display the text 580.
        //Text = Value - Offset.

        Text = Value == EmptyIfValue ? "" : Value.ToString();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            e.Handled = true;
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (!IsKeyboardFocusWithin)
            return;

        var step = Keyboard.Modifiers switch
        {
            ModifierKeys.Shift | ModifierKeys.Control => 50,
            ModifierKeys.Shift => 10,
            ModifierKeys.Control => 5,
            _ => StepValue
        };

        Value = e.Delta > 0 ?
            Math.Min(Maximum, Value + step) :
            Math.Max(Minimum, Value - step);

        e.Handled = !PropagateWheelEvent;
    }

    #endregion

    private void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            var text = e.DataObject.GetData(typeof(string)) as string;

            if (!IsTextAllowed(text))
                e.CancelCommand();
        }
        else
        {
            e.CancelCommand();
        }
    }

    private bool IsEntryAllowed(string text)
    {
        //Only numbers.
        var regex = new Regex(@"^-|[0-9]$");

        //Checks if it's a valid char based on the context.
        return regex.IsMatch(text);
    }

    private bool IsTextAllowed(string text)
    {
        return Minimum < 0 ? Regex.IsMatch(text, @"^[-]?(?:[0-9]{1,9})?$") : Regex.IsMatch(text, @"^(?:[0-9]{1,9})?$");
    }

    public void RaiseValueChangedEvent()
    {
        if (ValueChangedEvent == null || !IsLoaded)
            return;

        var newEventArgs = new RoutedEventArgs(ValueChangedEvent);
        RaiseEvent(newEventArgs);
    }
}
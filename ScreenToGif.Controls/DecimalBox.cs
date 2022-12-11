using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScreenToGif.Controls;

public class DecimalBox : ExTextBox
{
    #region Variables

    private bool _ignore;
    private readonly string _baseFormat = "{0:###,###,###,###,##0.";
    private string _format = "{0:###,###,###,###,##0.00}";

    #endregion

    #region Dependency Property

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(decimal.MaxValue, OnMaximumPropertyChanged));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(0M, OnValuePropertyChanged));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(0M, OnMinimumPropertyChanged));

    public static readonly DependencyProperty DecimalsProperty = DependencyProperty.Register(nameof(Decimals), typeof(int), typeof(DecimalBox), new FrameworkPropertyMetadata(2, OnDecimalsPropertyChanged));

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(nameof(StepValue), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(1M));

    public static readonly DependencyProperty UpdateOnInputProperty = DependencyProperty.Register(nameof(UpdateOnInput), typeof(bool), typeof(DecimalBox), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty DefaultValueIfEmptyProperty = DependencyProperty.Register(nameof(DefaultValueIfEmpty), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(0M));

    public static readonly DependencyProperty EmptyIfValueProperty = DependencyProperty.Register(nameof(EmptyIfValue), typeof(decimal), typeof(DecimalBox), new FrameworkPropertyMetadata(decimal.MinValue));

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DecimalBox));

    #endregion

    #region Properties

    [Bindable(true), Category("Common")]
    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    [Bindable(true), Category("Common")]
    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    [Bindable(true), Category("Common")]
    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    [Bindable(true), Category("Common")]
    public int Decimals
    {
        get => (int)GetValue(DecimalsProperty);
        set => SetValue(DecimalsProperty, value);
    }

    /// <summary>
    /// The Increment/Decrement value.
    /// </summary>
    [Description("The Increment/Decrement value.")]
    public decimal StepValue
    {
        get => (decimal)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    [Bindable(true), Category("Common")]
    public bool UpdateOnInput
    {
        get => (bool)GetValue(UpdateOnInputProperty);
        set => SetValue(UpdateOnInputProperty, value);
    }

    [Bindable(true), Category("Common")]
    public decimal DefaultValueIfEmpty
    {
        get => (decimal)GetValue(DefaultValueIfEmptyProperty);
        set => SetValue(DefaultValueIfEmptyProperty, value);
    }
    
    [Bindable(true), Category("Common")]
    public decimal EmptyIfValue
    {
        get => (decimal)GetValue(EmptyIfValueProperty);
        set => SetValue(EmptyIfValueProperty, value);
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
        if (d is not DecimalBox decimalBox)
            return;

        if (decimalBox.Value > decimalBox.Maximum)
            decimalBox.Value = decimalBox.Maximum;
    }

    private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DecimalBox decimalBox)
            return;

        if (decimalBox.Value > decimalBox.Maximum)
            decimalBox.Value = decimalBox.Maximum;

        else if (decimalBox.Value < decimalBox.Minimum)
            decimalBox.Value = decimalBox.Minimum;

        decimalBox.Value = Math.Round(decimalBox.Value, decimalBox.Decimals);

        if (!decimalBox._ignore)
        {
            var value = string.Format(CultureInfo.CurrentCulture, decimalBox._format, decimalBox.Value);

            if (!string.Equals(decimalBox.Text, value))
                decimalBox.Text = (decimalBox.EmptyIfValue == decimalBox.Value ? "" : value);
        }

        decimalBox.RaiseValueChangedEvent();
    }

    private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DecimalBox decimalBox)
            return;

        if (decimalBox.Value < decimalBox.Minimum)
            decimalBox.Value = decimalBox.Minimum;
    }

    private static void OnDecimalsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DecimalBox decimalBox)
            return;

        decimalBox._format = decimalBox._baseFormat + "".PadRight(decimalBox.Decimals, '0') + "}";

        decimalBox.Value = Math.Round(decimalBox.Value, decimalBox.Decimals);
    }

    #endregion

    static DecimalBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalBox), new FrameworkPropertyMetadata(typeof(DecimalBox)));
    }

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        AddHandler(DataObject.PastingEvent, new DataObjectPastingEventHandler(OnPasting));

        _format = _baseFormat + "".PadRight(Decimals, '0') + "}";
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        Text = Value == EmptyIfValue ? "" : string.Format(CultureInfo.CurrentCulture, _format, Value);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        //Only sets the focus if not clicking on the Up/Down buttons of a IntegerUpDown.
        if (e.OriginalSource is TextBlock or Border)
            return;

        if (IsKeyboardFocusWithin)
            return;

        e.Handled = true;
        Focus();
    }

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        if (e.Source is DecimalBox)
            SelectAll();
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text))
        {
            e.Handled = true;
            return;
        }

        if (!IsEntryAllowed(this, e.Text))
        {
            e.Handled = true;
            return;
        }

        base.OnPreviewTextInput(e);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        if (!UpdateOnInput || _ignore)
            return;

        if (string.IsNullOrEmpty(Text) || !IsTextAllowed(Text))
            return;
        
        _ignore = true;

        Value = Math.Round(Convert.ToDecimal(Text, CultureInfo.CurrentCulture), Decimals);

        _ignore = false;

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

            _ignore = true;

            Value = Convert.ToDecimal(Text, CultureInfo.CurrentCulture);
            Text = EmptyIfValue == Value ? "" : string.Format(CultureInfo.CurrentCulture, _format, Value);

            _ignore = false;
            return;
        }

        Text = Value == EmptyIfValue ? "" : string.Format(CultureInfo.CurrentCulture, _format, Value);
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
            (ModifierKeys.Shift | ModifierKeys.Control) => 50,
            ModifierKeys.Shift => 10,
            ModifierKeys.Control => 5,
            _ => StepValue
        };

        if (e.Delta > 0)
            Value += step;
        else
            Value -= step;

        e.Handled = true;
    }

    #endregion

    #region Methods

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

    private bool IsEntryAllowed(TextBox textBox, string text)
    {
        //Digits, points or commas.
        var regex = new Regex(@"^[0-9]|\.|\,$"); //TODO: Support for multiple cultures.

        //Checks if it's a valid char based on the context.
        return regex.IsMatch(text) && IsEntryAllowedInContext(textBox, text);
    }

    private bool IsEntryAllowedInContext(TextBox textBox, string next)
    {
        //if number, allow.
        if (char.IsNumber(next.ToCharArray().FirstOrDefault()))
            return true;

        #region Thousands

        var thousands = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        var thousandsChar = thousands.ToCharArray().FirstOrDefault();
        var decimals = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var decimalsChar = decimals.ToCharArray().FirstOrDefault();

        if (next.Equals(thousands))
        {
            var textAux = textBox.Text;

            if (!string.IsNullOrEmpty(textBox.SelectedText))
                textAux = textAux.Replace(textBox.SelectedText, "");

            var before = textAux.Substring(0, textBox.SelectionStart);
            var after = textAux.Substring(textBox.SelectionStart);

            //If there's no text, is not allowed to add a thousand separator.
            if (string.IsNullOrEmpty(after + before))
                return false;

            //Before the carret.
            if (!string.IsNullOrEmpty(before))
            {
                //You can't add a thousand separator after the decimal.
                if (before.Contains(decimals))
                    return false;

                //Check the previous usage of a thousand separator.
                if (before.Contains(thousands))
                {
                    var split = before.Split(thousandsChar);

                    //You can't add a thousand separators closer than 3 chars from each other.
                    if (split.Last().Length != 3)
                        return false;
                }
            }

            //After the carret.
            if (!string.IsNullOrEmpty(after))
            {
                var split = after.Split(thousandsChar, decimalsChar);

                //You can't add a thousand separators closer than 3 chars from another separator, decimal or thousands.
                if (split.First().Length != 3)
                    return true;
            }

            return false;
        }

        #endregion

        #region Decimal

        if (next.Equals(decimals))
            return !textBox.Text.Any(x => x.Equals(decimalsChar));

        #endregion

        return true;
    }

    private bool IsTextAllowed(string text)
    {
        return decimal.TryParse(text, out var result);

        //var regex = new Regex(@"^((\d+)|(\d{1,3}(\.\d{3})+)|(\d{1,3}(\.\d{3})(\,\d{3})+))((\,\d{4})|(\,\d{3})|(\,\d{2})|(\,\d{1})|(\,))?$", RegexOptions.CultureInvariant);
        //return regex.IsMatch(text);
    }

    public void RaiseValueChangedEvent()
    {
        if (ValueChangedEvent == null)
            return;

        RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
    }

    #endregion
}

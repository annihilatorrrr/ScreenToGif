using System.Windows.Controls.Primitives;
using System.Windows;

namespace ScreenToGif.Controls;

public class NullableIntegerStepper : NullableIntegerBox
{
    private const string UpButtonKey = "UpButton";
    private const string DownButtonKey = "DownButton";

    private RepeatButton _upButton;
    private RepeatButton _downButton;

    static NullableIntegerStepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableIntegerStepper), new FrameworkPropertyMetadata(typeof(NullableIntegerStepper)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _upButton = GetTemplateChild(UpButtonKey) as RepeatButton;
        _downButton = GetTemplateChild(DownButtonKey) as RepeatButton;

        if (_upButton != null)
            _upButton.Click += UpButton_Click;

        if (_downButton != null)
            _downButton.Click += DownButton_Click;
    }

    #region Event Handlers

    private void DownButton_Click(object sender, RoutedEventArgs e)
    {
        if (Value > Minimum)
            Value -= StepValue;
    }

    private void UpButton_Click(object sender, RoutedEventArgs e)
    {
        if (Value < Maximum)
            Value += StepValue;
    }

    #endregion
}
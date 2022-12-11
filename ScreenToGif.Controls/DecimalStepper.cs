using System.Windows.Controls.Primitives;
using System.Windows;

namespace ScreenToGif.Controls;

[TemplatePart(Name = UpButtonKey, Type = typeof(RepeatButton))]
[TemplatePart(Name = DownButtonKey, Type = typeof(RepeatButton))]
public class DecimalStepper : DecimalBox
{
    private const string UpButtonKey = "UpButton";
    private const string DownButtonKey = "DownButton";

    private RepeatButton _upButton;
    private RepeatButton _downButton;
    
    static DecimalStepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DecimalStepper), new FrameworkPropertyMetadata(typeof(DecimalStepper)));
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
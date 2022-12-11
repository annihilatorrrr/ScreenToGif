using ScreenToGif.Util.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenToGif.Controls;

//Bug: If the user drags quickly the Thumb and release afterwards, the OnAfterSelection event is not triggered.

[TemplatePart(Name = SpectrumBorderId, Type = typeof(Border))]
[TemplatePart(Name = ColorThumbId, Type = typeof(ColorThumb))]
public class SpectrumSlider : Slider
{
    private const string SpectrumBorderId = "SpectrumBorder";
    private const string ColorThumbId = "ColorThumb";

    private ColorThumb _colorThumb;
    private Border _spectrumRectangle;
    private LinearGradientBrush _pickerBrush;

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(SpectrumSlider), new PropertyMetadata(Colors.Transparent));
    public static readonly DependencyProperty IsAlphaSpectrumProperty = DependencyProperty.Register(nameof(IsAlphaSpectrum), typeof(bool), typeof(SpectrumSlider), new PropertyMetadata(false));
    public static readonly DependencyProperty SpectrumColorProperty = DependencyProperty.Register(nameof(SpectrumColor), typeof(Color), typeof(SpectrumSlider), new PropertyMetadata(default(Color), SpectrumColor_ChangedCallback));
    public static readonly RoutedEvent ColorSelectedEvent = EventManager.RegisterRoutedEvent(nameof(ColorSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SpectrumSlider));
    
    /// <summary>
    /// Current selected Color.
    /// </summary>
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    /// <summary>
    /// True if the spectrum will display the same color but under different alpha values.
    /// </summary>
    public bool IsAlphaSpectrum
    {
        get => (bool)GetValue(IsAlphaSpectrumProperty);
        set => SetValue(IsAlphaSpectrumProperty, value);
    }

    /// <summary>
    /// The color used by the alpha sectrum.
    /// </summary>
    public Color SpectrumColor
    {
        get => (Color)GetValue(SpectrumColorProperty);
        set => SetValue(SpectrumColorProperty, value);
    }

    /// <summary>
    /// Event raised when the numeric value is changed.
    /// </summary>
    public event RoutedEventHandler ColorSelected
    {
        add => AddHandler(ColorSelectedEvent, value);
        remove => RemoveHandler(ColorSelectedEvent, value);
    }

    static SpectrumSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumSlider), new FrameworkPropertyMetadata(typeof(SpectrumSlider)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _spectrumRectangle = GetTemplateChild(SpectrumBorderId) as Border;
        _colorThumb = GetTemplateChild(ColorThumbId) as ColorThumb;

        if (_colorThumb != null)
        {
            _colorThumb.PreviewMouseLeftButtonUp += ColorThumb_MouseLeftButtonUp;
            _colorThumb.MouseEnter += ColorThumb_MouseEnter;
        }

        UpdateColorSpectrum();

        OnValueChanged(double.NaN, Value);
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        SetValue(SelectedColorProperty, ColorExtensions.HsvToRgb(360 - newValue, 1, 1));
    }

    #region Events

    private void ColorThumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        RaiseColorSelectedEvent();
    }

    private void ColorThumb_MouseEnter(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && e.MouseDevice.Captured == null)
        {
            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/5fa7cbc2-c99f-4b71-b46c-f156bdf0a75a/making-the-slider-slide-with-one-click-anywhere-on-the-slider?forum=wpf
            //The left button is pressed on mouse enter, but the mouse isn't captured, so the thumb
            //must have been moved under the mouse in response to a click on the track thanks to IsMoveToPointEnabled.

            //Generate a MouseLeftButtonDown event.
            _colorThumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = MouseLeftButtonDownEvent
            });
        }
    }

    private static void SpectrumColor_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = d as SpectrumSlider;
        box?.UpdateColorSpectrum();
    }

    #endregion

    private void UpdateColorSpectrum()
    {
        if (_spectrumRectangle == null)
            return;

        _pickerBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1),
            ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation
        };

        var colorsList = IsAlphaSpectrum ? ColorExtensions.GenerateAlphaSpectrum(SpectrumColor) : ColorExtensions.GenerateHsvSpectrum(40);
        var stopIncrement = 1d / colorsList.Count;
        var isDecimal = stopIncrement % 1 > 0;

        for (var i = 0; i < (isDecimal ? colorsList.Count - 1 : colorsList.Count); i++)
            _pickerBrush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));

        if (isDecimal)
            _pickerBrush.GradientStops.Add(new GradientStop(colorsList[colorsList.Count - 1], 1d));

        _spectrumRectangle.Background = _pickerBrush;
    }

    public void RaiseColorSelectedEvent()
    {
        if (ColorSelectedEvent == null || !IsLoaded)
            return;

        var newEventArgs = new RoutedEventArgs(ColorSelectedEvent);
        RaiseEvent(newEventArgs);
    }
}
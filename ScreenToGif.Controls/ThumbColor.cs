using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ScreenToGif.Controls;

/// <summary>
/// The Thumb of the Spectrum Slider.
/// </summary>
public class ColorThumb : Thumb
{
    static ColorThumb()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorThumb), new FrameworkPropertyMetadata(typeof(ColorThumb)));
    }

    public static readonly DependencyProperty ThumbColorProperty = DependencyProperty.Register(nameof(ThumbColor), typeof(Color), typeof(ColorThumb), new FrameworkPropertyMetadata(Colors.Transparent));
    public static readonly DependencyProperty PointerOutlineThicknessProperty = DependencyProperty.Register(nameof(PointerOutlineThickness), typeof(double), typeof(ColorThumb), new FrameworkPropertyMetadata(1.0));
    public static readonly DependencyProperty PointerOutlineBrushProperty = DependencyProperty.Register(nameof(PointerOutlineBrush), typeof(Brush), typeof(ColorThumb), new FrameworkPropertyMetadata(null));

    /// <summary>
    /// The color of the Thumb.
    /// </summary>
    public Color ThumbColor
    {
        get => (Color)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    public double PointerOutlineThickness
    {
        get => (double)GetValue(PointerOutlineThicknessProperty);
        set => SetValue(PointerOutlineThicknessProperty, value);
    }

    public Brush PointerOutlineBrush
    {
        get => (Brush)GetValue(PointerOutlineBrushProperty);
        set => SetValue(PointerOutlineBrushProperty, value);
    }
}

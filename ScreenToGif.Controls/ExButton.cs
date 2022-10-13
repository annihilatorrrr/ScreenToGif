using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class ExButton : Button
{
    #region Variables

    public static readonly DependencyProperty IsAccentedProperty = DependencyProperty.Register(nameof(IsAccented), typeof(bool), typeof(ExButton), new PropertyMetadata(false));

    public static readonly DependencyProperty KeyGestureProperty = DependencyProperty.Register(nameof(KeyGesture), typeof(string), typeof(ExButton), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(ExButton), new FrameworkPropertyMetadata(TextWrapping.WrapWithOverflow,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    #region Properties

    /// <summary>
    /// True if the Button has accent color.
    /// </summary>
    [Description("True if the Button has accent color."), Category("Common")]
    public bool IsAccented
    {
        get => (bool)GetValue(IsAccentedProperty);
        set => SetCurrentValue(IsAccentedProperty, value);
    }
    
    /// <summary>
    /// The KeyGesture of the button.
    /// </summary>
    [Description("The KeyGesture of the button."), Category("Common")]
    public string KeyGesture
    {
        get => (string)GetValue(KeyGestureProperty);
        set => SetCurrentValue(KeyGestureProperty, value);
    }

    /// <summary>
    /// The TextWrapping property controls whether or not text wraps 
    /// when it reaches the flow edge of its containing block box. 
    /// </summary>
    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    #endregion

    static ExButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExButton), new FrameworkPropertyMetadata(typeof(ExButton)));
    }
}
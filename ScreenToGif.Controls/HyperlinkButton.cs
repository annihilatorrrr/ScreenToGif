using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class HyperlinkButton : Button
{
    #region Variables

    public static readonly DependencyProperty KeyGestureProperty = DependencyProperty.Register(nameof(KeyGesture), typeof(string), typeof(HyperlinkButton), new FrameworkPropertyMetadata(null));

    #endregion

    #region Properties

    /// <summary>
    /// The KeyGesture of the button.
    /// </summary>
    [Description("The KeyGesture of the button."), Category("Common")]
    public string KeyGesture
    {
        get => (string)GetValue(KeyGestureProperty);
        set => SetCurrentValue(KeyGestureProperty, value);
    }

    #endregion

    static HyperlinkButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperlinkButton), new FrameworkPropertyMetadata(typeof(HyperlinkButton)));
    }
}
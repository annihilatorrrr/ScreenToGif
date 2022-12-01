using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class ToggleSwitch : CheckBox
{
    public static readonly DependencyProperty OnContentProperty = DependencyProperty.Register(nameof(OnContent), typeof(string), typeof(ToggleSwitch), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty OffContentProperty = DependencyProperty.Register(nameof(OffContent), typeof(string), typeof(ToggleSwitch), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty IsContentBeforeProperty = DependencyProperty.Register(nameof(IsContentBefore), typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(default(bool)));

    public string OnContent
    {
        get => (string)GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }

    public string OffContent
    {
        get => (string)GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }

    public bool IsContentBefore
    {
        get => (bool)GetValue(IsContentBeforeProperty);
        set => SetValue(IsContentBeforeProperty, value);
    }
    
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }
}
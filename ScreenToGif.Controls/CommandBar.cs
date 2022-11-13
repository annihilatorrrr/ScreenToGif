using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class CommandBar : ItemsControl
{
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(CommandBar), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty SecondaryCommandsProperty = DependencyProperty.Register(nameof(SecondaryCommands), typeof(ObservableCollection<UIElement>), typeof(CommandBar), new PropertyMetadata(new ObservableCollection<UIElement>()));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public ObservableCollection<UIElement> SecondaryCommands
    {
        get => (ObservableCollection<UIElement>)GetValue(SecondaryCommandsProperty);
        set => SetValue(SecondaryCommandsProperty, value);
    }

    static CommandBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBar), new FrameworkPropertyMetadata(typeof(CommandBar)));
    }
}
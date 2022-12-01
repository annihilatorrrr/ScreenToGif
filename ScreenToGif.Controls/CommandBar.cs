using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class CommandBar : ItemsControl
{
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(CommandBar), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty SecondaryCommandsSourceProperty = DependencyProperty.Register(nameof(SecondaryCommandsSource), typeof(ICollection<UIElement>), typeof(CommandBar), new PropertyMetadata(default(ICollection<UIElement>)));

    public ICollection<UIElement> SecondaryCommandsSource
    {
        get => (ICollection<UIElement>)GetValue(SecondaryCommandsSourceProperty);
        set => SetValue(SecondaryCommandsSourceProperty, value);
    }

    public static readonly DependencyProperty HasSecondaryCommandsProperty = DependencyProperty.Register(nameof(HasSecondaryCommands), typeof(bool), typeof(CommandBar), new PropertyMetadata(default(bool)));

    public bool HasSecondaryCommands
    {
        get => (bool)GetValue(HasSecondaryCommandsProperty);
        set => SetValue(HasSecondaryCommandsProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public ObservableCollection<UIElement> SecondaryCommands { get; } = new();

    static CommandBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBar), new FrameworkPropertyMetadata(typeof(CommandBar)));
    }

    public CommandBar()
    {
        SecondaryCommands.CollectionChanged += SecondaryCommands_CollectionChanged;
    }

    private void SecondaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        //TODO: Not the best solution. Improve it.

        SecondaryCommandsSource ??= new List<UIElement>();

        if (e.NewItems != null)
            foreach (var newItem in e.NewItems)
                SecondaryCommandsSource.Add((UIElement)newItem);

        if (e.OldItems != null)
            foreach (var oldItem in e.OldItems)
                SecondaryCommandsSource.Remove((UIElement)oldItem);

        HasSecondaryCommands = SecondaryCommandsSource.Count > 0;
    }
}
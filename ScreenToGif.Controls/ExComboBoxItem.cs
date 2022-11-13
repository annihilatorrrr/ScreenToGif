using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class ExComboBoxItem: Control
{
    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ExComboBoxItem), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(string), typeof(ExComboBoxItem), new PropertyMetadata(default(string)));
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(nameof(Symbol), typeof(FluentSymbols), typeof(ExComboBoxItem), new PropertyMetadata(FluentSymbols.None));
    public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(ExComboBoxItem), new PropertyMetadata(default(string)));
    
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Content
    {
        get => (string)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public FluentSymbols Symbol
    {
        get => (FluentSymbols)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    static ExComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExComboBoxItem), new FrameworkPropertyMetadata(typeof(ExComboBoxItem)));
    }
}
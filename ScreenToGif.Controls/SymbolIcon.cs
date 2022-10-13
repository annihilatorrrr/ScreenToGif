using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Brush = System.Windows.Media.Brush;

namespace ScreenToGif.Controls;

public class SymbolIcon : TextBlock
{
    public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(nameof(Symbol), typeof(FluentSymbols), typeof(SymbolIcon), new PropertyMetadata(default(FluentSymbols), Symbol_Changed));

    public FluentSymbols Symbol
    {
        get => (FluentSymbols)GetValue(SymbolProperty);
        set => SetValue(SymbolProperty, value);
    }

    public SymbolIcon()
    {
        //SetValue(FontFamilyProperty, new FontFamily(new Uri("pack://application:,,,/ScreenToGif.Controls;component/Themes"), "FluentSystemIcons-Resizable"));
        SetValue(TextProperty, char.ConvertFromUtf32((int)Symbol));
    }

    static SymbolIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolIcon), new FrameworkPropertyMetadata(typeof(SymbolIcon)));
    }

    public override void OnApplyTemplate()
    {
        //Aid styling by binding foreground to parent's property.
        if (Foreground != default(Brush))
        {
            var binding = new Binding
            {
                RelativeSource = RelativeSource.TemplatedParent,
                Path = new PropertyPath(Control.ForegroundProperty)
            };
            SetBinding(ForegroundProperty, binding);
        }
        
        base.OnApplyTemplate();
    }

    private static void Symbol_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SymbolIcon icon)
            icon.SetValue(TextProperty, char.ConvertFromUtf32((int)icon.Symbol));
    }
}

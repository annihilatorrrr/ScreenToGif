using ScreenToGif.Domain.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScreenToGif.Controls;

public class ExMenuItem : MenuItem
{
    #region Variables

    public static new readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(FluentSymbols), typeof(ExMenuItem), new FrameworkPropertyMetadata(FluentSymbols.None));

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(nameof(TextWrapping), typeof(TextWrapping), typeof(ExMenuItem), new FrameworkPropertyMetadata(TextWrapping.NoWrap,
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion

    #region Properties

    public new FluentSymbols Icon
    {
        get => (FluentSymbols)GetValue(IconProperty);
        set => SetCurrentValue(IconProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => (TextWrapping)GetValue(TextWrappingProperty);
        set => SetCurrentValue(TextWrappingProperty, value);
    }

    #endregion

    static ExMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ExMenuItem), new FrameworkPropertyMetadata(typeof(ExMenuItem)));
    }
}
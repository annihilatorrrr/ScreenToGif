using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace ScreenToGif.Controls;

public class PathIcon : FrameworkElement
{
    private Pen _pen;

    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(PathIcon), new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, Foreground_Changed));

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(PathIcon), new FrameworkPropertyMetadata(default(Geometry), FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush Foreground
    {
        get => (Brush)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public Geometry Data
    {
        get => (Geometry)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    //static PathIcon()
    //{
    //    DefaultStyleKeyProperty.OverrideMetadata(typeof(PathIcon), new FrameworkPropertyMetadata(typeof(PathIcon)));
    //}

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

        SetPen();

        base.OnApplyTemplate();
    }
    
    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawGeometry(null, _pen, Data);

        base.OnRender(drawingContext);
    }

    private void SetPen()
    {
        _pen = new Pen(Foreground, 2);
    }
    
    private static void Foreground_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PathIcon icon)
            icon.SetPen();
    }
}
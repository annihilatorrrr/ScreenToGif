using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenToGif.Controls;

[TemplatePart(Name = IndicatorPathId, Type = typeof(Path))]
[TemplatePart(Name = IndicatorPathFigureId, Type = typeof(PathFigure))]
[TemplatePart(Name = IndicatorArcSegmentId, Type = typeof(ArcSegment))]
[TemplatePart(Name = TrackPathId, Type = typeof(Path))]
[TemplatePart(Name = TrackPathFigureId, Type = typeof(PathFigure))]
[TemplatePart(Name = TrackArcSegmentId, Type = typeof(ArcSegment))]
public class ProgressRing : ProgressBar
{
    private const string IndicatorPathId = "IndicatorPath";
    private const string IndicatorPathFigureId = "IndicatorPathFigure";
    private const string IndicatorArcSegmentId = "IndicatorArcSegment";
    private const string TrackPathId = "TrackPath";
    private const string TrackPathFigureId = "TrackPathFigure";
    private const string TrackArcSegmentId = "TrackArcSegment";

    private Path _indicatorPath;
    private PathFigure _indicatorPathFigure;
    private ArcSegment _indicatorArcSegment;
    private Path _trackPath;
    private PathFigure _trackPathFigure;
    private ArcSegment _trackArcSegment;

    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ProgressRing), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(ProgressRing), new FrameworkPropertyMetadata(6d, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }
    
    static ProgressRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _indicatorPath = GetTemplateChild(IndicatorPathId) as Path;
        _indicatorPathFigure = GetTemplateChild(IndicatorPathFigureId) as PathFigure;
        _indicatorArcSegment = GetTemplateChild(IndicatorArcSegmentId) as ArcSegment;
        _trackPath = GetTemplateChild(TrackPathId) as Path;
        _trackPathFigure = GetTemplateChild(TrackPathFigureId) as PathFigure;
        _trackArcSegment = GetTemplateChild(TrackArcSegmentId) as ArcSegment;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        UpdateArc();
        UpdateBaseArc();
    }

    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);

        if (!IsLoaded)
            return;

        UpdateArc();
        UpdateBaseArc();
    }

    private Point ComputeCartesianCoordinate(double angle, double radius)
    {
        //Convert to radians.
        var angleRad = (Math.PI / 180.0) * (angle - 90);

        var x = radius * Math.Cos(angleRad);
        var y = radius * Math.Sin(angleRad);

        return new Point(x, y);
    }

    public void UpdateArc()
    {
        if (Visibility != Visibility.Visible)
            return;

        //Inverted: Math.Abs((100F * (Value - 1)) / (Maximum - Minimum) - 100F);
        var radius = (ActualWidth) / 2d - StrokeThickness;
        var percentage = (100d * Value) / (Maximum - Minimum);
        var angle = (percentage * 360d) / 100d;

        var startPoint = new Point(radius + StrokeThickness / 2d, StrokeThickness / 2d);
        var endPoint = ComputeCartesianCoordinate(angle, radius);

        endPoint.X += radius + StrokeThickness / 2d;
        endPoint.Y += radius + StrokeThickness / 2d;

        if (_indicatorPath != null)
        {
            _indicatorPath.Width = radius * 2d + StrokeThickness;
            _indicatorPath.Height = radius * 2d + StrokeThickness;
        }
        
        if (_indicatorPathFigure != null)
            _indicatorPathFigure.StartPoint = startPoint;

        if (Math.Abs(startPoint.X - Math.Round(endPoint.X)) < 0.001d && Math.Abs(startPoint.Y - Math.Round(endPoint.Y)) < 0.001d)
            endPoint.X -= 0.01d;

        if (_indicatorArcSegment != null)
        {
            _indicatorArcSegment.Point = endPoint;
            _indicatorArcSegment.Size = new Size(radius, radius);
            _indicatorArcSegment.IsLargeArc = angle > 180d;
        }
    }

    private void UpdateBaseArc()
    {
        if (Visibility != Visibility.Visible)
            return;

        var radius = (ActualWidth) / 2d - StrokeThickness;
        var startPoint = new Point(radius + StrokeThickness / 2d, StrokeThickness / 2d);
        var endPoint = ComputeCartesianCoordinate(360, radius);

        endPoint.X += radius + StrokeThickness / 2d;
        endPoint.Y += radius + StrokeThickness / 2d;

        if (_indicatorPath != null)
        {
            _trackPath.Width = radius * 2 + StrokeThickness;
            _trackPath.Height = radius * 2 + StrokeThickness;
        }
        
        if (_trackPathFigure != null)
            _trackPathFigure.StartPoint = startPoint;

        if (Math.Abs(startPoint.X - Math.Round(endPoint.X)) < 0.001 && Math.Abs(startPoint.Y - Math.Round(endPoint.Y)) < 0.001)
            endPoint.X -= 0.01;

        if (_indicatorArcSegment != null)
        {
            _trackArcSegment.Point = endPoint;
            _trackArcSegment.Size = new Size(radius, radius);
        }
    }
}
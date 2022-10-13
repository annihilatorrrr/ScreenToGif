using ScreenToGif.Util.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenToGif.Controls;

public class DirectionalWrapPanel : WrapPanel
{
    public static readonly DependencyProperty ArrowWidthProperty = DependencyProperty.Register(nameof(ArrowWidth), typeof(double), typeof(DirectionalWrapPanel), new FrameworkPropertyMetadata(26d, FrameworkPropertyMetadataOptions.AffectsMeasure), IsWidthHeightValid);

    public double ArrowWidth
    {
        get => (double)GetValue(ArrowWidthProperty);
        set => SetValue(ArrowWidthProperty, value);
    }

    private static bool IsWidthHeightValid(object value)
    {
        var v = (double)value;
        return (double.IsNaN(v)) || (v >= 0.0d && !double.IsPositiveInfinity(v));
    }

    protected override Size MeasureOverride(Size constraint)
    {
        var currentLineSize = new OrientableSize(Orientation);
        var panelSize = new OrientableSize(Orientation);
        var uvConstraint = new OrientableSize(Orientation, constraint.Width, constraint.Height);

        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        var itemWidthSet = !double.IsNaN(itemWidth);
        var itemHeightSet = !double.IsNaN(itemHeight);

        var childConstraint = new Size(itemWidthSet ? itemWidth : constraint.Width, itemHeightSet ? itemHeight : constraint.Height);

        for (int i = 0, count = InternalChildren.Count; i < count; i++)
        {
            var child = InternalChildren[i];

            if (child == null)
                continue;

            child.Measure(childConstraint);

            var childrenSize = new OrientableSize(Orientation, itemWidthSet ? itemWidth : child.DesiredSize.Width, itemHeightSet ? itemHeight : child.DesiredSize.Height);

            //Calculate arrow size.
            if (i > 0 && InternalChildren.Count > 1)
            {
                //If there's another item before this one, add an arrow in the middle.
                var arrowSize = new OrientableSize(Orientation, ArrowWidth, ArrowWidth);

                if ((currentLineSize.U + arrowSize.U + childrenSize.U).GreaterThan(uvConstraint.U))
                {
                    //If jumping to another line -  (Width: 10px)
                    //                            ]
                    //  --------------------------
                    // [
                    //  ->

                    panelSize.U += 10;
                    panelSize.V += 10;
                    //TODO: the next item after a jump needs to have +10px, to accomodate the ending of the line.
                }
                else
                {
                    //If sideways ->
                    //Just add the space that the arrow will occupy.
                    currentLineSize.U += arrowSize.U;
                    currentLineSize.V = Math.Max(arrowSize.V, currentLineSize.V);
                }
            }

            if ((currentLineSize.U + childrenSize.U).GreaterThan(uvConstraint.U))
            {
                //Need to switch to another line.
                panelSize.U = Math.Max(currentLineSize.U, panelSize.U);
                panelSize.V += currentLineSize.V;
                currentLineSize = childrenSize;

                if (childrenSize.U.GreaterThan(uvConstraint.U)) 
                {
                    //The element is wider then the constraint - give it a separate line.
                    panelSize.U = Math.Max(childrenSize.U, panelSize.U);
                    panelSize.V += childrenSize.V;
                    currentLineSize = new OrientableSize(Orientation);
                }
            }
            else 
            {
                //Continue to accumulate a line.
                currentLineSize.U += childrenSize.U;
                currentLineSize.V = Math.Max(childrenSize.V, currentLineSize.V); //The line size follows the biggest element.
            }
        }

        //the last line size, if any should be added
        panelSize.U = Math.Max(currentLineSize.U, panelSize.U);
        panelSize.V += currentLineSize.V;

        return new Size(panelSize.Width, panelSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var firstInLine = 0;
        var itemWidth = ItemWidth;
        var itemHeight = ItemHeight;
        double accumulatedV = 0;
        var itemU = (Orientation == Orientation.Horizontal ? itemWidth : itemHeight);
        var currentLineSize = new OrientableSize(Orientation);
        var uvFinalSize = new OrientableSize(Orientation, finalSize.Width, finalSize.Height);
        var itemWidthSet = !double.IsNaN(itemWidth);
        var itemHeightSet = !double.IsNaN(itemHeight);
        var useItemU = (Orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet);

        var children = InternalChildren;
        
        for (int i = 0, count = children.Count; i < count; i++)
        {
            var child = children[i];

            if (child == null)
                continue;

            var childrenSize = new OrientableSize(Orientation, (itemWidthSet ? itemWidth : child.DesiredSize.Width), (itemHeightSet ? itemHeight : child.DesiredSize.Height));
            var previousPosition = new Point(uvFinalSize.U, uvFinalSize.V);

            //Calculate arrow size.
            if (i > 0 && InternalChildren.Count > 1)
            {
                //If there's another item before this one, add an arrow in the middle.
                var arrowSize = new OrientableSize(Orientation, ArrowWidth, ArrowWidth);
                
                if ((currentLineSize.U + arrowSize.U + childrenSize.U).GreaterThan(uvFinalSize.U))
                {
                    //If jumping to another line -  (Width: 10px)
                    //                            ]
                    //  --------------------------
                    // [
                    //  ->

                    //panelSize.U += 10;
                    accumulatedV += 10;
                    //TODO: the next item after a jump needs to have +10px, to accomodate the ending of the line.
                }
                else
                {
                    //If sideways ->
                    //Just add the space that the arrow will occupy.
                    currentLineSize.U += arrowSize.U;
                    currentLineSize.V = Math.Max(arrowSize.V, currentLineSize.V);
                }
            }

            if ((currentLineSize.U + childrenSize.U).GreaterThan(uvFinalSize.U)) //need to switch to another line
            {
                ArrangeLine(accumulatedV, currentLineSize.V, firstInLine, i, useItemU, itemU, previousPosition);

                accumulatedV += currentLineSize.V;
                currentLineSize = childrenSize;

                if (childrenSize.U.GreaterThan(uvFinalSize.U)) //the element is wider then the constraint - give it a separate line                    
                {
                    //switch to next line which only contain one element
                    ArrangeLine(accumulatedV, childrenSize.V, i, ++i, useItemU, itemU, previousPosition);

                    accumulatedV += childrenSize.V;
                    currentLineSize = new OrientableSize(Orientation);
                }

                firstInLine = i;
            }
            else //continue to accumulate a line
            {
                currentLineSize.U += childrenSize.U;
                currentLineSize.V = Math.Max(childrenSize.V, currentLineSize.V);
            }
        }

        //Arrange the last line, if any.
        if (firstInLine < children.Count)
            ArrangeLine(accumulatedV, currentLineSize.V, firstInLine, children.Count, useItemU, itemU, new Point(0, currentLineSize.V));
        
        return finalSize;
    }

    private void ArrangeLine(double v, double lineV, int start, int end, bool useItemU, double itemU, Point previousPos)
    {
        double u = 0;
        var isHorizontal = (Orientation == Orientation.Horizontal);

        var children = InternalChildren;
        var layer = AdornerLayer.GetAdornerLayer(this);

        for (var i = start; i < end; i++)
        {
            if (children[i] is not UIElement child)
                continue;

            var childSize = new OrientableSize(Orientation, child.DesiredSize.Width, child.DesiredSize.Height);
            var layoutSlotU = (useItemU ? itemU : childSize.U);

            var rect = new Rect((isHorizontal ? u : v), (isHorizontal ? v : u), (isHorizontal ? layoutSlotU : lineV), (isHorizontal ? lineV : layoutSlotU));

            if (i > 0)
                layer.Add(new LineAdorner(child, previousPos, rect.TopLeft));

            child.Arrange(rect);
            u += layoutSlotU;
        }
    }

    private class LineAdorner : Adorner
    {
        /// <summary>
        /// To store and manage the adorner's visual children.
        /// </summary>
        readonly VisualCollection _visualChildren;

        /// <summary>
        /// The current adorned element.
        /// </summary>
        private UIElement _adornedElement;

        public Point Start { get; set; }

        public Point End { get; set; }

        public LineAdorner(UIElement adornedElement, Point start, Point end) : base(adornedElement)
        {
            Start = start;
            End = end;

            _visualChildren = new VisualCollection(this);

            _visualChildren.Add(DrawLinkArrow(start, end));
        }

        private static Shape DrawLinkArrow(Point p1, Point p2)
        {
            var lineGroup = new GeometryGroup();
            var theta = Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)) * 180 / Math.PI;

            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            var p = new Point(p1.X + ((p2.X - p1.X) / 1.35), p1.Y + ((p2.Y - p1.Y) / 1.35));
            pathFigure.StartPoint = p;

            var lpoint = new Point(p.X + 6, p.Y + 15);
            var rpoint = new Point(p.X - 6, p.Y + 15);
            var seg1 = new LineSegment { Point = lpoint };
            pathFigure.Segments.Add(seg1);

            var seg2 = new LineSegment { Point = rpoint };
            pathFigure.Segments.Add(seg2);

            var seg3 = new LineSegment { Point = p };
            pathFigure.Segments.Add(seg3);

            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Transform = new RotateTransform { Angle = theta + 90, CenterX = p.X, CenterY = p.Y };
            lineGroup.Children.Add(pathGeometry);

            var connectorGeometry = new LineGeometry { StartPoint = p1, EndPoint = p2 };
            lineGroup.Children.Add(connectorGeometry);

            return new Path { Data = lineGroup, StrokeThickness = 2, Stroke = Brushes.Black, Fill = Brushes.Black };
        }
    }

    private struct OrientableSize
    {
        internal double U;
        internal double V;
        private readonly Orientation _orientation;

        internal OrientableSize(Orientation orientation, double width, double height)
        {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal OrientableSize(Orientation orientation)
        {
            U = V = 0d;
            _orientation = orientation;
        }
        
        internal double Width
        {
            get => (_orientation == Orientation.Horizontal ? U : V);
            set
            {
                if (_orientation != Orientation.Horizontal)
                    V = value;
                else
                    U = value;
            }
        }
        internal double Height
        {
            get => (_orientation == Orientation.Horizontal ? V : U);
            set
            {
                if (_orientation != Orientation.Horizontal)
                    U = value;
                else
                    V = value;
            }
        }
    }
}

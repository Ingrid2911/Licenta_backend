using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public static class LabelFactory
    {
        private static readonly Brush CanvasBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202B33"));
        public static Border CreateLabel(string text, Brush foreground, double fontSize)
        {
            return new Border
            {
                Background = CanvasBrush,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(3, 1, 3, 1),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = fontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = foreground
                }
            };
        }
        public static Point GetPointOnEdge(Point from, Point to, double t)
        {
            return new Point(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t);
        }
        public static Point GetOffsetPoint(Point from, Point to, double t, double offset, int side)
        {
            Point p = GetPointOnEdge(from, to, t);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.001)
                return p;

            double nx = -dy / len;
            double ny = dx / len;

            return new Point(
                p.X + nx * offset * side,
                p.Y + ny * offset * side);
        }
        public static int GetLabelSide(string fromNode, string toNode)
        {
            string key = fromNode + "->" + toNode;
            int sum = 0;

            foreach (char c in key)
                sum += c;

            return sum % 2 == 0 ? 1 : -1;
        }
        public static void AddEdgeLabel(
            Canvas canvas,
            Border labelBox,
            Point from,
            Point to,
            string fromNode,
            string toNode,
            Brush guideBrush,
            double t,
            double offset,
            int zIndex)
        {
            int side = GetLabelSide(fromNode, toNode);

            Point anchor = GetPointOnEdge(from, to, t);
            Point labelCenter = GetOffsetPoint(from, to, t, offset, side);

            var guide = new System.Windows.Shapes.Line
            {
                X1 = anchor.X,
                Y1 = anchor.Y,
                X2 = labelCenter.X,
                Y2 = labelCenter.Y,
                Stroke = guideBrush,
                StrokeThickness = 1.1,
                Opacity = 0.9
            };

            Panel.SetZIndex(guide, zIndex - 1);
            canvas.Children.Add(guide);

            labelBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size size = labelBox.DesiredSize;

            Canvas.SetLeft(labelBox, labelCenter.X - size.Width / 2);
            Canvas.SetTop(labelBox, labelCenter.Y - size.Height / 2);
            Panel.SetZIndex(labelBox, zIndex);

            canvas.Children.Add(labelBox);
        }
    }
}
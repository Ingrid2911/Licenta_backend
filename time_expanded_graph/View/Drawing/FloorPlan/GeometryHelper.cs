using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan.Common
{
    public static class GeometryHelper
    {
        public static void DrawPolylinePath(Canvas canvas, List<Point> points, Brush brush,
                                           double thickness, int zIndex, bool dashed = false,
                                           double opacity = 1.0)
        {
            if (points == null || points.Count < 2) return;

            var figure = new PathFigure
            {
                StartPoint = points[0],
                IsClosed = false,
                IsFilled = false
            };

            var segment = new PolyLineSegment();
            for (int i = 1; i < points.Count; i++)
                segment.Points.Add(points[i]);

            figure.Segments.Add(segment);

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            var path = new Path
            {
                Data = geometry,
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Flat,
                StrokeLineJoin = PenLineJoin.Miter,
                Opacity = opacity,
                IsHitTestVisible = false
            };

            if (dashed)
                path.StrokeDashArray = new DoubleCollection { 8, 4 };

            Panel.SetZIndex(path, zIndex);
            canvas.Children.Add(path);
        }

        public static void DrawArrow(Canvas canvas, double x1, double y1, double x2, double y2,
                                    Brush brush, double thick, int zIndex = 12, bool arrowOnly = false)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len < 1) return;

            double ux = dx / len, uy = dy / len;
            double arrowLen = 10, arrowW = 5;

            double ax = x2 - ux * arrowLen;
            double ay = y2 - uy * arrowLen;

            double wx1 = ax - uy * arrowW, wy1 = ay + ux * arrowW;
            double wx2 = ax + uy * arrowW, wy2 = ay - ux * arrowW;

            if (!arrowOnly)
            {
                var line = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = brush,
                    StrokeThickness = thick,
                    IsHitTestVisible = false
                };
                Panel.SetZIndex(line, zIndex);
                canvas.Children.Add(line);
            }

            var poly = new Polygon
            {
                Points = new PointCollection { new(x2, y2), new(wx1, wy1), new(wx2, wy2) },
                Fill = brush,
                IsHitTestVisible = false
            };
            Panel.SetZIndex(poly, zIndex + 1);
            canvas.Children.Add(poly);
        }

        public static Point GetBorderPointToward(BuildingElement el, Point target)
        {
            double x = el.Position.X;
            double y = el.Position.Y;
            double w = el.Width;
            double h = el.Height;

            Point center = el.Center;
            double dx = target.X - center.X;
            double dy = target.Y - center.Y;

            if (el.Type == BuildingElementType.StartPoint)
            {
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len < 0.001) return center;

                double r = Math.Min(el.Width, el.Height) / 2.0;
                return new Point(center.X + dx / len * r, center.Y + dy / len * r);
            }

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                return dx >= 0
                    ? new Point(x + w, center.Y)
                    : new Point(x, center.Y);
            }
            else
            {
                return dy >= 0
                    ? new Point(center.X, y + h)
                    : new Point(center.X, y);
            }
        }
    }
}
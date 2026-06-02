using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Drawing.FloorPlan.Common;

namespace time_expanded_graph.View.Drawing.FloorPlan.OptimalPath
{
    public class OptimalPathRenderer
    {
        private readonly Canvas _canvas;
        private readonly BuildingPlan _plan;

        private static readonly SolidColorBrush PathLine = new(Color.FromRgb(56, 142, 60));

        public OptimalPathRenderer(Canvas canvas, BuildingPlan plan)
        {
            _canvas = canvas;
            _plan = plan;
        }

        public void Draw(List<string> optimalPath)
        {
            var elements = new List<BuildingElement>();
            foreach (var id in optimalPath)
            {
                var el = _plan.Elements.FirstOrDefault(e => e.Id == id);
                if (el != null) elements.Add(el);
            }

            if (elements.Count < 2) return;

            // Draw arrows on each L-shaped route segment
            for (int i = 0; i < elements.Count - 1; i++)
            {
                List<Point> route = BuildLRouteBetweenElements(elements[i], elements[i + 1]);
                DrawArrowOnRoute(route);
            }

            // Draw circles on nodes
            foreach (var el in elements)
            {
                DrawNodeCircle(el);
            }
        }

        private List<Point> BuildLRouteBetweenElements(BuildingElement fromEl, BuildingElement toEl)
        {
            Point fromCenter = fromEl.Center;
            Point toCenter = toEl.Center;

            Point start = GeometryHelper.GetBorderPointToward(fromEl, toCenter);
            Point end = GeometryHelper.GetBorderPointToward(toEl, fromCenter);

            return BuildLRoute(start, end);
        }

        private List<Point> BuildLRoute(Point start, Point end)
        {
            var points = new List<Point> { start };

            double dx = Math.Abs(end.X - start.X);
            double dy = Math.Abs(end.Y - start.Y);

            if (dx < 0.001 || dy < 0.001)
            {
                points.Add(end);
                return points;
            }

            Point corner = dx >= dy
                ? new Point(end.X, start.Y)
                : new Point(start.X, end.Y);

            points.Add(corner);
            points.Add(end);
            return points;
        }

        private void DrawArrowOnRoute(List<Point> route)
        {
            if (route == null || route.Count < 2) return;

            // Place arrow on the last segment long enough
            for (int i = route.Count - 2; i >= 0; i--)
            {
                Point a = route[i];
                Point b = route[i + 1];
                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double len = System.Math.Sqrt(dx * dx + dy * dy);

                if (len < 12) continue;

                double ux = dx / len;
                double uy = dy / len;

                Point p2 = new Point(a.X + dx * 0.65, a.Y + dy * 0.65);
                Point p1 = new Point(p2.X - ux * 22, p2.Y - uy * 22);

                GeometryHelper.DrawArrow(_canvas, p1.X, p1.Y, p2.X, p2.Y, PathLine, 3, zIndex: 51);
                return;
            }
        }

        private void DrawNodeCircle(BuildingElement el)
        {
            Point p = el.Center;
            var c = new Ellipse
            {
                Width = 12,
                Height = 12,
                Fill = PathLine,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(c, p.X - 6); Canvas.SetTop(c, p.Y - 6);
            Panel.SetZIndex(c, 52);
            _canvas.Children.Add(c);
        }
    }
}
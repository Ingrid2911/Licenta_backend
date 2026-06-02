using System.Windows;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan
{
    public static class HallwayRouter
    {
        public static List<Point> BuildRoute(BuildingElement from, BuildingElement to)
        {
            Point fromCenter = from.Center;
            Point toCenter = to.Center;

            bool almostSameY = Math.Abs(fromCenter.Y - toCenter.Y) < 40;
            bool almostSameX = Math.Abs(fromCenter.X - toCenter.X) < 40;

            if (almostSameY)
            {
                Point start = GetPointOnElementBorder(from, toCenter);
                Point end = GetPointOnElementBorder(to, fromCenter);

                return new List<Point>
                {
                    start,
                    end
                };
            }

            if (almostSameX)
            {
                Point start = GetPointOnElementBorder(from, toCenter);
                Point end = GetPointOnElementBorder(to, fromCenter);

                return new List<Point>
                {
                    start,
                    end
                };
            }

            Point corner = new Point(toCenter.X, fromCenter.Y);

            Point startL = GetPointOnElementBorder(from, corner);
            Point endL = GetPointOnElementBorder(to, corner);

            return new List<Point>
            {
                startL,
                corner,
                endL
            };
        }

        private static Point GetPointOnElementBorder(BuildingElement element, Point targetPoint)
        {
            Rect rect = new Rect(
                element.Position.X,
                element.Position.Y,
                element.Width,
                element.Height);

            Point center = element.Center;

            double dx = targetPoint.X - center.X;
            double dy = targetPoint.Y - center.Y;

            if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
                return center;

            double halfWidth = rect.Width / 2.0;
            double halfHeight = rect.Height / 2.0;

            double scaleX = Math.Abs(dx) > 0.001
                ? halfWidth / Math.Abs(dx)
                : double.PositiveInfinity;

            double scaleY = Math.Abs(dy) > 0.001
                ? halfHeight / Math.Abs(dy)
                : double.PositiveInfinity;

            double scale = Math.Min(scaleX, scaleY);

            return new Point(
                center.X + dx * scale,
                center.Y + dy * scale);
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Drawing.FloorPlan.Common;

namespace time_expanded_graph.View.Drawing.FloorPlan.Connections
{
    public class ConnectionRenderer
    {
        private readonly Canvas _canvas;
        private readonly BuildingPlan _plan;
        private static readonly SolidColorBrush ConnLine = new(Color.FromRgb(100, 160, 230));
        private static readonly SolidColorBrush PathLine = new(Color.FromRgb(56, 142, 60));

        public ConnectionRenderer(Canvas canvas, BuildingPlan plan)
        {
            _canvas = canvas;
            _plan = plan;
        }
        public void DrawAll(List<string> optimalPath)
        {
            foreach (var conn in _plan.Connections)
                Draw(conn, optimalPath);
        }
        private void Draw(HallwayConnection conn, List<string> optimalPath)
        {
            var fromEl = _plan.Elements.FirstOrDefault(e => e.Id == conn.FromId);
            var toEl = _plan.Elements.FirstOrDefault(e => e.Id == conn.ToId);
            if (fromEl == null || toEl == null) return;

            bool onPath = IsConnOnPath(conn, optimalPath);
            List<Point> route = HallwayRouter.BuildRoute(fromEl, toEl);
            if (route.Count < 2) return;

            double hallwayThickness = CalculateHallwayThickness(fromEl, toEl);

            GeometryHelper.DrawPolylinePath(
                _canvas, route,
                onPath
                    ? new SolidColorBrush(Color.FromArgb(230, 56, 142, 60))
                    : new SolidColorBrush(Color.FromArgb(180, 100, 160, 230)),
                hallwayThickness,
                zIndex: onPath ? 6 : 3);

            GeometryHelper.DrawPolylinePath(
                _canvas, route,
                onPath ? PathLine : ConnLine,
                onPath ? 3.0 : 1.8,
                zIndex: onPath ? 7 : 4,
                dashed: !onPath);

            DrawLabel(route, $"c:{conn.Capacity} t:{conn.TravelTime}", onPath);
        }
        private double CalculateHallwayThickness(BuildingElement fromEl, BuildingElement toEl)
        {
            if (fromEl.Type == BuildingElementType.Room && toEl.Type == BuildingElementType.Room)
                return Math.Min(fromEl.Height, toEl.Height);

            if (fromEl.Type == BuildingElementType.Room)
                return fromEl.Height;

            if (toEl.Type == BuildingElementType.Room)
                return toEl.Height;

            return 48;
        }
        private void DrawLabel(List<Point> route, string text, bool onPath)
        {
            if (route.Count < 2) return;

            int segIndex = Math.Max(0, (route.Count - 1) / 2);
            Point a = route[segIndex];
            Point b = route[segIndex + 1];

            double mx = (a.X + b.X) / 2.0 + 4;
            double my = (a.Y + b.Y) / 2.0 - 10;

            var lbl = new TextBlock
            {
                Text = text,
                FontSize = 9,
                Foreground = onPath ? PathLine : new SolidColorBrush(Color.FromArgb(200, 210, 230, 255)),
                Background = new SolidColorBrush(Color.FromArgb(120, 28, 39, 48)),
                Padding = new Thickness(2, 0, 2, 0),
                IsHitTestVisible = false
            };

            Canvas.SetLeft(lbl, mx);
            Canvas.SetTop(lbl, my);
            Panel.SetZIndex(lbl, 8);
            _canvas.Children.Add(lbl);
        }
        private bool IsConnOnPath(HallwayConnection conn, List<string> optimalPath)
        {
            if (optimalPath.Count < 2) return false;

            for (int i = 0; i < optimalPath.Count - 1; i++)
            {
                if (optimalPath[i] == conn.FromId && optimalPath[i + 1] == conn.ToId ||
                    conn.IsBidirectional &&
                    optimalPath[i] == conn.ToId && optimalPath[i + 1] == conn.FromId)
                    return true;
            }
            return false;
        }
    }
}
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan.Elements
{
    public class ElevatorRenderer
    {
        private readonly Canvas _canvas;

        private static readonly SolidColorBrush ElevFill = new(Color.FromRgb(181, 212, 244));
        private static readonly SolidColorBrush ElevStroke = new(Color.FromRgb(55, 138, 221));
        private static readonly SolidColorBrush PathNode = new(Color.FromRgb(200, 30, 30));

        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;

        public ElevatorRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(BuildingElement el, bool onPath)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;

            var bg = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = ElevFill,
                Stroke = onPath ? PathNode : ElevStroke,
                StrokeThickness = onPath ? 2.5 : 1.5,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(bg, x); Canvas.SetTop(bg, y);
            Panel.SetZIndex(bg, 10);
            bg.MouseLeftButtonDown += (s, e) => ElementMouseDown?.Invoke(el, e);
            bg.MouseRightButtonDown += (s, e) =>
            {
                ContextMenuRequested?.Invoke(el, e);
                e.Handled = true;
            };
            _canvas.Children.Add(bg);
            var d1 = new Line
            {
                X1 = x + 6,
                Y1 = y + 6,
                X2 = x + w - 6,
                Y2 = y + h - 6,
                Stroke = ElevStroke,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            var d2 = new Line
            {
                X1 = x + w - 6,
                Y1 = y + 6,
                X2 = x + 6,
                Y2 = y + h - 6,
                Stroke = ElevStroke,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Panel.SetZIndex(d1, 11); Panel.SetZIndex(d2, 11);
            _canvas.Children.Add(d1); _canvas.Children.Add(d2);

            var tb = new TextBlock
            {
                Text = "LIFT",
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(24, 95, 165)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y + h + 2);
            Panel.SetZIndex(tb, 12);
            _canvas.Children.Add(tb);
        }
    }
}
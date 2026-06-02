using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan.Elements
{
    public class StartPointRenderer
    {
        private readonly Canvas _canvas;
        private static readonly SolidColorBrush StartFill = new(Color.FromRgb(99, 153, 34));

        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;

        public StartPointRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }
        public void Draw(BuildingElement el, bool onPath)
        {
            double cx = el.Position.X + el.Width / 2;
            double cy = el.Position.Y + el.Height / 2;
            double r = Math.Min(el.Width, el.Height) / 2;

            var circle = new Ellipse
            {
                Width = r * 2,
                Height = r * 2,
                Fill = onPath
                    ? new SolidColorBrush(Color.FromRgb(180, 20, 20))
                    : StartFill,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(circle, cx - r); Canvas.SetTop(circle, cy - r);
            Panel.SetZIndex(circle, 10);
            circle.MouseLeftButtonDown += (s, e) => ElementMouseDown?.Invoke(el, e);
            circle.MouseRightButtonDown += (s, e) =>
            {
                ContextMenuRequested?.Invoke(el, e);
                e.Handled = true;
            };
            _canvas.Children.Add(circle);

            var tb = new TextBlock
            {
                Text = "S",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = r * 2
            };
            Canvas.SetLeft(tb, cx - r); Canvas.SetTop(tb, cy - 10);
            Panel.SetZIndex(tb, 11);
            _canvas.Children.Add(tb);

            var idtb = new TextBlock
            {
                Text = el.Id,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 220, 100)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = 80
            };
            Canvas.SetLeft(idtb, cx - 40); Canvas.SetTop(idtb, cy + r + 2);
            Panel.SetZIndex(idtb, 11);
            _canvas.Children.Add(idtb);
        }
    }
}
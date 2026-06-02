using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;
using time_expanded_graph.View.Drawing.FloorPlan.Common;

namespace time_expanded_graph.View.Drawing.FloorPlan.Elements
{
    public class ExitDoorRenderer
    {
        private readonly Canvas _canvas;
        private static readonly SolidColorBrush ExitFill = new(Color.FromRgb(216, 90, 48));

        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;

        public ExitDoorRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(BuildingElement el, bool onPath)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;

            var box = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = onPath
                    ? new SolidColorBrush(Color.FromRgb(180, 20, 20))
                    : ExitFill,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                RadiusX = 3,
                RadiusY = 3,
                Cursor = Cursors.Hand,
                Tag = el.Id
            };
            Canvas.SetLeft(box, x); Canvas.SetTop(box, y);
            Panel.SetZIndex(box, 10);
            box.MouseLeftButtonDown += (s, e) => ElementMouseDown?.Invoke(el, e);
            box.MouseRightButtonDown += (s, e) =>
            {
                ContextMenuRequested?.Invoke(el, e);
                e.Handled = true;
            };
            _canvas.Children.Add(box);

            double cx = x + w / 2, cy = y + h / 2;
            GeometryHelper.DrawArrow(_canvas, cx - 8, cy, cx + w / 2 + 14, cy, Brushes.White, 2.5);

            var tb = new TextBlock
            {
                Text = "EXIT",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y + h / 2 - 8);
            Panel.SetZIndex(tb, 11);
            _canvas.Children.Add(tb);

            var idtb = new TextBlock
            {
                Text = el.Id,
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 200, 180)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w
            };
            Canvas.SetLeft(idtb, x); Canvas.SetTop(idtb, y + h + 2);
            Panel.SetZIndex(idtb, 11);
            _canvas.Children.Add(idtb);
        }
    }
}
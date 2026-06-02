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
    public class StairsRenderer
    {
        private readonly Canvas _canvas;
        private const double StairsStep = 12.0;

        private static readonly SolidColorBrush StairFill = new(Color.FromRgb(211, 209, 199));
        private static readonly SolidColorBrush StairLine = new(Color.FromRgb(136, 135, 128));
        private static readonly SolidColorBrush WallBrush = new(Color.FromRgb(44, 44, 42));
        private static readonly SolidColorBrush PathNode = new(Color.FromRgb(200, 30, 30));

        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;

        public StairsRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw(BuildingElement el, bool onPath)
        {
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;

            // Background
            var bg = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = StairFill,
                Stroke = onPath ? PathNode : WallBrush,
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

            // Step lines (horizontal)
            int nSteps = Math.Max(3, (int)((h - 10) / StairsStep));
            double stepH = (h - 10) / nSteps;
            for (int i = 1; i <= nSteps; i++)
            {
                double sy = y + i * stepH;
                var l = new Line
                {
                    X1 = x + 4,
                    Y1 = sy,
                    X2 = x + w - 4,
                    Y2 = sy,
                    Stroke = StairLine,
                    StrokeThickness = 1,
                    IsHitTestVisible = false
                };
                Panel.SetZIndex(l, 11);
                _canvas.Children.Add(l);
            }

            // Vertical arrow (up direction)
            GeometryHelper.DrawArrow(_canvas, x + w / 2, y + h - 8, x + w / 2, y + 8,
                      new SolidColorBrush(Color.FromRgb(68, 68, 65)), 1.5, zIndex: 12);

            // Label
            var tb = new TextBlock
            {
                Text = "SCARI",
                FontSize = 9,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(68, 68, 65)),
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Drawing.FloorPlan.Elements
{
    public class RoomRenderer
    {
        private readonly Canvas _canvas;
        private static readonly SolidColorBrush WallBrush = new(Color.FromRgb(44, 44, 42));
        private const double WallThick = 5.0;

        private static readonly Color[] RoomColors =
        {
            Color.FromArgb(60, 99, 153, 34),
            Color.FromArgb(60, 33, 150, 243),
            Color.FromArgb(60, 156, 39, 176),
            Color.FromArgb(60, 255, 152, 0),
            Color.FromArgb(60, 121, 85, 72),
        };

        public RoomRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }
        public void Draw(BuildingElement el, int colorIndex, bool onPath)
        {
            Color roomColor = RoomColors[colorIndex % RoomColors.Length];
            double x = el.Position.X, y = el.Position.Y;
            double w = el.Width, h = el.Height;

            DrawFill(x, y, w, h, roomColor);
            DrawBorder(el, x, y, w, h, onPath);
            DrawLabel(el, x, y, w, h);
            DrawIdLabel(el, x, y, w, h);
            DrawResizeHandle(el);
        }
        private void DrawFill(double x, double y, double w, double h, Color color)
        {
            var fill = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = new SolidColorBrush(color),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(fill, x); Canvas.SetTop(fill, y);
            Panel.SetZIndex(fill, 1);
            _canvas.Children.Add(fill);
        }
        private void DrawBorder(BuildingElement el, double x, double y, double w, double h, bool onPath)
        {
            var border = new Rectangle
            {
                Width = w,
                Height = h,
                Fill = Brushes.Transparent,
                Stroke = onPath
                    ? new SolidColorBrush(Color.FromRgb(200, 30, 30))
                    : WallBrush,
                StrokeThickness = onPath ? WallThick + 2 : WallThick,
                Cursor = Cursors.SizeAll,
                Tag = el.Id
            };
            Canvas.SetLeft(border, x); Canvas.SetTop(border, y);
            Panel.SetZIndex(border, 10);
            border.MouseLeftButtonDown += (s, e) => RaiseElementMouseDown(el, e);
            border.MouseRightButtonDown += (s, e) => RaiseContextMenu(el, e);
            _canvas.Children.Add(border);
        }
        private void DrawLabel(BuildingElement el, double x, double y, double w, double h)
        {
            var lbl = new TextBlock
            {
                Text = el.Label,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 190)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w - 10
            };
            Canvas.SetLeft(lbl, x + 5); Canvas.SetTop(lbl, y + 8);
            Panel.SetZIndex(lbl, 11);
            _canvas.Children.Add(lbl);
        }
        private void DrawIdLabel(BuildingElement el, double x, double y, double w, double h)
        {
            var idlbl = new TextBlock
            {
                Text = el.Id,
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromArgb(150, 200, 200, 180)),
                IsHitTestVisible = false,
                TextAlignment = TextAlignment.Center,
                Width = w - 10
            };
            Canvas.SetLeft(idlbl, x + 5); Canvas.SetTop(idlbl, y + h - 18);
            Panel.SetZIndex(idlbl, 11);
            _canvas.Children.Add(idlbl);
        }
        private void DrawResizeHandle(BuildingElement el)
        {
            double rx = el.Position.X + el.Width - 10;
            double ry = el.Position.Y + el.Height - 10;

            var handle = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Color.FromArgb(180, 100, 160, 220)),
                Stroke = Brushes.White,
                StrokeThickness = 0.8,
                Cursor = Cursors.SizeNWSE,
                Tag = "resize_" + el.Id
            };
            Canvas.SetLeft(handle, rx); Canvas.SetTop(handle, ry);
            Panel.SetZIndex(handle, 20);
            handle.MouseLeftButtonDown += (s, e) => RaiseResizeMouseDown(el, e);
            _canvas.Children.Add(handle);
        }
        public event Action<BuildingElement, MouseButtonEventArgs>? ElementMouseDown;
        public event Action<BuildingElement, MouseButtonEventArgs>? ContextMenuRequested;
        public event Action<BuildingElement, MouseButtonEventArgs>? ResizeMouseDown;
        private void RaiseElementMouseDown(BuildingElement el, MouseButtonEventArgs e)
            => ElementMouseDown?.Invoke(el, e);
        private void RaiseContextMenu(BuildingElement el, MouseButtonEventArgs e)
            => ContextMenuRequested?.Invoke(el, e);
        private void RaiseResizeMouseDown(BuildingElement el, MouseButtonEventArgs e)
            => ResizeMouseDown?.Invoke(el, e);
    }
}
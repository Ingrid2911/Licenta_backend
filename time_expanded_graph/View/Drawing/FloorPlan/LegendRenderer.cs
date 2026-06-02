using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace time_expanded_graph.View.Drawing.FloorPlan.Legend
{
    public class LegendRenderer
    {
        private readonly Canvas _canvas;

        public LegendRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw()
        {
            double cw = _canvas.ActualWidth > 10 ? _canvas.ActualWidth : 1200;
            double ch = _canvas.ActualHeight > 10 ? _canvas.ActualHeight : 800;
            double lx = cw - 180, ly = ch - 160;

            DrawBackground(lx, ly);
            DrawItems(lx, ly);
        }

        private void DrawBackground(double lx, double ly)
        {
            var bg = new Rectangle
            {
                Width = 170,
                Height = 150,
                Fill = new SolidColorBrush(Color.FromArgb(200, 20, 30, 40)),
                Stroke = new SolidColorBrush(Color.FromArgb(80, 100, 150, 200)),
                StrokeThickness = 0.8,
                RadiusX = 4,
                RadiusY = 4,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(bg, lx); Canvas.SetTop(bg, ly);
            Panel.SetZIndex(bg, 90);
            _canvas.Children.Add(bg);
        }

        private void DrawItems(double lx, double ly)
        {
            var items = new[]
            {
                ("■", Color.FromRgb(99, 153, 34),  "Camera"),
                ("∩", Color.FromRgb(44, 44, 42),   "Usa"),
                ("≡", Color.FromRgb(136, 135, 128), "Scari"),
                ("✕", Color.FromRgb(55, 138, 221),  "Lift"),
                ("►", Color.FromRgb(216, 90, 48),   "Iesire"),
                ("●", Color.FromRgb(99, 153, 34),   "Start"),
                ("─", Color.FromRgb(56, 142, 60),   "Ruta optima"),
            };

            for (int i = 0; i < items.Length; i++)
            {
                var (sym, col, text) = items[i];
                double iy = ly + 14 + i * 19;

                DrawSymbol(lx + 8, iy, sym, col);
                DrawText(lx + 30, iy + 1, text);
            }
        }

        private void DrawSymbol(double x, double y, string symbol, Color color)
        {
            var stb = new TextBlock
            {
                Text = symbol,
                FontSize = 12,
                Foreground = new SolidColorBrush(color),
                IsHitTestVisible = false,
                Width = 18,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(stb, x); Canvas.SetTop(stb, y);
            Panel.SetZIndex(stb, 91);
            _canvas.Children.Add(stb);
        }

        private void DrawText(double x, double y, string text)
        {
            var ttb = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromArgb(200, 200, 210, 220)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(ttb, x); Canvas.SetTop(ttb, y);
            Panel.SetZIndex(ttb, 91);
            _canvas.Children.Add(ttb);
        }
    }
}
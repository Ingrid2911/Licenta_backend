using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace time_expanded_graph.View.Drawing.FloorPlan.Grid
{
    public class GridRenderer
    {
        private readonly Canvas _canvas;
        private const double Step = 40;
        private static readonly SolidColorBrush GridBrush =
            new(Color.FromArgb(30, 255, 255, 255));

        public GridRenderer(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Draw()
        {
            double w = _canvas.ActualWidth > 10 ? _canvas.ActualWidth : 1200;
            double h = _canvas.ActualHeight > 10 ? _canvas.ActualHeight : 800;

            DrawVerticalLines(w, h);
            DrawHorizontalLines(w, h);
        }

        private void DrawVerticalLines(double width, double height)
        {
            for (double x = 0; x <= width; x += Step)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = GridBrush,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(line);
            }
        }

        private void DrawHorizontalLines(double width, double height)
        {
            for (double y = 0; y <= height; y += Step)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = GridBrush,
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(line);
            }
        }
    }
}
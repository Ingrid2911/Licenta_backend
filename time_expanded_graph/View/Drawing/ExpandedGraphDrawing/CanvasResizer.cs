using System.Windows;
using System.Windows.Controls;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public static class CanvasResizer
    {
        public static void Resize(Canvas canvas, Dictionary<string, Point> positions)
        {
            if (positions.Count == 0)
                return;

            double maxX = positions.Values.Max(p => p.X);
            double maxY = positions.Values.Max(p => p.Y);

            canvas.Width = Math.Max(canvas.ActualWidth, maxX + 250);
            canvas.Height = Math.Max(canvas.ActualHeight, maxY + 180);
        }
    }
}
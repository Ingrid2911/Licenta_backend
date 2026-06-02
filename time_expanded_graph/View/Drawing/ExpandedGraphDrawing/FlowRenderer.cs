using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    public class FlowRenderer
    {
        private static readonly Brush FlowLabelBrush = Brushes.White;
        public void DrawFlowLabels(
            List<(int from, FlowEdge edge)> flowEdges,
            Dictionary<int, string> indexMap,
            Dictionary<string, Point> positions,
            Canvas canvas)
        {
            foreach (var fe in flowEdges)
            {
                if (fe.edge.Flow <= 0)
                    continue;

                if (!indexMap.ContainsKey(fe.from) || !indexMap.ContainsKey(fe.edge.To))
                    continue;

                string fromNode = indexMap[fe.from];
                string toNode = indexMap[fe.edge.To];

                if (!positions.ContainsKey(fromNode) || !positions.ContainsKey(toNode))
                    continue;

                DrawFlowEdge(fromNode, toNode, fe.edge, positions, canvas);
            }
        }
        private void DrawFlowEdge(
            string fromNode,
            string toNode,
            FlowEdge edge,
            Dictionary<string, Point> positions,
            Canvas canvas)
        {
            Point from = positions[fromNode];
            Point to = positions[toNode];

            var flowLine = new Line
            {
                X1 = from.X,
                Y1 = from.Y,
                X2 = to.X,
                Y2 = to.Y,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };

            Panel.SetZIndex(flowLine, 3);
            canvas.Children.Add(flowLine);

            var labelBox = LabelFactory.CreateLabel(
                $"{edge.Flow}/{edge.Capacity}",
                FlowLabelBrush,
                10);

            LabelFactory.AddEdgeLabel(
                canvas: canvas,
                labelBox: labelBox,
                from: from,
                to: to,
                fromNode: fromNode,
                toNode: toNode,
                guideBrush: Brushes.Red,
                t: 0.55,
                offset: 22,
                zIndex: 30);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing.ExpandedGraphDrawing
{
    internal class ExpandedGraphDrawing
    {
        private readonly LayoutCalculator _layoutCalculator;
        private readonly NodeRenderer _nodeRenderer;
        private readonly EdgeRenderer _edgeRenderer;
        private readonly FlowRenderer _flowRenderer;

        private Dictionary<string, Point> _positions = new();

        public ExpandedGraphDrawing()
        {
            _layoutCalculator = new LayoutCalculator();
            _nodeRenderer = new NodeRenderer();
            _edgeRenderer = new EdgeRenderer();
            _flowRenderer = new FlowRenderer();
        }
        public void DrawWithFlow(
            ExpandedGraph graph,
            Canvas canvas,
            List<(int from, FlowEdge edge)> flowEdges,
            Dictionary<int, string> indexMap)
        {
            var flowEdgeKeys = new HashSet<string>();

            foreach (var fe in flowEdges)
            {
                if (fe.edge.Flow <= 0)
                    continue;

                if (!indexMap.ContainsKey(fe.from) || !indexMap.ContainsKey(fe.edge.To))
                    continue;

                string fromNode = indexMap[fe.from];
                string toNode = indexMap[fe.edge.To];

                flowEdgeKeys.Add($"{fromNode}->{toNode}");
            }

            Draw(graph, canvas, flowEdgeKeys);
            _flowRenderer.DrawFlowLabels(flowEdges, indexMap, _positions, canvas);
        }
        public void Draw(
            ExpandedGraph graph,
            Canvas canvas,
            HashSet<string>? hiddenCapacityLabels = null)
        {
            canvas.Children.Clear();

            if (graph == null)
                return;

            _positions = _layoutCalculator.CalculatePositions(graph);

            CanvasResizer.Resize(canvas, _positions);

            _edgeRenderer.DrawAll(graph.ExpandedEdges, _positions, canvas, hiddenCapacityLabels);
            _nodeRenderer.DrawAll(graph, _positions, canvas);
        }
    }
}
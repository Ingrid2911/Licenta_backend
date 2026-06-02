using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.Models.Graphs;

namespace time_expanded_graph.View.Drawing
{
    internal class ExpandedGraphDrawing
    {
        private const double NodeRadius = 15;

        private const double SpacingX = 150;
        private const double SpacingY = 95;

        private const double StartX = 180;
        private const double StartY = 120;

        private readonly Brush CanvasBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202B33"));

        private readonly Brush NodeLabelBrush = Brushes.White;
        private readonly Brush EdgeLabelBrush = Brushes.White;
        private readonly Brush FlowLabelBrush = Brushes.White;

        private Dictionary<string, Point> positions = new();

        // =========================================================
        // DESENARE GRAF CU FLOW
        // =========================================================
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

                Point from = positions[fromNode];
                Point to = positions[toNode];

                Line flowLine = new Line
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

                Border labelBox = CreateLabelBox(
                    $"{fe.edge.Flow}/{fe.edge.Capacity}",
                    FlowLabelBrush,
                    10);

                AddEdgeLabel(
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

        // =========================================================
        // DESENARE GRAF NORMAL
        // =========================================================
        public void Draw(
            ExpandedGraph graph,
            Canvas canvas,
            HashSet<string>? hiddenCapacityLabels = null)
        {
            canvas.Children.Clear();
            positions = new Dictionary<string, Point>();

            if (graph == null)
                return;

            var originalNodeColumns = BuildOriginalNodeColumns(graph);

            BuildExpandedNodePositions(graph, originalNodeColumns);
            AddSuperSourceAndSinkPositions(graph, originalNodeColumns.Count);

            ResizeCanvas(canvas);

            DrawEdges(graph, canvas, hiddenCapacityLabels);
            DrawNodes(graph, canvas);
        }

        // =========================================================
        // Construiește coloanele pentru nodurile originale.
        // Important:
        // merge și pentru noduri simple: a_0, b_1, s_0, t_3
        // merge și pentru noduri din plan: CAM_1_0, EXIT_5_3, S_4_2
        // =========================================================
        private Dictionary<string, int> BuildOriginalNodeColumns(ExpandedGraph graph)
        {
            var originalNodes = graph.ExpandedNodes
                .Where(IsTimeExpandedNode)
                .Select(GetOriginalNodeId)
                .Distinct()
                .OrderBy(GetNodeTypePriority)
                .ThenBy(ExtractNumberFromNode)
                .ThenBy(n => n)
                .ToList();

            var columns = new Dictionary<string, int>();

            for (int i = 0; i < originalNodes.Count; i++)
                columns[originalNodes[i]] = i;

            return columns;
        }

        private void BuildExpandedNodePositions(
            ExpandedGraph graph,
            Dictionary<string, int> originalNodeColumns)
        {
            foreach (string node in graph.ExpandedNodes)
            {
                if (!IsTimeExpandedNode(node))
                    continue;

                string originalNodeId = GetOriginalNodeId(node);
                int time = GetTimeFromExpandedNode(node);

                if (!originalNodeColumns.ContainsKey(originalNodeId))
                    continue;

                int column = originalNodeColumns[originalNodeId];

                double x = StartX + column * SpacingX;
                double y = StartY + time * SpacingY;

                positions[node] = new Point(x, y);
            }
        }

        private void AddSuperSourceAndSinkPositions(
            ExpandedGraph graph,
            int originalNodeCount)
        {
            double maxY = StartY + graph.TimeHorizon * SpacingY;
            double middleY = StartY + (maxY - StartY) / 2;

            positions[graph.SuperSource] = new Point(
                StartX - SpacingX,
                middleY);

            positions[graph.SuperSink] = new Point(
                StartX + originalNodeCount * SpacingX,
                middleY);
        }

        private void DrawEdges(
            ExpandedGraph graph,
            Canvas canvas,
            HashSet<string>? hiddenCapacityLabels)
        {
            foreach (var edge in graph.ExpandedEdges)
            {
                if (!positions.ContainsKey(edge.From) || !positions.ContainsKey(edge.To))
                    continue;

                Point from = positions[edge.From];
                Point to = positions[edge.To];

                Brush color = edge.Type == ExpandedEdgeType.Holdover
                    ? Brushes.Gray
                    : Brushes.DodgerBlue;

                DoubleCollection? dash = edge.Type == ExpandedEdgeType.Holdover
                    ? new DoubleCollection { 4, 4 }
                    : null;

                Line line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = color,
                    StrokeThickness = edge.Type == ExpandedEdgeType.Holdover ? 1.1 : 1.6,
                    StrokeDashArray = dash,
                    Opacity = edge.Type == ExpandedEdgeType.Holdover ? 0.55 : 0.95
                };

                Panel.SetZIndex(line, 1);
                canvas.Children.Add(line);

                string edgeKey = $"{edge.From}->{edge.To}";

                bool hideCapacityLabel =
                    hiddenCapacityLabels != null &&
                    hiddenCapacityLabels.Contains(edgeKey);

                if (edge.Type != ExpandedEdgeType.Holdover && !hideCapacityLabel)
                {
                    Border capLabel = CreateLabelBox(
                        edge.Capacity.ToString(),
                        EdgeLabelBrush,
                        10);

                    AddEdgeLabel(
                        canvas: canvas,
                        labelBox: capLabel,
                        from: from,
                        to: to,
                        fromNode: edge.From,
                        toNode: edge.To,
                        guideBrush: Brushes.DodgerBlue,
                        t: 0.45,
                        offset: 14,
                        zIndex: 12);
                }
            }
        }

        private void DrawNodes(ExpandedGraph graph, Canvas canvas)
        {
            foreach (var kvp in positions)
            {
                string node = kvp.Key;
                Point pos = kvp.Value;

                Brush fill = GetNodeFill(node, graph);

                Ellipse ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.4,
                    Fill = fill
                };

                Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                Canvas.SetTop(ellipse, pos.Y - NodeRadius);
                Panel.SetZIndex(ellipse, 10);

                canvas.Children.Add(ellipse);

                Border labelBox = CreateLabelBox(
                    node,
                    NodeLabelBrush,
                    10);

                labelBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size labelSize = labelBox.DesiredSize;

                Canvas.SetLeft(labelBox, pos.X - labelSize.Width / 2);
                Canvas.SetTop(labelBox, pos.Y + NodeRadius + 2);
                Panel.SetZIndex(labelBox, 20);

                canvas.Children.Add(labelBox);
            }
        }

        private Brush GetNodeFill(string node, ExpandedGraph graph)
        {
            if (node == graph.SuperSource)
                return Brushes.LightGreen;

            if (node == graph.SuperSink)
                return Brushes.Orange;

            string original = IsTimeExpandedNode(node)
                ? GetOriginalNodeId(node)
                : node;

            if (original.StartsWith("S", StringComparison.OrdinalIgnoreCase) ||
                original.StartsWith("START", StringComparison.OrdinalIgnoreCase) ||
                original.Equals("s", StringComparison.OrdinalIgnoreCase))
            {
                return Brushes.LightGreen;
            }

            if (original.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase) ||
                original.StartsWith("T", StringComparison.OrdinalIgnoreCase) ||
                original.Equals("t", StringComparison.OrdinalIgnoreCase))
            {
                return Brushes.Orange;
            }

            return Brushes.LightYellow;
        }

        // =========================================================
        // Redimensionare canvas ca să nu mai fie tăiat graful.
        // Merge bine dacă TabItem-ul are ScrollViewer.
        // =========================================================
        private void ResizeCanvas(Canvas canvas)
        {
            if (positions.Count == 0)
                return;

            double maxX = positions.Values.Max(p => p.X);
            double maxY = positions.Values.Max(p => p.Y);

            canvas.Width = Math.Max(canvas.ActualWidth, maxX + 250);
            canvas.Height = Math.Max(canvas.ActualHeight, maxY + 180);
        }

        // =========================================================
        // Nod time-expanded = orice nod care se termină în _număr.
        // Exemple:
        // a_0
        // s_0
        // CAM_1_0
        // EXIT_5_3
        // =========================================================
        private bool IsTimeExpandedNode(string node)
        {
            int lastUnderscore = node.LastIndexOf('_');

            if (lastUnderscore < 0 || lastUnderscore == node.Length - 1)
                return false;

            string timePart = node.Substring(lastUnderscore + 1);

            return int.TryParse(timePart, out _);
        }

        private string GetOriginalNodeId(string expandedNode)
        {
            int lastUnderscore = expandedNode.LastIndexOf('_');

            if (lastUnderscore < 0)
                return expandedNode;

            return expandedNode.Substring(0, lastUnderscore);
        }

        private int GetTimeFromExpandedNode(string expandedNode)
        {
            int lastUnderscore = expandedNode.LastIndexOf('_');

            if (lastUnderscore < 0 || lastUnderscore == expandedNode.Length - 1)
                return 0;

            string timePart = expandedNode.Substring(lastUnderscore + 1);

            return int.TryParse(timePart, out int time)
                ? time
                : 0;
        }

        // =========================================================
        // Ordine mai logică:
        // Start la stânga, camere la mijloc, ieșiri la dreapta.
        // =========================================================
        private int GetNodeTypePriority(string node)
        {
            if (node.Equals("s", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("S_", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("START", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (node.StartsWith("CAM", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("ROOM", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (node.StartsWith("SC", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("STAIR", StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            if (node.StartsWith("LIFT", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (node.Equals("t", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("T_", StringComparison.OrdinalIgnoreCase) ||
                node.StartsWith("EXIT", StringComparison.OrdinalIgnoreCase))
            {
                return 4;
            }

            return 2;
        }

        private int ExtractNumberFromNode(string node)
        {
            string digits = new string(node.Where(char.IsDigit).ToArray());

            return int.TryParse(digits, out int value)
                ? value
                : int.MaxValue;
        }

        // =========================================================
        // Creează etichetă text
        // =========================================================
        private Border CreateLabelBox(string text, Brush foreground, double fontSize)
        {
            return new Border
            {
                Background = CanvasBrush,
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(3, 1, 3, 1),
                Child = new TextBlock
                {
                    Text = text,
                    FontSize = fontSize,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = foreground
                }
            };
        }

        private Point GetPointOnEdge(Point from, Point to, double t)
        {
            return new Point(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t);
        }

        private Point GetOffsetPoint(Point from, Point to, double t, double offset, int side)
        {
            Point p = GetPointOnEdge(from, to, t);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.001)
                return p;

            double nx = -dy / len;
            double ny = dx / len;

            return new Point(
                p.X + nx * offset * side,
                p.Y + ny * offset * side);
        }

        private int GetLabelSide(string fromNode, string toNode)
        {
            string key = fromNode + "->" + toNode;

            int sum = 0;

            foreach (char c in key)
                sum += c;

            return sum % 2 == 0 ? 1 : -1;
        }

        private void AddEdgeLabel(
            Canvas canvas,
            Border labelBox,
            Point from,
            Point to,
            string fromNode,
            string toNode,
            Brush guideBrush,
            double t,
            double offset,
            int zIndex)
        {
            int side = GetLabelSide(fromNode, toNode);

            Point anchor = GetPointOnEdge(from, to, t);
            Point labelCenter = GetOffsetPoint(from, to, t, offset, side);

            Line guide = new Line
            {
                X1 = anchor.X,
                Y1 = anchor.Y,
                X2 = labelCenter.X,
                Y2 = labelCenter.Y,
                Stroke = guideBrush,
                StrokeThickness = 1.1,
                Opacity = 0.9
            };

            Panel.SetZIndex(guide, zIndex - 1);
            canvas.Children.Add(guide);

            labelBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size size = labelBox.DesiredSize;

            Canvas.SetLeft(labelBox, labelCenter.X - size.Width / 2);
            Canvas.SetTop(labelBox, labelCenter.Y - size.Height / 2);
            Panel.SetZIndex(labelBox, zIndex);

            canvas.Children.Add(labelBox);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using time_expanded_graph.MaxFlowAlgorithms;

namespace time_expanded_graph.ExpandedTimeGraph
{
    internal class ExpandedGraphDrawing
    {
        private const double NodeRadius = 15;
        private const double SpacingX = 120;
        private const double SpacingY = 95;

        private const double StartX = 180;
        private const double StartY = 420;

        private readonly Brush CanvasBrush =
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202B33"));

        private readonly Brush NodeLabelBrush = Brushes.White;
        private readonly Brush EdgeLabelBrush = Brushes.White;
        private readonly Brush FlowLabelBrush = Brushes.White;

        private Dictionary<string, Point> positions;

        // =========================================================
        // DESENARE GRAF CU FLOW
        // =========================================================
        public void DrawWithFlow(
            ExpandedGraph graph,
            Canvas canvas,
            List<(int from, FlowEdge edge)> flowEdges,
            Dictionary<int, string> indexMap)
        {
            // Reținem muchiile care au flux pozitiv.
            // Pentru acestea nu mai afișăm separat capacitatea albastră,
            // fiindcă raportul roșu flow/capacity este suficient.
            var flowEdgeKeys = new HashSet<string>();

            foreach (var fe in flowEdges)
            {
                if (fe.edge.Flow <= 0)
                    continue;

                string fromNode = indexMap[fe.from];
                string toNode = indexMap[fe.edge.To];

                flowEdgeKeys.Add($"{fromNode}->{toNode}");
            }

            // Desenăm graful normal, dar ascundem capacitățile albastre
            // de pe muchiile care au deja flux roșu.
            Draw(graph, canvas, flowEdgeKeys);

            // Desenăm muchiile cu flux
            foreach (var fe in flowEdges)
            {
                if (fe.edge.Flow <= 0)
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

                Panel.SetZIndex(flowLine, 2);
                canvas.Children.Add(flowLine);

                Border labelBox = CreateLabelBox(
                    $"{fe.edge.Flow}/{fe.edge.Capacity}",
                    FlowLabelBrush,
                    10
                );

                AddEdgeLabel(
                    canvas: canvas,
                    labelBox: labelBox,
                    from: from,
                    to: to,
                    fromNode: fromNode,
                    toNode: toNode,
                    guideBrush: Brushes.Red,
                    t: 0.55,
                    offset: 20,
                    zIndex: 20
                );
            }
        }

        // =========================================================
        // DESENARE GRAF NORMAL
        // Dacă hiddenCapacityLabels conține o muchie, capacitatea ei
        // nu mai este afișată separat.
        // =========================================================
        public void Draw(
            ExpandedGraph graph,
            Canvas canvas,
            HashSet<string> hiddenCapacityLabels = null)
        {
            canvas.Children.Clear();
            positions = new Dictionary<string, Point>();

            // calcul poziții noduri time-expanded
            foreach (var node in graph.ExpandedNodes)
            {
                if (!node.Contains("_"))
                    continue;

                var parts = node.Split('_');

                string original = parts[0];
                int time = int.Parse(parts.Last());

                double x = StartX + (original[0] - 'a' + 1) * SpacingX;

                if (original == "s")
                    x = StartX;

                if (original == "t")
                    x = StartX + 6 * SpacingX;

                double y = StartY + (graph.TimeHorizon - time) * SpacingY;

                positions[node] = new Point(x, y);
            }

            // poziții pentru S* și T*
            double middleY = StartY + (graph.TimeHorizon * SpacingY) / 2;

            positions[graph.SuperSource] = new Point(StartX - SpacingX, middleY);
            positions[graph.SuperSink] = new Point(StartX + 7 * SpacingX, middleY);

            // desenare muchii
            foreach (var edge in graph.ExpandedEdges)
            {
                if (!positions.ContainsKey(edge.From) || !positions.ContainsKey(edge.To))
                    continue;

                Point from = positions[edge.From];
                Point to = positions[edge.To];

                Brush color = edge.Type == ExpandedEdgeType.Holdover
                    ? Brushes.Gray
                    : Brushes.DodgerBlue;

                DoubleCollection dash = edge.Type == ExpandedEdgeType.Holdover
                    ? new DoubleCollection { 4, 4 }
                    : null;

                Line line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = color,
                    StrokeThickness = 1.5,
                    StrokeDashArray = dash
                };

                Panel.SetZIndex(line, 1);
                canvas.Children.Add(line);

                // capacitate pentru muchiile de mișcare
                // Nu o afișăm dacă muchia are deja flow label roșu, de tip 4/9.
                string edgeKey = $"{edge.From}->{edge.To}";

                bool hideCapacityLabel =
                    hiddenCapacityLabels != null &&
                    hiddenCapacityLabels.Contains(edgeKey);

                if (edge.Type != ExpandedEdgeType.Holdover && !hideCapacityLabel)
                {
                    Border capLabel = CreateLabelBox(
                        edge.Capacity.ToString(),
                        EdgeLabelBrush,
                        10
                    );

                    AddEdgeLabel(
                        canvas: canvas,
                        labelBox: capLabel,
                        from: from,
                        to: to,
                        fromNode: edge.From,
                        toNode: edge.To,
                        guideBrush: Brushes.DodgerBlue,
                        t: 0.45,
                        offset: 13,
                        zIndex: 8
                    );
                }
            }

            // desenare noduri
            foreach (var kvp in positions)
            {
                string node = kvp.Key;
                Point pos = kvp.Value;

                Brush fill = Brushes.LightYellow;

                if (node == graph.SuperSource)
                    fill = Brushes.LightGreen;

                if (node == graph.SuperSink)
                    fill = Brushes.Orange;

                Ellipse ellipse = new Ellipse
                {
                    Width = NodeRadius * 2,
                    Height = NodeRadius * 2,
                    Stroke = Brushes.Black,
                    Fill = fill
                };

                Canvas.SetLeft(ellipse, pos.X - NodeRadius);
                Canvas.SetTop(ellipse, pos.Y - NodeRadius);
                Panel.SetZIndex(ellipse, 5);

                canvas.Children.Add(ellipse);

                Border labelBox = CreateLabelBox(
                    node,
                    NodeLabelBrush,
                    10
                );

                Canvas.SetLeft(labelBox, pos.X - 16);
                Canvas.SetTop(labelBox, pos.Y + 14);
                Panel.SetZIndex(labelBox, 9);

                canvas.Children.Add(labelBox);
            }
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

        // =========================================================
        // Returnează un punct de pe muchie.
        // t = 0.0 -> începutul muchiei
        // t = 1.0 -> finalul muchiei
        // =========================================================
        private Point GetPointOnEdge(Point from, Point to, double t)
        {
            return new Point(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t
            );
        }

        // =========================================================
        // Returnează un punct deplasat perpendicular față de muchie.
        // offset = distanța față de muchie.
        // side = 1 sau -1, partea pe care se pune eticheta.
        // =========================================================
        private Point GetOffsetPoint(Point from, Point to, double t, double offset, int side)
        {
            Point p = GetPointOnEdge(from, to, t);

            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);

            if (len < 0.001)
                return p;

            // vector perpendicular normalizat
            double nx = -dy / len;
            double ny = dx / len;

            return new Point(
                p.X + nx * offset * side,
                p.Y + ny * offset * side
            );
        }

        // =========================================================
        // Decide deterministic pe ce parte a muchiei se pune eticheta.
        // Scopul este să nu fie toate etichetele pe aceeași parte.
        // =========================================================
        private int GetLabelSide(string fromNode, string toNode)
        {
            string key = fromNode + "->" + toNode;

            int sum = 0;

            foreach (char c in key)
                sum += c;

            return (sum % 2 == 0) ? 1 : -1;
        }

        // =========================================================
        // Adaugă etichetă deplasată + linie de legătură către muchie.
        // Asta face clar cărei muchii îi aparține eticheta.
        // =========================================================
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
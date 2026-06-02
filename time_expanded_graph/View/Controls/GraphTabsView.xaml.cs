using System;
using System.Collections.Generic;
using System.Windows.Controls;
using time_expanded_graph.Models.Building;
using time_expanded_graph.Models.Graphs;
using time_expanded_graph.View.Drawing;

namespace time_expanded_graph.View.Controls
{
    public partial class GraphTabsView : UserControl
    {
        public GraphTabsView()
        {
            InitializeComponent();
            BuildingPlan.BuildGraphRequested += OnBuildingPlanBuildGraphRequested;
        }
        public BuildingPlan GetBuildingPlan() => BuildingPlan.Plan;
        public event EventHandler? BuildingGraphRequested;

        private void OnBuildingPlanBuildGraphRequested(object? sender, EventArgs e)
        {
            BuildingGraphRequested?.Invoke(this, EventArgs.Empty);
        }
        public void HighlightEvacuationPath(IEnumerable<string> nodeIds)
        {
            BuildingPlan.HighlightEvacuationPath(nodeIds);
        }
        public void ClearEvacuationPath() => BuildingPlan.ClearPath();
        public void DrawSimpleGraph(SimpleGraph graph)
        {
            var renderer = new SimpleGraphDrawing();
            renderer.Draw(graph, OriginalGraphCanvas);
        }
        public void DrawExpandedGraph(ExpandedGraph graph)
        {
            var renderer = new ExpandedGraphDrawing();
            renderer.Draw(graph, ExpandedGraphCanvas);
        }
        public void DrawAlgorithmResults(
            ExpandedGraph graph,
            List<(int from, FlowEdge edge)> dinicEdges,
            Dictionary<int, string> dinicIndexToNodeMap,
            List<(int from, FlowEdge edge)> pushRelabelEdges,
            Dictionary<int, string> pushRelabelIndexToNodeMap,
            List<(int from, FlowEdge edge)> edmondsKarpEdges,
            Dictionary<int, string> edmondsKarpIndexToNodeMap)
        {
            var renderer = new ExpandedGraphDrawing();

            renderer.DrawWithFlow(graph, DinicGraphCanvas,
                dinicEdges, dinicIndexToNodeMap);

            renderer.DrawWithFlow(graph, PushRelabelGraphCanvas,
                pushRelabelEdges, pushRelabelIndexToNodeMap);

            renderer.DrawWithFlow(graph, EdmondsKarpGraphCanvas,
                edmondsKarpEdges, edmondsKarpIndexToNodeMap);
        }
        public void LoadBuildingPlan(BuildingPlan plan)
        {
            BuildingPlan.LoadPlan(plan);
        }
    }
}

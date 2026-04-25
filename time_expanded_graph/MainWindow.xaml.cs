using System.Diagnostics;
using System.Windows;
using time_expanded_graph.Edmonds_Karp;
using time_expanded_graph.ExpandedTimeGraph;
using time_expanded_graph.MaxFlowAlgorithms.Dinic;
using time_expanded_graph.MaxFlowAlgorithms.PushRelabel;


namespace time_expanded_graph
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var graph = GraphGenerator.GenerateGraph(6);

            var renderer = new SimpleGraphDrawing();
            renderer.Draw(graph, OriginalGraphCanvas);

            Debug.WriteLine("=== NODURI ===");
            foreach (var node in graph.Nodes)
            {
                Debug.WriteLine(node);
            }

            Debug.WriteLine("=== MUCHII ===");
            foreach (var edge in graph.Edges)
            {
                Debug.WriteLine($"{edge.From} -> {edge.To} | time={edge.TravelTime} | cap={edge.Capacity}");
            }
            Debug.WriteLine($"Număr total muchii: {graph.Edges.Count}");

            var builder = new TimeExpandedGraphBuilder();
            var expanded = builder.BuildTimeExpandedGraph(graph,4);
     
            var dinic = new DinicSolver(expanded);
            int flowDinic = dinic.ComputeMaxFlow("S*", "T*");
            var flowEdges = dinic.GetAllEdges();
            var indexMap = dinic.GetIndexToNodeMap();

            var pr = new PushRelabelSolver(expanded);
            int flowPR = pr.ComputeMaxFlow("S*", "T*");
            var flowEdgesPR = pr.GetAllEdges();
            var indexMapPR = pr.GetIndexToNodeMap();

            var ek = new EdmondsKarpSolver(expanded);
            int flowEK = ek.ComputeMaxFlow("S*", "T*");
            var flowEdgesEK = ek.GetAllEdges();
            var indexMapEK = ek.GetIndexToNodeMap();

            var expandedRenderer = new ExpandedGraphDrawing();
            expandedRenderer.Draw(expanded, ExpandedGraphCanvas);
            expandedRenderer.DrawWithFlow(expanded, DinicGraphCanvas, flowEdges, indexMap);
            expandedRenderer.DrawWithFlow(expanded, PushRelabelGraphCanvas, flowEdgesPR, indexMapPR);
            expandedRenderer.DrawWithFlow(expanded,EdmondsKarpGraphCanvas, flowEdgesEK, indexMapEK);

            //for (int t = 1; t <= 10; t++)
            //{
            //    var builder = new TimeExpandedGraphBuilder();
            //    var expanded = builder.BuildTimeExpandedGraph(graph, t);

            //    var dinic = new DinicSolver(expanded);
            //    int flow = dinic.ComputeMaxFlow("S*", "T*");

            //    Debug.WriteLine($"T={t} → flow={flow}");
            //}
            //int result = BinarySearch.FindMinimumTime(graph, 40, 10);//40 de oameni in maxim 10 unitati de timp
            //Debug.WriteLine($"Rezultat = {result}");

            //Debug.WriteLine("=== MUCHII EXTINSE (DE ASTEPTARE) ===");
            //foreach (var e in expanded.ExpandedEdges)
            //{
            //    Debug.WriteLine($"{e.From} -> {e.To} cap={e.Capacity}");
            //}
            //Debug.WriteLine("=== MUCHII SUPERSURSA SUPERDESTINATIE ===");
            //foreach (var e in expanded.ExpandedEdges)
            //{
            //    if (e.From == "S*" || e.To == "T*")
            //        Debug.WriteLine($"{e.From} -> {e.To}");
            //}

        }
    }
}
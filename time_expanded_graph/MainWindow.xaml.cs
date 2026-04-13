using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using time_expanded_graph.Edmonds_Karp;
using time_expanded_graph.ExpandedTimeGraph;
using time_expanded_graph.MaxFlowAlgorithms.Dinic;
using time_expanded_graph.MaxFlowAlgorithms.PushRelabel;


namespace time_expanded_graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            var graph = GraphGenerator.GenerateGraph(300);

            //var renderer = new SimpleGraphDrawing();
            //renderer.Draw(graph, OriginalGraphCanvas);


            Debug.WriteLine("=== NODURI ===");
            foreach (var node in graph.Nodes)
            {
                Debug.WriteLine(node);
            }

            //Debug.WriteLine("=== MUCHII ===");
            //foreach (var edge in graph.Edges)
            //{
            //    Debug.WriteLine($"{edge.From} -> {edge.To} | time={edge.TravelTime} | cap={edge.Capacity}");
            //}
            Debug.WriteLine($"Număr total muchii: {graph.Edges.Count}");

            //var builder = new TimeExpandedGraphBuilder();
            //var expanded = builder.BuildTimeExpandedGraph(graph, 25);


            //var render = new ExpandedGraphDrawing();
            //render.Draw(expanded, ExpandedGraphCanvas);

            //var stopwatch = Stopwatch.StartNew();
            //var solver = new EdmondsKarpSolver(expanded);

            //int flowEdmonds = solver.ComputeMaxFlow("S*", "T*");

            //stopwatch.Stop();

            //Debug.WriteLine($"MAX FLOW = {flowEdmonds}");
            //Debug.WriteLine($"EXECUTION TIME = {stopwatch.Elapsed.TotalMilliseconds} ms");


            //var sw = Stopwatch.StartNew();

            //var dinic = new DinicSolver(expanded);
            //int flowDinic = dinic.ComputeMaxFlow("S*", "T*");

            //sw.Stop();

            //Debug.WriteLine($"DINIC FLOW = {flowDinic}, TIME = {sw.Elapsed.TotalMilliseconds} ms");

            int peopleNeeded = 1;
            int maxTime = 50;

            var sw = Stopwatch.StartNew();

            int result = BinarySearch.FindMinimumTime(graph, peopleNeeded, maxTime);

            sw.Stop();

            Debug.WriteLine($"Timp minim = {result}");

            MessageBox.Show($"Timp minim pentru {peopleNeeded} persoane = {result}\nTimp execuție: {sw.Elapsed.TotalMilliseconds} ms");
            //var swPush = Stopwatch.StartNew();

            //var pr = new PushRelabelSolver(expanded);
            //int flowPushRelabel = pr.ComputeMaxFlow("S*", "T*");

            //swPush.Stop();

            //Debug.WriteLine($"PushRelabel FLOW = {flowPushRelabel}, TIME = {sw.Elapsed.TotalMilliseconds} ms");

            //MessageBox.Show($"Max Flow Dinic: {flowDinic}\nTime Dinic: {sw.Elapsed.TotalMilliseconds} ms\n");

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
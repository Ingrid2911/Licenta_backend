// =========================
// MainWindow.xaml.cs
// Refactorizare completă
// =========================

using System.Windows;
using time_expanded_graph.Edmonds_Karp;
using time_expanded_graph.ExpandedTimeGraph;
using time_expanded_graph.MaxFlowAlgorithms.Dinic;
using time_expanded_graph.MaxFlowAlgorithms.PushRelabel;

namespace time_expanded_graph
{
    public partial class MainWindow : Window
    {
        private SimpleGraph currentGraph;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateGraph_Click(object sender, RoutedEventArgs e)
        {
            int nodes = int.Parse(NodesInput.Text);
            int minCap = int.Parse(MinCapInput.Text);
            int maxCap = int.Parse(MaxCapInput.Text);

            currentGraph = GraphGenerator.GenerateGraph(nodes, minCap, maxCap);

            var renderer = new SimpleGraphDrawing();
            renderer.Draw(currentGraph, OriginalGraphCanvas);

            ResultText.Text = "Graf generat.";
            FlowResultsText.Text = "";
        }

        private void SolveEvacuation_Click(object sender, RoutedEventArgs e)
        {
            if (currentGraph == null)
            {
                MessageBox.Show("Generează mai întâi graful.");
                return;
            }
            if (!int.TryParse(SecondsPerTimeUnitInput.Text, out int secondsPerTimeUnit) ||
                secondsPerTimeUnit <= 0)
            {
                MessageBox.Show("Introduceți o valoare validă pentru secunde / unitate timp.");
                return;
            }

            int people = int.Parse(PeopleInput.Text);
            int maxTime = int.Parse(MaxTimeInput.Text);

            int minimumTime = BinarySearch.FindMinimumTime(
                currentGraph,
                people,
                maxTime
            );

            if (minimumTime == -1)
            {
                ResultText.Text = "Evacuarea nu este posibilă.";
                return;
            }

            var builder = new TimeExpandedGraphBuilder();
            var expanded = builder.BuildTimeExpandedGraph(currentGraph, minimumTime);

            // DINIC
            var dinic = new DinicSolver(expanded);
            int flowDinic = dinic.ComputeMaxFlow("S*", "T*");

            // PUSH-RELABEL
            var pr = new PushRelabelSolver(expanded);
            int flowPR = pr.ComputeMaxFlow("S*", "T*");

            // EDMONDS-KARP
            var ek = new EdmondsKarpSolver(expanded);
            int flowEK = ek.ComputeMaxFlow("S*", "T*");

            var renderer = new ExpandedGraphDrawing();

            renderer.Draw(expanded, ExpandedGraphCanvas);

            renderer.DrawWithFlow(
                expanded,
                DinicGraphCanvas,
                dinic.GetAllEdges(),
                dinic.GetIndexToNodeMap()
            );

            renderer.DrawWithFlow(
                expanded,
                PushRelabelGraphCanvas,
                pr.GetAllEdges(),
                pr.GetIndexToNodeMap()
            );

            renderer.DrawWithFlow(
                expanded,
                EdmondsKarpGraphCanvas,
                ek.GetAllEdges(),
                ek.GetIndexToNodeMap()
            );
            int evacuationTimeInSeconds = minimumTime * secondsPerTimeUnit;

            EvacuationTimeSecondsText.Text =
                $"Timpul minim pentru a evacua {people} persoane este de {evacuationTimeInSeconds} secunde.";

            ResultText.Text = $"Timp minim evacuare: {minimumTime} unitati de timp";

            FlowResultsText.Text =
                $"Dinic: {flowDinic} persoane     " +
                $"PushRelabel: {flowPR} persoane     " +
                $"Edmonds-Karp: {flowEK} persoane     ";
        }
    }
}
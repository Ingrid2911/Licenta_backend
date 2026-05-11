using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using time_expanded_graph.Models.Algorithms;
using time_expanded_graph.Models.Builders;
using time_expanded_graph.Models.Generators;
using time_expanded_graph.Models.Graphs;
using time_expanded_graph.Models.Utilities;

namespace time_expanded_graph.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private SimpleGraph currentGraph;
        private ExpandedGraph currentExpandedGraph;

        public SimpleGraph CurrentGraph => currentGraph;
        public ExpandedGraph CurrentExpandedGraph => currentExpandedGraph;

        public string Nodes { get; set; } = "";
        public string MinCap { get; set; } = "";
        public string MaxCap { get; set; } = "";
        public string People { get; set; } = "";
        public string MaxTime { get; set; } = "";
        public string SecondsPerTimeUnit { get; set; } = "";

        private string resultText;
        public string ResultText
        {
            get => resultText;
            set
            {
                resultText = value;
                OnPropertyChanged();
            }
        }

        private string flowResultsText;
        public string FlowResultsText
        {
            get => flowResultsText;
            set
            {
                flowResultsText = value;
                OnPropertyChanged();
            }
        }

        private string evacuationTimeSecondsText;
        public string EvacuationTimeSecondsText
        {
            get => evacuationTimeSecondsText;
            set
            {
                evacuationTimeSecondsText = value;
                OnPropertyChanged();
            }
        }

        public List<(int from, FlowEdge edge)> DinicEdges { get; private set; }
        public List<(int from, FlowEdge edge)> PushRelabelEdges { get; private set; }
        public List<(int from, FlowEdge edge)> EdmondsKarpEdges { get; private set; }
        public Dictionary<int, string> DinicIndexToNodeMap { get; private set; }

        public Dictionary<int, string> PushRelabelIndexToNodeMap { get; private set; }

        public Dictionary<int, string> EdmondsKarpIndexToNodeMap { get; private set; }

        public ICommand GenerateGraphCommand { get; }
        public ICommand SolveEvacuationCommand { get; }

        public event Action SimpleGraphGenerated;
        public event Action ExpandedGraphGenerated;
        public event Action AlgorithmsExecuted;

        public MainWindowViewModel()
        {
            ResultText = "Rezultate";
            FlowResultsText = "";
            EvacuationTimeSecondsText = "Soluție";

            GenerateGraphCommand = new RelayCommand(GenerateGraph);
            SolveEvacuationCommand = new RelayCommand(SolveEvacuation);
        }

        private void GenerateGraph()
        {
            if (!int.TryParse(Nodes, out int nodes) ||
                !int.TryParse(MinCap, out int minCap) ||
                !int.TryParse(MaxCap, out int maxCap))
            {
                MessageBox.Show("Introduceți valori numerice valide pentru noduri și capacități.");
                return;
            }

            currentGraph = GraphGenerator.GenerateGraph(nodes, minCap, maxCap);

            ResultText = "Graf generat.";
            FlowResultsText = "";
            EvacuationTimeSecondsText = "";

            SimpleGraphGenerated?.Invoke();
        }

        private void SolveEvacuation()
        {
            if (currentGraph == null)
            {
                MessageBox.Show("Generează mai întâi graful.");
                return;
            }

            if (!int.TryParse(People, out int people) ||
                !int.TryParse(MaxTime, out int maxTime) ||
                !int.TryParse(SecondsPerTimeUnit, out int secondsPerTimeUnit) ||
                secondsPerTimeUnit <= 0)
            {
                MessageBox.Show("Introduceți valori numerice valide pentru persoane, timp maxim și secunde / unitate timp.");
                return;
            }

            int minimumTime = BinarySearch.FindMinimumTime(
                currentGraph,
                people,
                maxTime
            );

            if (minimumTime == -1)
            {
                ResultText = "Evacuarea nu este posibilă.";
                return;
            }

            var builder = new TimeExpandedGraphBuilder();
            currentExpandedGraph = builder.BuildTimeExpandedGraph(currentGraph, minimumTime);

            ExpandedGraphGenerated?.Invoke();

            var dinic = new DinicSolver(currentExpandedGraph);
            int flowDinic = dinic.ComputeMaxFlow("S*", "T*");

            var pr = new PushRelabelSolver(currentExpandedGraph);
            int flowPR = pr.ComputeMaxFlow("S*", "T*");

            var ek = new EdmondsKarpSolver(currentExpandedGraph);
            int flowEK = ek.ComputeMaxFlow("S*", "T*");

            DinicEdges = dinic.GetAllEdges();
            DinicIndexToNodeMap = dinic.GetIndexToNodeMap();

            PushRelabelEdges = pr.GetAllEdges();
            PushRelabelIndexToNodeMap = pr.GetIndexToNodeMap();

            EdmondsKarpEdges = ek.GetAllEdges();
            EdmondsKarpIndexToNodeMap = ek.GetIndexToNodeMap();

            AlgorithmsExecuted?.Invoke();

            int evacuationTimeInSeconds = minimumTime * secondsPerTimeUnit;

            EvacuationTimeSecondsText =
                $"Timpul minim pentru a evacua {People} persoane este de {evacuationTimeInSeconds} secunde.";

            ResultText = $"Timp minim evacuare: {minimumTime} unitati de timp";

            FlowResultsText =
                $"Dinic: {flowDinic} persoane     " +
                $"PushRelabel: {flowPR} persoane     " +
                $"Edmonds-Karp: {flowEK} persoane     ";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

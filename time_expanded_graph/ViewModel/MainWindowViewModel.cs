using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using time_expanded_graph.Models.Algorithms;
using time_expanded_graph.Models.Builders;
using time_expanded_graph.Models.Building;
using time_expanded_graph.Models.Generators;
using time_expanded_graph.Models.Graphs;
using time_expanded_graph.Models.Utilities;

namespace time_expanded_graph.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private SimpleGraph? currentGraph;
        private ExpandedGraph? currentExpandedGraph;
        public SimpleGraph? CurrentGraph => currentGraph;
        public ExpandedGraph? CurrentExpandedGraph => currentExpandedGraph;
        public string Nodes { get; set; } = "";
        public string MinCap { get; set; } = "";
        public string MaxCap { get; set; } = "";
        public string People { get; set; } = "";
        public string MaxTime { get; set; } = "";
        public string SecondsPerTimeUnit { get; set; } = "";
        public BuildingPlan? GeneratedBuildingPlan { get; private set; }

        private string _resultText = "Rezultate";
        public string ResultText
        {
            get => _resultText;
            set { _resultText = value; OnPropertyChanged(); }
        }

        private string _flowResultsText = "";
        public string FlowResultsText
        {
            get => _flowResultsText;
            set { _flowResultsText = value; OnPropertyChanged(); }
        }

        private string _evacuationTimeSecondsText = "Soluție";
        public string EvacuationTimeSecondsText
        {
            get => _evacuationTimeSecondsText;
            set { _evacuationTimeSecondsText = value; OnPropertyChanged(); }
        }
        public List<(int from, FlowEdge edge)>? DinicEdges { get; private set; }
        public List<(int from, FlowEdge edge)>? PushRelabelEdges { get; private set; }
        public List<(int from, FlowEdge edge)>? EdmondsKarpEdges { get; private set; }
        public Dictionary<int, string>? DinicIndexToNodeMap { get; private set; }
        public Dictionary<int, string>? PushRelabelIndexToNodeMap { get; private set; }
        public Dictionary<int, string>? EdmondsKarpIndexToNodeMap { get; private set; }
        public List<string> OptimalEvacuationPath { get; private set; } = new();
        public ICommand GenerateGraphCommand { get; }
        public ICommand SolveEvacuationCommand { get; }

        public event Action? SimpleGraphGenerated;
        public event Action? ExpandedGraphGenerated;
        public event Action? AlgorithmsExecuted;
        public event Action? EvacuationPathReady;  
        public event Action? BuildingPlanGenerated;
        public MainWindowViewModel()
        {
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

            GeneratedBuildingPlan = BuildingPlanFactory.FromSimpleGraph(currentGraph);

            ResultText = "Graf generat.";
            FlowResultsText = "";
            EvacuationTimeSecondsText = "";
            OptimalEvacuationPath = new List<string>();

            SimpleGraphGenerated?.Invoke();
            BuildingPlanGenerated?.Invoke();
        }
        private void SolveEvacuation()
        {
            if (currentGraph == null)
            {
                MessageBox.Show("Generează mai întâi graful.");
                return;
            }

            RunEvacuationAlgorithms(currentGraph);
        }
        public void SolveFromBuildingPlan(
                    BuildingPlan plan,
                    BuildingSimulationParameters parameters)
        {
            var (graph, error) = plan.ToSimpleGraph();

            if (graph == null)
            {
                MessageBox.Show($"Planul nu este valid:\n{error}", "Eroare plan",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currentGraph = graph;
            SimpleGraphGenerated?.Invoke();

            RunEvacuationAlgorithms(
                graph,
                parameters.People,
                parameters.MaxTime,
                parameters.SecondsPerTimeUnit);
        }
        private void RunEvacuationAlgorithms(
            SimpleGraph graph,
            int people = -1,
            int maxTime = -1,
            int secondsPerTimeUnit = 1)
        {
            if (people < 0 && !int.TryParse(People, out people))
            {
                MessageBox.Show("Introduceți numărul de persoane.");
                return;
            }
            if (maxTime < 0 && !int.TryParse(MaxTime, out maxTime))
            {
                MessageBox.Show("Introduceți timpul maxim.");
                return;
            }
            if (secondsPerTimeUnit <= 0 &&
                !int.TryParse(SecondsPerTimeUnit, out secondsPerTimeUnit))
                secondsPerTimeUnit = 1;

            int minimumTime = BinarySearch.FindMinimumTime(graph, people, maxTime);

            if (minimumTime == -1)
            {
                ResultText = "Evacuarea nu este posibilă în timpul specificat.";
                FlowResultsText = "";
                EvacuationTimeSecondsText = "";
                return;
            }

            var builder = new TimeExpandedGraphBuilder();
            currentExpandedGraph = builder.BuildTimeExpandedGraph(graph, minimumTime);

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

            if (DinicEdges != null && DinicIndexToNodeMap != null)
            {
                OptimalEvacuationPath = EvacuationPathExtractor.ExtractPath(
                    DinicEdges, DinicIndexToNodeMap, graph);
            }

            EvacuationPathReady?.Invoke();

            int evacuationSec = minimumTime * secondsPerTimeUnit;

            ResultText = $"Timp minim evacuare: {minimumTime} unități de timp";
            EvacuationTimeSecondsText =
                $"Timpul minim pentru a evacua {people} persoane este de {evacuationSec} secunde.";
            FlowResultsText =
                $"Dinic: {flowDinic} persoane     " +
                $"PushRelabel: {flowPR} persoane     " +
                $"Edmonds-Karp: {flowEK} persoane     ";
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System;
using System.Windows;
using time_expanded_graph.ViewModels;

namespace time_expanded_graph
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            _viewModel.SimpleGraphGenerated += OnSimpleGraphGenerated;
            _viewModel.ExpandedGraphGenerated += OnExpandedGraphGenerated;
            _viewModel.AlgorithmsExecuted += OnAlgorithmsExecuted;
            _viewModel.EvacuationPathReady += OnEvacuationPathReady;

            // GraphTabs.BuildingGraphRequested se subscrie după ce controalele sunt inițializate
            Loaded += (s, e) =>
            {
                GraphTabs.BuildingGraphRequested += OnBuildingGraphRequested;
            };
        }

        private void OnSimpleGraphGenerated()
        {
            if (_viewModel.CurrentGraph != null)
                GraphTabs.DrawSimpleGraph(_viewModel.CurrentGraph);
        }

        private void OnExpandedGraphGenerated()
        {
            if (_viewModel.CurrentExpandedGraph != null)
                GraphTabs.DrawExpandedGraph(_viewModel.CurrentExpandedGraph);
        }

        private void OnAlgorithmsExecuted()
        {
            GraphTabs.DrawAlgorithmResults(
                _viewModel.CurrentExpandedGraph!,
                _viewModel.DinicEdges!,
                _viewModel.DinicIndexToNodeMap!,
                _viewModel.PushRelabelEdges!,
                _viewModel.PushRelabelIndexToNodeMap!,
                _viewModel.EdmondsKarpEdges!,
                _viewModel.EdmondsKarpIndexToNodeMap!
            );
        }

        private void OnEvacuationPathReady()
        {
            if (_viewModel.OptimalEvacuationPath.Count > 0)
                GraphTabs.HighlightEvacuationPath(_viewModel.OptimalEvacuationPath);
            else
                GraphTabs.ClearEvacuationPath();
        }

        private void OnBuildingGraphRequested(object? sender, EventArgs e)
        {
            var plan = GraphTabs.GetBuildingPlan();
            _viewModel.SolveFromBuildingPlan(plan);
        }
    }
}

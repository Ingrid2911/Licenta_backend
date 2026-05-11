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
        }

        private void OnSimpleGraphGenerated()
        {
            GraphTabs.DrawSimpleGraph(_viewModel.CurrentGraph);
        }

        private void OnExpandedGraphGenerated()
        {
            GraphTabs.DrawExpandedGraph(_viewModel.CurrentExpandedGraph);
        }

        private void OnAlgorithmsExecuted()
        {
            GraphTabs.DrawAlgorithmResults(
                _viewModel.CurrentExpandedGraph,
                _viewModel.DinicEdges,
                _viewModel.DinicIndexToNodeMap,
                _viewModel.PushRelabelEdges,
                _viewModel.PushRelabelIndexToNodeMap,
                _viewModel.EdmondsKarpEdges,
                _viewModel.EdmondsKarpIndexToNodeMap
            );
        }
    }
}
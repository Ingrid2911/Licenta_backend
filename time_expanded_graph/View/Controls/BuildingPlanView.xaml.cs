using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using time_expanded_graph.Models.Building;
using time_expanded_graph.Services;
using time_expanded_graph.View.Drawing.FloorPlan;

namespace time_expanded_graph.View.Controls
{
    public partial class BuildingPlanView : UserControl
    {
        private readonly BuildingPlan _plan;
        private readonly FloorPlanDrawing _drawing;

        public BuildingPlan Plan => _plan;

        /// <summary>
        /// Eveniment declanșat când utilizatorul apasă "⚡ Construiește Graf".
        /// GraphTabsView îl ascultă și îl propagă spre MainWindow.
        /// </summary>
        public event EventHandler? BuildGraphRequested;

        public BuildingPlanView()
        {
            InitializeComponent();

            _plan = new BuildingPlan();
            _drawing = new FloorPlanDrawing(PlanCanvas, _plan);

            _drawing.PlanChanged += UpdateStatus;

            Loaded += (s, e) => UpdateStatus();
        }

        // ─── Butoane paletă ──────────────────────────────────────────────────────

        private void BtnAddStart_Click(object sender, RoutedEventArgs e)
            => AddElementAtAuto(BuildingElementType.StartPoint);

        private void BtnAddRoom_Click(object sender, RoutedEventArgs e)
            => AddElementAtAuto(BuildingElementType.Room);

        private void BtnAddStairs_Click(object sender, RoutedEventArgs e)
            => AddElementAtAuto(BuildingElementType.Stairs);

        private void BtnAddElevator_Click(object sender, RoutedEventArgs e)
            => AddElementAtAuto(BuildingElementType.Elevator);

        private void BtnAddExit_Click(object sender, RoutedEventArgs e)
            => AddElementAtAuto(BuildingElementType.ExitDoor);

        private void AddElementAtAuto(BuildingElementType type)
        {
            double w = PlanCanvas.ActualWidth > 50 ? PlanCanvas.ActualWidth : 800;
            double h = PlanCanvas.ActualHeight > 50 ? PlanCanvas.ActualHeight : 600;

            int count = _plan.Elements.Count;
            int cols = Math.Max(1, (int)(w / 130));
            double x = 70 + (count % cols) * 130;
            double y = 70 + (count / cols) * 110;

            x = Math.Min(x, w - 60);
            y = Math.Min(y, h - 60);

            _drawing.AddElement(type, new Point(x, y));
        }

        // ─── Mod conectare ────────────────────────────────────────────────────────

        private void BtnConnectMode_Checked(object sender, RoutedEventArgs e)
        {
            _drawing.IsConnectMode = true;
            StatusText.Text = "🔗 Mod conectare: click pe primul nod, apoi pe al doilea.";
        }

        private void BtnConnectMode_Unchecked(object sender, RoutedEventArgs e)
        {
            _drawing.IsConnectMode = false;
            UpdateStatus();
        }

        // ─── Acțiuni ─────────────────────────────────────────────────────────────

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Ștergi tot planul?", "Confirmare",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (r == MessageBoxResult.Yes)
            {
                _plan.Clear();
                _drawing.Redraw();
                UpdateStatus();
            }
        }

        private void BtnBuildGraph_Click(object sender, RoutedEventArgs e)
        {
            BuildGraphRequested?.Invoke(this, EventArgs.Empty);
        }

        // ─── Vizualizare drum optim ───────────────────────────────────────────────

        public void HighlightEvacuationPath(IEnumerable<string> nodeIds)
            => _drawing.HighlightPath(nodeIds);

        public void ClearPath() => _drawing.ClearPath();

        // ─── Status ───────────────────────────────────────────────────────────────

        private void UpdateStatus()
        {
            if (_drawing.IsConnectMode) return;

            int nodes = _plan.Elements.Count;
            int conns = _plan.Connections.Count;
            int starts = _plan.Elements.Count(e => e.Type == BuildingElementType.StartPoint);
            int exits = _plan.Elements.Count(e => e.Type == BuildingElementType.ExitDoor);

            StatusText.Text = $"Noduri: {nodes}  |  Conexiuni: {conns}  |  Start: {starts}  |  Ieșiri: {exits}";
        }

        public void LoadPlan(BuildingPlan plan)
        {
            _plan.Clear();

            foreach (var element in plan.Elements)
                _plan.AddElement(element);

            foreach (var connection in plan.Connections)
                _plan.AddConnection(connection);

            _drawing.Redraw();
            UpdateStatus();
        }

        private void BtnSavePlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Salvează planul clădirii",
                Filter = "EvacPath plan (*.evacpath)|*.evacpath|JSON (*.json)|*.json",
                FileName = "plan_cladire.evacpath",
                DefaultExt = ".evacpath",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BuildingPlanStorageService.Save(_plan, dialog.FileName);

                MessageBox.Show(
                    "Planul a fost salvat cu succes.",
                    "Salvare reușită",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Planul nu a putut fi salvat.\n\n{ex.Message}",
                    "Eroare salvare",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnLoadPlan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Încarcă planul clădirii",
                Filter = "EvacPath plan (*.evacpath)|*.evacpath|JSON (*.json)|*.json",
                DefaultExt = ".evacpath",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                BuildingPlan loadedPlan = BuildingPlanStorageService.Load(dialog.FileName);

                LoadPlan(loadedPlan);

                MessageBox.Show(
                    "Planul a fost încărcat cu succes.",
                    "Încărcare reușită",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Planul nu a putut fi încărcat.\n\n{ex.Message}",
                    "Eroare încărcare",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

using System.Windows;
using time_expanded_graph.Models.Building;

namespace time_expanded_graph.View.Dialogs
{
    public partial class BuildingSimulationDialog : Window
    {
        public BuildingSimulationParameters Parameters { get; private set; }
        public BuildingSimulationDialog()
        {
            InitializeComponent();
            Parameters = new BuildingSimulationParameters
            {
                People = 10,
                MaxTime = 80,
                SecondsPerTimeUnit = 60
            };
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PeopleBox.Text, out int people) || people <= 0)
            {
                MessageBox.Show(
                    "Numărul de persoane trebuie să fie un număr pozitiv.",
                    "Date invalide",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PeopleBox.Focus();
                return;
            }
            if (!int.TryParse(MaxTimeBox.Text, out int maxTime) || maxTime <= 0)
            {
                MessageBox.Show(
                    "Timpul maxim trebuie să fie un număr pozitiv.",
                    "Date invalide",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                MaxTimeBox.Focus();
                return;
            }
            if (!int.TryParse(SecondsPerTimeUnitBox.Text, out int secondsPerTimeUnit) || secondsPerTimeUnit <= 0)
            {
                MessageBox.Show(
                    "Numărul de secunde pentru o unitate de timp trebuie să fie un număr pozitiv.",
                    "Date invalide",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                SecondsPerTimeUnitBox.Focus();
                return;
            }
            Parameters = new BuildingSimulationParameters
            {
                People = people,
                MaxTime = maxTime,
                SecondsPerTimeUnit = secondsPerTimeUnit
            };

            DialogResult = true;
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
using System.Windows;

namespace time_expanded_graph.View.Dialogs
{
    public partial class HallwaySettingsDialog : Window
    {
        public int Capacity { get; private set; }
        public int TravelTime { get; private set; }
        public HallwaySettingsDialog()
        {
            InitializeComponent();
        }
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(CapacityBox.Text, out int capacity) || capacity <= 0)
            {
                MessageBox.Show("Capacitatea trebuie să fie un număr pozitiv.");
                return;
            }

            if (!int.TryParse(TravelTimeBox.Text, out int travelTime) || travelTime <= 0)
            {
                MessageBox.Show("Timpul de traversare trebuie să fie un număr pozitiv.");
                return;
            }

            Capacity = capacity;
            TravelTime = travelTime;

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
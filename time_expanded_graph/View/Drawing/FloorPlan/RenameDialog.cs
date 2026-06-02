using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace time_expanded_graph.View.Dialogs
{
    public class RenameDialog : Window
    {
        private readonly TextBox _tb;
        public string NewName => _tb.Text;

        public RenameDialog(string current)
        {
            Title = "Redenumește";
            Width = 300;
            Height = 130;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = new SolidColorBrush(Color.FromRgb(32, 42, 52));

            var sp = new StackPanel { Margin = new Thickness(16) };

            _tb = new TextBox
            {
                Text = current,
                Height = 28,
                Background = new SolidColorBrush(Color.FromRgb(20, 28, 38)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(60, 80, 100)),
                Margin = new Thickness(0, 0, 0, 10)
            };
            _tb.SelectAll();

            var btn = new Button
            {
                Content = "OK",
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(0, 105, 92)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            btn.Click += (s, e) => { DialogResult = true; };
            _tb.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Return)
                {
                    DialogResult = true;
                }
            };

            sp.Children.Add(_tb);
            sp.Children.Add(btn);
            Content = sp;
            Loaded += (s, e) => _tb.Focus();
        }
    }
}
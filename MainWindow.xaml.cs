using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace PokeSqueeze
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TernaryNumberStart_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
    }
}

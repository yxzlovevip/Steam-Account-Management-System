using System.Windows;

namespace SteamAccountManager.Views
{
    public partial class RemarkDialog : Window
    {
        public string RemarkText => RemarkTextBox.Text;

        public RemarkDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}


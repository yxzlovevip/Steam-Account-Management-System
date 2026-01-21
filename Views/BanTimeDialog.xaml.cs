using System;
using System.Windows;

namespace SteamAccountManager.Views
{
    public partial class BanTimeDialog : Window
    {
        public int BanDays { get; private set; }

        public BanTimeDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = BanDaysComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem;
            if (selectedItem != null)
            {
                var content = selectedItem.Content?.ToString() ?? "1天";
                if (content == "永久")
                {
                    BanDays = 36500; // 100年表示永久
                }
                else
                {
                    // 提取数字
                    var daysStr = content.Replace("天", "");
                    if (int.TryParse(daysStr, out int days))
                    {
                        BanDays = days;
                    }
                    else
                    {
                        BanDays = 1;
                    }
                }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}


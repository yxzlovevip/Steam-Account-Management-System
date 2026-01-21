using SteamAccountManager.Services;
using System.Windows;

namespace SteamAccountManager.Views
{
    public partial class LogWindow : Window
    {
        private readonly LogService _logService;

        public LogWindow(LogService logService)
        {
            InitializeComponent();
            _logService = logService;
            LoadLogs();
        }

        private async void LoadLogs()
        {
            try
            {
                var logs = await _logService.GetRecentLogsAsync(200);
                LogDataGrid.ItemsSource = logs;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"加载日志失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定要清空所有日志吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _logService.ClearLogsAsync();
                    LoadLogs();
                    MessageBox.Show("日志已清空！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"清空日志失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}


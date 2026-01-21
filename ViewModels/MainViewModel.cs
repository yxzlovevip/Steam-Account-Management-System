using Microsoft.EntityFrameworkCore;
using SteamAccountManager.Data;
using SteamAccountManager.Models;
using SteamAccountManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SteamAccountManager.ViewModels
{
    /// <summary>
    /// 主窗口ViewModel
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly AppDbContext _context;
        private readonly EncryptionService _encryptionService;
        private readonly SteamService _steamService;
        private readonly LogService _logService;

        private string _searchText = string.Empty;
        private string _statusFilter = "全部";
        private string _remarkFilter = "全部";
        private bool _isPasswordVisible = false;

        public MainViewModel()
        {
            _context = new AppDbContext();
            _encryptionService = new EncryptionService();
            _steamService = new SteamService();
            _logService = new LogService(_context);

            Accounts = new ObservableCollection<AccountViewModel>();
            FilteredAccounts = new ObservableCollection<AccountViewModel>();

            // 初始化命令
            AddAccountCommand = new AsyncRelayCommand(async _ => await AddAccountAsync());
            ImportFromFileCommand = new AsyncRelayCommand(async _ => await ImportFromFileAsync());
            DeleteSelectedCommand = new AsyncRelayCommand(async _ => await DeleteSelectedAsync(), _ => HasSelectedAccounts);
            LoginAccountCommand = new AsyncRelayCommand(async param => await LoginAccountAsync(param as AccountViewModel));
            SetRemarkCommand = new AsyncRelayCommand(async _ => await SetRemarkForSelectedAsync(), _ => HasSelectedAccounts);
            SetBanTimeCommand = new AsyncRelayCommand(async _ => await SetBanTimeForSelectedAsync(), _ => HasSelectedAccounts);
            ClearBanTimeCommand = new AsyncRelayCommand(async _ => await ClearBanTimeForSelectedAsync(), _ => HasSelectedAccounts);
            ExportToFileCommand = new AsyncRelayCommand(async _ => await ExportToFileAsync(), _ => HasSelectedAccounts);
            CopyToClipboardCommand = new RelayCommand(_ => CopyToClipboard(), _ => HasSelectedAccounts);
            TogglePasswordVisibilityCommand = new RelayCommand(_ => TogglePasswordVisibility());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            RefreshCommand = new AsyncRelayCommand(async _ => await LoadAccountsAsync());
            ViewLogsCommand = new RelayCommand(_ => ViewLogs());

            // 初始化数据库并加载数据
            InitializeDatabaseAsync();
        }

        #region 属性

        public ObservableCollection<AccountViewModel> Accounts { get; }
        public ObservableCollection<AccountViewModel> FilteredAccounts { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string RemarkFilter
        {
            get => _remarkFilter;
            set
            {
                if (SetProperty(ref _remarkFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (SetProperty(ref _isPasswordVisible, value))
                {
                    foreach (var account in Accounts)
                    {
                        account.IsPasswordVisible = value;
                    }
                }
            }
        }

        public bool HasSelectedAccounts => Accounts.Any(a => a.IsSelected);

        public List<string> StatusFilterOptions { get; } = new List<string> { "全部", "正常", "封禁" };
        public List<string> RemarkFilterOptions { get; } = new List<string> { "全部", "已备注", "未备注" };

        #endregion

        #region 命令

        public ICommand AddAccountCommand { get; }
        public ICommand ImportFromFileCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand LoginAccountCommand { get; }
        public ICommand SetRemarkCommand { get; }
        public ICommand SetBanTimeCommand { get; }
        public ICommand ClearBanTimeCommand { get; }
        public ICommand ExportToFileCommand { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewLogsCommand { get; }

        #endregion

        #region 方法

        private async void InitializeDatabaseAsync()
        {
            try
            {
                // 确保数据库已创建（同步操作）
                _context.Database.EnsureCreated();
                
                // 等待一下确保数据库完全创建
                await Task.Delay(100);
                
                await LoadAccountsAsync();
                await _logService.AddLogAsync("系统", "程序启动");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化数据库失败: {ex.Message}\n\n详细信息: {ex.InnerException?.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                var accounts = await _context.SteamAccounts.ToListAsync();
                Accounts.Clear();

                foreach (var account in accounts)
                {
                    var vm = new AccountViewModel(account);
                    try
                    {
                        vm.Password = _encryptionService.Decrypt(account.EncryptedPassword);
                    }
                    catch
                    {
                        vm.Password = "解密失败";
                    }
                    Accounts.Add(vm);
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载账号失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            FilteredAccounts.Clear();

            var filtered = Accounts.AsEnumerable();

            // 搜索过滤
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(a =>
                    a.Username.ToLower().Contains(search) ||
                    (a.Remark?.ToLower().Contains(search) ?? false)
                );
            }

            // 状态过滤
            if (StatusFilter == "正常")
            {
                filtered = filtered.Where(a => a.Account.Status == 0);
            }
            else if (StatusFilter == "封禁")
            {
                filtered = filtered.Where(a => a.Account.Status == 1);
            }

            // 备注过滤
            if (RemarkFilter == "已备注")
            {
                filtered = filtered.Where(a => a.HasRemark);
            }
            else if (RemarkFilter == "未备注")
            {
                filtered = filtered.Where(a => !a.HasRemark);
            }

            foreach (var account in filtered)
            {
                FilteredAccounts.Add(account);
            }
        }

        private async Task AddAccountAsync()
        {
            var dialog = new Views.AddAccountDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var username = dialog.Username;
                    var password = dialog.Password;

                    // 检查账号是否已存在
                    if (await _context.SteamAccounts.AnyAsync(a => a.Username == username))
                    {
                        MessageBox.Show("该账号已存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var account = new SteamAccount
                    {
                        Username = username,
                        EncryptedPassword = _encryptionService.Encrypt(password)
                    };

                    _context.SteamAccounts.Add(account);
                    await _context.SaveChangesAsync();

                    await _logService.AddLogAsync("添加账号", $"添加账号: {username}", username);
                    await LoadAccountsAsync();

                    MessageBox.Show("账号添加成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"添加账号失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    await _logService.AddLogAsync("添加账号", $"添加失败: {ex.Message}", null, 1);
                }
            }
        }

        private async Task ImportFromFileAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "文本文件|*.txt",
                Title = "选择导入文件"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(dialog.FileName, Encoding.UTF8);
                    int successCount = 0;
                    int failCount = 0;

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(new[] { "----" }, StringSplitOptions.None);
                        if (parts.Length != 2)
                        {
                            failCount++;
                            continue;
                        }

                        var username = parts[0].Trim();
                        var password = parts[1].Trim();

                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            failCount++;
                            continue;
                        }

                        // 检查是否已存在
                        if (await _context.SteamAccounts.AnyAsync(a => a.Username == username))
                        {
                            failCount++;
                            continue;
                        }

                        var account = new SteamAccount
                        {
                            Username = username,
                            EncryptedPassword = _encryptionService.Encrypt(password)
                        };

                        _context.SteamAccounts.Add(account);
                        successCount++;
                    }

                    await _context.SaveChangesAsync();
                    await _logService.AddLogAsync("批量导入", $"成功: {successCount}, 失败: {failCount}");
                    await LoadAccountsAsync();

                    MessageBox.Show($"导入完成！\n成功: {successCount}\n失败或重复: {failCount}", "导入结果", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    await _logService.AddLogAsync("批量导入", $"导入失败: {ex.Message}", null, 1);
                }
            }
        }

        private async Task DeleteSelectedAsync()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            var result = MessageBox.Show($"确定要删除选中的 {selected.Count} 个账号吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                foreach (var vm in selected)
                {
                    _context.SteamAccounts.Remove(vm.Account);
                }

                await _context.SaveChangesAsync();
                await _logService.AddLogAsync("批量删除", $"删除了 {selected.Count} 个账号");
                await LoadAccountsAsync();

                MessageBox.Show("删除成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                await _logService.AddLogAsync("批量删除", $"删除失败: {ex.Message}", null, 1);
            }
        }

        private async Task LoginAccountAsync(AccountViewModel? accountVm)
        {
            if (accountVm == null)
                return;

            try
            {
                var result = MessageBox.Show($"确定要登录账号 {accountVm.Username} 吗？\n这将关闭当前运行的Steam。", "确认登录", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;

                await _steamService.LoginAccount(accountVm.Username, accountVm.Password);
                accountVm.UpdateLastLoginTime();
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync("登录账号", $"登录账号: {accountVm.Username}", accountVm.Username);
                MessageBox.Show("Steam正在启动，请稍候...", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"登录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                await _logService.AddLogAsync("登录账号", $"登录失败: {ex.Message}", accountVm.Username, 1);
            }
        }

        private async Task SetRemarkForSelectedAsync()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            var dialog = new Views.RemarkDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var remark = dialog.RemarkText;
                    foreach (var vm in selected)
                    {
                        vm.Remark = remark;
                    }

                    await _context.SaveChangesAsync();
                    await _logService.AddLogAsync("批量备注", $"为 {selected.Count} 个账号设置备注");
                    ApplyFilters();

                    MessageBox.Show("备注设置成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"设置备注失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SetBanTimeForSelectedAsync()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            var dialog = new Views.BanTimeDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var banDays = dialog.BanDays;
                    var banTime = DateTime.Now.AddDays(banDays);

                    foreach (var vm in selected)
                    {
                        vm.UpdateBanTime(banTime);
                    }

                    await _context.SaveChangesAsync();
                    await _logService.AddLogAsync("设置封禁", $"为 {selected.Count} 个账号设置封禁 {banDays} 天");
                    ApplyFilters();

                    MessageBox.Show($"已设置 {selected.Count} 个账号封禁 {banDays} 天！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"设置封禁失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ClearBanTimeForSelectedAsync()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            try
            {
                foreach (var vm in selected)
                {
                    vm.UpdateBanTime(null);
                }

                await _context.SaveChangesAsync();
                await _logService.AddLogAsync("清除封禁", $"清除 {selected.Count} 个账号的封禁时间");
                ApplyFilters();

                MessageBox.Show("封禁时间已清除！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清除失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToFileAsync()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "文本文件|*.txt",
                Title = "导出账号",
                FileName = $"账号导出_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var lines = selected.Select(vm => $"{vm.Username}----{vm.Password}");
                    await File.WriteAllLinesAsync(dialog.FileName, lines, Encoding.UTF8);

                    await _logService.AddLogAsync("导出账号", $"导出 {selected.Count} 个账号到文件");
                    MessageBox.Show($"成功导出 {selected.Count} 个账号！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CopyToClipboard()
        {
            var selected = Accounts.Where(a => a.IsSelected).ToList();
            if (selected.Count == 0)
                return;

            try
            {
                var text = string.Join(Environment.NewLine, selected.Select(vm => $"{vm.Username}----{vm.Password}"));
                Clipboard.SetText(text);

                MessageBox.Show($"已复制 {selected.Count} 个账号到剪贴板！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void SelectAll()
        {
            var allSelected = FilteredAccounts.All(a => a.IsSelected);
            foreach (var account in FilteredAccounts)
            {
                account.IsSelected = !allSelected;
            }
            OnPropertyChanged(nameof(HasSelectedAccounts));
        }

        private void ViewLogs()
        {
            var logWindow = new Views.LogWindow(_logService);
            logWindow.Show();
        }

        #endregion
    }
}


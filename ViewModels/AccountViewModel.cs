using SteamAccountManager.Models;
using System;

namespace SteamAccountManager.ViewModels
{
    /// <summary>
    /// 账号显示ViewModel
    /// </summary>
    public class AccountViewModel : ViewModelBase
    {
        private readonly SteamAccount _account;
        private bool _isSelected;
        private bool _isPasswordVisible;

        public AccountViewModel(SteamAccount account)
        {
            _account = account;
        }

        public SteamAccount Account => _account;

        public int Id => _account.Id;

        public string Username => _account.Username;

        public string Password { get; set; } = string.Empty; // 解密后的密码

        public string DisplayPassword => _isPasswordVisible ? Password : new string('●', Math.Min(Password.Length, 8));

        public string StatusText => _account.Status == 0 ? "正常" : "封禁";

        public string BanTimeText => _account.BanTime?.ToString("yyyy-MM-dd HH:mm") ?? "";

        public string Remark
        {
            get => _account.Remark ?? "";
            set
            {
                if (_account.Remark != value)
                {
                    _account.Remark = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasRemark));
                }
            }
        }

        public bool HasRemark => !string.IsNullOrWhiteSpace(_account.Remark);

        public string LastLoginTimeText => _account.LastLoginTime?.ToString("yyyy-MM-dd HH:mm") ?? "从未登录";

        public string AvailableUntilText => _account.AvailableUntil?.ToString("yyyy-MM-dd HH:mm") ?? "";

        public int? Level
        {
            get => _account.Level;
            set
            {
                if (_account.Level != value)
                {
                    _account.Level = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal? Amount
        {
            get => _account.Amount;
            set
            {
                if (_account.Amount != value)
                {
                    _account.Amount = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (SetProperty(ref _isPasswordVisible, value))
                {
                    OnPropertyChanged(nameof(DisplayPassword));
                }
            }
        }

        public void UpdateBanTime(DateTime? banTime)
        {
            _account.BanTime = banTime;
            _account.Status = banTime.HasValue ? 1 : 0;
            OnPropertyChanged(nameof(BanTimeText));
            OnPropertyChanged(nameof(StatusText));
        }

        public void UpdateLastLoginTime()
        {
            _account.LastLoginTime = DateTime.Now;
            OnPropertyChanged(nameof(LastLoginTimeText));
        }

        public void UpdateAvailableTime(int? minutes)
        {
            _account.AvailableMinutes = minutes;
            if (minutes.HasValue)
            {
                _account.AvailableUntil = DateTime.Now.AddMinutes(minutes.Value);
            }
            else
            {
                _account.AvailableUntil = null;
            }
            OnPropertyChanged(nameof(AvailableUntilText));
        }
    }
}


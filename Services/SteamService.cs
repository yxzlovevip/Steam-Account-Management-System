using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SteamAccountManager.Services
{
    /// <summary>
    /// Steam进程管理和登录服务
    /// </summary>
    public class SteamService
    {
        /// <summary>
        /// 获取Steam安装路径
        /// </summary>
        public string? GetSteamPath()
        {
            try
            {
                // 从注册表读取Steam安装路径
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key != null)
                {
                    var steamPath = key.GetValue("SteamExe") as string;
                    if (!string.IsNullOrEmpty(steamPath) && File.Exists(steamPath))
                    {
                        return steamPath;
                    }
                }

                // 尝试默认路径
                var defaultPath = @"C:\Program Files (x86)\Steam\steam.exe";
                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 关闭所有Steam进程
        /// </summary>
        public async Task<bool> KillSteamProcesses()
        {
            try
            {
                var steamProcesses = Process.GetProcessesByName("steam");
                var steamServiceProcesses = Process.GetProcessesByName("steamservice");
                var steamWebHelperProcesses = Process.GetProcessesByName("steamwebhelper");

                foreach (var process in steamProcesses.Concat(steamServiceProcesses).Concat(steamWebHelperProcesses))
                {
                    try
                    {
                        process.Kill();
                        await process.WaitForExitAsync();
                    }
                    catch
                    {
                        // 忽略单个进程关闭失败
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }

                // 等待进程完全关闭
                await Task.Delay(2000);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"关闭Steam进程失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 启动Steam并登录指定账号
        /// </summary>
        public async Task<bool> LoginAccount(string username, string password)
        {
            try
            {
                var steamPath = GetSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    throw new Exception("未找到Steam安装路径，请确保Steam已正确安装");
                }

                // 先关闭现有Steam进程
                await KillSteamProcesses();

                // 启动Steam并传递登录参数
                var startInfo = new ProcessStartInfo
                {
                    FileName = steamPath,
                    Arguments = $"-login {username} {password}",
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"启动Steam失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查Steam是否正在运行
        /// </summary>
        public bool IsSteamRunning()
        {
            return Process.GetProcessesByName("steam").Length > 0;
        }
    }
}


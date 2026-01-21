using System;
using System.Security.Cryptography;
using System.Text;

namespace SteamAccountManager.Services
{
    /// <summary>
    /// 密码加密服务（使用DPAPI）
    /// </summary>
    public class EncryptionService
    {
        /// <summary>
        /// 加密密码
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(
                    plainBytes,
                    null,
                    DataProtectionScope.CurrentUser
                );
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"加密失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 解密密码
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                byte[] plainBytes = ProtectedData.Unprotect(
                    encryptedBytes,
                    null,
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"解密失败: {ex.Message}", ex);
            }
        }
    }
}


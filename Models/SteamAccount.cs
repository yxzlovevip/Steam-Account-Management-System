using System;
using System.ComponentModel.DataAnnotations;

namespace SteamAccountManager.Models
{
    /// <summary>
    /// Steam账号实体类
    /// </summary>
    public class SteamAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string EncryptedPassword { get; set; } = string.Empty;

        /// <summary>
        /// 账号状态：0-正常，1-封禁
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// 封禁时间
        /// </summary>
        public DateTime? BanTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 可用时长（分钟）
        /// </summary>
        public int? AvailableMinutes { get; set; }

        /// <summary>
        /// 可用截止时间
        /// </summary>
        public DateTime? AvailableUntil { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Amount { get; set; }
    }
}


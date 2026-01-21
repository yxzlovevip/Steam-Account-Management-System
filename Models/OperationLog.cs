using System;
using System.ComponentModel.DataAnnotations;

namespace SteamAccountManager.Models
{
    /// <summary>
    /// 操作日志实体类
    /// </summary>
    public class OperationLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作描述
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 关联账号用户名
        /// </summary>
        [MaxLength(100)]
        public string? RelatedUsername { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 操作结果：0-成功，1-失败
        /// </summary>
        public int Result { get; set; } = 0;
    }
}


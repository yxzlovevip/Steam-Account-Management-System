using Microsoft.EntityFrameworkCore;
using SteamAccountManager.Data;
using SteamAccountManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamAccountManager.Services
{
    /// <summary>
    /// 操作日志服务
    /// </summary>
    public class LogService
    {
        private readonly AppDbContext _context;

        public LogService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 添加操作日志
        /// </summary>
        public async Task AddLogAsync(string operationType, string description, string? relatedUsername = null, int result = 0)
        {
            try
            {
                var log = new OperationLog
                {
                    OperationType = operationType,
                    Description = description,
                    RelatedUsername = relatedUsername,
                    Result = result,
                    OperationTime = DateTime.Now
                };

                _context.OperationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // 日志记录失败不影响主流程
            }
        }

        /// <summary>
        /// 获取最近的日志
        /// </summary>
        public async Task<List<OperationLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _context.OperationLogs
                .OrderByDescending(l => l.OperationTime)
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        public async Task ClearLogsAsync()
        {
            _context.OperationLogs.RemoveRange(_context.OperationLogs);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 按日期范围获取日志
        /// </summary>
        public async Task<List<OperationLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OperationLogs
                .Where(l => l.OperationTime >= startDate && l.OperationTime <= endDate)
                .OrderByDescending(l => l.OperationTime)
                .ToListAsync();
        }
    }
}


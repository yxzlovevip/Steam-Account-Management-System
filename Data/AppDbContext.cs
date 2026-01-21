using Microsoft.EntityFrameworkCore;
using SteamAccountManager.Models;
using System;
using System.IO;

namespace SteamAccountManager.Data
{
    /// <summary>
    /// 数据库上下文
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<SteamAccount> SteamAccounts { get; set; }
        public DbSet<OperationLog> OperationLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 数据库存储在 C:\Users\<用户>\AppData\Local\SteamAccountManager
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SteamAccountManager"
            );

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            var dbPath = Path.Combine(appDataPath, "accounts.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置索引
            modelBuilder.Entity<SteamAccount>()
                .HasIndex(a => a.Username);

            modelBuilder.Entity<OperationLog>()
                .HasIndex(l => l.OperationTime);
        }
    }
}


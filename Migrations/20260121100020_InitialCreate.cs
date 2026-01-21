using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamAccountManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperationType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RelatedUsername = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    OperationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Result = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    BanTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AvailableMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    AvailableUntil = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperationLogs_OperationTime",
                table: "OperationLogs",
                column: "OperationTime");

            migrationBuilder.CreateIndex(
                name: "IX_SteamAccounts_Username",
                table: "SteamAccounts",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationLogs");

            migrationBuilder.DropTable(
                name: "SteamAccounts");
        }
    }
}

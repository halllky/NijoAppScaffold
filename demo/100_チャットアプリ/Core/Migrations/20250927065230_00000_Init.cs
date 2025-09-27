using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class _00000_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "アカウント",
                columns: table => new
                {
                    アカウントID = table.Column<string>(type: "TEXT", nullable: false),
                    アカウント名 = table.Column<string>(type: "TEXT", nullable: false),
                    パスワード = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_アカウント", x => x.アカウントID);
                });

            migrationBuilder.CreateTable(
                name: "チャンネル",
                columns: table => new
                {
                    チャンネルID = table.Column<string>(type: "TEXT", nullable: false),
                    チャンネル名 = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_チャンネル", x => x.チャンネルID);
                });

            migrationBuilder.CreateTable(
                name: "メッセージ",
                columns: table => new
                {
                    メッセージSEQ = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    本文 = table.Column<string>(type: "TEXT", nullable: true),
                    記載者_アカウントID = table.Column<string>(type: "TEXT", nullable: true),
                    チャンネル_チャンネルID = table.Column<string>(type: "TEXT", nullable: true),
                    チャンネル直下か = table.Column<bool>(type: "INTEGER", nullable: true),
                    返信先メッセージSEQ = table.Column<int>(type: "INTEGER", nullable: true),
                    編集済みか = table.Column<bool>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_メッセージ", x => x.メッセージSEQ);
                    table.ForeignKey(
                        name: "FK_アカウント_メッセージ_XA91C946",
                        column: x => x.記載者_アカウントID,
                        principalTable: "アカウント",
                        principalColumn: "アカウントID");
                    table.ForeignKey(
                        name: "FK_チャンネル_メッセージ_XCC521C9",
                        column: x => x.チャンネル_チャンネルID,
                        principalTable: "チャンネル",
                        principalColumn: "チャンネルID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_メッセージ_チャンネル_チャンネルID",
                table: "メッセージ",
                column: "チャンネル_チャンネルID");

            migrationBuilder.CreateIndex(
                name: "IX_メッセージ_記載者_アカウントID",
                table: "メッセージ",
                column: "記載者_アカウントID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "メッセージ");

            migrationBuilder.DropTable(
                name: "アカウント");

            migrationBuilder.DropTable(
                name: "チャンネル");
        }
    }
}

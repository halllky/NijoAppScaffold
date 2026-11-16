using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations {
    /// <inheritdoc />
    public partial class _00007_通知 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "通知",
                columns: table => new {
                    通知ID = table.Column<string>(type: "TEXT", nullable: false),
                    宛先 = table.Column<string>(type: "TEXT", nullable: false),
                    本文 = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    リンク先 = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    既読日時 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_通知", x => x.通知ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_通知_宛先_既読日時",
                table: "通知",
                columns: new[] { "宛先", "既読日時" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "通知");
        }
    }
}

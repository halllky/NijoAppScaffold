using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations {
    /// <inheritdoc />
    public partial class _00009_添付ファイル : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "添付ファイル",
                columns: table => new {
                    識別子 = table.Column<string>(type: "TEXT", nullable: false),
                    名前 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    サイズ = table.Column<int>(type: "INTEGER", nullable: false),
                    種類 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    状態 = table.Column<int>(type: "INTEGER", nullable: false),
                    参照なし日時 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_添付ファイル", x => x.識別子);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "添付ファイル");
        }
    }
}

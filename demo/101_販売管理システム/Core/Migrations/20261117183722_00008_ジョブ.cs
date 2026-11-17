using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations {
    /// <inheritdoc />
    public partial class _00008_ジョブ : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "ジョブ",
                columns: table => new {
                    ジョブID = table.Column<string>(type: "TEXT", nullable: false),
                    種類 = table.Column<string>(type: "TEXT", nullable: false),
                    引数 = table.Column<string>(type: "TEXT", nullable: true),
                    状態 = table.Column<int>(type: "INTEGER", nullable: false),
                    起動したユーザー = table.Column<string>(type: "TEXT", nullable: true),
                    開始日時 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    終了日時 = table.Column<DateTime>(type: "TEXT", nullable: true),
                    実行したサーバー = table.Column<string>(type: "TEXT", nullable: true),
                    結果メッセージ = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_ジョブ", x => x.ジョブID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ジョブ_状態_登録日時",
                table: "ジョブ",
                columns: new[] { "状態", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "ジョブ");
        }
    }
}

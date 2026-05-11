using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo000.Migrations
{
    /// <inheritdoc />
    public partial class _00001_非主キーのユニーク制約 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "旧システム部署情報",
                columns: table => new
                {
                    旧システムコード = table.Column<string>(type: "TEXT", nullable: false),
                    名称 = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_旧システム部署情報", x => x.旧システムコード);
                });

            migrationBuilder.CreateTable(
                name: "課",
                columns: table => new
                {
                    Parent_部署ID = table.Column<int>(type: "INTEGER", nullable: false),
                    コード = table.Column<string>(type: "TEXT", nullable: false),
                    旧システムコード_旧システムコード = table.Column<string>(type: "TEXT", nullable: true),
                    課名称 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_課", x => new { x.Parent_部署ID, x.コード });
                    table.ForeignKey(
                        name: "FK_旧システム部署情報_課_X93285BC",
                        column: x => x.旧システムコード_旧システムコード,
                        principalTable: "旧システム部署情報",
                        principalColumn: "旧システムコード");
                    table.ForeignKey(
                        name: "FK_部署_課",
                        column: x => x.Parent_部署ID,
                        principalTable: "部署",
                        principalColumn: "部署ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_課_旧システムコード_旧システムコード",
                table: "課",
                column: "旧システムコード_旧システムコード",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "課");

            migrationBuilder.DropTable(
                name: "旧システム部署情報");
        }
    }
}

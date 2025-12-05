using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo000.Migrations
{
    /// <inheritdoc />
    public partial class _00001_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "部署",
                columns: table => new
                {
                    部署ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    部署名 = table.Column<string>(type: "TEXT", nullable: false),
                    事業所_事業所ID = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_部署", x => x.部署ID);
                });

            migrationBuilder.CreateTable(
                name: "社員",
                columns: table => new
                {
                    社員ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    氏名 = table.Column<string>(type: "TEXT", nullable: false),
                    所属部署_部署ID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_社員", x => x.社員ID);
                    table.ForeignKey(
                        name: "FK_部署_社員_XA26A7C4",
                        column: x => x.所属部署_部署ID,
                        principalTable: "部署",
                        principalColumn: "部署ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_社員_所属部署_部署ID",
                table: "社員",
                column: "所属部署_部署ID");

            migrationBuilder.CreateIndex(
                name: "IX_部署_事業所_事業所ID",
                table: "部署",
                column: "事業所_事業所ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "社員");

            migrationBuilder.DropTable(
                name: "部署");
        }
    }
}

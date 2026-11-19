using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations {
    /// <inheritdoc />
    public partial class _00010_入荷の添付書類 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "添付書類",
                columns: table => new {
                    Parent_入荷ID = table.Column<string>(type: "TEXT", nullable: false),
                    添付書類ID = table.Column<string>(type: "TEXT", nullable: false),
                    ファイル_識別子 = table.Column<string>(type: "TEXT", nullable: false),
                    備考 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_添付書類", x => new { x.Parent_入荷ID, x.添付書類ID });
                    table.ForeignKey(
                        name: "FK_入荷_添付書類",
                        column: x => x.Parent_入荷ID,
                        principalTable: "入荷",
                        principalColumn: "入荷ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_添付ファイル_添付書類_XF12EBE1",
                        column: x => x.ファイル_識別子,
                        principalTable: "添付ファイル",
                        principalColumn: "識別子");
                });

            migrationBuilder.CreateIndex(
                name: "IX_添付書類_ファイル_識別子",
                table: "添付書類",
                column: "ファイル_識別子");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "添付書類");
        }
    }
}

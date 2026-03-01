using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo000.Migrations
{
    /// <inheritdoc />
    public partial class _00002_論理削除 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "課名称",
                table: "課",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "係",
                columns: table => new
                {
                    Parent_Parent_部署ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Parent_コード = table.Column<string>(type: "TEXT", nullable: false),
                    連番 = table.Column<int>(type: "INTEGER", nullable: false),
                    係名称 = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_係", x => new { x.Parent_Parent_部署ID, x.Parent_コード, x.連番 });
                    table.ForeignKey(
                        name: "FK_課_係",
                        columns: x => new { x.Parent_Parent_部署ID, x.Parent_コード },
                        principalTable: "課",
                        principalColumns: new[] { "Parent_部署ID", "コード" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "部署_DELETED",
                columns: table => new
                {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    部署ID = table.Column<int>(type: "INTEGER", nullable: true),
                    部署名 = table.Column<string>(type: "TEXT", nullable: true),
                    事業所_事業所ID = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_部署_DELETED", x => x.DeletedUuid);
                });

            migrationBuilder.CreateTable(
                name: "課_DELETED",
                columns: table => new
                {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_部署ID = table.Column<int>(type: "INTEGER", nullable: true),
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
                    table.PrimaryKey("PK_課_DELETED", x => new { x.DeletedUuid, x.コード });
                    table.ForeignKey(
                        name: "FK_DELETED_部署_課",
                        column: x => x.DeletedUuid,
                        principalTable: "部署_DELETED",
                        principalColumn: "DeletedUuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "係_DELETED",
                columns: table => new
                {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_Parent_部署ID = table.Column<int>(type: "INTEGER", nullable: true),
                    Parent_コード = table.Column<string>(type: "TEXT", nullable: false),
                    連番 = table.Column<int>(type: "INTEGER", nullable: false),
                    係名称 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_係_DELETED", x => new { x.DeletedUuid, x.Parent_コード, x.連番 });
                    table.ForeignKey(
                        name: "FK_DELETED_課_係",
                        columns: x => new { x.DeletedUuid, x.Parent_コード },
                        principalTable: "課_DELETED",
                        principalColumns: new[] { "DeletedUuid", "コード" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_部署_DELETED_DELETED_AT",
                table: "部署_DELETED",
                column: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "係");

            migrationBuilder.DropTable(
                name: "係_DELETED");

            migrationBuilder.DropTable(
                name: "課_DELETED");

            migrationBuilder.DropTable(
                name: "部署_DELETED");

            migrationBuilder.AlterColumn<string>(
                name: "課名称",
                table: "課",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}

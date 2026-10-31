using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo200.Migrations {
    /// <inheritdoc />
    public partial class _00004_Update : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.CreateTable(
                name: "保守計画",
                columns: table => new {
                    PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PLAN_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_保守計画", x => x.PLAN_ID);
                });

            migrationBuilder.CreateTable(
                name: "保守計画_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PLAN_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_保守計画_DELETED", x => x.DeletedUuid);
                });

            migrationBuilder.CreateTable(
                name: "PROCESS",
                columns: table => new {
                    Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCESS_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PROCESS", x => new { x.Parent_PLAN_ID, x.PROCESS_NO });
                    table.ForeignKey(
                        name: "FK_保守計画_PROCESS",
                        column: x => x.Parent_PLAN_ID,
                        principalTable: "保守計画",
                        principalColumn: "PLAN_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROCESS_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCESS_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PROCESS_DELETED", x => new { x.DeletedUuid, x.PROCESS_NO });
                    table.ForeignKey(
                        name: "FK_DELETED_保守計画_PROCESS",
                        column: x => x.DeletedUuid,
                        principalTable: "保守計画_DELETED",
                        principalColumn: "DeletedUuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROCEDURE",
                columns: table => new {
                    Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCEDURE_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCEDURE_TEXT = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PROCEDURE", x => new { x.Parent_Parent_PLAN_ID, x.Parent_PROCESS_NO, x.PROCEDURE_NO });
                    table.ForeignKey(
                        name: "FK_PROCESS_PROCEDURE",
                        columns: x => new { x.Parent_Parent_PLAN_ID, x.Parent_PROCESS_NO },
                        principalTable: "PROCESS",
                        principalColumns: new[] { "Parent_PLAN_ID", "PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WORK_TEAM",
                columns: table => new {
                    Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    LEADER_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_WORK_TEAM", x => new { x.Parent_Parent_PLAN_ID, x.Parent_PROCESS_NO });
                    table.ForeignKey(
                        name: "FK_PROCESS_WORK_TEAM",
                        columns: x => new { x.Parent_Parent_PLAN_ID, x.Parent_PROCESS_NO },
                        principalTable: "PROCESS",
                        principalColumns: new[] { "Parent_PLAN_ID", "PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROCEDURE_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCEDURE_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    PROCEDURE_TEXT = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_PROCEDURE_DELETED", x => new { x.DeletedUuid, x.Parent_PROCESS_NO, x.PROCEDURE_NO });
                    table.ForeignKey(
                        name: "FK_DELETED_PROCESS_PROCEDURE",
                        columns: x => new { x.DeletedUuid, x.Parent_PROCESS_NO },
                        principalTable: "PROCESS_DELETED",
                        principalColumns: new[] { "DeletedUuid", "PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WORK_TEAM_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    LEADER_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_WORK_TEAM_DELETED", x => new { x.DeletedUuid, x.Parent_PROCESS_NO });
                    table.ForeignKey(
                        name: "FK_DELETED_PROCESS_WORK_TEAM",
                        columns: x => new { x.DeletedUuid, x.Parent_PROCESS_NO },
                        principalTable: "PROCESS_DELETED",
                        principalColumns: new[] { "DeletedUuid", "PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CHECK_ITEM",
                columns: table => new {
                    Parent_Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    Parent_PROCEDURE_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    CHECK_ITEM_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    CHECK_TEXT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_CHECK_ITEM", x => new { x.Parent_Parent_Parent_PLAN_ID, x.Parent_Parent_PROCESS_NO, x.Parent_PROCEDURE_NO, x.CHECK_ITEM_NO });
                    table.ForeignKey(
                        name: "FK_PROCEDURE_CHECK_ITEM",
                        columns: x => new { x.Parent_Parent_Parent_PLAN_ID, x.Parent_Parent_PROCESS_NO, x.Parent_PROCEDURE_NO },
                        principalTable: "PROCEDURE",
                        principalColumns: new[] { "Parent_Parent_PLAN_ID", "Parent_PROCESS_NO", "PROCEDURE_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WORKER",
                columns: table => new {
                    Parent_Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    WORKER_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    WORKER_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_WORKER", x => new { x.Parent_Parent_Parent_PLAN_ID, x.Parent_Parent_PROCESS_NO, x.WORKER_NO });
                    table.ForeignKey(
                        name: "FK_WORK_TEAM_WORKER",
                        columns: x => new { x.Parent_Parent_Parent_PLAN_ID, x.Parent_Parent_PROCESS_NO },
                        principalTable: "WORK_TEAM",
                        principalColumns: new[] { "Parent_Parent_PLAN_ID", "Parent_PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CHECK_ITEM_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Parent_Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    Parent_PROCEDURE_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    CHECK_ITEM_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    CHECK_TEXT = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_CHECK_ITEM_DELETED", x => new { x.DeletedUuid, x.Parent_Parent_PROCESS_NO, x.Parent_PROCEDURE_NO, x.CHECK_ITEM_NO });
                    table.ForeignKey(
                        name: "FK_DELETED_PROCEDURE_CHECK_ITEM",
                        columns: x => new { x.DeletedUuid, x.Parent_Parent_PROCESS_NO, x.Parent_PROCEDURE_NO },
                        principalTable: "PROCEDURE_DELETED",
                        principalColumns: new[] { "DeletedUuid", "Parent_PROCESS_NO", "PROCEDURE_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WORKER_DELETED",
                columns: table => new {
                    DeletedUuid = table.Column<Guid>(type: "TEXT", nullable: false),
                    Parent_Parent_Parent_PLAN_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Parent_Parent_PROCESS_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    WORKER_NO = table.Column<int>(type: "INTEGER", nullable: false),
                    WORKER_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_WORKER_DELETED", x => new { x.DeletedUuid, x.Parent_Parent_PROCESS_NO, x.WORKER_NO });
                    table.ForeignKey(
                        name: "FK_DELETED_WORK_TEAM_WORKER",
                        columns: x => new { x.DeletedUuid, x.Parent_Parent_PROCESS_NO },
                        principalTable: "WORK_TEAM_DELETED",
                        principalColumns: new[] { "DeletedUuid", "Parent_PROCESS_NO" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_保守計画_DELETED_DELETED_AT",
                table: "保守計画_DELETED",
                column: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "CHECK_ITEM");

            migrationBuilder.DropTable(
                name: "CHECK_ITEM_DELETED");

            migrationBuilder.DropTable(
                name: "WORKER");

            migrationBuilder.DropTable(
                name: "WORKER_DELETED");

            migrationBuilder.DropTable(
                name: "PROCEDURE");

            migrationBuilder.DropTable(
                name: "PROCEDURE_DELETED");

            migrationBuilder.DropTable(
                name: "WORK_TEAM");

            migrationBuilder.DropTable(
                name: "WORK_TEAM_DELETED");

            migrationBuilder.DropTable(
                name: "PROCESS");

            migrationBuilder.DropTable(
                name: "PROCESS_DELETED");

            migrationBuilder.DropTable(
                name: "保守計画");

            migrationBuilder.DropTable(
                name: "保守計画_DELETED");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo000.Migrations
{
    /// <inheritdoc />
    public partial class _00003_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "社員",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "契約種別_区分値",
                table: "社員",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "汎用マスタDbEntity区分値",
                table: "社員",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "汎用マスタDbEntity汎用種別",
                table: "社員",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "勤怠管理区分_区分値",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<string>(
                name: "勤怠管理区分_区分値",
                table: "係",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "汎用マスタDbEntity区分値",
                table: "係",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "汎用マスタDbEntity汎用種別",
                table: "係",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "汎用マスタ",
                columns: table => new
                {
                    汎用種別 = table.Column<string>(type: "TEXT", nullable: false),
                    区分値 = table.Column<string>(type: "TEXT", nullable: false),
                    表示名称 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_汎用マスタ", x => new { x.汎用種別, x.区分値 });
                });

            migrationBuilder.CreateIndex(
                name: "IX_社員_契約種別_区分値",
                table: "社員",
                column: "契約種別_区分値");

            migrationBuilder.CreateIndex(
                name: "IX_社員_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "社員",
                columns: new[] { "汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値" });

            migrationBuilder.CreateIndex(
                name: "IX_係_勤怠管理区分_区分値",
                table: "係",
                column: "勤怠管理区分_区分値");

            migrationBuilder.CreateIndex(
                name: "IX_係_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "係",
                columns: new[] { "汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値" });

            migrationBuilder.AddForeignKey(
                name: "FK_係_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "係",
                columns: new[] { "汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値" },
                principalTable: "汎用マスタ",
                principalColumns: new[] { "汎用種別", "区分値" });

            migrationBuilder.AddForeignKey(
                name: "FK_社員_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "社員",
                columns: new[] { "汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値" },
                principalTable: "汎用マスタ",
                principalColumns: new[] { "汎用種別", "区分値" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_係_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "係");

            migrationBuilder.DropForeignKey(
                name: "FK_社員_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "社員");

            migrationBuilder.DropTable(
                name: "汎用マスタ");

            migrationBuilder.DropIndex(
                name: "IX_社員_契約種別_区分値",
                table: "社員");

            migrationBuilder.DropIndex(
                name: "IX_社員_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "社員");

            migrationBuilder.DropIndex(
                name: "IX_係_勤怠管理区分_区分値",
                table: "係");

            migrationBuilder.DropIndex(
                name: "IX_係_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値",
                table: "係");

            migrationBuilder.DropColumn(
                name: "契約種別_区分値",
                table: "社員");

            migrationBuilder.DropColumn(
                name: "汎用マスタDbEntity区分値",
                table: "社員");

            migrationBuilder.DropColumn(
                name: "汎用マスタDbEntity汎用種別",
                table: "社員");

            migrationBuilder.DropColumn(
                name: "勤怠管理区分_区分値",
                table: "係_DELETED");

            migrationBuilder.DropColumn(
                name: "勤怠管理区分_区分値",
                table: "係");

            migrationBuilder.DropColumn(
                name: "汎用マスタDbEntity区分値",
                table: "係");

            migrationBuilder.DropColumn(
                name: "汎用マスタDbEntity汎用種別",
                table: "係");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "社員",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "社員",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "係_DELETED",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "UpdateUser",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "CreateUser",
                table: "係",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);
        }
    }
}

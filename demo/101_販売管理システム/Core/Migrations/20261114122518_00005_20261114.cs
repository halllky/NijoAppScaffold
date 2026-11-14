using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations {
    /// <inheritdoc />
    public partial class _00005_20261114 : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "最終ログイン日時",
                table: "セッション",
                newName: "最終操作日時");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.RenameColumn(
                name: "最終操作日時",
                table: "セッション",
                newName: "最終ログイン日時");
        }
    }
}

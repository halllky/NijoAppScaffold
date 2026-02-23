using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations
{
    /// <inheritdoc />
    public partial class _00003_ユニーク制約の例 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_商品_外部システム側ID",
                table: "商品",
                column: "外部システム側ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_商品_外部システム側ID",
                table: "商品");
        }
    }
}

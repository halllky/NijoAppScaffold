using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo101.Migrations
{
    /// <inheritdoc />
    public partial class _00001_販売管理システム : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "従業員",
                columns: table => new
                {
                    従業員番号 = table.Column<string>(type: "TEXT", nullable: false),
                    氏名 = table.Column<string>(type: "TEXT", nullable: false),
                    パスワード = table.Column<byte[]>(type: "BLOB", nullable: false),
                    SALT = table.Column<byte[]>(type: "BLOB", nullable: false),
                    入荷担当 = table.Column<bool>(type: "INTEGER", nullable: false),
                    販売担当 = table.Column<bool>(type: "INTEGER", nullable: false),
                    システム管理者 = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_従業員", x => x.従業員番号);
                });

            migrationBuilder.CreateTable(
                name: "商品",
                columns: table => new
                {
                    商品SEQ = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    外部システム側ID = table.Column<string>(type: "TEXT", nullable: false),
                    商品名 = table.Column<string>(type: "TEXT", nullable: false),
                    売値単価_税抜 = table.Column<decimal>(type: "TEXT", nullable: true),
                    消費税区分 = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_商品", x => x.商品SEQ);
                });

            migrationBuilder.CreateTable(
                name: "セッション",
                columns: table => new
                {
                    セッションキー = table.Column<string>(type: "TEXT", nullable: false),
                    ユーザ_従業員番号 = table.Column<string>(type: "TEXT", nullable: true),
                    最終ログイン日時 = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_セッション", x => x.セッションキー);
                    table.ForeignKey(
                        name: "FK_従業員_セッション_X809B944",
                        column: x => x.ユーザ_従業員番号,
                        principalTable: "従業員",
                        principalColumn: "従業員番号");
                });

            migrationBuilder.CreateTable(
                name: "入荷",
                columns: table => new
                {
                    入荷ID = table.Column<string>(type: "TEXT", nullable: false),
                    入荷日時 = table.Column<DateTime>(type: "TEXT", nullable: false),
                    担当者_従業員番号 = table.Column<string>(type: "TEXT", nullable: true),
                    備考 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_入荷", x => x.入荷ID);
                    table.ForeignKey(
                        name: "FK_従業員_入荷_XB332A41",
                        column: x => x.担当者_従業員番号,
                        principalTable: "従業員",
                        principalColumn: "従業員番号");
                });

            migrationBuilder.CreateTable(
                name: "売上",
                columns: table => new
                {
                    売上SEQ = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    売上日時 = table.Column<DateTime>(type: "TEXT", nullable: false),
                    担当者_従業員番号 = table.Column<string>(type: "TEXT", nullable: true),
                    備考 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_売上", x => x.売上SEQ);
                    table.ForeignKey(
                        name: "FK_従業員_売上_XB332A41",
                        column: x => x.担当者_従業員番号,
                        principalTable: "従業員",
                        principalColumn: "従業員番号");
                });

            migrationBuilder.CreateTable(
                name: "在庫調整",
                columns: table => new
                {
                    在庫調整ID = table.Column<string>(type: "TEXT", nullable: false),
                    在庫調整日時 = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    担当者_従業員番号 = table.Column<string>(type: "TEXT", nullable: true),
                    商品_商品SEQ = table.Column<int>(type: "INTEGER", nullable: true),
                    増減数 = table.Column<int>(type: "INTEGER", nullable: true),
                    絶対数 = table.Column<int>(type: "INTEGER", nullable: true),
                    在庫調整理由 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_在庫調整", x => x.在庫調整ID);
                    table.ForeignKey(
                        name: "FK_商品_在庫調整_X84D8979",
                        column: x => x.商品_商品SEQ,
                        principalTable: "商品",
                        principalColumn: "商品SEQ");
                    table.ForeignKey(
                        name: "FK_従業員_在庫調整_XB332A41",
                        column: x => x.担当者_従業員番号,
                        principalTable: "従業員",
                        principalColumn: "従業員番号");
                });

            migrationBuilder.CreateTable(
                name: "入荷明細",
                columns: table => new
                {
                    入荷明細ID = table.Column<string>(type: "TEXT", nullable: false),
                    入荷_入荷ID = table.Column<string>(type: "TEXT", nullable: true),
                    在庫調整 = table.Column<string>(type: "TEXT", nullable: true),
                    商品_商品SEQ = table.Column<int>(type: "INTEGER", nullable: true),
                    仕入単価_税抜 = table.Column<decimal>(type: "TEXT", nullable: true),
                    消費税区分 = table.Column<int>(type: "INTEGER", nullable: true),
                    入荷数量 = table.Column<int>(type: "INTEGER", nullable: false),
                    残数量 = table.Column<int>(type: "INTEGER", nullable: false),
                    備考 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_入荷明細", x => x.入荷明細ID);
                    table.ForeignKey(
                        name: "FK_入荷_入荷明細_X4913180",
                        column: x => x.入荷_入荷ID,
                        principalTable: "入荷",
                        principalColumn: "入荷ID");
                    table.ForeignKey(
                        name: "FK_商品_入荷明細_X84D8979",
                        column: x => x.商品_商品SEQ,
                        principalTable: "商品",
                        principalColumn: "商品SEQ");
                });

            migrationBuilder.CreateTable(
                name: "売上明細",
                columns: table => new
                {
                    Parent_売上SEQ = table.Column<int>(type: "INTEGER", nullable: false),
                    明細ID = table.Column<string>(type: "TEXT", nullable: false),
                    商品_商品SEQ = table.Column<int>(type: "INTEGER", nullable: true),
                    区分 = table.Column<int>(type: "INTEGER", nullable: false),
                    売上数量 = table.Column<int>(type: "INTEGER", nullable: false),
                    売上総額_税込 = table.Column<decimal>(type: "TEXT", nullable: false),
                    備考 = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_売上明細", x => new { x.Parent_売上SEQ, x.明細ID });
                    table.ForeignKey(
                        name: "FK_商品_売上明細_X84D8979",
                        column: x => x.商品_商品SEQ,
                        principalTable: "商品",
                        principalColumn: "商品SEQ");
                    table.ForeignKey(
                        name: "FK_売上_売上明細",
                        column: x => x.Parent_売上SEQ,
                        principalTable: "売上",
                        principalColumn: "売上SEQ",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "在庫調整引当明細",
                columns: table => new
                {
                    Parent_在庫調整ID = table.Column<string>(type: "TEXT", nullable: false),
                    入荷明細_入荷明細ID = table.Column<string>(type: "TEXT", nullable: false),
                    引当数 = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_在庫調整引当明細", x => new { x.Parent_在庫調整ID, x.入荷明細_入荷明細ID });
                    table.ForeignKey(
                        name: "FK_入荷明細_在庫調整引当明細_X726F99E",
                        column: x => x.入荷明細_入荷明細ID,
                        principalTable: "入荷明細",
                        principalColumn: "入荷明細ID");
                    table.ForeignKey(
                        name: "FK_在庫調整_在庫調整引当明細",
                        column: x => x.Parent_在庫調整ID,
                        principalTable: "在庫調整",
                        principalColumn: "在庫調整ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "引当明細",
                columns: table => new
                {
                    Parent_Parent_売上SEQ = table.Column<int>(type: "INTEGER", nullable: false),
                    Parent_明細ID = table.Column<string>(type: "TEXT", nullable: false),
                    入荷_入荷明細ID = table.Column<string>(type: "TEXT", nullable: false),
                    引当数量 = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_引当明細", x => new { x.Parent_Parent_売上SEQ, x.Parent_明細ID, x.入荷_入荷明細ID });
                    table.ForeignKey(
                        name: "FK_入荷明細_引当明細_X4913180",
                        column: x => x.入荷_入荷明細ID,
                        principalTable: "入荷明細",
                        principalColumn: "入荷明細ID");
                    table.ForeignKey(
                        name: "FK_売上明細_引当明細",
                        columns: x => new { x.Parent_Parent_売上SEQ, x.Parent_明細ID },
                        principalTable: "売上明細",
                        principalColumns: new[] { "Parent_売上SEQ", "明細ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_セッション_ユーザ_従業員番号",
                table: "セッション",
                column: "ユーザ_従業員番号");

            migrationBuilder.CreateIndex(
                name: "IX_引当明細_入荷_入荷明細ID",
                table: "引当明細",
                column: "入荷_入荷明細ID");

            migrationBuilder.CreateIndex(
                name: "IX_在庫調整_商品_商品SEQ",
                table: "在庫調整",
                column: "商品_商品SEQ");

            migrationBuilder.CreateIndex(
                name: "IX_在庫調整_担当者_従業員番号",
                table: "在庫調整",
                column: "担当者_従業員番号");

            migrationBuilder.CreateIndex(
                name: "IX_在庫調整引当明細_入荷明細_入荷明細ID",
                table: "在庫調整引当明細",
                column: "入荷明細_入荷明細ID");

            migrationBuilder.CreateIndex(
                name: "IX_入荷_担当者_従業員番号",
                table: "入荷",
                column: "担当者_従業員番号");

            migrationBuilder.CreateIndex(
                name: "IX_入荷明細_商品_商品SEQ",
                table: "入荷明細",
                column: "商品_商品SEQ");

            migrationBuilder.CreateIndex(
                name: "IX_入荷明細_入荷_入荷ID",
                table: "入荷明細",
                column: "入荷_入荷ID");

            migrationBuilder.CreateIndex(
                name: "IX_売上_担当者_従業員番号",
                table: "売上",
                column: "担当者_従業員番号");

            migrationBuilder.CreateIndex(
                name: "IX_売上明細_商品_商品SEQ",
                table: "売上明細",
                column: "商品_商品SEQ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "セッション");

            migrationBuilder.DropTable(
                name: "引当明細");

            migrationBuilder.DropTable(
                name: "在庫調整引当明細");

            migrationBuilder.DropTable(
                name: "売上明細");

            migrationBuilder.DropTable(
                name: "入荷明細");

            migrationBuilder.DropTable(
                name: "在庫調整");

            migrationBuilder.DropTable(
                name: "売上");

            migrationBuilder.DropTable(
                name: "入荷");

            migrationBuilder.DropTable(
                name: "商品");

            migrationBuilder.DropTable(
                name: "従業員");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo200.Migrations
{
    /// <inheritdoc />
    public partial class _00001_Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BUSHO",
                columns: table => new
                {
                    BUSHO_CD = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    BUSHO_NAME = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BUSHO", x => x.BUSHO_CD);
                });

            migrationBuilder.CreateTable(
                name: "EMPLOYEE",
                columns: table => new
                {
                    ID = table.Column<string>(type: "TEXT", nullable: false),
                    NAME = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    NAME_KANA = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    TAISHOKU = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPLOYEE", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "患者マスタ",
                columns: table => new
                {
                    CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CUSTOMER_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CUSTOMER_KANA = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    BIRTH_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    GENDER = table.Column<int>(type: "INTEGER", nullable: true),
                    EMAIL = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PHONE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_患者マスタ", x => x.CUSTOMER_ID);
                });

            migrationBuilder.CreateTable(
                name: "機器分類マスタ",
                columns: table => new
                {
                    CATEGORY_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CATEGORY_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_機器分類マスタ", x => x.CATEGORY_ID);
                });

            migrationBuilder.CreateTable(
                name: "供給業者マスタ",
                columns: table => new
                {
                    SUPPLIER_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SUPPLIER_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CONTACT_PERSON = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PHONE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EMAIL = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_供給業者マスタ", x => x.SUPPLIER_ID);
                });

            migrationBuilder.CreateTable(
                name: "AUTHORITY",
                columns: table => new
                {
                    Parent_ID = table.Column<string>(type: "TEXT", nullable: false),
                    AUTHORITY_LEVEL = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUTHORITY", x => new { x.Parent_ID, x.AUTHORITY_LEVEL });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_AUTHORITY",
                        column: x => x.Parent_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SHOZOKU",
                columns: table => new
                {
                    Parent_ID = table.Column<string>(type: "TEXT", nullable: false),
                    NENDO = table.Column<int>(type: "INTEGER", nullable: false),
                    診療科_BUSHO_CD = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SHOZOKU", x => new { x.Parent_ID, x.NENDO });
                    table.ForeignKey(
                        name: "FK_BUSHO_SHOZOKU_X6518C1A",
                        column: x => x.診療科_BUSHO_CD,
                        principalTable: "BUSHO",
                        principalColumn: "BUSHO_CD");
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_SHOZOKU",
                        column: x => x.Parent_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "医療従事者プロフィール",
                columns: table => new
                {
                    医療従事者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    PHOTO_URL = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SELF_INTRODUCTION = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    SPECIALTY = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_医療従事者プロフィール", x => x.医療従事者_ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_医療従事者プロフィール_X30E339A",
                        column: x => x.医療従事者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "診療科マスタ",
                columns: table => new
                {
                    STORE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    STORE_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PHONE = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    科長_ID = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_診療科マスタ", x => x.STORE_ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_診療科マスタ_X170D2D7",
                        column: x => x.科長_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "保管庫マスタ",
                columns: table => new
                {
                    WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    WAREHOUSE_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    管理責任者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_保管庫マスタ", x => x.WAREHOUSE_ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_保管庫マスタ_XD847631",
                        column: x => x.管理責任者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "CUSTOMER_ADDRESS",
                columns: table => new
                {
                    Parent_CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    POSTAL_CODE = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    PREFECTURE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CITY = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ADDRESS_LINE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUSTOMER_ADDRESS", x => x.Parent_CUSTOMER_ID);
                    table.ForeignKey(
                        name: "FK_患者マスタ_CUSTOMER_ADDRESS",
                        column: x => x.Parent_CUSTOMER_ID,
                        principalTable: "患者マスタ",
                        principalColumn: "CUSTOMER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MEMBERSHIP",
                columns: table => new
                {
                    Parent_CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RANK = table.Column<int>(type: "INTEGER", nullable: true),
                    JOIN_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    LAST_VISIT = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEMBERSHIP", x => x.Parent_CUSTOMER_ID);
                    table.ForeignKey(
                        name: "FK_患者マスタ_MEMBERSHIP",
                        column: x => x.Parent_CUSTOMER_ID,
                        principalTable: "患者マスタ",
                        principalColumn: "CUSTOMER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "予約",
                columns: table => new
                {
                    RESERVATION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RESERVATION_DATETIME = table.Column<DateTime>(type: "TEXT", nullable: false),
                    患者_CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RESERVATION_TYPE = table.Column<int>(type: "INTEGER", nullable: true),
                    RESERVATION_NOTE = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    担当医_ID = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_予約", x => x.RESERVATION_ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_予約_XDECF289",
                        column: x => x.担当医_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_患者マスタ_予約_XFB576B4",
                        column: x => x.患者_CUSTOMER_ID,
                        principalTable: "患者マスタ",
                        principalColumn: "CUSTOMER_ID");
                });

            migrationBuilder.CreateTable(
                name: "医療機器マスタ",
                columns: table => new
                {
                    PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PRODUCT_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PRICE = table.Column<int>(type: "INTEGER", nullable: false),
                    機器分類_CATEGORY_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    供給業者_SUPPLIER_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_医療機器マスタ", x => x.PRODUCT_ID);
                    table.ForeignKey(
                        name: "FK_供給業者マスタ_医療機器マスタ_X89DCA22",
                        column: x => x.供給業者_SUPPLIER_ID,
                        principalTable: "供給業者マスタ",
                        principalColumn: "SUPPLIER_ID");
                    table.ForeignKey(
                        name: "FK_機器分類マスタ_医療機器マスタ_X7D2BEB8",
                        column: x => x.機器分類_CATEGORY_ID,
                        principalTable: "機器分類マスタ",
                        principalColumn: "CATEGORY_ID");
                });

            migrationBuilder.CreateTable(
                name: "QUALIFICATIONS",
                columns: table => new
                {
                    Parent_医療従事者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    QUALIFICATION_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ACQUISITION_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUALIFICATIONS", x => new { x.Parent_医療従事者_ID, x.QUALIFICATION_NAME });
                    table.ForeignKey(
                        name: "FK_医療従事者プロフィール_QUALIFICATIONS",
                        column: x => x.Parent_医療従事者_ID,
                        principalTable: "医療従事者プロフィール",
                        principalColumn: "医療従事者_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BUSINESS_HOURS",
                columns: table => new
                {
                    Parent_STORE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    OPENING_TIME = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    CLOSING_TIME = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BUSINESS_HOURS", x => x.Parent_STORE_ID);
                    table.ForeignKey(
                        name: "FK_診療科マスタ_BUSINESS_HOURS",
                        column: x => x.Parent_STORE_ID,
                        principalTable: "診療科マスタ",
                        principalColumn: "STORE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STORE_ADDRESS",
                columns: table => new
                {
                    Parent_STORE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    POSTAL_CODE = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    PREFECTURE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CITY = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ADDRESS_LINE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STORE_ADDRESS", x => x.Parent_STORE_ID);
                    table.ForeignKey(
                        name: "FK_診療科マスタ_STORE_ADDRESS",
                        column: x => x.Parent_STORE_ID,
                        principalTable: "診療科マスタ",
                        principalColumn: "STORE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "勤務スケジュール",
                columns: table => new
                {
                    医療従事者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    診療科_STORE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DATE = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    START_TIME = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    END_TIME = table.Column<string>(type: "TEXT", maxLength: 5, nullable: true),
                    BREAK_TIME = table.Column<int>(type: "INTEGER", nullable: true),
                    REMARKS = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_勤務スケジュール", x => new { x.医療従事者_ID, x.診療科_STORE_ID, x.DATE });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_勤務スケジュール_X30E339A",
                        column: x => x.医療従事者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_診療科マスタ_勤務スケジュール_X6518C1A",
                        column: x => x.診療科_STORE_ID,
                        principalTable: "診療科マスタ",
                        principalColumn: "STORE_ID");
                });

            migrationBuilder.CreateTable(
                name: "診療履歴",
                columns: table => new
                {
                    ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ORDER_DATE = table.Column<DateTime>(type: "TEXT", nullable: false),
                    患者_CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    診療科_STORE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    担当医_ID = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_診療履歴", x => x.ORDER_ID);
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_診療履歴_XDECF289",
                        column: x => x.担当医_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_患者マスタ_診療履歴_XFB576B4",
                        column: x => x.患者_CUSTOMER_ID,
                        principalTable: "患者マスタ",
                        principalColumn: "CUSTOMER_ID");
                    table.ForeignKey(
                        name: "FK_診療科マスタ_診療履歴_X6518C1A",
                        column: x => x.診療科_STORE_ID,
                        principalTable: "診療科マスタ",
                        principalColumn: "STORE_ID");
                });

            migrationBuilder.CreateTable(
                name: "WAREHOUSE_ADDRESS",
                columns: table => new
                {
                    Parent_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    POSTAL_CODE = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    PREFECTURE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CITY = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ADDRESS_LINE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WAREHOUSE_ADDRESS", x => x.Parent_WAREHOUSE_ID);
                    table.ForeignKey(
                        name: "FK_保管庫マスタ_WAREHOUSE_ADDRESS",
                        column: x => x.Parent_WAREHOUSE_ID,
                        principalTable: "保管庫マスタ",
                        principalColumn: "WAREHOUSE_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "POINT_HISTORY",
                columns: table => new
                {
                    Parent_Parent_CUSTOMER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HISTORY_ID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    POINTS = table.Column<int>(type: "INTEGER", nullable: true),
                    REASON = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POINT_HISTORY", x => new { x.Parent_Parent_CUSTOMER_ID, x.HISTORY_ID });
                    table.ForeignKey(
                        name: "FK_MEMBERSHIP_POINT_HISTORY",
                        column: x => x.Parent_Parent_CUSTOMER_ID,
                        principalTable: "MEMBERSHIP",
                        principalColumn: "Parent_CUSTOMER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "診察記録",
                columns: table => new
                {
                    予約_RESERVATION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    START_TIME = table.Column<DateTime>(type: "TEXT", nullable: true),
                    END_TIME = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TEMPERATURE = table.Column<decimal>(type: "TEXT", nullable: true),
                    BLOOD_PRESSURE_HIGH = table.Column<int>(type: "INTEGER", nullable: true),
                    BLOOD_PRESSURE_LOW = table.Column<int>(type: "INTEGER", nullable: true),
                    NOTE = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_診察記録", x => x.予約_RESERVATION_ID);
                    table.ForeignKey(
                        name: "FK_予約_診察記録_XF01C7D4",
                        column: x => x.予約_RESERVATION_ID,
                        principalTable: "予約",
                        principalColumn: "RESERVATION_ID");
                });

            migrationBuilder.CreateTable(
                name: "INVENTORY",
                columns: table => new
                {
                    Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    STOCK_QUANTITY = table.Column<int>(type: "INTEGER", nullable: true),
                    INVENTORY_DATE = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INVENTORY", x => new { x.Parent_PRODUCT_ID, x.保管庫_WAREHOUSE_ID });
                    table.ForeignKey(
                        name: "FK_保管庫マスタ_INVENTORY_X48C69C7",
                        column: x => x.保管庫_WAREHOUSE_ID,
                        principalTable: "保管庫マスタ",
                        principalColumn: "WAREHOUSE_ID");
                    table.ForeignKey(
                        name: "FK_医療機器マスタ_INVENTORY",
                        column: x => x.Parent_PRODUCT_ID,
                        principalTable: "医療機器マスタ",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_DETAIL",
                columns: table => new
                {
                    Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_DETAIL", x => x.Parent_PRODUCT_ID);
                    table.ForeignKey(
                        name: "FK_医療機器マスタ_PRODUCT_DETAIL",
                        column: x => x.Parent_PRODUCT_ID,
                        principalTable: "医療機器マスタ",
                        principalColumn: "PRODUCT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ORDER_DETAILS",
                columns: table => new
                {
                    Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    医療機器_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    QUANTITY = table.Column<int>(type: "INTEGER", nullable: false),
                    UNIT_PRICE = table.Column<int>(type: "INTEGER", nullable: false),
                    SUBTOTAL = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORDER_DETAILS", x => new { x.Parent_ORDER_ID, x.医療機器_PRODUCT_ID });
                    table.ForeignKey(
                        name: "FK_医療機器マスタ_ORDER_DETAILS_X5D5C85D",
                        column: x => x.医療機器_PRODUCT_ID,
                        principalTable: "医療機器マスタ",
                        principalColumn: "PRODUCT_ID");
                    table.ForeignKey(
                        name: "FK_診療履歴_ORDER_DETAILS",
                        column: x => x.Parent_ORDER_ID,
                        principalTable: "診療履歴",
                        principalColumn: "ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PAYMENT_INFO",
                columns: table => new
                {
                    Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PAYMENT_TYPE = table.Column<int>(type: "INTEGER", nullable: false),
                    PAYMENT_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    PAYMENT_STATUS = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_INFO", x => x.Parent_ORDER_ID);
                    table.ForeignKey(
                        name: "FK_診療履歴_PAYMENT_INFO",
                        column: x => x.Parent_ORDER_ID,
                        principalTable: "診療履歴",
                        principalColumn: "ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SHIPPING_INFO",
                columns: table => new
                {
                    Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SHIPPING_METHOD = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SHIPPING_INFO", x => x.Parent_ORDER_ID);
                    table.ForeignKey(
                        name: "FK_診療履歴_SHIPPING_INFO",
                        column: x => x.Parent_ORDER_ID,
                        principalTable: "診療履歴",
                        principalColumn: "ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRESCRIPTIONS",
                columns: table => new
                {
                    Parent_予約_RESERVATION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MEDICINE_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MEDICINE_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DOSAGE = table.Column<int>(type: "INTEGER", nullable: true),
                    USAGE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    DAYS = table.Column<int>(type: "INTEGER", nullable: true),
                    REMARKS = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRESCRIPTIONS", x => new { x.Parent_予約_RESERVATION_ID, x.MEDICINE_ID });
                    table.ForeignKey(
                        name: "FK_診察記録_PRESCRIPTIONS",
                        column: x => x.Parent_予約_RESERVATION_ID,
                        principalTable: "診察記録",
                        principalColumn: "予約_RESERVATION_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "STOCK_HISTORY",
                columns: table => new
                {
                    Parent_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    HISTORY_ID = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    CHANGE_DATE = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PREVIOUS_QUANTITY = table.Column<int>(type: "INTEGER", nullable: true),
                    CURRENT_QUANTITY = table.Column<int>(type: "INTEGER", nullable: true),
                    担当者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STOCK_HISTORY", x => new { x.Parent_Parent_PRODUCT_ID, x.Parent_保管庫_WAREHOUSE_ID, x.HISTORY_ID });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_STOCK_HISTORY_XB332A41",
                        column: x => x.担当者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_INVENTORY_STOCK_HISTORY",
                        columns: x => new { x.Parent_Parent_PRODUCT_ID, x.Parent_保管庫_WAREHOUSE_ID },
                        principalTable: "INVENTORY",
                        principalColumns: new[] { "Parent_PRODUCT_ID", "保管庫_WAREHOUSE_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "機器点検報告",
                columns: table => new
                {
                    対象機器_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象機器_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SURVEY_DATE = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    点検担当者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    ACTUAL_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    INVENTORY_DIFF = table.Column<int>(type: "INTEGER", nullable: true),
                    SURVEY_NOTE = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PHOTO_URL = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_機器点検報告", x => new { x.対象機器_Parent_PRODUCT_ID, x.対象機器_保管庫_WAREHOUSE_ID });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_機器点検報告_XC084C68",
                        column: x => x.点検担当者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_INVENTORY_機器点検報告_X62E8D1C",
                        columns: x => new { x.対象機器_Parent_PRODUCT_ID, x.対象機器_保管庫_WAREHOUSE_ID },
                        principalTable: "INVENTORY",
                        principalColumns: new[] { "Parent_PRODUCT_ID", "保管庫_WAREHOUSE_ID" });
                });

            migrationBuilder.CreateTable(
                name: "ACCESSORIES",
                columns: table => new
                {
                    Parent_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ACCESSORY_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ACCESSORY_NAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    QUANTITY = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCESSORIES", x => new { x.Parent_Parent_PRODUCT_ID, x.ACCESSORY_ID });
                    table.ForeignKey(
                        name: "FK_PRODUCT_DETAIL_ACCESSORIES",
                        column: x => x.Parent_Parent_PRODUCT_ID,
                        principalTable: "PRODUCT_DETAIL",
                        principalColumn: "Parent_PRODUCT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PRODUCT_SPEC",
                columns: table => new
                {
                    Parent_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WEIGHT = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRODUCT_SPEC", x => x.Parent_Parent_PRODUCT_ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_DETAIL_PRODUCT_SPEC",
                        column: x => x.Parent_Parent_PRODUCT_ID,
                        principalTable: "PRODUCT_DETAIL",
                        principalColumn: "Parent_PRODUCT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DISCOUNT_INFO",
                columns: table => new
                {
                    Parent_Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_医療機器_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DISCOUNT_CODE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    DISCOUNT_RATE = table.Column<decimal>(type: "TEXT", nullable: true),
                    DISCOUNT_AMOUNT = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DISCOUNT_INFO", x => new { x.Parent_Parent_ORDER_ID, x.Parent_医療機器_PRODUCT_ID });
                    table.ForeignKey(
                        name: "FK_ORDER_DETAILS_DISCOUNT_INFO",
                        columns: x => new { x.Parent_Parent_ORDER_ID, x.Parent_医療機器_PRODUCT_ID },
                        principalTable: "ORDER_DETAILS",
                        principalColumns: new[] { "Parent_ORDER_ID", "医療機器_PRODUCT_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CARD_INFO",
                columns: table => new
                {
                    Parent_Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CARD_TYPE = table.Column<int>(type: "INTEGER", nullable: true),
                    LAST_FOUR_DIGITS = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    EXPIRY_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARD_INFO", x => x.Parent_Parent_ORDER_ID);
                    table.ForeignKey(
                        name: "FK_PAYMENT_INFO_CARD_INFO",
                        column: x => x.Parent_Parent_ORDER_ID,
                        principalTable: "PAYMENT_INFO",
                        principalColumn: "Parent_ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SHIPPING_ADDRESS",
                columns: table => new
                {
                    Parent_Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    POSTAL_CODE = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    PREFECTURE = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CITY = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ADDRESS_LINE = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SHIPPING_ADDRESS", x => x.Parent_Parent_ORDER_ID);
                    table.ForeignKey(
                        name: "FK_SHIPPING_INFO_SHIPPING_ADDRESS",
                        column: x => x.Parent_Parent_ORDER_ID,
                        principalTable: "SHIPPING_INFO",
                        principalColumn: "Parent_ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SHIPPING_STATUS",
                columns: table => new
                {
                    Parent_Parent_ORDER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    STATUS = table.Column<int>(type: "INTEGER", nullable: false),
                    UPDATE_DATE = table.Column<DateTime>(type: "TEXT", nullable: true),
                    REMARKS = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SHIPPING_STATUS", x => new { x.Parent_Parent_ORDER_ID, x.STATUS });
                    table.ForeignKey(
                        name: "FK_SHIPPING_INFO_SHIPPING_STATUS",
                        column: x => x.Parent_Parent_ORDER_ID,
                        principalTable: "SHIPPING_INFO",
                        principalColumn: "Parent_ORDER_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ACTIONS",
                columns: table => new
                {
                    Parent_対象機器_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_対象機器_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ACTION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ACTION_TYPE = table.Column<int>(type: "INTEGER", nullable: true),
                    STATUS = table.Column<int>(type: "INTEGER", nullable: true),
                    ACTION_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    実施担当者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    ACTION_DETAIL = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACTIONS", x => new { x.Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象機器_保管庫_WAREHOUSE_ID, x.ACTION_ID });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_ACTIONS_X4FD1CEF",
                        column: x => x.実施担当者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_機器点検報告_ACTIONS",
                        columns: x => new { x.Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象機器_保管庫_WAREHOUSE_ID },
                        principalTable: "機器点検報告",
                        principalColumns: new[] { "対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SIZE",
                columns: table => new
                {
                    Parent_Parent_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WIDTH = table.Column<int>(type: "INTEGER", nullable: true),
                    HEIGHT = table.Column<int>(type: "INTEGER", nullable: true),
                    DEPTH = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SIZE", x => x.Parent_Parent_Parent_PRODUCT_ID);
                    table.ForeignKey(
                        name: "FK_PRODUCT_SPEC_SIZE",
                        column: x => x.Parent_Parent_Parent_PRODUCT_ID,
                        principalTable: "PRODUCT_SPEC",
                        principalColumn: "Parent_Parent_PRODUCT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "措置結果",
                columns: table => new
                {
                    対象措置_Parent_対象機器_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    対象措置_ACTION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    RESULT_DATETIME = table.Column<DateTime>(type: "TEXT", nullable: false),
                    結果担当者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    ACHIEVEMENT = table.Column<int>(type: "INTEGER", nullable: true),
                    RESULT_STATUS = table.Column<int>(type: "INTEGER", nullable: false),
                    FEEDBACK = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_措置結果", x => new { x.対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.対象措置_ACTION_ID });
                    table.ForeignKey(
                        name: "FK_ACTIONS_措置結果_XFDF1611",
                        columns: x => new { x.対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.対象措置_ACTION_ID },
                        principalTable: "ACTIONS",
                        principalColumns: new[] { "Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID", "ACTION_ID" });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_措置結果_XD70B07D",
                        column: x => x.結果担当者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ATTACHMENTS",
                columns: table => new
                {
                    Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Parent_対象措置_ACTION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DOCUMENT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DOCUMENT_NAME = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DOCUMENT_TYPE = table.Column<int>(type: "INTEGER", nullable: true),
                    FILE_PATH = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    REGISTER_DATETIME = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ATTACHMENTS", x => new { x.Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.Parent_対象措置_ACTION_ID, x.DOCUMENT_ID });
                    table.ForeignKey(
                        name: "FK_措置結果_ATTACHMENTS",
                        columns: x => new { x.Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.Parent_対象措置_ACTION_ID },
                        principalTable: "措置結果",
                        principalColumns: new[] { "対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NEXT_ACTION",
                columns: table => new
                {
                    Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Parent_対象措置_ACTION_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NEXT_ACTION_TYPE = table.Column<int>(type: "INTEGER", nullable: true),
                    PLANNED_DATE = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    担当者_ID = table.Column<string>(type: "TEXT", nullable: false),
                    NEXT_CONTENT = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreateUser = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateUser = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NEXT_ACTION", x => new { x.Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.Parent_対象措置_ACTION_ID });
                    table.ForeignKey(
                        name: "FK_EMPLOYEE_NEXT_ACTION_XB332A41",
                        column: x => x.担当者_ID,
                        principalTable: "EMPLOYEE",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_措置結果_NEXT_ACTION",
                        columns: x => new { x.Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID, x.Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID, x.Parent_対象措置_ACTION_ID },
                        principalTable: "措置結果",
                        principalColumns: new[] { "対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACTIONS_実施担当者_ID",
                table: "ACTIONS",
                column: "実施担当者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_INVENTORY_保管庫_WAREHOUSE_ID",
                table: "INVENTORY",
                column: "保管庫_WAREHOUSE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_NEXT_ACTION_担当者_ID",
                table: "NEXT_ACTION",
                column: "担当者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ORDER_DETAILS_医療機器_PRODUCT_ID",
                table: "ORDER_DETAILS",
                column: "医療機器_PRODUCT_ID");

            migrationBuilder.CreateIndex(
                name: "IX_SHOZOKU_診療科_BUSHO_CD",
                table: "SHOZOKU",
                column: "診療科_BUSHO_CD");

            migrationBuilder.CreateIndex(
                name: "IX_STOCK_HISTORY_担当者_ID",
                table: "STOCK_HISTORY",
                column: "担当者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_医療機器マスタ_機器分類_CATEGORY_ID",
                table: "医療機器マスタ",
                column: "機器分類_CATEGORY_ID");

            migrationBuilder.CreateIndex(
                name: "IX_医療機器マスタ_供給業者_SUPPLIER_ID",
                table: "医療機器マスタ",
                column: "供給業者_SUPPLIER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_機器点検報告_点検担当者_ID",
                table: "機器点検報告",
                column: "点検担当者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_勤務スケジュール_診療科_STORE_ID",
                table: "勤務スケジュール",
                column: "診療科_STORE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_診療科マスタ_科長_ID",
                table: "診療科マスタ",
                column: "科長_ID");

            migrationBuilder.CreateIndex(
                name: "IX_診療履歴_患者_CUSTOMER_ID",
                table: "診療履歴",
                column: "患者_CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_診療履歴_診療科_STORE_ID",
                table: "診療履歴",
                column: "診療科_STORE_ID");

            migrationBuilder.CreateIndex(
                name: "IX_診療履歴_担当医_ID",
                table: "診療履歴",
                column: "担当医_ID");

            migrationBuilder.CreateIndex(
                name: "IX_措置結果_結果担当者_ID",
                table: "措置結果",
                column: "結果担当者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_保管庫マスタ_管理責任者_ID",
                table: "保管庫マスタ",
                column: "管理責任者_ID");

            migrationBuilder.CreateIndex(
                name: "IX_予約_患者_CUSTOMER_ID",
                table: "予約",
                column: "患者_CUSTOMER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_予約_担当医_ID",
                table: "予約",
                column: "担当医_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCESSORIES");

            migrationBuilder.DropTable(
                name: "ATTACHMENTS");

            migrationBuilder.DropTable(
                name: "AUTHORITY");

            migrationBuilder.DropTable(
                name: "BUSINESS_HOURS");

            migrationBuilder.DropTable(
                name: "CARD_INFO");

            migrationBuilder.DropTable(
                name: "CUSTOMER_ADDRESS");

            migrationBuilder.DropTable(
                name: "DISCOUNT_INFO");

            migrationBuilder.DropTable(
                name: "NEXT_ACTION");

            migrationBuilder.DropTable(
                name: "POINT_HISTORY");

            migrationBuilder.DropTable(
                name: "PRESCRIPTIONS");

            migrationBuilder.DropTable(
                name: "QUALIFICATIONS");

            migrationBuilder.DropTable(
                name: "SHIPPING_ADDRESS");

            migrationBuilder.DropTable(
                name: "SHIPPING_STATUS");

            migrationBuilder.DropTable(
                name: "SHOZOKU");

            migrationBuilder.DropTable(
                name: "SIZE");

            migrationBuilder.DropTable(
                name: "STOCK_HISTORY");

            migrationBuilder.DropTable(
                name: "STORE_ADDRESS");

            migrationBuilder.DropTable(
                name: "WAREHOUSE_ADDRESS");

            migrationBuilder.DropTable(
                name: "勤務スケジュール");

            migrationBuilder.DropTable(
                name: "PAYMENT_INFO");

            migrationBuilder.DropTable(
                name: "ORDER_DETAILS");

            migrationBuilder.DropTable(
                name: "措置結果");

            migrationBuilder.DropTable(
                name: "MEMBERSHIP");

            migrationBuilder.DropTable(
                name: "診察記録");

            migrationBuilder.DropTable(
                name: "医療従事者プロフィール");

            migrationBuilder.DropTable(
                name: "SHIPPING_INFO");

            migrationBuilder.DropTable(
                name: "BUSHO");

            migrationBuilder.DropTable(
                name: "PRODUCT_SPEC");

            migrationBuilder.DropTable(
                name: "ACTIONS");

            migrationBuilder.DropTable(
                name: "予約");

            migrationBuilder.DropTable(
                name: "診療履歴");

            migrationBuilder.DropTable(
                name: "PRODUCT_DETAIL");

            migrationBuilder.DropTable(
                name: "機器点検報告");

            migrationBuilder.DropTable(
                name: "患者マスタ");

            migrationBuilder.DropTable(
                name: "診療科マスタ");

            migrationBuilder.DropTable(
                name: "INVENTORY");

            migrationBuilder.DropTable(
                name: "保管庫マスタ");

            migrationBuilder.DropTable(
                name: "医療機器マスタ");

            migrationBuilder.DropTable(
                name: "EMPLOYEE");

            migrationBuilder.DropTable(
                name: "供給業者マスタ");

            migrationBuilder.DropTable(
                name: "機器分類マスタ");
        }
    }
}

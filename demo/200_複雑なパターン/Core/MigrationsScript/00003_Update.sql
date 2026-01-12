BEGIN TRANSACTION;
CREATE TABLE "ef_temp_予約" (
    "RESERVATION_ID" TEXT NOT NULL CONSTRAINT "PK_予約" PRIMARY KEY,
    "RESERVATION_DATETIME" TEXT NOT NULL,
    "患者_CUSTOMER_ID" TEXT NOT NULL,
    "RESERVATION_TYPE" INTEGER NULL,
    "RESERVATION_NOTE" TEXT NULL,
    "担当医_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_EMPLOYEE_予約_XDECF289" FOREIGN KEY ("担当医_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_患者マスタ_予約_XFB576B4" FOREIGN KEY ("患者_CUSTOMER_ID") REFERENCES "患者マスタ" ("CUSTOMER_ID")
);

INSERT INTO "ef_temp_予約" ("RESERVATION_ID", "CreateUser", "CreatedAt", "RESERVATION_DATETIME", "RESERVATION_NOTE", "RESERVATION_TYPE", "UpdateUser", "UpdatedAt", "Version", "患者_CUSTOMER_ID", "担当医_ID")
SELECT "RESERVATION_ID", "CreateUser", "CreatedAt", IFNULL("RESERVATION_DATETIME", '0001-01-01 00:00:00'), "RESERVATION_NOTE", "RESERVATION_TYPE", "UpdateUser", "UpdatedAt", "Version", IFNULL("患者_CUSTOMER_ID", ''), "担当医_ID"
FROM "予約";

CREATE TABLE "ef_temp_措置結果" (
    "対象措置_Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "対象措置_ACTION_ID" TEXT NOT NULL,
    "RESULT_DATETIME" TEXT NOT NULL,
    "結果担当者_ID" TEXT NOT NULL,
    "ACHIEVEMENT" INTEGER NULL,
    "RESULT_STATUS" INTEGER NOT NULL,
    "FEEDBACK" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "PK_措置結果" PRIMARY KEY ("対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID"),
    CONSTRAINT "FK_ACTIONS_措置結果_XFDF1611" FOREIGN KEY ("対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID") REFERENCES "ACTIONS" ("Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID", "ACTION_ID"),
    CONSTRAINT "FK_EMPLOYEE_措置結果_XD70B07D" FOREIGN KEY ("結果担当者_ID") REFERENCES "EMPLOYEE" ("ID")
);

INSERT INTO "ef_temp_措置結果" ("対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID", "ACHIEVEMENT", "CreateUser", "CreatedAt", "FEEDBACK", "RESULT_DATETIME", "RESULT_STATUS", "UpdateUser", "UpdatedAt", "Version", "結果担当者_ID")
SELECT "対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID", "ACHIEVEMENT", "CreateUser", "CreatedAt", "FEEDBACK", IFNULL("RESULT_DATETIME", '0001-01-01 00:00:00'), IFNULL("RESULT_STATUS", 0), "UpdateUser", "UpdatedAt", "Version", IFNULL("結果担当者_ID", '')
FROM "措置結果";

CREATE TABLE "ef_temp_診療履歴" (
    "ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_診療履歴" PRIMARY KEY,
    "ORDER_DATE" TEXT NOT NULL,
    "患者_CUSTOMER_ID" TEXT NOT NULL,
    "診療科_STORE_ID" TEXT NOT NULL,
    "担当医_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_EMPLOYEE_診療履歴_XDECF289" FOREIGN KEY ("担当医_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_患者マスタ_診療履歴_XFB576B4" FOREIGN KEY ("患者_CUSTOMER_ID") REFERENCES "患者マスタ" ("CUSTOMER_ID"),
    CONSTRAINT "FK_診療科マスタ_診療履歴_X6518C1A" FOREIGN KEY ("診療科_STORE_ID") REFERENCES "診療科マスタ" ("STORE_ID")
);

INSERT INTO "ef_temp_診療履歴" ("ORDER_ID", "CreateUser", "CreatedAt", "ORDER_DATE", "UpdateUser", "UpdatedAt", "Version", "患者_CUSTOMER_ID", "担当医_ID", "診療科_STORE_ID")
SELECT "ORDER_ID", "CreateUser", "CreatedAt", IFNULL("ORDER_DATE", '0001-01-01 00:00:00'), "UpdateUser", "UpdatedAt", "Version", IFNULL("患者_CUSTOMER_ID", ''), "担当医_ID", IFNULL("診療科_STORE_ID", '')
FROM "診療履歴";

CREATE TABLE "ef_temp_機器点検報告" (
    "対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "SURVEY_DATE" TEXT NOT NULL,
    "点検担当者_ID" TEXT NOT NULL,
    "ACTUAL_COUNT" INTEGER NOT NULL,
    "INVENTORY_DIFF" INTEGER NULL,
    "SURVEY_NOTE" TEXT NULL,
    "PHOTO_URL" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "PK_機器点検報告" PRIMARY KEY ("対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID"),
    CONSTRAINT "FK_EMPLOYEE_機器点検報告_XC084C68" FOREIGN KEY ("点検担当者_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_INVENTORY_機器点検報告_X62E8D1C" FOREIGN KEY ("対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID") REFERENCES "INVENTORY" ("Parent_PRODUCT_ID", "保管庫_WAREHOUSE_ID")
);

INSERT INTO "ef_temp_機器点検報告" ("対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID", "ACTUAL_COUNT", "CreateUser", "CreatedAt", "INVENTORY_DIFF", "PHOTO_URL", "SURVEY_DATE", "SURVEY_NOTE", "UpdateUser", "UpdatedAt", "Version", "点検担当者_ID")
SELECT "対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID", IFNULL("ACTUAL_COUNT", 0), "CreateUser", "CreatedAt", "INVENTORY_DIFF", "PHOTO_URL", IFNULL("SURVEY_DATE", '0001-01-01'), "SURVEY_NOTE", "UpdateUser", "UpdatedAt", "Version", IFNULL("点検担当者_ID", '')
FROM "機器点検報告";

CREATE TABLE "ef_temp_患者マスタ" (
    "CUSTOMER_ID" TEXT NOT NULL CONSTRAINT "PK_患者マスタ" PRIMARY KEY,
    "CUSTOMER_NAME" TEXT NOT NULL,
    "CUSTOMER_KANA" TEXT NULL,
    "BIRTH_DATE" TEXT NULL,
    "GENDER" INTEGER NULL,
    "EMAIL" TEXT NULL,
    "PHONE" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_患者マスタ" ("CUSTOMER_ID", "BIRTH_DATE", "CUSTOMER_KANA", "CUSTOMER_NAME", "CreateUser", "CreatedAt", "EMAIL", "GENDER", "PHONE", "UpdateUser", "UpdatedAt", "Version")
SELECT "CUSTOMER_ID", "BIRTH_DATE", "CUSTOMER_KANA", IFNULL("CUSTOMER_NAME", ''), "CreateUser", "CreatedAt", "EMAIL", "GENDER", "PHONE", "UpdateUser", "UpdatedAt", "Version"
FROM "患者マスタ";

CREATE TABLE "ef_temp_医療機器マスタ" (
    "PRODUCT_ID" TEXT NOT NULL CONSTRAINT "PK_医療機器マスタ" PRIMARY KEY,
    "PRODUCT_NAME" TEXT NOT NULL,
    "PRICE" INTEGER NOT NULL,
    "機器分類_CATEGORY_ID" TEXT NOT NULL,
    "供給業者_SUPPLIER_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_供給業者マスタ_医療機器マスタ_X89DCA22" FOREIGN KEY ("供給業者_SUPPLIER_ID") REFERENCES "供給業者マスタ" ("SUPPLIER_ID"),
    CONSTRAINT "FK_機器分類マスタ_医療機器マスタ_X7D2BEB8" FOREIGN KEY ("機器分類_CATEGORY_ID") REFERENCES "機器分類マスタ" ("CATEGORY_ID")
);

INSERT INTO "ef_temp_医療機器マスタ" ("PRODUCT_ID", "CreateUser", "CreatedAt", "PRICE", "PRODUCT_NAME", "UpdateUser", "UpdatedAt", "Version", "供給業者_SUPPLIER_ID", "機器分類_CATEGORY_ID")
SELECT "PRODUCT_ID", "CreateUser", "CreatedAt", IFNULL("PRICE", 0), IFNULL("PRODUCT_NAME", ''), "UpdateUser", "UpdatedAt", "Version", "供給業者_SUPPLIER_ID", IFNULL("機器分類_CATEGORY_ID", '')
FROM "医療機器マスタ";

CREATE TABLE "ef_temp_SHOZOKU" (
    "Parent_ID" TEXT NOT NULL,
    "NENDO" INTEGER NOT NULL,
    "診療科_BUSHO_CD" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_SHOZOKU" PRIMARY KEY ("Parent_ID", "NENDO"),
    CONSTRAINT "FK_BUSHO_SHOZOKU_X6518C1A" FOREIGN KEY ("診療科_BUSHO_CD") REFERENCES "BUSHO" ("BUSHO_CD"),
    CONSTRAINT "FK_EMPLOYEE_SHOZOKU" FOREIGN KEY ("Parent_ID") REFERENCES "EMPLOYEE" ("ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SHOZOKU" ("Parent_ID", "NENDO", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "診療科_BUSHO_CD")
SELECT "Parent_ID", "NENDO", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", IFNULL("診療科_BUSHO_CD", '')
FROM "SHOZOKU";

CREATE TABLE "ef_temp_SHIPPING_INFO" (
    "Parent_ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_SHIPPING_INFO" PRIMARY KEY,
    "SHIPPING_METHOD" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "FK_診療履歴_SHIPPING_INFO" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SHIPPING_INFO" ("Parent_ORDER_ID", "CreateUser", "CreatedAt", "SHIPPING_METHOD", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "CreateUser", "CreatedAt", IFNULL("SHIPPING_METHOD", 0), "UpdateUser", "UpdatedAt"
FROM "SHIPPING_INFO";

CREATE TABLE "ef_temp_PAYMENT_INFO" (
    "Parent_ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_PAYMENT_INFO" PRIMARY KEY,
    "PAYMENT_TYPE" INTEGER NOT NULL,
    "PAYMENT_DATE" TEXT NULL,
    "PAYMENT_STATUS" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "FK_診療履歴_PAYMENT_INFO" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_PAYMENT_INFO" ("Parent_ORDER_ID", "CreateUser", "CreatedAt", "PAYMENT_DATE", "PAYMENT_STATUS", "PAYMENT_TYPE", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "CreateUser", "CreatedAt", "PAYMENT_DATE", "PAYMENT_STATUS", IFNULL("PAYMENT_TYPE", 0), "UpdateUser", "UpdatedAt"
FROM "PAYMENT_INFO";

CREATE TABLE "ef_temp_ORDER_DETAILS" (
    "Parent_ORDER_ID" TEXT NOT NULL,
    "医療機器_PRODUCT_ID" TEXT NOT NULL,
    "QUANTITY" INTEGER NOT NULL,
    "UNIT_PRICE" INTEGER NOT NULL,
    "SUBTOTAL" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_ORDER_DETAILS" PRIMARY KEY ("Parent_ORDER_ID", "医療機器_PRODUCT_ID"),
    CONSTRAINT "FK_医療機器マスタ_ORDER_DETAILS_X5D5C85D" FOREIGN KEY ("医療機器_PRODUCT_ID") REFERENCES "医療機器マスタ" ("PRODUCT_ID"),
    CONSTRAINT "FK_診療履歴_ORDER_DETAILS" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_ORDER_DETAILS" ("Parent_ORDER_ID", "医療機器_PRODUCT_ID", "CreateUser", "CreatedAt", "QUANTITY", "SUBTOTAL", "UNIT_PRICE", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "医療機器_PRODUCT_ID", "CreateUser", "CreatedAt", IFNULL("QUANTITY", 0), IFNULL("SUBTOTAL", 0), IFNULL("UNIT_PRICE", 0), "UpdateUser", "UpdatedAt"
FROM "ORDER_DETAILS";

CREATE TABLE "ef_temp_EMPLOYEE" (
    "ID" TEXT NOT NULL CONSTRAINT "PK_EMPLOYEE" PRIMARY KEY,
    "NAME" TEXT NOT NULL,
    "NAME_KANA" TEXT NULL,
    "TAISHOKU" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_EMPLOYEE" ("ID", "CreateUser", "CreatedAt", "NAME", "NAME_KANA", "TAISHOKU", "UpdateUser", "UpdatedAt", "Version")
SELECT "ID", "CreateUser", "CreatedAt", IFNULL("NAME", ''), "NAME_KANA", "TAISHOKU", "UpdateUser", "UpdatedAt", "Version"
FROM "EMPLOYEE";

CREATE TABLE "ef_temp_BUSHO" (
    "BUSHO_CD" TEXT NOT NULL CONSTRAINT "PK_BUSHO" PRIMARY KEY,
    "BUSHO_NAME" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_BUSHO" ("BUSHO_CD", "BUSHO_NAME", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version")
SELECT "BUSHO_CD", IFNULL("BUSHO_NAME", ''), "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version"
FROM "BUSHO";

CREATE TABLE "ef_temp_ATTACHMENTS" (
    "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "Parent_対象措置_ACTION_ID" TEXT NOT NULL,
    "DOCUMENT_ID" TEXT NOT NULL,
    "DOCUMENT_NAME" TEXT NOT NULL,
    "DOCUMENT_TYPE" INTEGER NULL,
    "FILE_PATH" TEXT NULL,
    "REGISTER_DATETIME" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_ATTACHMENTS" PRIMARY KEY ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "DOCUMENT_ID"),
    CONSTRAINT "FK_措置結果_ATTACHMENTS" FOREIGN KEY ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID") REFERENCES "措置結果" ("対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_ATTACHMENTS" ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "DOCUMENT_ID", "CreateUser", "CreatedAt", "DOCUMENT_NAME", "DOCUMENT_TYPE", "FILE_PATH", "REGISTER_DATETIME", "UpdateUser", "UpdatedAt")
SELECT "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "DOCUMENT_ID", "CreateUser", "CreatedAt", IFNULL("DOCUMENT_NAME", ''), "DOCUMENT_TYPE", "FILE_PATH", "REGISTER_DATETIME", "UpdateUser", "UpdatedAt"
FROM "ATTACHMENTS";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "予約";

ALTER TABLE "ef_temp_予約" RENAME TO "予約";

DROP TABLE "措置結果";

ALTER TABLE "ef_temp_措置結果" RENAME TO "措置結果";

DROP TABLE "診療履歴";

ALTER TABLE "ef_temp_診療履歴" RENAME TO "診療履歴";

DROP TABLE "機器点検報告";

ALTER TABLE "ef_temp_機器点検報告" RENAME TO "機器点検報告";

DROP TABLE "患者マスタ";

ALTER TABLE "ef_temp_患者マスタ" RENAME TO "患者マスタ";

DROP TABLE "医療機器マスタ";

ALTER TABLE "ef_temp_医療機器マスタ" RENAME TO "医療機器マスタ";

DROP TABLE "SHOZOKU";

ALTER TABLE "ef_temp_SHOZOKU" RENAME TO "SHOZOKU";

DROP TABLE "SHIPPING_INFO";

ALTER TABLE "ef_temp_SHIPPING_INFO" RENAME TO "SHIPPING_INFO";

DROP TABLE "PAYMENT_INFO";

ALTER TABLE "ef_temp_PAYMENT_INFO" RENAME TO "PAYMENT_INFO";

DROP TABLE "ORDER_DETAILS";

ALTER TABLE "ef_temp_ORDER_DETAILS" RENAME TO "ORDER_DETAILS";

DROP TABLE "EMPLOYEE";

ALTER TABLE "ef_temp_EMPLOYEE" RENAME TO "EMPLOYEE";

DROP TABLE "BUSHO";

ALTER TABLE "ef_temp_BUSHO" RENAME TO "BUSHO";

DROP TABLE "ATTACHMENTS";

ALTER TABLE "ef_temp_ATTACHMENTS" RENAME TO "ATTACHMENTS";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_予約_患者_CUSTOMER_ID" ON "予約" ("患者_CUSTOMER_ID");

CREATE INDEX "IX_予約_担当医_ID" ON "予約" ("担当医_ID");

CREATE INDEX "IX_措置結果_結果担当者_ID" ON "措置結果" ("結果担当者_ID");

CREATE INDEX "IX_診療履歴_患者_CUSTOMER_ID" ON "診療履歴" ("患者_CUSTOMER_ID");

CREATE INDEX "IX_診療履歴_診療科_STORE_ID" ON "診療履歴" ("診療科_STORE_ID");

CREATE INDEX "IX_診療履歴_担当医_ID" ON "診療履歴" ("担当医_ID");

CREATE INDEX "IX_機器点検報告_点検担当者_ID" ON "機器点検報告" ("点検担当者_ID");

CREATE INDEX "IX_医療機器マスタ_機器分類_CATEGORY_ID" ON "医療機器マスタ" ("機器分類_CATEGORY_ID");

CREATE INDEX "IX_医療機器マスタ_供給業者_SUPPLIER_ID" ON "医療機器マスタ" ("供給業者_SUPPLIER_ID");

CREATE INDEX "IX_SHOZOKU_診療科_BUSHO_CD" ON "SHOZOKU" ("診療科_BUSHO_CD");

CREATE INDEX "IX_ORDER_DETAILS_医療機器_PRODUCT_ID" ON "ORDER_DETAILS" ("医療機器_PRODUCT_ID");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260112094403_00003_Update', '9.0.3');


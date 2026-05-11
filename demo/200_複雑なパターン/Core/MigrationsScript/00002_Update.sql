BEGIN TRANSACTION;
CREATE TABLE "ef_temp_予約" (
    "RESERVATION_ID" TEXT NOT NULL CONSTRAINT "PK_予約" PRIMARY KEY,
    "RESERVATION_DATETIME" TEXT NULL,
    "患者_CUSTOMER_ID" TEXT NULL,
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
SELECT "RESERVATION_ID", "CreateUser", "CreatedAt", "RESERVATION_DATETIME", "RESERVATION_NOTE", "RESERVATION_TYPE", "UpdateUser", "UpdatedAt", "Version", "患者_CUSTOMER_ID", "担当医_ID"
FROM "予約";

CREATE TABLE "ef_temp_保管庫マスタ" (
    "WAREHOUSE_ID" TEXT NOT NULL CONSTRAINT "PK_保管庫マスタ" PRIMARY KEY,
    "WAREHOUSE_NAME" TEXT NULL,
    "管理責任者_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_EMPLOYEE_保管庫マスタ_XD847631" FOREIGN KEY ("管理責任者_ID") REFERENCES "EMPLOYEE" ("ID")
);

INSERT INTO "ef_temp_保管庫マスタ" ("WAREHOUSE_ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "WAREHOUSE_NAME", "管理責任者_ID")
SELECT "WAREHOUSE_ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "WAREHOUSE_NAME", "管理責任者_ID"
FROM "保管庫マスタ";

CREATE TABLE "ef_temp_措置結果" (
    "対象措置_Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "対象措置_ACTION_ID" TEXT NOT NULL,
    "RESULT_DATETIME" TEXT NULL,
    "結果担当者_ID" TEXT NULL,
    "ACHIEVEMENT" INTEGER NULL,
    "RESULT_STATUS" INTEGER NULL,
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
SELECT "対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID", "ACHIEVEMENT", "CreateUser", "CreatedAt", "FEEDBACK", "RESULT_DATETIME", "RESULT_STATUS", "UpdateUser", "UpdatedAt", "Version", "結果担当者_ID"
FROM "措置結果";

CREATE TABLE "ef_temp_診療履歴" (
    "ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_診療履歴" PRIMARY KEY,
    "ORDER_DATE" TEXT NULL,
    "患者_CUSTOMER_ID" TEXT NULL,
    "診療科_STORE_ID" TEXT NULL,
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
SELECT "ORDER_ID", "CreateUser", "CreatedAt", "ORDER_DATE", "UpdateUser", "UpdatedAt", "Version", "患者_CUSTOMER_ID", "担当医_ID", "診療科_STORE_ID"
FROM "診療履歴";

CREATE TABLE "ef_temp_診療科マスタ" (
    "STORE_ID" TEXT NOT NULL CONSTRAINT "PK_診療科マスタ" PRIMARY KEY,
    "STORE_NAME" TEXT NULL,
    "PHONE" TEXT NULL,
    "科長_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_EMPLOYEE_診療科マスタ_X170D2D7" FOREIGN KEY ("科長_ID") REFERENCES "EMPLOYEE" ("ID")
);

INSERT INTO "ef_temp_診療科マスタ" ("STORE_ID", "CreateUser", "CreatedAt", "PHONE", "STORE_NAME", "UpdateUser", "UpdatedAt", "Version", "科長_ID")
SELECT "STORE_ID", "CreateUser", "CreatedAt", "PHONE", "STORE_NAME", "UpdateUser", "UpdatedAt", "Version", "科長_ID"
FROM "診療科マスタ";

CREATE TABLE "ef_temp_機器点検報告" (
    "対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "SURVEY_DATE" TEXT NULL,
    "点検担当者_ID" TEXT NULL,
    "ACTUAL_COUNT" INTEGER NULL,
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
SELECT "対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID", "ACTUAL_COUNT", "CreateUser", "CreatedAt", "INVENTORY_DIFF", "PHOTO_URL", "SURVEY_DATE", "SURVEY_NOTE", "UpdateUser", "UpdatedAt", "Version", "点検担当者_ID"
FROM "機器点検報告";

CREATE TABLE "ef_temp_患者マスタ" (
    "CUSTOMER_ID" TEXT NOT NULL CONSTRAINT "PK_患者マスタ" PRIMARY KEY,
    "CUSTOMER_NAME" TEXT NULL,
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
SELECT "CUSTOMER_ID", "BIRTH_DATE", "CUSTOMER_KANA", "CUSTOMER_NAME", "CreateUser", "CreatedAt", "EMAIL", "GENDER", "PHONE", "UpdateUser", "UpdatedAt", "Version"
FROM "患者マスタ";

CREATE TABLE "ef_temp_医療機器マスタ" (
    "PRODUCT_ID" TEXT NOT NULL CONSTRAINT "PK_医療機器マスタ" PRIMARY KEY,
    "PRODUCT_NAME" TEXT NULL,
    "PRICE" INTEGER NULL,
    "機器分類_CATEGORY_ID" TEXT NULL,
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
SELECT "PRODUCT_ID", "CreateUser", "CreatedAt", "PRICE", "PRODUCT_NAME", "UpdateUser", "UpdatedAt", "Version", "供給業者_SUPPLIER_ID", "機器分類_CATEGORY_ID"
FROM "医療機器マスタ";

CREATE TABLE "ef_temp_STOCK_HISTORY" (
    "Parent_Parent_PRODUCT_ID" TEXT NOT NULL,
    "Parent_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "HISTORY_ID" TEXT NOT NULL,
    "CHANGE_DATE" TEXT NULL,
    "PREVIOUS_QUANTITY" INTEGER NULL,
    "CURRENT_QUANTITY" INTEGER NULL,
    "担当者_ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_STOCK_HISTORY" PRIMARY KEY ("Parent_Parent_PRODUCT_ID", "Parent_保管庫_WAREHOUSE_ID", "HISTORY_ID"),
    CONSTRAINT "FK_EMPLOYEE_STOCK_HISTORY_XB332A41" FOREIGN KEY ("担当者_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_INVENTORY_STOCK_HISTORY" FOREIGN KEY ("Parent_Parent_PRODUCT_ID", "Parent_保管庫_WAREHOUSE_ID") REFERENCES "INVENTORY" ("Parent_PRODUCT_ID", "保管庫_WAREHOUSE_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_STOCK_HISTORY" ("Parent_Parent_PRODUCT_ID", "Parent_保管庫_WAREHOUSE_ID", "HISTORY_ID", "CHANGE_DATE", "CURRENT_QUANTITY", "CreateUser", "CreatedAt", "PREVIOUS_QUANTITY", "UpdateUser", "UpdatedAt", "担当者_ID")
SELECT "Parent_Parent_PRODUCT_ID", "Parent_保管庫_WAREHOUSE_ID", "HISTORY_ID", "CHANGE_DATE", "CURRENT_QUANTITY", "CreateUser", "CreatedAt", "PREVIOUS_QUANTITY", "UpdateUser", "UpdatedAt", "担当者_ID"
FROM "STOCK_HISTORY";

CREATE TABLE "ef_temp_SHOZOKU" (
    "Parent_ID" TEXT NOT NULL,
    "NENDO" INTEGER NOT NULL,
    "診療科_BUSHO_CD" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_SHOZOKU" PRIMARY KEY ("Parent_ID", "NENDO"),
    CONSTRAINT "FK_BUSHO_SHOZOKU_X6518C1A" FOREIGN KEY ("診療科_BUSHO_CD") REFERENCES "BUSHO" ("BUSHO_CD"),
    CONSTRAINT "FK_EMPLOYEE_SHOZOKU" FOREIGN KEY ("Parent_ID") REFERENCES "EMPLOYEE" ("ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SHOZOKU" ("Parent_ID", "NENDO", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "診療科_BUSHO_CD")
SELECT "Parent_ID", "NENDO", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "診療科_BUSHO_CD"
FROM "SHOZOKU";

CREATE TABLE "ef_temp_SHIPPING_INFO" (
    "Parent_ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_SHIPPING_INFO" PRIMARY KEY,
    "SHIPPING_METHOD" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "FK_診療履歴_SHIPPING_INFO" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_SHIPPING_INFO" ("Parent_ORDER_ID", "CreateUser", "CreatedAt", "SHIPPING_METHOD", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "CreateUser", "CreatedAt", "SHIPPING_METHOD", "UpdateUser", "UpdatedAt"
FROM "SHIPPING_INFO";

CREATE TABLE "ef_temp_PAYMENT_INFO" (
    "Parent_ORDER_ID" TEXT NOT NULL CONSTRAINT "PK_PAYMENT_INFO" PRIMARY KEY,
    "PAYMENT_TYPE" INTEGER NULL,
    "PAYMENT_DATE" TEXT NULL,
    "PAYMENT_STATUS" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "FK_診療履歴_PAYMENT_INFO" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_PAYMENT_INFO" ("Parent_ORDER_ID", "CreateUser", "CreatedAt", "PAYMENT_DATE", "PAYMENT_STATUS", "PAYMENT_TYPE", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "CreateUser", "CreatedAt", "PAYMENT_DATE", "PAYMENT_STATUS", "PAYMENT_TYPE", "UpdateUser", "UpdatedAt"
FROM "PAYMENT_INFO";

CREATE TABLE "ef_temp_ORDER_DETAILS" (
    "Parent_ORDER_ID" TEXT NOT NULL,
    "医療機器_PRODUCT_ID" TEXT NOT NULL,
    "QUANTITY" INTEGER NULL,
    "UNIT_PRICE" INTEGER NULL,
    "SUBTOTAL" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_ORDER_DETAILS" PRIMARY KEY ("Parent_ORDER_ID", "医療機器_PRODUCT_ID"),
    CONSTRAINT "FK_医療機器マスタ_ORDER_DETAILS_X5D5C85D" FOREIGN KEY ("医療機器_PRODUCT_ID") REFERENCES "医療機器マスタ" ("PRODUCT_ID"),
    CONSTRAINT "FK_診療履歴_ORDER_DETAILS" FOREIGN KEY ("Parent_ORDER_ID") REFERENCES "診療履歴" ("ORDER_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_ORDER_DETAILS" ("Parent_ORDER_ID", "医療機器_PRODUCT_ID", "CreateUser", "CreatedAt", "QUANTITY", "SUBTOTAL", "UNIT_PRICE", "UpdateUser", "UpdatedAt")
SELECT "Parent_ORDER_ID", "医療機器_PRODUCT_ID", "CreateUser", "CreatedAt", "QUANTITY", "SUBTOTAL", "UNIT_PRICE", "UpdateUser", "UpdatedAt"
FROM "ORDER_DETAILS";

CREATE TABLE "ef_temp_NEXT_ACTION" (
    "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "Parent_対象措置_ACTION_ID" TEXT NOT NULL,
    "NEXT_ACTION_TYPE" INTEGER NULL,
    "PLANNED_DATE" TEXT NULL,
    "担当者_ID" TEXT NULL,
    "NEXT_CONTENT" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_NEXT_ACTION" PRIMARY KEY ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID"),
    CONSTRAINT "FK_EMPLOYEE_NEXT_ACTION_XB332A41" FOREIGN KEY ("担当者_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_措置結果_NEXT_ACTION" FOREIGN KEY ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID") REFERENCES "措置結果" ("対象措置_Parent_対象機器_Parent_PRODUCT_ID", "対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "対象措置_ACTION_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_NEXT_ACTION" ("Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "CreateUser", "CreatedAt", "NEXT_ACTION_TYPE", "NEXT_CONTENT", "PLANNED_DATE", "UpdateUser", "UpdatedAt", "担当者_ID")
SELECT "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "CreateUser", "CreatedAt", "NEXT_ACTION_TYPE", "NEXT_CONTENT", "PLANNED_DATE", "UpdateUser", "UpdatedAt", "担当者_ID"
FROM "NEXT_ACTION";

CREATE TABLE "ef_temp_EMPLOYEE" (
    "ID" TEXT NOT NULL CONSTRAINT "PK_EMPLOYEE" PRIMARY KEY,
    "NAME" TEXT NULL,
    "NAME_KANA" TEXT NULL,
    "TAISHOKU" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_EMPLOYEE" ("ID", "CreateUser", "CreatedAt", "NAME", "NAME_KANA", "TAISHOKU", "UpdateUser", "UpdatedAt", "Version")
SELECT "ID", "CreateUser", "CreatedAt", "NAME", "NAME_KANA", "TAISHOKU", "UpdateUser", "UpdatedAt", "Version"
FROM "EMPLOYEE";

CREATE TABLE "ef_temp_BUSHO" (
    "BUSHO_CD" TEXT NOT NULL CONSTRAINT "PK_BUSHO" PRIMARY KEY,
    "BUSHO_NAME" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_BUSHO" ("BUSHO_CD", "BUSHO_NAME", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version")
SELECT "BUSHO_CD", "BUSHO_NAME", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version"
FROM "BUSHO";

CREATE TABLE "ef_temp_ATTACHMENTS" (
    "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "Parent_対象措置_ACTION_ID" TEXT NOT NULL,
    "DOCUMENT_ID" TEXT NOT NULL,
    "DOCUMENT_NAME" TEXT NULL,
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
SELECT "Parent_対象措置_Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象措置_Parent_対象機器_保管庫_WAREHOUSE_ID", "Parent_対象措置_ACTION_ID", "DOCUMENT_ID", "CreateUser", "CreatedAt", "DOCUMENT_NAME", "DOCUMENT_TYPE", "FILE_PATH", "REGISTER_DATETIME", "UpdateUser", "UpdatedAt"
FROM "ATTACHMENTS";

CREATE TABLE "ef_temp_ACTIONS" (
    "Parent_対象機器_Parent_PRODUCT_ID" TEXT NOT NULL,
    "Parent_対象機器_保管庫_WAREHOUSE_ID" TEXT NOT NULL,
    "ACTION_ID" TEXT NOT NULL,
    "ACTION_TYPE" INTEGER NULL,
    "STATUS" INTEGER NULL,
    "ACTION_DATE" TEXT NULL,
    "実施担当者_ID" TEXT NULL,
    "ACTION_DETAIL" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_ACTIONS" PRIMARY KEY ("Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID", "ACTION_ID"),
    CONSTRAINT "FK_EMPLOYEE_ACTIONS_X4FD1CEF" FOREIGN KEY ("実施担当者_ID") REFERENCES "EMPLOYEE" ("ID"),
    CONSTRAINT "FK_機器点検報告_ACTIONS" FOREIGN KEY ("Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID") REFERENCES "機器点検報告" ("対象機器_Parent_PRODUCT_ID", "対象機器_保管庫_WAREHOUSE_ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_ACTIONS" ("Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID", "ACTION_ID", "ACTION_DATE", "ACTION_DETAIL", "ACTION_TYPE", "CreateUser", "CreatedAt", "STATUS", "UpdateUser", "UpdatedAt", "実施担当者_ID")
SELECT "Parent_対象機器_Parent_PRODUCT_ID", "Parent_対象機器_保管庫_WAREHOUSE_ID", "ACTION_ID", "ACTION_DATE", "ACTION_DETAIL", "ACTION_TYPE", "CreateUser", "CreatedAt", "STATUS", "UpdateUser", "UpdatedAt", "実施担当者_ID"
FROM "ACTIONS";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "予約";

ALTER TABLE "ef_temp_予約" RENAME TO "予約";

DROP TABLE "保管庫マスタ";

ALTER TABLE "ef_temp_保管庫マスタ" RENAME TO "保管庫マスタ";

DROP TABLE "措置結果";

ALTER TABLE "ef_temp_措置結果" RENAME TO "措置結果";

DROP TABLE "診療履歴";

ALTER TABLE "ef_temp_診療履歴" RENAME TO "診療履歴";

DROP TABLE "診療科マスタ";

ALTER TABLE "ef_temp_診療科マスタ" RENAME TO "診療科マスタ";

DROP TABLE "機器点検報告";

ALTER TABLE "ef_temp_機器点検報告" RENAME TO "機器点検報告";

DROP TABLE "患者マスタ";

ALTER TABLE "ef_temp_患者マスタ" RENAME TO "患者マスタ";

DROP TABLE "医療機器マスタ";

ALTER TABLE "ef_temp_医療機器マスタ" RENAME TO "医療機器マスタ";

DROP TABLE "STOCK_HISTORY";

ALTER TABLE "ef_temp_STOCK_HISTORY" RENAME TO "STOCK_HISTORY";

DROP TABLE "SHOZOKU";

ALTER TABLE "ef_temp_SHOZOKU" RENAME TO "SHOZOKU";

DROP TABLE "SHIPPING_INFO";

ALTER TABLE "ef_temp_SHIPPING_INFO" RENAME TO "SHIPPING_INFO";

DROP TABLE "PAYMENT_INFO";

ALTER TABLE "ef_temp_PAYMENT_INFO" RENAME TO "PAYMENT_INFO";

DROP TABLE "ORDER_DETAILS";

ALTER TABLE "ef_temp_ORDER_DETAILS" RENAME TO "ORDER_DETAILS";

DROP TABLE "NEXT_ACTION";

ALTER TABLE "ef_temp_NEXT_ACTION" RENAME TO "NEXT_ACTION";

DROP TABLE "EMPLOYEE";

ALTER TABLE "ef_temp_EMPLOYEE" RENAME TO "EMPLOYEE";

DROP TABLE "BUSHO";

ALTER TABLE "ef_temp_BUSHO" RENAME TO "BUSHO";

DROP TABLE "ATTACHMENTS";

ALTER TABLE "ef_temp_ATTACHMENTS" RENAME TO "ATTACHMENTS";

DROP TABLE "ACTIONS";

ALTER TABLE "ef_temp_ACTIONS" RENAME TO "ACTIONS";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_予約_患者_CUSTOMER_ID" ON "予約" ("患者_CUSTOMER_ID");

CREATE INDEX "IX_予約_担当医_ID" ON "予約" ("担当医_ID");

CREATE INDEX "IX_保管庫マスタ_管理責任者_ID" ON "保管庫マスタ" ("管理責任者_ID");

CREATE INDEX "IX_措置結果_結果担当者_ID" ON "措置結果" ("結果担当者_ID");

CREATE INDEX "IX_診療履歴_患者_CUSTOMER_ID" ON "診療履歴" ("患者_CUSTOMER_ID");

CREATE INDEX "IX_診療履歴_診療科_STORE_ID" ON "診療履歴" ("診療科_STORE_ID");

CREATE INDEX "IX_診療履歴_担当医_ID" ON "診療履歴" ("担当医_ID");

CREATE INDEX "IX_診療科マスタ_科長_ID" ON "診療科マスタ" ("科長_ID");

CREATE INDEX "IX_機器点検報告_点検担当者_ID" ON "機器点検報告" ("点検担当者_ID");

CREATE INDEX "IX_医療機器マスタ_機器分類_CATEGORY_ID" ON "医療機器マスタ" ("機器分類_CATEGORY_ID");

CREATE INDEX "IX_医療機器マスタ_供給業者_SUPPLIER_ID" ON "医療機器マスタ" ("供給業者_SUPPLIER_ID");

CREATE INDEX "IX_STOCK_HISTORY_担当者_ID" ON "STOCK_HISTORY" ("担当者_ID");

CREATE INDEX "IX_SHOZOKU_診療科_BUSHO_CD" ON "SHOZOKU" ("診療科_BUSHO_CD");

CREATE INDEX "IX_ORDER_DETAILS_医療機器_PRODUCT_ID" ON "ORDER_DETAILS" ("医療機器_PRODUCT_ID");

CREATE INDEX "IX_NEXT_ACTION_担当者_ID" ON "NEXT_ACTION" ("担当者_ID");

CREATE INDEX "IX_ACTIONS_実施担当者_ID" ON "ACTIONS" ("実施担当者_ID");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260112093204_00002_Update', '9.0.3');


BEGIN TRANSACTION;
CREATE TABLE "ef_temp_売上明細" (
    "Parent_売上SEQ" INTEGER NOT NULL,
    "明細ID" TEXT NOT NULL,
    "商品_商品SEQ" INTEGER NOT NULL,
    "区分" INTEGER NOT NULL,
    "売上数量" INTEGER NOT NULL,
    "売上総額_税込" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_売上明細" PRIMARY KEY ("Parent_売上SEQ", "明細ID"),
    CONSTRAINT "FK_商品_売上明細_X84D8979" FOREIGN KEY ("商品_商品SEQ") REFERENCES "商品" ("商品SEQ"),
    CONSTRAINT "FK_売上_売上明細" FOREIGN KEY ("Parent_売上SEQ") REFERENCES "売上" ("売上SEQ") ON DELETE CASCADE
);

INSERT INTO "ef_temp_売上明細" ("Parent_売上SEQ", "明細ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "区分", "商品_商品SEQ", "売上数量", "売上総額_税込")
SELECT "Parent_売上SEQ", "明細ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "区分", IFNULL("商品_商品SEQ", 0), "売上数量", "売上総額_税込"
FROM "売上明細";

CREATE TABLE "ef_temp_売上" (
    "売上SEQ" INTEGER NOT NULL CONSTRAINT "PK_売上" PRIMARY KEY AUTOINCREMENT,
    "売上日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NOT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_売上_XB332A41" FOREIGN KEY ("担当者_従業員番号") REFERENCES "従業員" ("従業員番号")
);

INSERT INTO "ef_temp_売上" ("売上SEQ", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "備考", "売上日時", "担当者_従業員番号")
SELECT "売上SEQ", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "備考", "売上日時", IFNULL("担当者_従業員番号", '')
FROM "売上";

CREATE TABLE "ef_temp_入荷明細" (
    "入荷明細ID" TEXT NOT NULL CONSTRAINT "PK_入荷明細" PRIMARY KEY,
    "入荷_入荷ID" TEXT NULL,
    "在庫調整" TEXT NULL,
    "商品_商品SEQ" INTEGER NOT NULL,
    "仕入単価_税抜" TEXT NULL,
    "消費税区分" INTEGER NULL,
    "入荷数量" INTEGER NOT NULL,
    "残数量" INTEGER NOT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_入荷_入荷明細_X4913180" FOREIGN KEY ("入荷_入荷ID") REFERENCES "入荷" ("入荷ID"),
    CONSTRAINT "FK_商品_入荷明細_X84D8979" FOREIGN KEY ("商品_商品SEQ") REFERENCES "商品" ("商品SEQ")
);

INSERT INTO "ef_temp_入荷明細" ("入荷明細ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "仕入単価_税抜", "備考", "入荷_入荷ID", "入荷数量", "商品_商品SEQ", "在庫調整", "残数量", "消費税区分")
SELECT "入荷明細ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "仕入単価_税抜", "備考", "入荷_入荷ID", "入荷数量", IFNULL("商品_商品SEQ", 0), "在庫調整", "残数量", "消費税区分"
FROM "入荷明細";

CREATE TABLE "ef_temp_入荷" (
    "入荷ID" TEXT NOT NULL CONSTRAINT "PK_入荷" PRIMARY KEY,
    "入荷日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NOT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_入荷_XB332A41" FOREIGN KEY ("担当者_従業員番号") REFERENCES "従業員" ("従業員番号")
);

INSERT INTO "ef_temp_入荷" ("入荷ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "備考", "入荷日時", "担当者_従業員番号")
SELECT "入荷ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "備考", "入荷日時", IFNULL("担当者_従業員番号", '')
FROM "入荷";

CREATE TABLE "ef_temp_在庫調整" (
    "在庫調整ID" TEXT NOT NULL CONSTRAINT "PK_在庫調整" PRIMARY KEY,
    "在庫調整日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NOT NULL,
    "商品_商品SEQ" INTEGER NULL,
    "増減数" INTEGER NULL,
    "絶対数" INTEGER NULL,
    "在庫調整理由" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_商品_在庫調整_X84D8979" FOREIGN KEY ("商品_商品SEQ") REFERENCES "商品" ("商品SEQ"),
    CONSTRAINT "FK_従業員_在庫調整_XB332A41" FOREIGN KEY ("担当者_従業員番号") REFERENCES "従業員" ("従業員番号")
);

INSERT INTO "ef_temp_在庫調整" ("在庫調整ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "商品_商品SEQ", "在庫調整日時", "在庫調整理由", "増減数", "担当者_従業員番号", "絶対数")
SELECT "在庫調整ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "商品_商品SEQ", "在庫調整日時", "在庫調整理由", "増減数", IFNULL("担当者_従業員番号", ''), "絶対数"
FROM "在庫調整";

CREATE TABLE "ef_temp_セッション" (
    "セッションキー" TEXT NOT NULL CONSTRAINT "PK_セッション" PRIMARY KEY,
    "ユーザ_従業員番号" TEXT NOT NULL,
    "最終ログイン日時" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_セッション_X809B944" FOREIGN KEY ("ユーザ_従業員番号") REFERENCES "従業員" ("従業員番号")
);

INSERT INTO "ef_temp_セッション" ("セッションキー", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "ユーザ_従業員番号", "最終ログイン日時")
SELECT "セッションキー", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", IFNULL("ユーザ_従業員番号", ''), "最終ログイン日時"
FROM "セッション";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "売上明細";

ALTER TABLE "ef_temp_売上明細" RENAME TO "売上明細";

DROP TABLE "売上";

ALTER TABLE "ef_temp_売上" RENAME TO "売上";

DROP TABLE "入荷明細";

ALTER TABLE "ef_temp_入荷明細" RENAME TO "入荷明細";

DROP TABLE "入荷";

ALTER TABLE "ef_temp_入荷" RENAME TO "入荷";

DROP TABLE "在庫調整";

ALTER TABLE "ef_temp_在庫調整" RENAME TO "在庫調整";

DROP TABLE "セッション";

ALTER TABLE "ef_temp_セッション" RENAME TO "セッション";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_売上明細_商品_商品SEQ" ON "売上明細" ("商品_商品SEQ");

CREATE INDEX "IX_売上_担当者_従業員番号" ON "売上" ("担当者_従業員番号");

CREATE INDEX "IX_入荷明細_商品_商品SEQ" ON "入荷明細" ("商品_商品SEQ");

CREATE INDEX "IX_入荷明細_入荷_入荷ID" ON "入荷明細" ("入荷_入荷ID");

CREATE INDEX "IX_入荷_担当者_従業員番号" ON "入荷" ("担当者_従業員番号");

CREATE INDEX "IX_在庫調整_商品_商品SEQ" ON "在庫調整" ("商品_商品SEQ");

CREATE INDEX "IX_在庫調整_担当者_従業員番号" ON "在庫調整" ("担当者_従業員番号");

CREATE INDEX "IX_セッション_ユーザ_従業員番号" ON "セッション" ("ユーザ_従業員番号");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260112091202_00002_Update', '9.0.3');


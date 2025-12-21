CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;

CREATE TABLE "従業員" (
    "従業員番号" TEXT NOT NULL CONSTRAINT "PK_従業員" PRIMARY KEY,
    "氏名" TEXT NOT NULL,
    "パスワード" BLOB NOT NULL,
    "SALT" BLOB NOT NULL,
    "入荷担当" INTEGER NOT NULL,
    "販売担当" INTEGER NOT NULL,
    "システム管理者" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "商品" (
    "商品SEQ" INTEGER NOT NULL CONSTRAINT "PK_商品" PRIMARY KEY AUTOINCREMENT,
    "外部システム側ID" TEXT NOT NULL,
    "商品名" TEXT NOT NULL,
    "売値単価_税抜" TEXT NULL,
    "消費税区分" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "セッション" (
    "セッションキー" TEXT NOT NULL CONSTRAINT "PK_セッション" PRIMARY KEY,
    "ユーザ_従業員番号" TEXT NULL,
    "最終ログイン日時" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_セッション_X809B944" FOREIGN KEY ("ユーザ_従業員番号") REFERENCES "従業員" ("従業員番号")
);

CREATE TABLE "入荷" (
    "入荷ID" TEXT NOT NULL CONSTRAINT "PK_入荷" PRIMARY KEY,
    "入荷日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_入荷_XB332A41" FOREIGN KEY ("担当者_従業員番号") REFERENCES "従業員" ("従業員番号")
);

CREATE TABLE "売上" (
    "売上SEQ" INTEGER NOT NULL CONSTRAINT "PK_売上" PRIMARY KEY AUTOINCREMENT,
    "売上日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_従業員_売上_XB332A41" FOREIGN KEY ("担当者_従業員番号") REFERENCES "従業員" ("従業員番号")
);

CREATE TABLE "在庫調整" (
    "在庫調整ID" TEXT NOT NULL CONSTRAINT "PK_在庫調整" PRIMARY KEY,
    "在庫調整日時" TEXT NOT NULL,
    "担当者_従業員番号" TEXT NULL,
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

CREATE TABLE "入荷明細" (
    "入荷明細ID" TEXT NOT NULL CONSTRAINT "PK_入荷明細" PRIMARY KEY,
    "入荷_入荷ID" TEXT NULL,
    "在庫調整" TEXT NULL,
    "商品_商品SEQ" INTEGER NULL,
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

CREATE TABLE "売上明細" (
    "Parent_売上SEQ" INTEGER NOT NULL,
    "明細ID" TEXT NOT NULL,
    "商品_商品SEQ" INTEGER NULL,
    "区分" INTEGER NOT NULL,
    "売上数量" INTEGER NOT NULL,
    "売上総額_税込" TEXT NOT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_売上明細" PRIMARY KEY ("Parent_売上SEQ", "明細ID"),
    CONSTRAINT "FK_商品_売上明細_X84D8979" FOREIGN KEY ("商品_商品SEQ") REFERENCES "商品" ("商品SEQ"),
    CONSTRAINT "FK_売上_売上明細" FOREIGN KEY ("Parent_売上SEQ") REFERENCES "売上" ("売上SEQ") ON DELETE CASCADE
);

CREATE TABLE "在庫調整引当明細" (
    "Parent_在庫調整ID" TEXT NOT NULL,
    "入荷明細_入荷明細ID" TEXT NOT NULL,
    "引当数" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_在庫調整引当明細" PRIMARY KEY ("Parent_在庫調整ID", "入荷明細_入荷明細ID"),
    CONSTRAINT "FK_入荷明細_在庫調整引当明細_X726F99E" FOREIGN KEY ("入荷明細_入荷明細ID") REFERENCES "入荷明細" ("入荷明細ID"),
    CONSTRAINT "FK_在庫調整_在庫調整引当明細" FOREIGN KEY ("Parent_在庫調整ID") REFERENCES "在庫調整" ("在庫調整ID") ON DELETE CASCADE
);

CREATE TABLE "引当明細" (
    "Parent_Parent_売上SEQ" INTEGER NOT NULL,
    "Parent_明細ID" TEXT NOT NULL,
    "入荷_入荷明細ID" TEXT NOT NULL,
    "引当数量" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_引当明細" PRIMARY KEY ("Parent_Parent_売上SEQ", "Parent_明細ID", "入荷_入荷明細ID"),
    CONSTRAINT "FK_入荷明細_引当明細_X4913180" FOREIGN KEY ("入荷_入荷明細ID") REFERENCES "入荷明細" ("入荷明細ID"),
    CONSTRAINT "FK_売上明細_引当明細" FOREIGN KEY ("Parent_Parent_売上SEQ", "Parent_明細ID") REFERENCES "売上明細" ("Parent_売上SEQ", "明細ID") ON DELETE CASCADE
);

CREATE INDEX "IX_セッション_ユーザ_従業員番号" ON "セッション" ("ユーザ_従業員番号");

CREATE INDEX "IX_引当明細_入荷_入荷明細ID" ON "引当明細" ("入荷_入荷明細ID");

CREATE INDEX "IX_在庫調整_商品_商品SEQ" ON "在庫調整" ("商品_商品SEQ");

CREATE INDEX "IX_在庫調整_担当者_従業員番号" ON "在庫調整" ("担当者_従業員番号");

CREATE INDEX "IX_在庫調整引当明細_入荷明細_入荷明細ID" ON "在庫調整引当明細" ("入荷明細_入荷明細ID");

CREATE INDEX "IX_入荷_担当者_従業員番号" ON "入荷" ("担当者_従業員番号");

CREATE INDEX "IX_入荷明細_商品_商品SEQ" ON "入荷明細" ("商品_商品SEQ");

CREATE INDEX "IX_入荷明細_入荷_入荷ID" ON "入荷明細" ("入荷_入荷ID");

CREATE INDEX "IX_売上_担当者_従業員番号" ON "売上" ("担当者_従業員番号");

CREATE INDEX "IX_売上明細_商品_商品SEQ" ON "売上明細" ("商品_商品SEQ");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251221045152_00001_販売管理システム', '9.0.3');

COMMIT;


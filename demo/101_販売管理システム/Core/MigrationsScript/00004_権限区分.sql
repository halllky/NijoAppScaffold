BEGIN TRANSACTION;
CREATE TABLE "権限" (
    "Parent_従業員番号" TEXT NOT NULL,
    "権限区分" INTEGER NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_権限" PRIMARY KEY ("Parent_従業員番号", "権限区分"),
    CONSTRAINT "FK_従業員_権限" FOREIGN KEY ("Parent_従業員番号") REFERENCES "従業員" ("従業員番号") ON DELETE CASCADE
);

CREATE TABLE "ef_temp_従業員" (
    "従業員番号" TEXT NOT NULL CONSTRAINT "PK_従業員" PRIMARY KEY,
    "氏名" TEXT NOT NULL,
    "パスワード" BLOB NOT NULL,
    "SALT" BLOB NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_従業員" ("従業員番号", "CreateUser", "CreatedAt", "SALT", "UpdateUser", "UpdatedAt", "Version", "パスワード", "氏名")
SELECT "従業員番号", "CreateUser", "CreatedAt", "SALT", "UpdateUser", "UpdatedAt", "Version", "パスワード", "氏名"
FROM "従業員";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "従業員";

ALTER TABLE "ef_temp_従業員" RENAME TO "従業員";

COMMIT;

PRAGMA foreign_keys = 1;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261031102231_00004_権限区分', '10.0.12');


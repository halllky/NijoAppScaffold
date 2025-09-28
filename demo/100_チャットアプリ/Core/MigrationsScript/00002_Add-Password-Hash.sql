BEGIN TRANSACTION;
ALTER TABLE "アカウント" ADD "パスワードハッシュ" BLOB NULL;

CREATE TABLE "ef_temp_アカウント" (
    "アカウントID" TEXT NOT NULL CONSTRAINT "PK_アカウント" PRIMARY KEY,
    "アカウント名" TEXT NOT NULL,
    "パスワード" TEXT NULL,
    "パスワードハッシュ" BLOB NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "ef_temp_アカウント" ("アカウントID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "アカウント名", "パスワード", "パスワードハッシュ")
SELECT "アカウントID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "アカウント名", "パスワード", "パスワードハッシュ"
FROM "アカウント";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "アカウント";

ALTER TABLE "ef_temp_アカウント" RENAME TO "アカウント";

COMMIT;

PRAGMA foreign_keys = 1;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250928000118_00002_Add-Password-Hash', '9.0.3');


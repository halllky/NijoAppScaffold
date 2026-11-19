BEGIN TRANSACTION;
CREATE TABLE "添付ファイル" (
    "識別子" TEXT NOT NULL CONSTRAINT "PK_添付ファイル" PRIMARY KEY,
    "名前" TEXT NOT NULL,
    "サイズ" INTEGER NOT NULL,
    "種類" TEXT NOT NULL,
    "状態" INTEGER NOT NULL,
    "参照なし日時" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261119131157_00009_添付ファイル', '10.0.12');

COMMIT;


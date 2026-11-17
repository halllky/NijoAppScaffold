BEGIN TRANSACTION;
CREATE TABLE "ジョブ" (
    "ジョブID" TEXT NOT NULL CONSTRAINT "PK_ジョブ" PRIMARY KEY,
    "種類" TEXT NOT NULL,
    "引数" TEXT NULL,
    "状態" INTEGER NOT NULL,
    "起動したユーザー" TEXT NULL,
    "開始日時" TEXT NULL,
    "終了日時" TEXT NULL,
    "実行したサーバー" TEXT NULL,
    "結果メッセージ" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE INDEX "IX_ジョブ_状態_登録日時" ON "ジョブ" ("状態", "CreatedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261117183722_00008_ジョブ', '10.0.12');

COMMIT;


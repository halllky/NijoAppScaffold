BEGIN TRANSACTION;
ALTER TABLE "セッション" RENAME COLUMN "最終ログイン日時" TO "最終操作日時";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261114122518_00005_20261114', '10.0.12');

COMMIT;


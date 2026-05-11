BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_商品_外部システム側ID" ON "商品" ("外部システム側ID");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260223071143_00003_ユニーク制約の例', '9.0.3');

COMMIT;


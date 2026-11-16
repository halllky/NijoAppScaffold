BEGIN TRANSACTION;
CREATE TABLE "通知" (
    "通知ID" TEXT NOT NULL CONSTRAINT "PK_通知" PRIMARY KEY,
    "宛先" TEXT NOT NULL,
    "本文" TEXT NOT NULL,
    "リンク先" TEXT NULL,
    "既読日時" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE INDEX "IX_通知_宛先_既読日時" ON "通知" ("宛先", "既読日時");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261116172843_00007_通知', '10.0.12');

COMMIT;


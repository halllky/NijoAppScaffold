BEGIN TRANSACTION;
CREATE TABLE "冪等キー" (
    "キー" TEXT NOT NULL CONSTRAINT "PK_冪等キー" PRIMARY KEY,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261115132127_00006_冪等キー', '10.0.12');

COMMIT;


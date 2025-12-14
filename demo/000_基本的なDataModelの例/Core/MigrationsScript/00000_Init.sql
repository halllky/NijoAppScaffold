CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "部署" (
    "部署ID" INTEGER NOT NULL CONSTRAINT "PK_部署" PRIMARY KEY AUTOINCREMENT,
    "部署名" TEXT NOT NULL,
    "事業所_事業所ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "社員" (
    "社員ID" INTEGER NOT NULL CONSTRAINT "PK_社員" PRIMARY KEY AUTOINCREMENT,
    "氏名" TEXT NOT NULL,
    "所属部署_部署ID" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_部署_社員_XA26A7C4" FOREIGN KEY ("所属部署_部署ID") REFERENCES "部署" ("部署ID")
);

CREATE INDEX "IX_社員_所属部署_部署ID" ON "社員" ("所属部署_部署ID");

CREATE INDEX "IX_部署_事業所_事業所ID" ON "部署" ("事業所_事業所ID");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251214000616_00000_Init', '9.0.3');

COMMIT;


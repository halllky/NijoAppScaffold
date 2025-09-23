CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250921102815_00000_Init', '9.0.3');

CREATE TABLE "アカウント" (
    "アカウントID" TEXT NOT NULL CONSTRAINT "PK_アカウント" PRIMARY KEY,
    "アカウント名" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "チャンネル" (
    "チャンネルID" TEXT NOT NULL CONSTRAINT "PK_チャンネル" PRIMARY KEY,
    "チャンネル名" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "メッセージ" (
    "メッセージSEQ" INTEGER NOT NULL CONSTRAINT "PK_メッセージ" PRIMARY KEY AUTOINCREMENT,
    "本文" TEXT NULL,
    "記載者_アカウントID" TEXT NULL,
    "チャンネル_チャンネルID" TEXT NULL,
    "チャンネル直下か" INTEGER NULL,
    "返信先メッセージSEQ" TEXT NULL,
    "編集済みか" INTEGER NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "FK_アカウント_メッセージ_XA91C946" FOREIGN KEY ("記載者_アカウントID") REFERENCES "アカウント" ("アカウントID"),
    CONSTRAINT "FK_チャンネル_メッセージ_XCC521C9" FOREIGN KEY ("チャンネル_チャンネルID") REFERENCES "チャンネル" ("チャンネルID")
);

CREATE INDEX "IX_メッセージ_チャンネル_チャンネルID" ON "メッセージ" ("チャンネル_チャンネルID");

CREATE INDEX "IX_メッセージ_記載者_アカウントID" ON "メッセージ" ("記載者_アカウントID");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250923002832_00001_Init2', '9.0.3');

COMMIT;


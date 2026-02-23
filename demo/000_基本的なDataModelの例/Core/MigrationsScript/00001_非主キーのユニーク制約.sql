BEGIN TRANSACTION;
CREATE TABLE "旧システム部署情報" (
    "旧システムコード" TEXT NOT NULL CONSTRAINT "PK_旧システム部署情報" PRIMARY KEY,
    "名称" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL
);

CREATE TABLE "課" (
    "Parent_部署ID" INTEGER NOT NULL,
    "コード" TEXT NOT NULL,
    "旧システムコード_旧システムコード" TEXT NULL,
    "課名称" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_課" PRIMARY KEY ("Parent_部署ID", "コード"),
    CONSTRAINT "FK_旧システム部署情報_課_X93285BC" FOREIGN KEY ("旧システムコード_旧システムコード") REFERENCES "旧システム部署情報" ("旧システムコード"),
    CONSTRAINT "FK_部署_課" FOREIGN KEY ("Parent_部署ID") REFERENCES "部署" ("部署ID") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_課_旧システムコード_旧システムコード" ON "課" ("旧システムコード_旧システムコード");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260223070856_00001_非主キーのユニーク制約', '9.0.3');

COMMIT;


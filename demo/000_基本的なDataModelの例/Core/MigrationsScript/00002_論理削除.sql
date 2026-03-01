BEGIN TRANSACTION;
CREATE TABLE "係" (
    "Parent_Parent_部署ID" INTEGER NOT NULL,
    "Parent_コード" TEXT NOT NULL,
    "連番" INTEGER NOT NULL,
    "係名称" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_係" PRIMARY KEY ("Parent_Parent_部署ID", "Parent_コード", "連番"),
    CONSTRAINT "FK_課_係" FOREIGN KEY ("Parent_Parent_部署ID", "Parent_コード") REFERENCES "課" ("Parent_部署ID", "コード") ON DELETE CASCADE
);

CREATE TABLE "部署_DELETED" (
    "DeletedUuid" TEXT NOT NULL CONSTRAINT "PK_部署_DELETED" PRIMARY KEY,
    "部署ID" INTEGER NULL,
    "部署名" TEXT NULL,
    "事業所_事業所ID" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NULL,
    "DeletedAt" TEXT NULL,
    "DeletedUser" TEXT NULL
);

CREATE TABLE "課_DELETED" (
    "DeletedUuid" TEXT NOT NULL,
    "Parent_部署ID" INTEGER NULL,
    "コード" TEXT NOT NULL,
    "旧システムコード_旧システムコード" TEXT NULL,
    "課名称" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_課_DELETED" PRIMARY KEY ("DeletedUuid", "コード"),
    CONSTRAINT "FK_DELETED_部署_課" FOREIGN KEY ("DeletedUuid") REFERENCES "部署_DELETED" ("DeletedUuid") ON DELETE CASCADE
);

CREATE TABLE "係_DELETED" (
    "DeletedUuid" TEXT NOT NULL,
    "Parent_Parent_部署ID" INTEGER NULL,
    "Parent_コード" TEXT NOT NULL,
    "連番" INTEGER NOT NULL,
    "係名称" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_係_DELETED" PRIMARY KEY ("DeletedUuid", "Parent_コード", "連番"),
    CONSTRAINT "FK_DELETED_課_係" FOREIGN KEY ("DeletedUuid", "Parent_コード") REFERENCES "課_DELETED" ("DeletedUuid", "コード") ON DELETE CASCADE
);

CREATE INDEX "IX_部署_DELETED_DELETED_AT" ON "部署_DELETED" ("DeletedAt");

CREATE TABLE "ef_temp_課" (
    "Parent_部署ID" INTEGER NOT NULL,
    "コード" TEXT NOT NULL,
    "旧システムコード_旧システムコード" TEXT NULL,
    "課名称" TEXT NOT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_課" PRIMARY KEY ("Parent_部署ID", "コード"),
    CONSTRAINT "FK_旧システム部署情報_課_X93285BC" FOREIGN KEY ("旧システムコード_旧システムコード") REFERENCES "旧システム部署情報" ("旧システムコード"),
    CONSTRAINT "FK_部署_課" FOREIGN KEY ("Parent_部署ID") REFERENCES "部署" ("部署ID") ON DELETE CASCADE
);

INSERT INTO "ef_temp_課" ("Parent_部署ID", "コード", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "旧システムコード_旧システムコード", "課名称")
SELECT "Parent_部署ID", "コード", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "旧システムコード_旧システムコード", IFNULL("課名称", '')
FROM "課";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "課";

ALTER TABLE "ef_temp_課" RENAME TO "課";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE UNIQUE INDEX "IX_課_旧システムコード_旧システムコード" ON "課" ("旧システムコード_旧システムコード");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260301005958_00002_論理削除', '9.0.3');


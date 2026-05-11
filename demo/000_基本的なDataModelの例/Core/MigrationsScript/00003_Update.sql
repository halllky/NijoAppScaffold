BEGIN TRANSACTION;
ALTER TABLE "社員" ADD "契約種別_区分値" TEXT NULL;

ALTER TABLE "社員" ADD "汎用マスタDbEntity区分値" TEXT NULL;

ALTER TABLE "社員" ADD "汎用マスタDbEntity汎用種別" TEXT NULL;

ALTER TABLE "係_DELETED" ADD "勤怠管理区分_区分値" TEXT NULL;

ALTER TABLE "係" ADD "勤怠管理区分_区分値" TEXT NULL;

ALTER TABLE "係" ADD "汎用マスタDbEntity区分値" TEXT NULL;

ALTER TABLE "係" ADD "汎用マスタDbEntity汎用種別" TEXT NULL;

CREATE TABLE "汎用マスタ" (
    "汎用種別" TEXT NOT NULL,
    "区分値" TEXT NOT NULL,
    "表示名称" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    CONSTRAINT "PK_汎用マスタ" PRIMARY KEY ("汎用種別", "区分値")
);

CREATE INDEX "IX_社員_契約種別_区分値" ON "社員" ("契約種別_区分値");

CREATE INDEX "IX_社員_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" ON "社員" ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値");

CREATE INDEX "IX_係_勤怠管理区分_区分値" ON "係" ("勤怠管理区分_区分値");

CREATE INDEX "IX_係_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" ON "係" ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値");

CREATE TABLE "ef_temp_社員" (
    "社員ID" INTEGER NOT NULL CONSTRAINT "PK_社員" PRIMARY KEY AUTOINCREMENT,
    "氏名" TEXT NOT NULL,
    "所属部署_部署ID" INTEGER NULL,
    "契約種別_区分値" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "Version" INTEGER NOT NULL,
    "汎用マスタDbEntity区分値" TEXT NULL,
    "汎用マスタDbEntity汎用種別" TEXT NULL,
    CONSTRAINT "FK_社員_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" FOREIGN KEY ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値") REFERENCES "汎用マスタ" ("汎用種別", "区分値"),
    CONSTRAINT "FK_部署_社員_XA26A7C4" FOREIGN KEY ("所属部署_部署ID") REFERENCES "部署" ("部署ID")
);

INSERT INTO "ef_temp_社員" ("社員ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "契約種別_区分値", "所属部署_部署ID", "氏名", "汎用マスタDbEntity区分値", "汎用マスタDbEntity汎用種別")
SELECT "社員ID", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "Version", "契約種別_区分値", "所属部署_部署ID", "氏名", "汎用マスタDbEntity区分値", "汎用マスタDbEntity汎用種別"
FROM "社員";

CREATE TABLE "ef_temp_係_DELETED" (
    "DeletedUuid" TEXT NOT NULL,
    "Parent_Parent_部署ID" INTEGER NULL,
    "Parent_コード" TEXT NOT NULL,
    "連番" INTEGER NOT NULL,
    "係名称" TEXT NULL,
    "勤怠管理区分_区分値" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_係_DELETED" PRIMARY KEY ("DeletedUuid", "Parent_コード", "連番"),
    CONSTRAINT "FK_DELETED_課_係" FOREIGN KEY ("DeletedUuid", "Parent_コード") REFERENCES "課_DELETED" ("DeletedUuid", "コード") ON DELETE CASCADE
);

INSERT INTO "ef_temp_係_DELETED" ("DeletedUuid", "Parent_コード", "連番", "CreateUser", "CreatedAt", "Parent_Parent_部署ID", "UpdateUser", "UpdatedAt", "係名称", "勤怠管理区分_区分値")
SELECT "DeletedUuid", "Parent_コード", "連番", "CreateUser", "CreatedAt", "Parent_Parent_部署ID", "UpdateUser", "UpdatedAt", "係名称", "勤怠管理区分_区分値"
FROM "係_DELETED";

CREATE TABLE "ef_temp_係" (
    "Parent_Parent_部署ID" INTEGER NOT NULL,
    "Parent_コード" TEXT NOT NULL,
    "連番" INTEGER NOT NULL,
    "係名称" TEXT NOT NULL,
    "勤怠管理区分_区分値" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    "汎用マスタDbEntity区分値" TEXT NULL,
    "汎用マスタDbEntity汎用種別" TEXT NULL,
    CONSTRAINT "PK_係" PRIMARY KEY ("Parent_Parent_部署ID", "Parent_コード", "連番"),
    CONSTRAINT "FK_係_汎用マスタ_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" FOREIGN KEY ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値") REFERENCES "汎用マスタ" ("汎用種別", "区分値"),
    CONSTRAINT "FK_課_係" FOREIGN KEY ("Parent_Parent_部署ID", "Parent_コード") REFERENCES "課" ("Parent_部署ID", "コード") ON DELETE CASCADE
);

INSERT INTO "ef_temp_係" ("Parent_Parent_部署ID", "Parent_コード", "連番", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "係名称", "勤怠管理区分_区分値", "汎用マスタDbEntity区分値", "汎用マスタDbEntity汎用種別")
SELECT "Parent_Parent_部署ID", "Parent_コード", "連番", "CreateUser", "CreatedAt", "UpdateUser", "UpdatedAt", "係名称", "勤怠管理区分_区分値", "汎用マスタDbEntity区分値", "汎用マスタDbEntity汎用種別"
FROM "係";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "社員";

ALTER TABLE "ef_temp_社員" RENAME TO "社員";

DROP TABLE "係_DELETED";

ALTER TABLE "ef_temp_係_DELETED" RENAME TO "係_DELETED";

DROP TABLE "係";

ALTER TABLE "ef_temp_係" RENAME TO "係";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_社員_契約種別_区分値" ON "社員" ("契約種別_区分値");

CREATE INDEX "IX_社員_所属部署_部署ID" ON "社員" ("所属部署_部署ID");

CREATE INDEX "IX_社員_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" ON "社員" ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値");

CREATE INDEX "IX_係_勤怠管理区分_区分値" ON "係" ("勤怠管理区分_区分値");

CREATE INDEX "IX_係_汎用マスタDbEntity汎用種別_汎用マスタDbEntity区分値" ON "係" ("汎用マスタDbEntity汎用種別", "汎用マスタDbEntity区分値");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260502122127_00003_Update', '9.0.3');


BEGIN TRANSACTION;
CREATE TABLE "添付書類" (
    "Parent_入荷ID" TEXT NOT NULL,
    "添付書類ID" TEXT NOT NULL,
    "ファイル_識別子" TEXT NOT NULL,
    "備考" TEXT NULL,
    "CreatedAt" TEXT NULL,
    "CreateUser" TEXT NULL,
    "UpdatedAt" TEXT NULL,
    "UpdateUser" TEXT NULL,
    CONSTRAINT "PK_添付書類" PRIMARY KEY ("Parent_入荷ID", "添付書類ID"),
    CONSTRAINT "FK_入荷_添付書類" FOREIGN KEY ("Parent_入荷ID") REFERENCES "入荷" ("入荷ID") ON DELETE CASCADE,
    CONSTRAINT "FK_添付ファイル_添付書類_XF12EBE1" FOREIGN KEY ("ファイル_識別子") REFERENCES "添付ファイル" ("識別子")
);

CREATE INDEX "IX_添付書類_ファイル_識別子" ON "添付書類" ("ファイル_識別子");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261119131832_00010_入荷の添付書類', '10.0.12');

COMMIT;


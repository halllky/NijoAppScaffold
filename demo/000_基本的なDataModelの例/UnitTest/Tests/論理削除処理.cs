using MyApp;
using MyApp.Debugging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

public class 論理削除処理 {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 部署の論理削除でDeletedテーブルに移送される() {
        var scope = TestUtilImpl.Instance.CreateScope("論理削除処理");
        var db = scope.App.DbContext;

        await using var transaction = await db.Database.BeginTransactionAsync();

        const string legacyCode = "DEL-001";
        const int departmentId = 301;

        var legacy = db.StartCreating旧システム部署情報();
        legacy.旧システムコード = legacyCode;
        legacy.名称 = "論理削除用旧システム部署";
        (await db.TrySaveAsync(legacy.AssertValid())).AssertSucceeded();

        var department = db.StartCreating部署();
        department.部署ID = departmentId;
        department.部署名 = "論理削除対象部署";
        department.事業所 = new() { 事業所ID = "1" };
        department.課 = [new() {
            コード = "DEL-SEC",
            旧システムコード = new() { 旧システムコード = legacyCode },
            課名称 = "論理削除対象課",
            係 = [
                new() { 連番 = 1, 係名称 = "係A", 勤怠管理区分 = null },
                new() { 連番 = 2, 係名称 = "係B", 勤怠管理区分 = null },
            ],
        }];
        var saved = (await db.TrySaveAsync(department.AssertValid())).AssertSucceeded();

        (await db.TrySaveAsync(saved.ToDeleteCommand().AssertValid())).AssertSucceeded();

        Assert.That(await db.部署DbSet.AnyAsync(x => x.部署ID == departmentId), Is.False, "論理削除後も部署が残っています。");
        Assert.That(await db.課DbSet.AnyAsync(x => x.Parent_部署ID == departmentId), Is.False, "論理削除後も課が残っています。");
        Assert.That(await db.係DbSet.AnyAsync(x => x.Parent_Parent_部署ID == departmentId), Is.False, "論理削除後も係が残っています。");

        var deletedDepartment = await db.部署DeletedDbSet
            .Include(x => x.課)
            .ThenInclude(x => x.係)
            .SingleAsync(x => x.部署ID == departmentId);

        Assert.That(deletedDepartment.DeletedUuid, Is.Not.Null, "DeletedUuid が設定されていません。");
        Assert.That(deletedDepartment.部署名, Is.EqualTo("論理削除対象部署"));

        var deletedSection = deletedDepartment.課.Single();
        Assert.That(deletedSection.Parent_部署ID, Is.EqualTo(departmentId));
        Assert.That(deletedSection.DeletedUuid, Is.EqualTo(deletedDepartment.DeletedUuid));
        Assert.That(deletedSection.課名称, Is.EqualTo("論理削除対象課"));

        var deletedLines = deletedSection.係.OrderBy(x => x.連番).ToList();
        Assert.That(deletedLines.Count, Is.EqualTo(2));
        Assert.That(deletedLines[0].DeletedUuid, Is.EqualTo(deletedDepartment.DeletedUuid));
        Assert.That(deletedLines[0].Parent_コード, Is.EqualTo("DEL-SEC"));
        Assert.That(deletedLines[0].連番, Is.EqualTo(1));
        Assert.That(deletedLines[0].係名称, Is.EqualTo("係A"));
        Assert.That(deletedLines[1].DeletedUuid, Is.EqualTo(deletedDepartment.DeletedUuid));
        Assert.That(deletedLines[1].Parent_コード, Is.EqualTo("DEL-SEC"));
        Assert.That(deletedLines[1].連番, Is.EqualTo(2));
        Assert.That(deletedLines[1].係名称, Is.EqualTo("係B"));

        await transaction.CommitAsync();
    }
}

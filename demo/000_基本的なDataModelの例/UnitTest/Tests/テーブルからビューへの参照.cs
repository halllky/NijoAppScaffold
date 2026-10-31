using MyApp;
using MyApp.Debugging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

public class テーブルからビューへの参照 {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 部署起点で事業所ビューを参照できる() {
        var scope = TestUtilImpl.Instance.CreateScope("テーブルからビューへの参照/部署起点");
        var db = scope.App.DbContext;

        await using var transaction = await db.Database.BeginTransactionAsync();

        const int departmentId = 501;
        const string officeId = "1";

        await Create部署Async(db, departmentId, "部署と事業所の結合テスト", officeId, "VIEW-SEC1", "ビュー検証課");

        await transaction.CommitAsync();

        var department = await db.部署DbSet
            .Include(x => x.事業所)
            .SingleAsync(x => x.部署ID == departmentId);

        Assert.That(department.事業所, Is.Not.Null, "部署から事業所へのナビゲーションが解決できません。");
        Assert.That(department.事業所!.事業所ID, Is.EqualTo(officeId));
        Assert.That(department.事業所!.事業所名, Is.EqualTo("東京本社"));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 事業所ビュー起点で部署を参照できる() {
        var scope = TestUtilImpl.Instance.CreateScope("テーブルからビューへの参照/ビュー起点");
        var db = scope.App.DbContext;

        await using var transaction = await db.Database.BeginTransactionAsync();

        const int departmentId = 502;
        const string officeId = "2";

        await Create部署Async(db, departmentId, "ビュー起点部署", officeId, "VIEW-SEC2", "ビュー逆参照課");

        await transaction.CommitAsync();

        var office = await db.事業所DbSet
            .Include(x => x.RefFrom部署_事業所)
            .SingleAsync(x => x.事業所ID == officeId);

        var department = office.RefFrom部署_事業所.SingleOrDefault(x => x.部署ID == departmentId);

        Assert.That(department, Is.Not.Null, "事業所から部署へのナビゲーションが解決できません。");
        Assert.That(department!.部署名, Is.EqualTo("ビュー起点部署"));
        Assert.That(department.事業所_事業所ID, Is.EqualTo(officeId));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 事業所ビューに存在しないコードでも部署が取得できる() {
        var scope = TestUtilImpl.Instance.CreateScope("テーブルからビューへの参照/ビュー未登録コード");
        var db = scope.App.DbContext;

        await using var transaction = await db.Database.BeginTransactionAsync();

        const int departmentId = 503;
        const string missingOfficeId = "999";

        await Create部署Async(db, departmentId, "存在しない事業所を参照する部署", missingOfficeId, "VIEW-SEC3", "未登録事業所課");

        await transaction.CommitAsync();

        var departments = await db.部署DbSet
            .Include(x => x.事業所)
            .Where(x => x.部署ID == departmentId)
            .ToListAsync();

        Assert.That(departments.Count, Is.EqualTo(1), "ビューに存在しないコードで部署が取得できません。");
        Assert.That(departments[0].事業所_事業所ID, Is.EqualTo(missingOfficeId));
        Assert.That(departments[0].事業所, Is.Null, "ビューに存在しないコードでナビゲーションが解決されてしまっています。");
    }

    private static async Task Create部署Async(OverridedDbContext db, int departmentId, string name, string officeId, string sectionCode, string sectionName) {
        var department = db.StartCreating部署();
        department.部署ID = departmentId;
        department.部署名 = name;
        department.事業所 = new() { 事業所ID = officeId };
        department.課 = [new() {
            コード = sectionCode,
            旧システムコード = new() { 旧システムコード = null },
            課名称 = sectionName,
            係 = [],
        }];
        (await db.TrySaveAsync(department)).AssertSucceeded();
    }
}

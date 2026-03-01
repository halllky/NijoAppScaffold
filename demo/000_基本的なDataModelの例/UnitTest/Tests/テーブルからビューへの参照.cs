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
        var presentationContext = new PresentationContextInUnitTest {
            ValidationOnly = false,
            Confirms = [],
            Messages = MessageSetter.GetImpl<IMessageSetter>([], new()),
        };

        await using var transaction = await scope.App.DbContext.Database.BeginTransactionAsync();

        const int departmentId = 501;
        const string officeId = "1";

        var createResult = await scope.App.Create部署Async(new() {
            部署ID = departmentId,
            部署名 = "部署と事業所の結合テスト",
            事業所 = new() { 事業所ID = officeId },
            課 = [new() {
                コード = "VIEW-SEC1",
                旧システムコード = new() { 旧システムコード = null },
                課名称 = "ビュー検証課",
                係 = [],
            }],
        }, presentationContext);

        Assert.That(createResult.Result, Is.EqualTo(DataModelSaveResultType.Completed), "部署の作成に失敗しました。");

        await transaction.CommitAsync();

        var department = await scope.App.DbContext.部署DbSet
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
        var presentationContext = new PresentationContextInUnitTest {
            ValidationOnly = false,
            Confirms = [],
            Messages = MessageSetter.GetImpl<IMessageSetter>([], new()),
        };

        await using var transaction = await scope.App.DbContext.Database.BeginTransactionAsync();

        const int departmentId = 502;
        const string officeId = "2";

        var createResult = await scope.App.Create部署Async(new() {
            部署ID = departmentId,
            部署名 = "ビュー起点部署",
            事業所 = new() { 事業所ID = officeId },
            課 = [new() {
                コード = "VIEW-SEC2",
                課名称 = "ビュー逆参照課",
                旧システムコード = new() { 旧システムコード = null },
                係 = [],
            }],
        }, presentationContext);

        Assert.That(createResult.Result, Is.EqualTo(DataModelSaveResultType.Completed), "部署の作成に失敗しました。");

        await transaction.CommitAsync();

        var office = await scope.App.DbContext.事業所DbSet
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
        var presentationContext = new PresentationContextInUnitTest {
            ValidationOnly = false,
            Confirms = [],
            Messages = MessageSetter.GetImpl<IMessageSetter>([], new()),
        };

        await using var transaction = await scope.App.DbContext.Database.BeginTransactionAsync();

        const int departmentId = 503;
        const string missingOfficeId = "999";

        var createResult = await scope.App.Create部署Async(new() {
            部署ID = departmentId,
            部署名 = "存在しない事業所を参照する部署",
            事業所 = new() { 事業所ID = missingOfficeId },
            課 = [new() {
                コード = "VIEW-SEC3",
                旧システムコード = new() { 旧システムコード = null },
                課名称 = "未登録事業所課",
                係 = [],
            }],
        }, presentationContext);

        Assert.That(createResult.Result, Is.EqualTo(DataModelSaveResultType.Completed), "部署の作成に失敗しました。");

        await transaction.CommitAsync();

        var departments = await scope.App.DbContext.部署DbSet
            .Include(x => x.事業所)
            .Where(x => x.部署ID == departmentId)
            .ToListAsync();

        Assert.That(departments.Count, Is.EqualTo(1), "ビューに存在しないコードで部署が取得できません。");
        Assert.That(departments[0].事業所_事業所ID, Is.EqualTo(missingOfficeId));
        Assert.That(departments[0].事業所, Is.Null, "ビューに存在しないコードでナビゲーションが解決されてしまっています。");
    }
}

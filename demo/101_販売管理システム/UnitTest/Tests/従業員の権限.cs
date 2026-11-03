using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp.UnitTest;

/// <summary>
/// DBに接続せずに確認できる、権限の判定のテスト
/// </summary>
public class 従業員の権限 {

    [Test]
    public void システム管理者はすべての権限をもつ() {
        var user = CreateLoginUser(権限区分.システム管理者);

        Assert.That(user.Isシステム管理者, Is.True);
        Assert.That(Enum.GetValues<権限区分>().All(user.Has権限), Is.True);
    }

    [Test]
    public void 権限がなければ何の権限ももたない() {
        var user = CreateLoginUser();

        Assert.That(user.Isシステム管理者, Is.False);
        Assert.That(Enum.GetValues<権限区分>().Any(user.Has権限), Is.False);
    }

    private static LoginUser CreateLoginUser(params 権限区分[] 権限) => new() {
        従業員番号 = "E1",
        氏名 = "氏名",
        権限 = 権限,
    };
}

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 従業員一括更新で権限を追加と削除できる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(従業員一括更新で権限を追加と削除できる));
        await new DummyDataGeneratorInUnitTest(new CommandQueryContext { ValidationOnly = false }).GenerateAsync(scope.App);

        // 新規追加
        await Execute従業員一括更新Async(scope.App, new 更新対象従業員一覧DisplayData {
            ExistsInDatabase = false,
            従業員 = new() { 従業員番号 = "E1", 氏名 = "氏名", 従業員マスタの権限 = [new() { 権限区分 = 権限区分.システム管理者 }] },
        });
        Assert.That(await Load権限Async(scope.App.DbContext, "E1"), Is.EqualTo(new[] { 権限区分.システム管理者 }));

        // 権限をなくす
        var saved = await scope.App.DbContext.従業員DbSet.AsNoTracking().SingleAsync(e => e.従業員番号 == "E1");
        await Execute従業員一括更新Async(scope.App, new 更新対象従業員一覧DisplayData {
            ExistsInDatabase = true,
            WillBeChanged = true,
            従業員 = new() { 従業員番号 = "E1", 氏名 = "氏名", Version = saved.Version, 従業員マスタの権限 = [] },
        });
        Assert.That(await Load権限Async(scope.App.DbContext, "E1"), Is.Empty);
    }

    private static async Task Execute従業員一括更新Async(OverridedApplicationService app, 更新対象従業員一覧DisplayData row) {
        var messages = new 従業員一括更新ParameterMessages([], new());
        var context = new CommandQueryContext { ValidationOnly = false };
        await app.Execute従業員一括更新Async(new() { 更新対象従業員一覧 = [row] }, messages, context);
        Assert.That(messages.HasError(), Is.False, TestFailureMessage.Build("従業員一括更新に失敗しました。", context, messages));
    }

    private static async Task<権限区分?[]> Load権限Async(OverridedDbContext db, string 従業員番号) {
        var employee = await db.従業員DbSet
            .AsNoTracking()
            .Include(e => e.従業員の権限)
            .SingleAsync(e => e.従業員番号 == 従業員番号);
        return employee.従業員の権限.Select(x => x.権限区分).ToArray();
    }
}

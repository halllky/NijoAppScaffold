using MyApp.Core.Authorization;
using 呼び出し可能な利用者 = MyApp.NijoAttr.呼び出し可能な利用者Attribute.E_Value;

namespace MyApp.UnitTest;

/// <summary>
/// 処理に必要な権限の宣言（AUZ-007）
/// </summary>
public partial class 処理の呼び出し可否 {

    [Test]
    [Category("DB接続なし")]
    public void 権限で指定する処理だけが必要な権限を持つ() {
        var inconsistent = Operation<OperationContext>.All
            .Where(op => (op.Metadata.呼び出し可能な利用者 == 呼び出し可能な利用者.権限で指定) != (op.Metadata.必要な権限.Count > 0))
            .Select(op => op.Name)
            .ToArray();

        Assert.That(inconsistent, Is.Empty, "「権限で指定」の処理には必要な権限を1つ以上、それ以外の処理には0個指定すること");
    }

    [Test]
    [Category("DB接続なし")]
    public void 参照検索は参照先の一覧検索の宣言を引き継ぐ() {
        var load = GetMetadata("従業員マスタ");
        var loadRefs = GetMetadata("ref-to:従業員マスタ");

        Assert.That(loadRefs.呼び出し可能な利用者, Is.EqualTo(load.呼び出し可能な利用者));
        Assert.That(loadRefs.必要な権限, Is.EquivalentTo(load.必要な権限));
    }

    [Test]
    [Category("DB接続なし")]
    public void 権限を持たないユーザーはログインしていれば呼び出せる処理だけを呼び出せる() {
        var user = CreateUser();

        Assert.That(GetMetadata("パスワード変更").CanBeInvokedBy(user), Is.True);
        Assert.That(GetMetadata("従業員一括更新").CanBeInvokedBy(user), Is.False);
    }

    [Test]
    [Category("DB接続なし")]
    public void システム管理者はすべての処理を呼び出せる() {
        var admin = CreateUser(権限区分.システム管理者);
        var forbidden = Operation<OperationContext>.All
            .Where(op => !op.Metadata.CanBeInvokedBy(admin))
            .Select(op => op.Name)
            .ToArray();

        Assert.That(forbidden, Is.Empty);
    }

    [Test]
    [Category("DB接続なし")]
    public void ログインしていなければログイン不要の処理だけを呼び出せる() {
        var invokable = Operation<OperationContext>.All
            .Where(op => op.Metadata.呼び出し可能な利用者 != 呼び出し可能な利用者.内部処理のみ)
            .Where(op => op.Metadata.CanBeInvokedBy(null))
            .Select(op => op.Metadata.呼び出し可能な利用者)
            .Distinct()
            .ToArray();

        Assert.That(invokable, Is.EqualTo(new[] { 呼び出し可能な利用者.ログイン不要 }));
    }

    private static OperationMetadata GetMetadata(string name) {
        return Operation<OperationContext>.All.Single(op => op.Name == name).Metadata;
    }

    private static LoginUser CreateUser(params 権限区分[] 権限) => new() {
        従業員番号 = "USER",
        氏名 = "ユーザー",
        権限 = 権限,
    };
}

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public void 呼び出せない処理は本体を実行する前に例外になる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(呼び出せない処理は本体を実行する前に例外になる));
        var messages = new 従業員マスタSearchConditionMessages([], new());
        var context = new OperationContext { ValidationOnly = false };

        // ログインしていないので、システム管理者の権限が必要な一覧検索は呼び出せない
        Assert.ThrowsAsync<OperationForbiddenException>(() => scope.App.LoadAsync(new 従業員マスタSearchCondition(), messages, context));
    }
}

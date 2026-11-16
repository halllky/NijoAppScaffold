using MyApp.Core.Authorization;

namespace MyApp.UnitTest;

partial class 処理の呼び出し可否 {

    [Test]
    [Category("DB接続なし")]
    public void 必要な権限のいずれかを持つユーザーだけが呼び出せる() {
        var 商品一覧 = GetMetadata("商品一覧");

        Assert.That(商品一覧.CanBeInvokedBy(CreateUser(権限区分.入荷担当)), Is.True);
        Assert.That(商品一覧.CanBeInvokedBy(CreateUser(権限区分.販売担当)), Is.True);
        Assert.That(GetMetadata("入荷登録").CanBeInvokedBy(CreateUser(権限区分.販売担当)), Is.False);
        Assert.That(商品一覧.CanBeInvokedBy(CreateUser()), Is.False);
    }
}

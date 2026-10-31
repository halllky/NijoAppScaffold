using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ダミーデータの従業員に販売管理業務の権限が割り当てられる() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ダミーデータの従業員に販売管理業務の権限が割り当てられる));
        await new DummyDataGeneratorInUnitTest(messages => new PresentationContextInUnitTest<MessageSetter> {
            ValidationOnly = false,
            Confirms = [],
            Messages = messages.As<MessageSetter>(),
        }).GenerateAsync(scope.App);

        var user006 = await scope.App.DbContext.従業員DbSet
            .Include(e => e.従業員の権限)
            .SingleAsync(e => e.従業員番号 == "user006");

        Assert.That(
            user006.従業員の権限.Select(x => x.権限区分),
            Is.EquivalentTo(new 権限区分?[] { 権限区分.入荷担当, 権限区分.販売担当 }));
    }
}

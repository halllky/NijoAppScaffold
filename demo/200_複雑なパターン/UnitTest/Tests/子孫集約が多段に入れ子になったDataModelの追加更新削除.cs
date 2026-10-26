using Microsoft.EntityFrameworkCore;
using MyApp.WebApi.Base;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    /// <summary>
    /// Children の中の Child の中の Children のように、子孫集約が多段に入れ子になったデータモデルで、
    /// 途中の Child が存在しない行が混在していても新規登録・更新・論理削除が正常に動作するかを確認する。
    /// </summary>
    [Test]
    public async Task 途中のChildが存在しない行を含む多段の子孫集約の追加更新削除() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(途中のChildが存在しない行を含む多段の子孫集約の追加更新削除));
        var presentationContext = new PresentationContextInUnitTest {
            ValidationOnly = false,
            Confirms = [],
            Messages = MessageSetter.GetImpl<I保守計画SaveCommandMessages>([], new()),
        };

        await using var transaction = await scope.App.DbContext.Database.BeginTransactionAsync();

        // 作業体制がある工程とない工程を混在させて新規登録
        var createResult = await scope.App.Create保守計画Async(new() {
            保守計画ID = "PLAN001",
            計画名 = "定期保守",
            作業工程 = [
                new() {
                    工程番号 = 1,
                    工程名 = "点検",
                    作業手順 = [
                        new() {
                            手順番号 = 1,
                            手順内容 = "外観を確認する",
                            確認項目 = [new() { 確認項目番号 = 1, 確認内容 = "破損がないこと" }],
                        },
                    ],
                    作業体制 = new() {
                        責任者名 = "責任者A",
                        作業者 = [new() { 作業者番号 = 1, 作業者名 = "作業者A" }],
                    },
                },
                new() {
                    工程番号 = 2,
                    工程名 = "清掃",
                    作業手順 = [],
                    作業体制 = null,
                },
            ],
        }, presentationContext);
        Assert.That(createResult.IsSaveCompleted(out var created), Is.True, presentationContext.BuildFailureMessage("保守計画の新規登録に失敗しました。"));

        // 作業体制の有無を入れ替えて更新
        var updateResult = await scope.App.Update保守計画Async(new() {
            保守計画ID = "PLAN001",
            Version = created!.Version,
        }, data => {
            data.作業工程[0].作業体制 = null;
            data.作業工程[1].作業体制 = new() {
                責任者名 = "責任者B",
                作業者 = [new() { 作業者番号 = 1, 作業者名 = "作業者B" }],
            };
        }, presentationContext);
        Assert.That(updateResult.IsSaveCompleted(out var updated), Is.True, presentationContext.BuildFailureMessage("保守計画の更新に失敗しました。"));
        Assert.Multiple(() => {
            Assert.That(scope.App.DbContext.作業体制DbSet.Single().Parent_工程番号, Is.EqualTo(2));
            Assert.That(scope.App.DbContext.作業者DbSet.Single().作業者名, Is.EqualTo("作業者B"));
        });

        // 論理削除
        var deleteResult = await scope.App.SoftDelete保守計画Async(new() {
            保守計画ID = "PLAN001",
            Version = updated!.Version,
        }, presentationContext);
        Assert.That(deleteResult.IsSaveCompleted(), Is.True, presentationContext.BuildFailureMessage("保守計画の論理削除に失敗しました。"));
        Assert.Multiple(() => {
            Assert.That(scope.App.DbContext.保守計画DbSet.Any(), Is.False);
            Assert.That(scope.App.DbContext.作業工程DbSet.Any(), Is.False);
            Assert.That(scope.App.DbContext.作業者DbSet.Any(), Is.False);
            Assert.That(scope.App.DbContext.保守計画DeletedDbSet.Count(), Is.EqualTo(1));
            Assert.That(scope.App.DbContext.作業工程DeletedDbSet.Count(), Is.EqualTo(2));
            Assert.That(scope.App.DbContext.作業者DeletedDbSet.Count(), Is.EqualTo(1));
        });
    }
}

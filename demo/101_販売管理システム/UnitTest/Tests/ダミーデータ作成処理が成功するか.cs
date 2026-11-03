using MyApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ダミーデータ作成処理が成功するか() {
        var scope = TestUtilImpl.Instance.CreateScope("ダミーデータ作成処理が成功するか");

        var context = new CommandQueryContext { ValidationOnly = false };
        var generator = new DummyDataGeneratorInUnitTest(context);

        var result = await generator.GenerateAsync(scope.App);

        Assert.That(
            result.HasError(),
            Is.False,
            TestFailureMessage.Build("ダミーデータの作成に失敗しました。", context, result));
    }
}

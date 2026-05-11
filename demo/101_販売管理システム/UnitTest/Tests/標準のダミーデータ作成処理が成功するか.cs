using MyApp;
using MyApp.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 標準のダミーデータ作成処理が成功するか() {
        var scope = TestUtilImpl.Instance.CreateScope("標準のダミーデータ作成処理が成功するか");

        var generator = new DummyDataGeneratorInUnitTest(messages => new PresentationContextInUnitTest<MessageSetter> {
            ValidationOnly = false,
            Confirms = [],
            Messages = messages.As<MessageSetter>(),
        });

        var result = await generator.GenerateAsync(scope.App);

        Assert.That(
            result.HasError(),
            Is.False,
            "ダミーデータの作成に失敗しました。エラー: " + string.Join(", ", result.GetAllMessages()));
    }
}

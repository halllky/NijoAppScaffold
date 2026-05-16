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
            BuildFailureMessage(result, "ダミーデータの作成に失敗しました。"));
    }

    private static string BuildFailureMessage(IMessageSetter messages, string message) {
        var details = new List<string>();
        var messageState = messages.GetState();

        if (messageState is not null) {
            var nodesWithMessages = messageState
                .DescendantsAndSelf()
                .Where(node => node.Errors.Count > 0 || node.Warns.Count > 0 || node.Infos.Count > 0)
                .ToArray();

            if (nodesWithMessages.Length > 0) {
                details.Add("Messages:");
                foreach (var node in nodesWithMessages) {
                    var path = node.Path.Length == 0 ? "(root)" : string.Join('.', node.Path);
                    foreach (var error in node.Errors) {
                        details.Add($"  [Error] {path}: {error}");
                    }
                    foreach (var warn in node.Warns) {
                        details.Add($"  [Warn] {path}: {warn}");
                    }
                    foreach (var info in node.Infos) {
                        details.Add($"  [Info] {path}: {info}");
                    }
                }
            }
        }

        return details.Count == 0
            ? message
            : $"{message}{Environment.NewLine}{string.Join(Environment.NewLine, details)}";
    }
}

using MyApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.UnitTest;

/// <summary>
/// <see cref="IPresentationContext"/> のユニットテスト用の実装
/// </summary>
internal class PresentationContextInUnitTest : IPresentationContext, IConfirmablePresentationContext {
    public required bool ValidationOnly { get; init; }
    public List<string> Confirms { get; } = [];

    /// <summary>
    /// テスト失敗時のメッセージに、この処理で発生した確認メッセージと、引数のメッセージの内容を付加して返します。
    /// </summary>
    public string BuildFailureMessage(string message, IMessageSetter messages) {
        var details = new List<string>();

        if (Confirms.Count > 0) {
            details.Add("Confirm:");
            details.AddRange(Confirms.Select(confirm => $"  - {confirm}"));
        }

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

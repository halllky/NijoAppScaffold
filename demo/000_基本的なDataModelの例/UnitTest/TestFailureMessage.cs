using MyApp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.UnitTest;

/// <summary>
/// クエリ・コマンドを呼び出すテストが失敗したときに表示するメッセージ。
/// </summary>
internal static class TestFailureMessage {

    /// <summary>
    /// 失敗の説明に、呼び出しで発生した確認メッセージと、引数に対するメッセージの内容を付加して返します。
    /// </summary>
    public static string Build(string message, OperationContext context, IMessageSetter messages) {
        var details = new List<string>();

        if (context.Confirms.Count > 0) {
            details.Add("Confirm:");
            details.AddRange(context.Confirms.Select(confirm => $"  - {confirm}"));
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

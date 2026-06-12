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
    public required IMessageSetter Messages { get; init; }
    public required List<string> Confirms { get; init; }

    public string BuildFailureMessage(string message) {
        var details = new List<string>();

        if (Confirms.Count > 0) {
            details.Add("Confirm:");
            details.AddRange(Confirms.Select(confirm => $"  - {confirm}"));
        }

        var messageState = Messages.GetState();
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

    public void AddConfirm(string text) {
        Confirms.Add(text);
    }
    public bool HasConfirm() {
        return Confirms.Count > 0;
    }

    public IPresentationContext<T> As<T>() where T : IMessageSetter {
        return new PresentationContextInUnitTest<T> {
            Messages = Messages.As<T>(),
            ValidationOnly = ValidationOnly,
            Confirms = Confirms,
        };
    }

    IPresentationContextWithReturnValue<TReturnValue, TMessage> IPresentationContext.AsWithReturnValue<TReturnValue, TMessage>() {
        return new PresentationContextInUnitTest<TReturnValue, TMessage> {
            Messages = Messages.As<TMessage>(),
            ValidationOnly = ValidationOnly,
            Confirms = Confirms,
        };
    }
}

/// <inheritdoc cref="PresentationContextInUnitTest"/>
internal class PresentationContextInUnitTest<TMessage> : PresentationContextInUnitTest, IPresentationContext<TMessage> where TMessage : IMessageSetter {
    TMessage IPresentationContext<TMessage>.Messages => (TMessage)Messages;
}

/// <inheritdoc cref="PresentationContextInUnitTest"/>
internal class PresentationContextInUnitTest<TReturnValue, TMessage> : PresentationContextInUnitTest<TMessage>, IPresentationContextWithReturnValue<TReturnValue, TMessage>
    where TMessage : IMessageSetter
    where TReturnValue : new() {

    public TReturnValue ReturnValue { get; set; } = new();
}

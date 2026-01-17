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

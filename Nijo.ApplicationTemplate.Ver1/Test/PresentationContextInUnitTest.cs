using MyApp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Test;

/// <summary>
/// <see cref="IPresentationContext"/> のユニットテスト用の実装
/// </summary>
internal class PresentationContextInUnitTest : IPresentationContext {
    internal PresentationContextInUnitTest(Type messageRootType, IPresentationContextOptions options) {
        MessageContext = new PresentationMessageContext();
        Messages = MessageContainer.GetDefaultClass(messageRootType, [], MessageContext);
        Options = options;
    }
    protected PresentationContextInUnitTest(PresentationMessageContext messageContext, IMessageContainer messageRoot, IPresentationContextOptions options) {
        MessageContext = messageContext;
        Messages = messageRoot;
        Options = options;
    }

    public IPresentationContextOptions Options { get; }
    public IMessageContainer Messages { get; } // メッセージ設定用ヘルパー
    public PresentationMessageContext MessageContext { get; } // メッセージの格納先
    public List<string> Confirms { get; private set; } = [];

    public void AddConfirm(string text) {
        Confirms.Add(text);
    }
    public bool HasConfirm() {
        return Confirms.Count > 0;
    }
}

/// <inheritdoc cref="PresentationContextInUnitTest"/>
internal class PresentationContextInUnitTest<TMessage> : PresentationContextInUnitTest, IPresentationContext<TMessage> where TMessage : IMessageContainer {
    internal PresentationContextInUnitTest(
        PresentationMessageContext messageContext,
        TMessage messageRoot,
        IPresentationContextOptions options) : base(messageContext, messageRoot, options) { }

    public new TMessage Messages => (TMessage)base.Messages;
    IMessageContainer IPresentationContext.Messages => Messages;
}

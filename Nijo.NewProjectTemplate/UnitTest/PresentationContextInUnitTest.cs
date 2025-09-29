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
internal class PresentationContextInUnitTest : IPresentationContext {
    internal PresentationContextInUnitTest(Type messageRootType, IPresentationContextOptions options) {
        Messages = MessageSetter.GetDefaultClass(messageRootType, [], new PresentationMessageContext());
        Options = options;
    }
    protected PresentationContextInUnitTest(IMessageSetter messageRoot, IPresentationContextOptions options) {
        Messages = messageRoot;
        Options = options;
    }

    public IPresentationContextOptions Options { get; }
    public IMessageSetter Messages { get; }
    public List<string> Confirms { get; private set; } = [];

    public void AddConfirm(string text) {
        Confirms.Add(text);
    }
    public bool HasConfirm() {
        return Confirms.Count > 0;
    }

    public IPresentationContext<TMessageRoot> Cast<TMessageRoot>() where TMessageRoot : IMessageSetter {
        return new PresentationContextInUnitTest<TMessageRoot>((TMessageRoot)Messages, Options);
    }

}

/// <inheritdoc cref="PresentationContextInUnitTest"/>
internal class PresentationContextInUnitTest<TMessage> : PresentationContextInUnitTest, IPresentationContext<TMessage> where TMessage : IMessageSetter {
    internal PresentationContextInUnitTest(TMessage messageRoot, IPresentationContextOptions options) : base(messageRoot, options) { }

    public new TMessage Messages => (TMessage)base.Messages;
    IMessageSetter IPresentationContext.Messages => Messages;
}

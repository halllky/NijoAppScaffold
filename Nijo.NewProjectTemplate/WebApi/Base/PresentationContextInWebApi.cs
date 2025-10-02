using MyApp;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageSetter {

    internal PresentationContextInWebApi(TMessageRoot messageRoot, IPresentationContextOptions options) {
        Messages = messageRoot;
        Options = options;
    }

    public TMessageRoot Messages { get; }

    public IPresentationContextOptions Options { get; }


    #region 確認メッセージ
    internal List<string> Confirms { get; } = [];

    public void AddConfirm(string text) {
        Confirms.Add(text);
    }
    public bool HasConfirm() {
        return Confirms.Count > 0;
    }
    #endregion 確認メッセージ
}

/// <summary>
/// <see cref="IPresentationContextWithReturnValue{TReturnValue, TMessageRoot}"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TReturnValue, TMessageRoot> : PresentationContextInWebApi<TMessageRoot>, IPresentationContextWithReturnValue<TReturnValue, TMessageRoot>
    where TMessageRoot : IMessageSetter
    where TReturnValue : new() {

    internal PresentationContextInWebApi(TMessageRoot messageRoot, IPresentationContextOptions options) : base(messageRoot, options) {
        ReturnValue = new TReturnValue();
    }

    public TReturnValue ReturnValue { get; set; }
}

/// <summary>
/// <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextOptions : IPresentationContextOptions {
    public required bool IgnoreConfirm { get; init; }
}

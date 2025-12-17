using MyApp;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageSetter {

    public required TMessageRoot Messages { get; init; }
    public required IPresentationContextOptions Options { get; init; }


    #region 確認メッセージ
    public required List<string> Confirms { get; init; }

    public void AddConfirm(string text) {
        Confirms.Add(text);
    }
    public bool HasConfirm() {
        return Confirms.Count > 0;
    }
    #endregion 確認メッセージ

    public bool HasError() {
        return Messages.UnderlyingContext.Root.HasError();
    }

    public IPresentationContext<T> As<T>() where T : IMessageSetter {
        return new PresentationContextInWebApi<T> {
            Messages = Messages.As<T>(),
            Options = Options,
            Confirms = Confirms,
        };
    }
}

/// <summary>
/// <see cref="IPresentationContextWithReturnValue{TReturnValue, TMessageRoot}"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TReturnValue, TMessageRoot> : PresentationContextInWebApi<TMessageRoot>, IPresentationContextWithReturnValue<TReturnValue, TMessageRoot>
    where TMessageRoot : IMessageSetter
    where TReturnValue : new() {

    public TReturnValue ReturnValue { get; set; } = new();
}

/// <summary>
/// <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextOptions : IPresentationContextOptions {
    public required bool IgnoreConfirm { get; init; }
}

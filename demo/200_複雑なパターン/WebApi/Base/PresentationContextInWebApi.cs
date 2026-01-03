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

    // この型は戻り値を持たないので常にnullになる。戻り値ありの方（このクラスを継承した方）で使用される。
    public object? ReturnValue { get; set; } = null;

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

    public PresentationContextInWebApi() {
        base.ReturnValue = new TReturnValue();
    }

    TReturnValue IPresentationContextWithReturnValue<TReturnValue, TMessageRoot>.ReturnValue {
        get => (TReturnValue)base.ReturnValue!;
        set => base.ReturnValue = value;
    }
}

/// <summary>
/// <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextOptions : IPresentationContextOptions {
    public required bool IgnoreConfirm { get; init; }
}

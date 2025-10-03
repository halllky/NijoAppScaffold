using MyApp.Core;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageSetter {

    public required TMessageRoot Messages { get; init; } // メッセージ設定用ヘルパー

    public required IPresentationContextOptions Options { get; init; }

    /// <summary>
    /// トーストメッセージ。
    /// UIに依存するため自動生成とは関係ない、カスタマイズ属性。
    /// </summary>
    public string? ToastMessage { get; set; }


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
            ToastMessage = ToastMessage,
            Confirms = Confirms,
        };
    }
}

/// <summary>
/// webapiにおける <see cref="IPresentationContextWithReturnValue{TReturnValue, TMessageRoot}"/> のデフォルトの実装
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

public static class PresentationContextExtensions {
    /// <summary>
    /// トーストメッセージを付加します。
    /// </summary>
    public static void SetToastMessage<T>(this IPresentationContext<T> presentationContext, string text) where T : IMessageSetter {
        if (presentationContext is not PresentationContextInWebApi<T> instance) {
            throw new InvalidOperationException($"インスタンス {presentationContext} は {nameof(PresentationContextInWebApi<T>)} 型ではありません。");
        }
        instance.ToastMessage = text;
    }
}

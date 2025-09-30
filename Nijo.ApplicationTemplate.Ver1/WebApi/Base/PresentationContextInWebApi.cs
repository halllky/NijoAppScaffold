using MyApp.Core;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageSetter {

    internal PresentationContextInWebApi(MessageContainer messageContext, IPresentationContextOptions options) {
        MessageContext = messageContext;
        Messages = MessageSetter.GetImpl<TMessageRoot>([], messageContext);
        Options = options;
    }

    internal MessageContainer MessageContext { get; } // メッセージの格納先
    public TMessageRoot Messages { get; } // メッセージ設定用ヘルパー
    IMessageSetter IPresentationContext.Messages => Messages;

    public IPresentationContextOptions Options { get; }


    /// <summary>
    /// トーストメッセージ。
    /// UIに依存するため自動生成とは関係ない、カスタマイズ属性。
    /// </summary>
    public string? ToastMessage { get; set; }


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

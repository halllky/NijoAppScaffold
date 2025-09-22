using MyApp;
using System.Text.Json.Nodes;

namespace MyApp.WebApi.Base;

/// <summary>
/// webapiにおける <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextInWebApi<TMessageRoot> : IPresentationContext<TMessageRoot>
    where TMessageRoot : IMessageContainer {

    internal PresentationContextInWebApi(TMessageRoot messageRoot, IPresentationContextOptions options) {
        Messages = messageRoot;
        Options = options;
    }

    public TMessageRoot Messages { get; }
    IMessageContainer IPresentationContext.Messages => Messages;

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

    IPresentationContext<TMessageRoot1> IPresentationContext.Cast<TMessageRoot1>() {
        throw new NotImplementedException(
            "WebでCastが必要になるケースは想定外" +
            "（登録更新や検索で発生したエラーを適切に画面上の特定の項目に転送しなければならないので、" +
            "画面のQueryModelやCommandModelは必ず登録更新処理のメッセージコンテナの型を実装しているはず）");
    }
}

/// <summary>
/// <see cref="IPresentationContext"/> のデフォルトの実装
/// </summary>
public class PresentationContextOptions : IPresentationContextOptions {
    public required bool IgnoreConfirm { get; init; }
}

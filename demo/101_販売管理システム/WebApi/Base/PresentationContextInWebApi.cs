namespace MyApp.WebApi.Base;

/// <summary>
/// webapi における <see cref="IPresentationContext"/> のデフォルトの実装。
/// HTTPリクエスト1回分の、処理の呼び出し元としての情報を持つ。
/// </summary>
public class PresentationContextInWebApi : IPresentationContext, IConfirmablePresentationContext {
    /// <summary>
    /// HTTPクライアントから送られてくる、確認メッセージを無視するかどうかのフラグ。
    /// 何らかのデータを更新する画面などで、「保存」などのボタンを押した時、
    /// 1回目のHTTPリクエストでは入力チェックのみを行い、確認メッセージをレスポンスとして返し、
    /// 2回目のHTTPリクエストで再度入力チェックしたうえで実際の更新処理を行う、という2段階の処理を行うことが多い。
    /// このプロパティを介してその挙動を実現する。
    /// </summary>
    public required bool IgnoreConfirm { get; init; }
    bool IPresentationContext.ValidationOnly => !IgnoreConfirm;

    /// <summary>
    /// この処理中で発生した確認メッセージの一覧
    /// </summary>
    public List<string> Confirms { get; } = [];
}

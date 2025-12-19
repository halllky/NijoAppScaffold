namespace MyApp.Core.Authorization;

/// <summary>
/// セッションキーを提供するインターフェース
/// </summary>
public interface ISessionKeyProvider {

    /// <summary>
    /// APサーバー側で発番されたセッションキーをクライアント側に返す。
    /// 次回以降のリクエストで Cookie 等に含めて送信してもらうため。
    /// </summary>
    /// <param name="sessionKey">新たに発番されたセッションキー</param>
    void ReturnSessionKeyToClient(string sessionKey);

    /// <summary>
    /// 現在のログインユーザーのセッションキーを取得する。
    /// ログインしていない場合は null を返す。
    /// </summary>
    string? GetSessionKey();

    /// <summary>
    /// 現在のログインユーザーのセッションキーをクリアする。
    /// ログインされていない場合は何もしない。
    /// </summary>
    void ClearSessionKey();
}

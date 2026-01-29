using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp.WebApi.Authorization;

public class SessionProviderInWebApi : ISessionKeyProvider {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionProviderInWebApi(IHttpContextAccessor httpContextAccessor) {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// セッションキーを保存する Cookie のキー名
    /// </summary>
    private const string SESSION_ITEM_KEY = "SessionProviderInWebApi.Session";

    void ISessionKeyProvider.ReturnSessionKeyToClient(string sessionKey) {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) {
            return;
        }

        // ログイン時に発番されたセッションキーを Set-Cookie ヘッダーでクライアントに返す
        httpContext.Response.Cookies.Append(SESSION_ITEM_KEY, sessionKey, new CookieOptions {
            HttpOnly = true, // JavaScript から参照できないようにする
            SameSite = SameSiteMode.Lax, // クロスサイトリクエストフォージェリ(CSRF)対策
            // Secure = true, // HTTPS 環境の場合は有効化する。デモはHTTP環境なのでコメントアウト
            Path = "/", // アプリケーション全体で有効にする
            MaxAge = null, // ブラウザ終了時に破棄するセッションCookieとする
        });
    }

    string? ISessionKeyProvider.GetSessionKey() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) {
            return null;
        }

        // ログイン時に発番され、APサーバー側で Set-Cookie で設定されるセッションキーを取得する。
        // クライアント側では、 JavaScript の fetch API で credential: true すると Cookie が送信される。
        return httpContext.Request.Cookies.TryGetValue(SESSION_ITEM_KEY, out var sessionKey)
            ? sessionKey
            : null;
    }

    void ISessionKeyProvider.ClearSessionKey() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) {
            return;
        }

        // ログアウト時にセッションキーをクリアする
        httpContext.Response.Cookies.Delete(SESSION_ITEM_KEY, new CookieOptions {
            Path = "/", // Cookie設定時と同じPathを指定すること
        });
    }
}

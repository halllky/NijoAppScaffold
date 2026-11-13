using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp.WebApi.Authorization;

public class SessionProviderInWebApi : ISessionKeyProvider {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;

    public SessionProviderInWebApi(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment) {
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
    }

    /// <summary>
    /// セッションキーを保存する Cookie の名前。
    /// 開発環境以外では頭に __Host- を付け、Secure 属性・Path=/・Domain なしをブラウザに強制させる（サブドメインからの上書きを防ぐ）。
    /// 開発環境は HTTP で動かし Secure 属性を付けられないので、付けない。
    /// </summary>
    private string SessionCookieName => _environment.IsDevelopment() ? SESSION_COOKIE_NAME : $"__Host-{SESSION_COOKIE_NAME}";
    private const string SESSION_COOKIE_NAME = "SessionProviderInWebApi.Session";

    void ISessionKeyProvider.ReturnSessionKeyToClient(string sessionKey) {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) {
            return;
        }

        // ログイン時に発番されたセッションキーを Set-Cookie ヘッダーでクライアントに返す
        httpContext.Response.Cookies.Append(SessionCookieName, sessionKey, new CookieOptions {
            HttpOnly = true, // JavaScript から参照できないようにする
            SameSite = SameSiteMode.Lax, // クロスサイトリクエストフォージェリ(CSRF)対策
            // HTTPS で送られたときだけブラウザが返すようにする。
            // 開発環境は HTTP で動かすので付けない。本番環境は、前段の装置で HTTPS を終端していても、ブラウザから見れば HTTPS なので付けてよい
            Secure = !_environment.IsDevelopment(),
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
        return httpContext.Request.Cookies.TryGetValue(SessionCookieName, out var sessionKey)
            ? sessionKey
            : null;
    }

    void ISessionKeyProvider.ClearSessionKey() {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) {
            return;
        }

        // ログアウト時にセッションキーをクリアする
        httpContext.Response.Cookies.Delete(SessionCookieName, new CookieOptions {
            Path = "/", // Cookie設定時と同じPathを指定すること
            Secure = !_environment.IsDevelopment(), // __Host- の Cookie は Secure 属性が無いと消す指示もブラウザに無視される
        });
    }
}

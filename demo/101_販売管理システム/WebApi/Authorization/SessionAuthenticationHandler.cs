using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MyApp.Core.Authorization;

namespace MyApp.WebApi.Authorization;

/// <summary>
/// セッションキーの Cookie からログインユーザーを特定する、ASP.NET Core の認証処理。
/// ログインしていなければ認証されていないものとし、認証が必要なエンドポイントには 401 を返させる（ASP.NET Core の既定の振る舞い）。
/// ログインユーザーの特定そのものは <see cref="LoginUserProvider"/> が行う。
/// </summary>
public sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    LoginUserProvider loginUserProvider) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {

    /// <summary>この認証処理のスキーム名</summary>
    public const string SCHEME = "Session";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        var loginUser = loginUserProvider.GetLoginUser();
        if (loginUser == null) return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, loginUser.従業員番号)], SCHEME);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SCHEME);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

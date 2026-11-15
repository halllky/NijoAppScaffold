using System.Diagnostics;
using System.Security.Cryptography;

namespace MyApp.WebApi;

/// <summary>
/// HTTP リクエスト1回の中で出力されるログに、ログインユーザー ID・処理名・処理1回分の識別子を付け、
/// リクエストの完了時に結果と処理時間をログに出力する。
/// これにより、多数のユーザーが同時にアプリケーションを利用し、ログが同時に並行で出力される場合でも、
/// ユーザー単位でのログ抽出やHTTPリクエスト単位でのログ抽出が可能になる。
/// 処理1回分の識別子は <see cref="HttpContext.TraceIdentifier"/> にも設定するので、
/// 同じリクエストの中では、どこからでもログと同じ識別子を参照できる。
/// 未処理の例外のログにもこれらの情報が付くよう、未処理の例外を扱う共通の処理より先に登録すること。
/// </summary>
internal class WebLogScopeMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext httpContext) {
        var appSrv = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var currentProcess = httpContext.RequestServices.GetRequiredService<ICurrentProcess>();

        var scopeId = NewScopeId();
        httpContext.TraceIdentifier = scopeId;

        // このスコープのインスタンスが破棄されるまでの間、ログ出力時に以下の情報が常に付与される。
        // ここで設定される情報のキーは NLog の出力設定箇所で指定しているキー名と一致している必要がある。
        using var logScope = appSrv.Log.BeginScope(new Dictionary<string, string?> {
            [OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID] = GetLoginUserIdOrNull(appSrv),
            [OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME] = currentProcess.Name,
            [OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID] = scopeId,
        });

        var stopwatch = Stopwatch.StartNew();
        await next(httpContext);
        stopwatch.Stop();

        // 本番環境で「遅い」「エラーになった」という問い合わせを調べられるよう、リクエストごとに結果と処理時間をログ出力
        appSrv.Log.LogInformation("{Method} {Path} 完了: {Elapsed}ms, Status: {StatusCode}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value,
            stopwatch.ElapsedMilliseconds,
            httpContext.Response.StatusCode);
    }

    /// <summary>
    /// 処理1回分の識別子を新しく作る。
    /// 利用者が問い合わせ番号として口頭でも伝えられるよう、読み違えやすい文字（I・L・O・U）を除いた英大文字と数字の8文字を、
    /// 4文字ずつハイフンで区切った形にする。
    /// 約1兆通りなので、すべてのリクエストを通して一意とは限らないが、ログを日時と合わせて探せば、ほかのリクエストと区別できる。
    /// </summary>
    private static string NewScopeId() {
        const string CHARS = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        var id = RandomNumberGenerator.GetString(CHARS, 8);
        return $"{id[..4]}-{id[4..]}";
    }

    /// <summary>
    /// ログインユーザー ID を返す。ログインしていないときや、ログインユーザーの特定に失敗したとき（DB の障害など）は null を返す。
    /// この処理は未処理の例外を扱う共通の処理より先に動くので、ここで起きた例外はその処理で扱われず、
    /// 利用者に決まった形のエラーを返せない。そのため例外を外に出さない。
    /// 失敗の原因の例外は、この後のログインユーザーの特定で改めて起き、未処理の例外としてログに出力される。
    /// </summary>
    private static string? GetLoginUserIdOrNull(OverridedApplicationService appSrv) {
        try {
            return appSrv.LoginUser?.従業員番号;
        } catch {
            return null;
        }
    }
}

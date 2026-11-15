using System.Diagnostics;

namespace MyApp.WebApi;

/// <summary>
/// HTTP リクエスト1回の中で出力されるログに、ログインユーザー ID・処理名・処理1回分の識別子を付け、
/// リクエストの完了時に結果と処理時間をログに出力する。
/// これにより、多数のユーザーが同時にアプリケーションを利用し、ログが同時に並行で出力される場合でも、
/// ユーザー単位でのログ抽出やHTTPリクエスト単位でのログ抽出が可能になる。
/// 処理1回分の識別子は <see cref="HttpContext.TraceIdentifier"/> にも設定するので、
/// 同じリクエストの中では、どこからでもログと同じ識別子を参照できる。
/// 未処理の例外のログにもこれらの情報が付くよう、未処理の例外を扱う共通の処理より外側に置くこと。
/// </summary>
internal class WebLogScopeMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext httpContext) {
        var appSrv = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var currentProcess = httpContext.RequestServices.GetRequiredService<ICurrentProcess>();

        var scopeId = Guid.NewGuid().ToString("N").ToUpperInvariant();
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
    /// ログインユーザー ID を返す。ログインしていないときや、ログインユーザーの特定に失敗したとき（DB の障害など）は null を返す。
    /// この処理は未処理の例外を扱う共通の処理より外側にあり、ここで例外が起きると利用者に決まった形のエラーを返せないため、例外を外に出さない。
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

using System.Diagnostics;

namespace MyApp.WebApi;

/// <summary>
/// ログ出力するための情報を設定する。
/// このHTTPリクエスト内部で発生したログでは常にここで設定した情報が使用される。
/// これにより、多数のユーザーが同時にアプリケーションを利用し、ログが同時に並行で出力される場合でも、
/// ユーザー単位でのログ抽出やHTTPリクエスト単位でのログ抽出が可能になる。
/// </summary>
internal class WebLogScopeMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext httpContext) {
        var appSrv = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var currentProcess = httpContext.RequestServices.GetRequiredService<ICurrentProcess>();

        // このスコープのインスタンスが破棄されるまでの間、ログ出力時に以下の情報が常に付与される。
        // ここで設定される情報のキーは NLog の出力設定箇所で指定しているキー名と一致している必要がある。
        using var logScope = appSrv.Log.BeginScope(new Dictionary<string, string?> {
            [OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID] = appSrv.LoginUser?.従業員番号,
            [OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME] = currentProcess.Name,
            [OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID] = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
        });

        var stopwatch = Stopwatch.StartNew();
        await next(httpContext);
        stopwatch.Stop();

        // パフォーマンス障害の調査などに備えて処理時間をログ出力
        if (appSrv.Log.IsEnabled(LogLevel.Debug)) {
            appSrv.Log.LogDebug("{Method} 完了: {Elapsed}ms, Status: {StatusCode}",
                httpContext.Request.Method,
                stopwatch.ElapsedMilliseconds,
                httpContext.Response.StatusCode);
        }
    }
}

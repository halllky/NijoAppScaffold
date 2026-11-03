using System.Net;

namespace MyApp.WebApi;

/// <summary>
/// エンドポイント内でハンドルされなかった例外をキャッチし、
/// エラーログの出力と統一されたエラーレスポンスの返却を行う。
/// </summary>
internal class WebExceptionMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext httpContext) {
        try {
            await next(httpContext);

        } catch (Exception ex) {
            var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();

            app.Log.LogError(ex,
                "Unhandled exception: {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            if (httpContext.Response.HasStarted) throw;

            // 例外の内容は利用者に見せない
            httpContext.Response.Clear();
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new { message = "システムエラーが発生しました。" });
        }
    }
}

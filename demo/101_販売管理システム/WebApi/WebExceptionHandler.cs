using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.WebApi;

/// <summary>
/// エンドポイント内でハンドルされなかった例外を、エラーログに出力し、統一されたエラーレスポンス（ProblemDetails）にする。
/// 例外の内容は利用者に見せない。ログと突き合わせられるよう、レスポンスには traceId が付く。
/// </summary>
internal sealed class WebExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken) {
        var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        app.Log.LogError(exception,
            "Unhandled exception: {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new() {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails {
                Status = StatusCodes.Status500InternalServerError,
                Title = "システムエラーが発生しました。",
            },
            Exception = exception,
        });
    }
}

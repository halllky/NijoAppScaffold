using Microsoft.AspNetCore.Authorization;

namespace MyApp.WebApi.Authorization;

/// <summary>
/// ログインしていない状態での Web API へのリクエストを弾く。
/// <see cref="IAllowAnonymous"/> が付与されたエンドポイント（AllowAnonymous 属性や AllowAnonymous() で設定したもの）は対象外。
/// </summary>
public class LoginAuthorizationMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext httpContext) {
        var isWebApi = httpContext.Request.Path.StartsWithSegments("/api");
        var allowAnonymous = httpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null;

        if (isWebApi && !allowAnonymous) {
            var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
            if (app.LoginUser == null) {
                // ログイン状態が切れたことを呼び出し元が他のエラーと区別できるようにする（SES-005）
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next(httpContext);
    }
}

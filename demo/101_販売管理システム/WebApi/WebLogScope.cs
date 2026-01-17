
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyApp.WebApi;

/// <summary>
/// ログ出力するための情報を設定する。
/// このHTTPリクエスト内部で発生したログでは常にここで設定した情報が使用される。
/// これにより、多数のユーザーが同時にアプリケーションを利用し、ログが同時に並行で出力される場合でも、
/// ユーザー単位でのログ抽出やHTTPリクエスト単位でのログ抽出が可能になる。
/// </summary>
internal class WebLogScope : IAsyncActionFilter {
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        var appSrv = context.HttpContext.RequestServices.GetRequiredService<OverridedApplicationService>();

        // このスコープのインスタンスが破棄されるまでの間、ログ出力時に以下の情報が常に付与される。
        // ここで設定される情報のキーは NLog の出力設定箇所で指定しているキー名と一致している必要がある。
        using var logScope = appSrv.Log.BeginScope(new Dictionary<string, string?> {
            ["LoginUserId"] = appSrv.LoginUser?.従業員番号,
            ["ProcessName"] = context.HttpContext.Request.Path.ToString(),
            ["ScopeId"] = Guid.NewGuid().ToString().ToUpper().Replace("-", ""),
        });

        await next();
    }
}

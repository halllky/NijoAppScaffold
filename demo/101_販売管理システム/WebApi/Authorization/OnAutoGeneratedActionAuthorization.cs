using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using MyApp;

namespace MyApp.WebApi.Authorization;

public class LoginAuthorizationFilter : IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        // ログイン不要のエンドポイント
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
            .Any();
        if (allowAnonymous) {
            return;
        }

        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor
            && actionDescriptor.ControllerTypeInfo.AsType() == typeof(ログインController)) {
            return;
        }

        // 上記以外は未ログイン状態でのアクセスを禁止
        var app = context.HttpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        if (app.LoginUser == null) {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}

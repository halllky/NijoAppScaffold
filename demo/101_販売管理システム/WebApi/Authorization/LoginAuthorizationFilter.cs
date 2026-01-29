using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using MyApp;

namespace MyApp.WebApi.Authorization;

public class LoginAuthorizationFilter : IAuthorizationFilter {
    void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context) {
        // ログイン不要のエンドポイント
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
            .Any();
        if (allowAnonymous) {
            return;
        }

        // ログイン不要のコントローラー
        if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor) {
            var controllerType = actionDescriptor.ControllerTypeInfo.AsType();

            if (controllerType == typeof(ログインController)) return;
            if (controllerType == typeof(ログアウトController)) return;
            if (controllerType == typeof(ログイン状態確認Controller)) return;
        }

        // 上記以外は未ログイン状態でのアクセスを禁止
        var app = context.HttpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        if (app.LoginUser == null) {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}

#if DEBUG

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Debugging;
using MyApp.WebApi.Base;

namespace MyApp.WebApi.Debugging;

/// <summary>
/// 開発中にデータベースを作り直すデバッグ用エンドポイント。
/// </summary>
[ApiController]
[Route("api/debug")]
[AllowAnonymous]
public class DebugDatabaseController : ControllerBase {

    /// <summary>
    /// データベースを削除して再作成する。
    /// </summary>
    [HttpPost("destroy-and-recreate-database")]
    public async Task<IActionResult> DestroyAndRecreateDatabase([FromServices] OverridedApplicationService app) {
        try {
            // データベースを削除して再作成
            await app.DbContext.Database.EnsureDeletedAsync();
            await app.DbContext.EnsureCreatedAsyncEx(app.Settings);

            // ダミーデータの生成
            var generator = new OverridedDummyDataGenerator(messages => new PresentationContextInWebApi<MessageSetter> {
                IgnoreConfirm = true,
                Messages = messages.As<MessageSetter>(),
                Confirms = [],
            });
            var result = await generator.GenerateAsync(app);

            if (result.HasError()) {
                return Problem(string.Join("\n", [
                    "ダミーデータの生成に失敗しました。",
                    ..result.GetAllMessages(),
                ]));
            }

            return Ok();

        } catch (Exception ex) {
            return Problem(ex.ToString());
        }
    }
}

#endif

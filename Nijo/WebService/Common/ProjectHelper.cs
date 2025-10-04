using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nijo.WebService.Common;

namespace Nijo.WebService.Common;

/// <summary>
/// プロジェクト関連のヘルパー
/// </summary>
internal static class ProjectHelper {

    /// <summary>
    /// クエリパラメータ名: プロジェクトディレクトリ
    /// ※React側と合わせる必要あり
    /// </summary>
    private const string PROJECT_DIR_PARAMETER = "pj";

    /// <summary>
    /// HttpContextからプロジェクトディレクトリを取得し、GeneratedProjectを作成します。
    /// 失敗した場合は適切なエラーレスポンスを送信します。
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <returns>成功した場合はGeneratedProject、失敗した場合はnull（エラーレスポンスは既に送信済み）</returns>
    internal static async Task<GeneratedProject?> GetProjectAndSetResponseIfErrorAsync(HttpContext context) {
        // クエリパラメータから取得
        if (!HttpResponseHelper.TryGetQueryParameter(context, PROJECT_DIR_PARAMETER, out var projectDir)) {
            context.Response.StatusCode = 400;
            await HttpResponseHelper.WriteErrorResponseAsync(context, 400, $"{PROJECT_DIR_PARAMETER} query parameter is required", context.RequestAborted);
            return null;
        }

        var projectRoot = Path.GetFullPath(projectDir);
        if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
            context.Response.StatusCode = 400;
            await HttpResponseHelper.WriteErrorResponseAsync(context, 400, error ?? "Unknown error", context.RequestAborted);
            return null;
        }

        return project;
    }
}

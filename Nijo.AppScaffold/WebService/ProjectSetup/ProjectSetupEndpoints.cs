using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nijo.WebService.Common;

namespace Nijo.WebService.ProjectSetup;

/// <summary>
/// 編集対象フォルダの状態取得と、そのフォルダの直下へのプロジェクト新規作成のエンドポイントハンドラ
/// </summary>
internal class ProjectSetupEndpoints {

    internal ProjectSetupEndpoints(NijoWebService webService) {
        _webService = webService;
    }

    private readonly NijoWebService _webService;

    /// <summary>
    /// 編集対象フォルダの状態と、新規作成時に選択可能なテンプレートの一覧を返す
    /// </summary>
    internal async Task HandleGetServeTarget(HttpContext context) {
        try {
            var projectRoot = _webService.ProjectRoot;
            var response = new {
                projectRoot,
                directorySeparator = Path.DirectorySeparatorChar.ToString(),
                isNijoProject = GeneratedProject.TryOpen(projectRoot, out _, out _),
                templates = GeneratedProject.Templates
                    .Select(template => new { name = template.Key, description = template.Value.Description })
                    .ToArray(),
            };

            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.WriteJsonAsync(response, cancellationToken: context.RequestAborted);
        } catch (Exception ex) {
            await Console.Error.WriteLineAsync(ex.ToString());

            await context.WriteErrorAsync(
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>
    /// 編集対象フォルダの直下にプロジェクトを新規作成し、以降はそのプロジェクトを編集対象にする
    /// </summary>
    internal async Task HandleCreateProject(HttpContext context) {
        try {
            // このサービスに認証はないため、他サイトのページからのフォームPOSTでファイルが作られないようにする。
            // JSONのリクエストならブラウザがプリフライトを送り、CORS未設定のこのサービスへの送信は拒否される。
            if (!context.Request.HasJsonContentType()) {
                await context.WriteErrorAsync(
                    StatusCodes.Status415UnsupportedMediaType,
                    "application/json でリクエストしてください。",
                    context.RequestAborted);
                return;
            }

            var request = await context.Request.ReadFromJsonAsync<CreateProjectRequest>(context.RequestAborted)
                ?? throw new Exception("request is null");

            var (success, errorMessage) = _webService.CreateProject(request.ProjectName, request.TemplateName);
            if (!success) {
                await context.WriteErrorAsync(
                    (int)HttpStatusCode.BadRequest,
                    errorMessage ?? "プロジェクトの作成に失敗しました。",
                    context.RequestAborted);
                return;
            }

            var logger = context.RequestServices.GetRequiredService<ILogger<NijoWebService>>();
            logger.LogInformation("プロジェクトの作成が完了しました: {projectRoot}", _webService.ProjectRoot);

            context.Response.StatusCode = StatusCodes.Status200OK;
        } catch (Exception ex) {
            await Console.Error.WriteLineAsync(ex.ToString());

            await context.WriteErrorAsync(
                (int)HttpStatusCode.InternalServerError,
                ex.Message,
                context.RequestAborted);
        }
    }

    /// <summary>/nijo-api/create-project のリクエストボディ</summary>
    private class CreateProjectRequest {
        [JsonPropertyName("projectName")]
        public string ProjectName { get; set; } = string.Empty;
        [JsonPropertyName("templateName")]
        public string TemplateName { get; set; } = string.Empty;
    }
}

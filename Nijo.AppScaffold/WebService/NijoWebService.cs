using System;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nijo.WebService.Common;
using Nijo.WebService.Previewing;

namespace Nijo.WebService;

/// <summary>
/// スキーマ定義をGUIで編集するアプリケーションのために、
/// nijo.xml を読み込んだり、バリデーションを行ったり、XMLファイルの保存を行ったりする。
/// 編集対象のフォルダは nijo serve の起動時に1つに決まり、
/// その後変わるのはプロジェクトでないフォルダの直下にプロジェクトを新規作成したときのみ。
/// また、生成後アプリのデバッグプロセス（プレビュー）を nijo serve の生存期間中保持する。
/// プレビューが保持するOSプロセス・ログファイルハンドルを解放するため <see cref="IDisposable"/> を実装する。
/// </summary>
public class NijoWebService : IDisposable {

    /// <param name="projectRoot">編集対象フォルダの絶対パス。nijoプロジェクトでなくてもよく、存在しなくてもよい</param>
    public NijoWebService(string projectRoot) {
        ProjectRoot = projectRoot;
        Preview = new Preview(projectRoot);
    }

    /// <summary>編集対象フォルダの絶対パス</summary>
    public string ProjectRoot { get; private set; }

    /// <summary>編集対象フォルダのプレビュー</summary>
    public Preview Preview { get; private set; }

    /// <summary>新規作成の確認から編集対象の切り替えまでを、同時に来た別の作成要求から守る</summary>
    private readonly System.Threading.Lock _createProjectGate = new();

    /// <summary>
    /// 編集対象フォルダの直下にプロジェクトを新規作成し、以降はそのプロジェクトを編集対象にする。
    /// 編集対象フォルダが既にプロジェクトの場合や、プロジェクト名がフォルダ名として不正な場合は何も作成しない。
    /// </summary>
    /// <param name="projectName">プロジェクトのフォルダ名</param>
    /// <param name="templateName"><see cref="GeneratedProject.Templates"/> のキー</param>
    /// <returns>成功した場合は true、エラーメッセージ付きで失敗した場合は false</returns>
    internal (bool Success, string? ErrorMessage) CreateProject(string projectName, string templateName) {
        lock (_createProjectGate) {
            if (GeneratedProject.TryOpen(ProjectRoot, out _, out _)) {
                return (false, $"既にプロジェクトが存在します: {ProjectRoot}");
            }
            if (!IsValidProjectName(projectName)) {
                return (false, $"プロジェクト名にはフォルダ名として使える名前を指定してください: {projectName}");
            }

            var newProjectRoot = Path.Combine(ProjectRoot, projectName);
            var result = GeneratedProject.CreateProjectFromTemplate(newProjectRoot, templateName);
            if (result.Success) {
                // プロジェクトでなかった間はプレビューを起動できないので、止めるべきプロセスはない
                ProjectRoot = newProjectRoot;
                Preview = new Preview(newProjectRoot);
            }
            return result;
        }
    }

    /// <summary>
    /// 編集対象フォルダの直下に作るフォルダの名前として使えるかを返す。
    /// フォルダの外に作られないよう、パスの区切りや上位フォルダの指定を拒否する。
    /// OSによって末尾の空白とピリオドが取り除かれ別の名前になるので、それらも拒否する。
    /// </summary>
    private static bool IsValidProjectName(string projectName) {
        return !string.IsNullOrWhiteSpace(projectName)
            && projectName == projectName.Trim()
            && !projectName.EndsWith('.')
            && projectName.IndexOfAny(['/', '\\']) < 0
            && projectName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
    }

    /// <summary>
    /// 編集対象フォルダをプロジェクトとして開く。
    /// 失敗した場合は適切なエラーレスポンスを送信する。
    /// </summary>
    /// <returns>成功した場合はGeneratedProject、失敗した場合はnull（エラーレスポンスは既に送信済み）</returns>
    internal async Task<GeneratedProject?> OpenProjectOrWriteErrorAsync(HttpContext context) {
        if (!GeneratedProject.TryOpen(ProjectRoot, out var project, out var error)) {
            await context.WriteErrorAsync(400, error ?? "Unknown error", context.RequestAborted);
            return null;
        }

        return project;
    }

    /// <summary>
    /// IDisposableの実装。using 文や呼び出し漏れに対する最終防衛ラインとして、プレビューを破棄する。
    /// </summary>
    public void Dispose() {
        Preview.Dispose();
    }

    /// <summary>
    /// Reactアプリケーションからのリクエストを受け取るWebサーバーを設定して返す
    /// </summary>
    public WebApplication BuildWebApplication(ILogger logger) {
        logger.LogInformation("Webサービスを構築します。");

        var builder = WebApplication.CreateBuilder();

        // JSONオプション
        builder.Services.ConfigureHttpJsonOptions(options => {
            options.SerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        var app = builder.Build();
        app.UseRouting();

        // スキーマ編集エンドポイント
        var schemaEditor = new SchemaEditor.SchemaEditorEndpoints(this);
        app.MapGet("/nijo-api/load", schemaEditor.HandleLoad);
        app.MapGet("/nijo-api/schema-rule", schemaEditor.HandleGetRule);
        app.MapPost("/nijo-api/validate", schemaEditor.HandleValidate);
        app.MapPost("/nijo-api/save", schemaEditor.HandleSave);
        app.MapPost("/nijo-api/generate", schemaEditor.HandleGenerate);
        app.MapPost("/nijo-api/types", schemaEditor.HandleGetTypes);

        // プレビュー（生成後アプリのデバッグプロセス）エンドポイント
        var previewHandlers = new PreviewEndpointHandlers(this);
        app.MapPost("/nijo-api/preview/start", previewHandlers.HandleStartPreview);
        app.MapPost("/nijo-api/preview/stop", previewHandlers.HandleStopPreview);
        app.MapPost("/nijo-api/preview/state", previewHandlers.HandleGetPreviewState);

        // 編集対象フォルダの状態取得とプロジェクト新規作成エンドポイント
        var projectSetup = new ProjectSetup.ProjectSetupEndpoints(this);
        app.MapGet("/nijo-api/serve-target", projectSetup.HandleGetServeTarget);
        app.MapPost("/nijo-api/create-project", projectSetup.HandleCreateProject);

        // 上位のいずれにも該当しないエンドポイントへのリクエストは
        // React.js のビルド後html（js, css がすべて1つのhtmlファイル内にバンドルされているもの）を返す。
        app.MapGet("/{*path}", ServeReactHtml);

        return app;
    }

    /// <summary>
    /// React.js のビルド後htmlを返す
    /// </summary>
    private static async Task ServeReactHtml(HttpContext context) {
        var assembly = Assembly.GetExecutingAssembly();
        const string RESOURCE_NAME = "Nijo.GuiWebAppHtml.index.html";

        using var stream = assembly.GetManifestResourceStream(RESOURCE_NAME)
            ?? throw new InvalidOperationException($"htmlファイルが見つかりません。{assembly.GetName().Name}のビルド前に 'npm run build:schema-editor' が実行されたか確認してください。");

        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync();

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(html);
    }

}

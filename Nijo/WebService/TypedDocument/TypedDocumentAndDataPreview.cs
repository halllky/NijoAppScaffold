using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Nijo.WebService.Common;

namespace Nijo.WebService.TypedDocument;

/// <summary>
/// 型つきドキュメントとデータプレビューに関するサービスを提供する。
/// </summary>
internal class TypedDocumentAndDataPreview {

    internal TypedDocumentAndDataPreview() {
    }

    private const string SETTINGS_FILE_NAME = "settings.json";
    private const string MEMO_DIRECTORY_NAME = "memo";
    private const string DOCUMENTS_DIRECTORY_NAME = "documents";
    private const string DATA_PREVIEW_DIRECTORY_NAME = "data-previews";
    private const string ATTRIBUTE_NAME_ID = "perspectiveId";
    private const string ATTRIBUTE_NAME_NAME = "name";
    private const string ATTRIBUTE_NAME_DATA_PREVIEW_ID = "id";

    private string GetMemoDirectoryPath(string projectRoot) {
        var memoDir = Path.Combine(projectRoot, MEMO_DIRECTORY_NAME);
        if (!Directory.Exists(memoDir)) {
            Directory.CreateDirectory(memoDir);
        }
        return memoDir;
    }

    private string GetSettingsFilePath(string projectRoot) {
        return Path.Combine(GetMemoDirectoryPath(projectRoot), SETTINGS_FILE_NAME);
    }
    private string GetMemoFilePath(string projectRoot, string entityId) {
        FileStorageService.ThrowIfInvalidFileIdentifier(entityId, nameof(entityId));
        return Path.Combine(GetMemoDirectoryPath(projectRoot), DOCUMENTS_DIRECTORY_NAME, $"{entityId}.json");
    }

    private string GetDataPreviewFilePath(string projectRoot, string dataPreviewId) {
        FileStorageService.ThrowIfInvalidFileIdentifier(dataPreviewId, nameof(dataPreviewId));
        return Path.Combine(GetMemoDirectoryPath(projectRoot), DATA_PREVIEW_DIRECTORY_NAME, $"{dataPreviewId}.json");
    }

    internal void ConfigureWebApplication(WebApplication app) {
        app.MapGet("/api/typed-document/load-settings", LoadSettings);
        app.MapPost("/api/typed-document/save-settings", SaveSettings);
        app.MapGet("/api/typed-document/load", LoadTypedDocument);
        app.MapPost("/api/typed-document/save", SaveTypedDocument);
        app.MapGet("/api/data-preview/load", LoadDataPreview);
        app.MapPost("/api/data-preview/save", SaveDataPreview);
    }

    /// <summary>
    /// アプリケーション全体の設定を読み込む。
    /// エンティティ種類の一覧はディレクトリ内にあるJSONファイルを正とする。
    /// アプリケーション名や、エンティティ種類の並べ方の順番は、設定ファイルの内容を読み込んで取得する。
    /// </summary>
    private async Task LoadSettings(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;
        var memoDir = GetMemoDirectoryPath(projectRoot);

        var appSettingsForDisplay = new JsonObject();
        var entityTypeOrder = new List<string>(); // 型つきドキュメントの順番
        var dataPreviewOrder = new List<string>(); // データプレビューの順番

        // settings.json
        var settingsFilePath = GetSettingsFilePath(projectRoot);
        var settingsData = await FileStorageService.LoadJsonAsync(settingsFilePath, context.RequestAborted);
        if (settingsData != null) {
            appSettingsForDisplay["applicationName"] = settingsData.TryGetPropertyValue("applicationName", out var applicationName)
                ? applicationName?.ToString()
                : "";
            entityTypeOrder = settingsData.TryGetPropertyValue("entityTypeOrder", out var entityTypeOrderJson)
                ? entityTypeOrderJson.Deserialize<List<string>>()!
                : new List<string>();
            dataPreviewOrder = settingsData.TryGetPropertyValue("dataPreviewOrder", out var dataPreviewOrderJson)
                ? dataPreviewOrderJson.Deserialize<List<string>>()!
                : new List<string>();
        }

        // 型つきドキュメントの一覧
        var typedDocumentDirectory = Path.Combine(GetMemoDirectoryPath(projectRoot), DOCUMENTS_DIRECTORY_NAME);
        var entityTypeList = await FileStorageService.ListJsonFilesAsync(
            typedDocumentDirectory,
            (fileName, data) => new JsonObject {
                ["entityTypeId"] = fileName,
                ["entityTypeName"] = data[ATTRIBUTE_NAME_NAME]?.ToString() ?? fileName,
            },
            Console.WriteLine,
            context.RequestAborted);

        // データプレビューの一覧
        var dataPreviewDirectory = Path.Combine(GetMemoDirectoryPath(projectRoot), DATA_PREVIEW_DIRECTORY_NAME);
        var dataPreviewList = await FileStorageService.ListJsonFilesAsync(
            dataPreviewDirectory,
            (fileName, data) => new JsonObject {
                ["id"] = fileName,
                ["title"] = data.TryGetPropertyValue("title", out var title) ? title?.ToString() : fileName,
            },
            Console.WriteLine,
            context.RequestAborted);

        // 設定ファイル未指定の場合
        if (!appSettingsForDisplay.TryGetPropertyValue("applicationName", out var _)) {
            appSettingsForDisplay["applicationName"] = "アプリケーション名未設定";
        }

        // エンティティ種類の一覧を、保存された順番で並べ替える。
        var orderedTypedDocuments = entityTypeList
            .OrderBy(e => {
                var index = entityTypeOrder.IndexOf(e["entityTypeId"]?.ToString() ?? "");
                return index == -1 ? int.MaxValue : index;
            })
            .ToList();
        var entityTypeListJsonArray = new JsonArray();
        foreach (var entityType in orderedTypedDocuments) {
            entityTypeListJsonArray.Add(entityType);
        }
        appSettingsForDisplay["entityTypeList"] = entityTypeListJsonArray;

        // データプレビューの一覧を、保存された順番で並べ替える。
        var orderedDataPreviews = dataPreviewList
            .OrderBy(d => {
                var index = dataPreviewOrder.IndexOf(d["id"]?.ToString() ?? "");
                return index == -1 ? int.MaxValue : index;
            })
            .ToList();
        var dataPreviewListJsonArray = new JsonArray();
        foreach (var dataPreview in orderedDataPreviews) {
            dataPreviewListJsonArray.Add(dataPreview);
        }
        appSettingsForDisplay["dataPreviewList"] = dataPreviewListJsonArray;

        context.Response.StatusCode = StatusCodes.Status200OK;
        await HttpResponseHelper.WriteJsonResponseAsync(context, appSettingsForDisplay, FileStorageService.JsonOptions, context.RequestAborted);
    }

    private async Task SaveSettings(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;

        var data = await context.Request.ReadFromJsonAsync<JsonObject>(FileStorageService.JsonOptions, context.RequestAborted);
        if (data == null) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Request body is empty.", context.RequestAborted);
            return;
        }

        var filePath = GetSettingsFilePath(projectRoot);
        await FileStorageService.SaveJsonAsync(filePath, data, context.RequestAborted);
    }

    private async Task LoadTypedDocument(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;

        if (!HttpResponseHelper.TryGetQueryParameter(context, "typeId", out var typeId)) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Query parameter 'typeId' is required.", context.RequestAborted);
            return;
        }

        var filePath = GetMemoFilePath(projectRoot, typeId);
        var data = await FileStorageService.LoadJsonAsync(filePath, context.RequestAborted);

        if (data == null) {
            await HttpResponseHelper.WriteNotFoundAsync(context, cancellationToken: context.RequestAborted);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        await HttpResponseHelper.WriteJsonResponseAsync(context, data, FileStorageService.JsonOptions, context.RequestAborted);
    }

    private async Task SaveTypedDocument(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;

        JsonObject? data;
        try {
            data = await context.Request.ReadFromJsonAsync<JsonObject>(FileStorageService.JsonOptions, context.RequestAborted);
        } catch (Exception ex) {
            await HttpResponseHelper.WriteBadRequestAsync(context, $"Invalid JSON format: {ex.Message}", context.RequestAborted);
            return;
        }

        if (data == null) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Request body is empty.", context.RequestAborted);
            return;
        }

        var entityId = data[ATTRIBUTE_NAME_ID]?.ToString();
        if (string.IsNullOrEmpty(entityId)) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Request body is empty or 'entityId' is missing.", context.RequestAborted);
            return;
        }

        try {
            var filePath = GetMemoFilePath(projectRoot, entityId);
            await FileStorageService.SaveJsonAsync(filePath, data, context.RequestAborted);
            await HttpResponseHelper.WriteSuccessMessageAsync(context, "Data saved successfully.", context.RequestAborted);
        } catch (Exception ex) {
            await HttpResponseHelper.WriteInternalServerErrorAsync(context, $"Failed to save data: {ex.Message}", context.RequestAborted);
        }
    }

    private async Task LoadDataPreview(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;

        if (!HttpResponseHelper.TryGetQueryParameter(context, "dataPreviewId", out var dataPreviewId)) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Query parameter 'dataPreviewId' is required.", context.RequestAborted);
            return;
        }

        var filePath = GetDataPreviewFilePath(projectRoot, dataPreviewId);
        var data = await FileStorageService.LoadJsonAsync(filePath, context.RequestAborted);

        if (data == null) {
            await HttpResponseHelper.WriteNotFoundAsync(context, cancellationToken: context.RequestAborted);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        await HttpResponseHelper.WriteJsonResponseAsync(context, data, FileStorageService.JsonOptions, context.RequestAborted);
    }

    private async Task SaveDataPreview(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var projectRoot = project.ProjectRoot;

        JsonObject? data;
        try {
            data = await context.Request.ReadFromJsonAsync<JsonObject>(FileStorageService.JsonOptions, context.RequestAborted);
        } catch (Exception ex) {
            await HttpResponseHelper.WriteBadRequestAsync(context, $"Invalid JSON format: {ex.Message}", context.RequestAborted);
            return;
        }

        if (data == null) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Request body is empty.", context.RequestAborted);
            return;
        }

        var dataPreviewId = data[ATTRIBUTE_NAME_DATA_PREVIEW_ID]?.ToString();
        if (string.IsNullOrEmpty(dataPreviewId)) {
            await HttpResponseHelper.WriteBadRequestAsync(context, "Request body is empty or 'dataPreviewId' is missing.", context.RequestAborted);
            return;
        }

        try {
            var filePath = GetDataPreviewFilePath(projectRoot, dataPreviewId);
            await FileStorageService.SaveJsonAsync(filePath, data, context.RequestAborted);
            await HttpResponseHelper.WriteSuccessMessageAsync(context, "Data saved successfully.", context.RequestAborted);
        } catch (Exception ex) {
            await HttpResponseHelper.WriteInternalServerErrorAsync(context, $"Failed to save data: {ex.Message}", context.RequestAborted);
        }
    }
}


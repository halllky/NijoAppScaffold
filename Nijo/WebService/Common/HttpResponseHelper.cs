using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Nijo.WebService.Common;

/// <summary>
/// HTTPレスポンスの共通処理を提供する
/// </summary>
internal static class HttpResponseHelper {

    /// <summary>
    /// JSONレスポンスを返す。
    /// ステータスコードは呼び出し側で事前に設定する必要がある。
    /// </summary>
    internal static async Task WriteJsonResponseAsync<T>(
        HttpContext context,
        T data,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default) {

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(data, options ?? FileStorageService.JsonOptions, cancellationToken);
    }

    /// <summary>
    /// エラーレスポンス（テキスト）を返す
    /// </summary>
    internal static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string errorMessage,
        CancellationToken cancellationToken = default) {

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(errorMessage, cancellationToken);
    }

    /// <summary>
    /// Bad Request (400) レスポンスを返す
    /// </summary>
    internal static Task WriteBadRequestAsync(
        HttpContext context,
        string errorMessage,
        CancellationToken cancellationToken = default) {

        return WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, errorMessage, cancellationToken);
    }

    /// <summary>
    /// Not Found (404) レスポンスを返す
    /// </summary>
    internal static Task WriteNotFoundAsync(
        HttpContext context,
        string errorMessage = "File not found.",
        CancellationToken cancellationToken = default) {

        return WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, errorMessage, cancellationToken);
    }

    /// <summary>
    /// Internal Server Error (500) レスポンスを返す
    /// </summary>
    internal static Task WriteInternalServerErrorAsync(
        HttpContext context,
        string errorMessage,
        CancellationToken cancellationToken = default) {

        return WriteErrorResponseAsync(context, StatusCodes.Status500InternalServerError, errorMessage, cancellationToken);
    }

    /// <summary>
    /// 成功レスポンス（テキスト）を返す
    /// </summary>
    internal static async Task WriteSuccessMessageAsync(
        HttpContext context,
        string message,
        CancellationToken cancellationToken = default) {

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(message, cancellationToken);
    }

    /// <summary>
    /// クエリパラメータを取得する。存在しない場合はBad Requestを返す
    /// </summary>
    internal static bool TryGetQueryParameter(
        HttpContext context,
        string parameterName,
        out string value) {

        var values = context.Request.Query[parameterName];
        var firstValue = values.FirstOrDefault();

        if (string.IsNullOrEmpty(firstValue)) {
            value = string.Empty;
            return false;
        }

        value = firstValue;
        return true;
    }
}


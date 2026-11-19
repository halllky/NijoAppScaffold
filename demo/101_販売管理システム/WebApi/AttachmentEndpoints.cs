using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http.Features;

namespace MyApp.WebApi;

/// <summary>
/// 添付ファイルの一時保存とダウンロードのエンドポイント。どちらもログインが必要。
/// </summary>
public static class AttachmentEndpoints {

    /// <summary>
    /// 一時保存のリクエスト（multipart/form-data）で、ファイルが入っているセクションの名前
    /// </summary>
    private const string FORM_FIELD_FILE = "file";
    /// <summary>
    /// リクエストの本体の上限を、ファイルのサイズの上限よりどれだけ大きくするか。multipart の区切りやヘッダーの分
    /// </summary>
    private const long MULTIPART_OVERHEAD_BYTES = 64 * 1024;

    /// <summary>
    /// テンプレート文字列に定義した URL に、一時保存（POST）とダウンロード（GET）を登録する。
    /// </summary>
    public static void MapAttachmentEndpoints(this IEndpointRouteBuilder endpoints) {
        endpoints.MapPost(TemplatePattern.API_URL.AttachmentUpload(), UploadAsync);
        endpoints.MapGet(TemplatePattern.API_URL.AttachmentDownload("{識別子}"), DownloadAsync);
    }

    /// <summary>
    /// ファイルを1つ受け取って一時保存し、識別子・名前・サイズ・種類を返す。
    /// サイズの上限を超えるファイルは 413、名前が長すぎるなどのエラーは 422 で、処理の呼び出しと同じ形のメッセージを返す。
    /// </summary>
    /// <remarks>
    /// 引数が HttpContext だけだと RequestDelegate として登録され、戻り値の IResult が捨てられるので、CancellationToken も受け取る。
    /// </remarks>
    private static async Task<IResult> UploadAsync(HttpContext httpContext, CancellationToken ct) {
        var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var settings = app.Settings.Attachment;
        var messages = new MessageSetter([], new MessageContainer());

        // ASP.NET Core が受け付けるリクエストの本体の上限を、ファイルのサイズの上限に合わせる。
        // 上限を超えるリクエストを、最後まで受け取る前に打ち切るため
        var maxRequestBytes = settings.MaxFileSizeBytes + MULTIPART_OVERHEAD_BYTES;
        var sizeFeature = httpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (sizeFeature is { IsReadOnly: false }) sizeFeature.MaxRequestBodySize = maxRequestBytes;

        if (!httpContext.Request.HasFormContentType) {
            return Results.BadRequest($"リクエストの Content-Type は multipart/form-data で、セクション '{FORM_FIELD_FILE}' を含んでいる必要があります。");
        }

        IFormFile? file;
        try {
            var form = await httpContext.Request.ReadFormAsync(new FormOptions { MultipartBodyLengthLimit = maxRequestBytes }, ct);
            file = form.Files.GetFile(FORM_FIELD_FILE);
        } catch (Exception ex) when (ex is BadHttpRequestException { StatusCode: StatusCodes.Status413PayloadTooLarge } or InvalidDataException) {
            messages.AddError(Templates<TemplateKind>.MSG.AttachmentTooLargeError($"{settings.MaxFileSizeMegabytes}MB"));
            return MessagesResult(messages, HttpStatusCode.RequestEntityTooLarge);
        }
        if (file == null) {
            return Results.BadRequest($"リクエストにセクション '{FORM_FIELD_FILE}' が含まれていません。");
        }

        await using var content = file.OpenReadStream();
        var saved = await app.一時保存添付ファイルAsync(file.FileName, file.ContentType, content, messages, ct);
        return saved == null
            ? MessagesResult(messages, HttpStatusCode.UnprocessableEntity)
            : Results.Json(saved);
    }

    /// <summary>
    /// ファイルを返す。ダウンロードできないファイルは、存在しないファイルと同じく 404 を返す。
    /// ファイルの種類によらず、ブラウザで表示させずに必ずダウンロードさせる。
    /// 送られた HTML などが、このサイトのページとしてブラウザで開かれる（XSS の原因になる）のを防ぐため。
    /// </summary>
    private static async Task<IResult> DownloadAsync(string 識別子, HttpContext httpContext, CancellationToken ct) {
        var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
        var file = await app.Findダウンロードできる添付ファイルAsync(識別子, ct);
        if (file == null) return Results.NotFound();

        // fileDownloadName を指定すると Content-Disposition: attachment になる
        return Results.File(file.中身, file.種類, fileDownloadName: file.名前);
    }

    private static IResult MessagesResult(MessageSetter messages, HttpStatusCode statusCode) {
        var body = new JsonObject {
            ["messages"] = OperationEndpoints.ToJsonObject(messages.GetState()!),
        };
        return Results.Json(body, statusCode: (int)statusCode);
    }
}

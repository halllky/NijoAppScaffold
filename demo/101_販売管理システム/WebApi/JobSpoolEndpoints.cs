namespace MyApp.WebApi;

/// <summary>
/// ジョブのスプールのファイルをダウンロードするエンドポイント。
/// URL はクライアント側（ジョブ一覧画面）と合わせる必要がある。
/// </summary>
public static class JobSpoolEndpoints {

    /// <summary>
    /// GET /api/job/{ジョブID}/spool/{ファイル名} を登録する。ログインが必要。
    /// ダウンロードの権限（起動したユーザーかシステム管理者か）は、アプリケーションサービスが確認する。
    /// 権限のないファイルは、存在しないファイルと同じく 404 を返す。
    /// 作業フォルダはジョブを実行したサーバーにしかないので、ほかのサーバーが受けたリクエストも 404 になる。
    /// </summary>
    public static void MapJobSpoolDownload(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGet("/api/job/{jobId}/spool/{fileName}", async (string jobId, string fileName, HttpContext httpContext) => {
            var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
            var path = await app.FindスプールのファイルAsync(jobId, fileName);
            if (path == null) return Results.NotFound();

            // ファイルの種類によらず、ブラウザで表示させずに必ずダウンロードさせる。
            // スプールの HTML ファイルなどが、このサイトのページとしてブラウザで開かれる（XSS の原因になる）のを防ぐため
            return Results.File(path, "application/octet-stream", fileDownloadName: fileName);
        });
    }
}

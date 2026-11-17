namespace MyApp.WebApi;

/// <summary>
/// ジョブのスプールのファイルをダウンロードするエンドポイント。
/// URL は画面の側（ジョブの一覧）と取り決めている。
/// </summary>
public static class JobSpoolEndpoints {

    /// <summary>
    /// GET /api/job/{ジョブID}/spool/{ファイル名} を登録する。ログインが必要。
    /// ダウンロードしてよいか（起動したユーザーかシステム管理者か）は、アプリケーションサービスが確かめる。
    /// ダウンロードしてよくないファイルは、存在しないファイルと同じく 404 を返す。
    /// 作業フォルダはジョブを実行したサーバーにしかないので、別のサーバーで受けたリクエストも 404 になる。
    /// </summary>
    public static void MapJobSpoolDownload(this IEndpointRouteBuilder endpoints) {
        endpoints.MapGet("/api/job/{jobId}/spool/{fileName}", async (string jobId, string fileName, HttpContext httpContext) => {
            var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
            var path = await app.FindスプールのファイルAsync(jobId, fileName);
            if (path == null) return Results.NotFound();

            // ファイルの中身によらず、ブラウザで開かせずに必ずダウンロードさせる。
            // スプールのファイルが、このサイトのページとしてブラウザで開かれないようにするため
            return Results.File(path, "application/octet-stream", fileDownloadName: fileName);
        });
    }
}

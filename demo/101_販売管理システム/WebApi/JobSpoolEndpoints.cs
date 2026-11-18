namespace MyApp.WebApi;

/// <summary>
/// ジョブのスプールのファイルをダウンロードするエンドポイント。
/// </summary>
public static class JobSpoolEndpoints {

    /// <summary>
    /// テンプレート文字列に定義した、スプールのファイルの URL を GET で登録する。ログインが必要。
    /// ダウンロードの権限（起動したユーザーかシステム管理者か）は、アプリケーションサービスが確認する。
    /// 権限のないファイルは、存在しないファイルと同じく 404 を返す。
    /// 作業フォルダはジョブを実行したサーバーにしかないので、ほかのサーバーが受けたリクエストも 404 になる。
    /// </summary>
    public static void MapJobSpoolDownload(this IEndpointRouteBuilder endpoints) {
        var pattern = Templates<RoutePatternTemplateKind>.API_URL.JobSpoolFile("{ジョブID}", "{ファイル名}");
        endpoints.MapGet(pattern, async (string ジョブID, string ファイル名, HttpContext httpContext) => {
            var app = httpContext.RequestServices.GetRequiredService<OverridedApplicationService>();
            var path = await app.FindスプールのファイルAsync(ジョブID, ファイル名);
            if (path == null) return Results.NotFound();

            // ファイルの種類によらず、ブラウザで表示させずに必ずダウンロードさせる。
            // スプールの HTML ファイルなどが、このサイトのページとしてブラウザで開かれる（XSS の原因になる）のを防ぐため
            return Results.File(path, "application/octet-stream", fileDownloadName: ファイル名);
        });
    }
}

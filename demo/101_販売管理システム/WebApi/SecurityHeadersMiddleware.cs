namespace MyApp.WebApi;

/// <summary>
/// ブラウザに守らせるセキュリティの指定を、すべての応答のヘッダーに付ける。
/// 画面の HTML に対してだけ意味をもつヘッダーもあるが、付け漏れを防ぐため、応答の種類で付け分けない。
/// 開発環境では画面の HTML を Vite の開発サーバーが返すので、そちらの設定にも同じヘッダーを書いている。変えるときは両方を変える。
/// </summary>
internal class SecurityHeadersMiddleware(RequestDelegate next) {
    public Task InvokeAsync(HttpContext httpContext) {
        // 未処理の例外が起きたときは、エラーの応答を書く前に応答のヘッダーが消される。
        // そのため、ここで直接付けるのではなく、応答を送り始める直前に付ける
        httpContext.Response.OnStarting(() => {
            var headers = httpContext.Response.Headers;
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["Content-Security-Policy"] = "frame-ancestors 'self'";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "same-origin";
            return Task.CompletedTask;
        });
        return next(httpContext);
    }
}

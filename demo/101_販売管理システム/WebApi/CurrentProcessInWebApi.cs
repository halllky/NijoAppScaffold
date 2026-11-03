namespace MyApp.WebApi;

/// <summary>
/// Web における <see cref="ICurrentProcess"/>。処理名は URL のパスとする（LOG-003）。
/// </summary>
public class CurrentProcessInWebApi(IHttpContextAccessor httpContextAccessor) : ICurrentProcess {
    public string Name => httpContextAccessor.HttpContext?.Request.Path.ToString()
        ?? throw new InvalidOperationException("HTTPリクエストの外で処理名を参照することはできません。");
}

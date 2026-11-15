using MyApp.Core.冪等キー;

namespace MyApp.WebApi;

/// <summary>
/// Web における <see cref="IIdempotencyKeyProvider"/>。クライアントが付けた HTTP ヘッダーの値を冪等キーとする。
/// </summary>
public class IdempotencyKeyProviderInWebApi(IHttpContextAccessor httpContextAccessor) : IIdempotencyKeyProvider {
    /// <summary>冪等キーを入れる HTTP ヘッダーの名前</summary>
    public const string HEADER_NAME = "Idempotency-Key";

    public string? Key {
        get {
            var value = httpContextAccessor.HttpContext?.Request.Headers[HEADER_NAME].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}

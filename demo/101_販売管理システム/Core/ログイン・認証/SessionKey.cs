using System.Security.Cryptography;
using System.Text;

namespace MyApp.Core.Authorization;

/// <summary>
/// セッションキーの発番と、DB に保存する値への変換、ログイン状態の有効期限。
/// </summary>
public static class SessionKey {

    /// <summary>
    /// 最後に操作してからこの時間が経ったセッションは、ログインしていないものとみなす。
    /// </summary>
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromHours(24);

    /// <summary>
    /// セッションの最終操作日時を更新する間隔。
    /// リクエストのたびに DB を更新すると負荷になるので、前回の更新からこの時間が経ったときだけ更新する。
    /// そのため、有効期限の判定にはこの時間ぶんの誤差がある。
    /// </summary>
    public static readonly TimeSpan LastAccessUpdateInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 新しいセッションキーを発番する。暗号用の乱数生成器で作った 256 ビットの値を、Cookie にそのまま入れられる文字列にしたもの。
    /// </summary>
    public static string Generate() {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// セッションキーを、DB に保存して照合するための値に変換する。
    /// DB が漏えいしても、保存された値からセッションキーを復元してなりすますことができないよう、ハッシュ値にする。
    /// セッションキーは十分に長い乱数なので、パスワードと違い、SALT やストレッチングは不要。
    /// </summary>
    public static string ToStoredValue(string sessionKey) {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sessionKey)));
    }
}

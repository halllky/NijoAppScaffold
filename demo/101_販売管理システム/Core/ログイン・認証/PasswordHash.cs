using System.Security.Cryptography;

namespace MyApp.Core.Authorization;

/// <summary>
/// パスワードを、ユーザーごとに異なる SALT を付けてストレッチングしたハッシュ値（PWD-001）。
/// 平文のパスワードの代わりに、ハッシュ値と SALT の組を保存する。
/// </summary>
internal sealed class PasswordHash {
    public required byte[] Hash { get; init; }
    public required byte[] Salt { get; init; }

    /// <summary>
    /// 新しい SALT を発番してパスワードをハッシュ化する。
    /// </summary>
    public static PasswordHash Create(string password) {
        var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        return new() { Hash = Derive(password, salt), Salt = salt };
    }

    /// <summary>
    /// 入力されたパスワードが、保存されているハッシュ値と SALT の組に一致するかを返す。
    /// </summary>
    public static bool Verify(string password, byte[] hash, byte[] salt) {
        // 比較にかかる時間からハッシュ値を推測されないよう、一致する長さによらず同じ時間で比較する
        return CryptographicOperations.FixedTimeEquals(Derive(password, salt), hash);
    }

    private static byte[] Derive(string password, byte[] salt) {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, HashAlgorithmName.SHA256, HASH_SIZE);
    }

    private const int SALT_SIZE = 32; // 256 bits
    private const int HASH_SIZE = 32; // 256 bits
    /// <summary>ストレッチングの回数。OWASP Password Storage Cheat Sheet の PBKDF2-HMAC-SHA256 の推奨値</summary>
    private const int ITERATIONS = 600_000;
}

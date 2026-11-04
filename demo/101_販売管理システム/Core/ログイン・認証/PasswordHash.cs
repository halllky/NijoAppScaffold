using System.Buffers.Binary;
using System.Security.Cryptography;

namespace MyApp.Core.Authorization;

/// <summary>
/// パスワードを、ユーザーごとに異なる SALT を付けてストレッチングしたハッシュ値。
/// 平文のパスワードの代わりに、ハッシュ値と SALT の組を保存する。
///
/// ハッシュ値には、形式の版とストレッチングの回数を含めて保存する。
/// 方式や回数を将来強化しても、保存済みのハッシュ値はその時点の方式で照合でき、
/// 照合に成功したときに <see cref="VerificationResult.SuccessRehashNeeded"/> を返すので、呼び出し側で新しい方式で保存し直す。
/// </summary>
internal sealed class PasswordHash {
    public required byte[] Hash { get; init; }
    public required byte[] Salt { get; init; }

    /// <summary>
    /// 新しい SALT を発番し、現在の方式でパスワードをハッシュ化する。
    /// </summary>
    public static PasswordHash Create(string password) {
        var salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        var hash = new byte[HEADER_SIZE + KEY_SIZE];
        hash[0] = FORMAT_V1;
        BinaryPrimitives.WriteInt32BigEndian(hash.AsSpan(1, 4), ITERATIONS);
        Pbkdf2Sha256(password, salt, ITERATIONS).CopyTo(hash.AsSpan(HEADER_SIZE));
        return new() { Hash = hash, Salt = salt };
    }

    /// <summary>
    /// 入力されたパスワードが、保存されているハッシュ値と SALT の組に一致するかを返す。
    /// </summary>
    public static VerificationResult Verify(string password, byte[] hash, byte[] salt) {
        if (hash.Length == HEADER_SIZE + KEY_SIZE && hash[0] == FORMAT_V1) {
            var iterations = BinaryPrimitives.ReadInt32BigEndian(hash.AsSpan(1, 4));
            if (!FixedTimeEquals(Pbkdf2Sha256(password, salt, iterations), hash.AsSpan(HEADER_SIZE))) return VerificationResult.Failed;
            return iterations < ITERATIONS ? VerificationResult.SuccessRehashNeeded : VerificationResult.Success;
        }

        // 形式の版を含めるようにする前に保存されたハッシュ値。どちらの方式か区別できないので両方で照合する
        if (hash.Length == KEY_SIZE) {
            var matched = FixedTimeEquals(Pbkdf2Sha256(password, salt, LEGACY_PBKDF2_ITERATIONS), hash)
                       || FixedTimeEquals(HMACSHA256.HashData(salt, System.Text.Encoding.UTF8.GetBytes(password)), hash);
            return matched ? VerificationResult.SuccessRehashNeeded : VerificationResult.Failed;
        }

        return VerificationResult.Failed;
    }

    /// <summary>照合の結果</summary>
    public enum VerificationResult {
        /// <summary>一致しない</summary>
        Failed,
        /// <summary>一致した</summary>
        Success,
        /// <summary>一致したが、古い方式で保存されているので、現在の方式で保存し直す必要がある</summary>
        SuccessRehashNeeded,
    }

    private static byte[] Pbkdf2Sha256(string password, byte[] salt, int iterations) {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KEY_SIZE);
    }

    /// <summary>比較にかかる時間からハッシュ値を推測されないよう、一致する長さによらず同じ時間で比較する</summary>
    private static bool FixedTimeEquals(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right) {
        return CryptographicOperations.FixedTimeEquals(left, right);
    }

    /// <summary>形式の版 1: PBKDF2-HMAC-SHA256。ハッシュ値は [版 1バイト][回数 4バイト][鍵]</summary>
    private const byte FORMAT_V1 = 0x01;
    private const int HEADER_SIZE = 1 + 4;
    private const int SALT_SIZE = 32; // 256 bits
    private const int KEY_SIZE = 32; // 256 bits
    /// <summary>ストレッチングの回数。OWASP Password Storage Cheat Sheet の PBKDF2-HMAC-SHA256 の推奨値</summary>
    private const int ITERATIONS = 600_000;
    /// <summary>形式の版を含めるようにする前の、PBKDF2 のストレッチングの回数</summary>
    private const int LEGACY_PBKDF2_ITERATIONS = 600_000;
}

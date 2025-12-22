using System.Security.Cryptography;
using System.Text;

namespace MyApp;

partial class OverridedApplicationService {
    /// <summary>
    /// パスワードのハッシュ化に使用するソルトを生成します。
    /// </summary>
    /// <returns></returns>
    internal static byte[] GenerateSalt() {
        var salt = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(salt);
        }
        return salt;
    }

    /// <summary>
    /// パスワードをハッシュ化します。
    /// </summary>
    internal static byte[] ComputeHash(string password, byte[] salt) {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        using var hmac = new HMACSHA256(salt);
        return hmac.ComputeHash(passwordBytes);
    }
}

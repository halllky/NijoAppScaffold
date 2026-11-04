using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MyApp.UnitTest;

partial class DB接続あり_更新あり {

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 旧方式のHMACで保存されたパスワードでログインでき現在の方式で保存し直される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(旧方式のHMACで保存されたパスワードでログインでき現在の方式で保存し直される));
        var salt = RandomNumberGenerator.GetBytes(32);
        await Add従業員Async(scope.App.DbContext, HMACSHA256.HashData(salt, Encoding.UTF8.GetBytes(PASSWORD)), salt);

        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True);
        Assert.That(await LoadIterationsAsync(scope.App.DbContext), Is.EqualTo(CURRENT_ITERATIONS));
        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True, "保存し直した後もログインできること");
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 形式の版を含まないPBKDF2で保存されたパスワードでログインでき現在の方式で保存し直される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(形式の版を含まないPBKDF2で保存されたパスワードでログインでき現在の方式で保存し直される));
        var salt = RandomNumberGenerator.GetBytes(32);
        await Add従業員Async(scope.App.DbContext, Rfc2898DeriveBytes.Pbkdf2(PASSWORD, salt, CURRENT_ITERATIONS, HashAlgorithmName.SHA256, 32), salt);

        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True);
        Assert.That(await LoadIterationsAsync(scope.App.DbContext), Is.EqualTo(CURRENT_ITERATIONS));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task ストレッチングの回数が少ない方式で保存されたパスワードは現在の回数で保存し直される() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(ストレッチングの回数が少ない方式で保存されたパスワードは現在の回数で保存し直される));
        var salt = RandomNumberGenerator.GetBytes(32);
        await Add従業員Async(scope.App.DbContext, CreateVersion1Hash(PASSWORD, salt, iterations: 1000), salt);

        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True);
        Assert.That(await LoadIterationsAsync(scope.App.DbContext), Is.EqualTo(CURRENT_ITERATIONS));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 現在の方式で保存されたパスワードは保存し直さない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(現在の方式で保存されたパスワードは保存し直さない));
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = CreateVersion1Hash(PASSWORD, salt, CURRENT_ITERATIONS);
        await Add従業員Async(scope.App.DbContext, hash, salt);

        Assert.That(await TryLoginAsync(scope.App, PASSWORD), Is.True);
        Assert.That(await LoadHashAsync(scope.App.DbContext), Is.EqualTo(hash));
    }

    [Test]
    [Category("DB接続あり（更新あり）")]
    public async Task 誤ったパスワードではログインできない() {
        var scope = TestUtilImpl.Instance.CreateScope(nameof(誤ったパスワードではログインできない));
        var salt = RandomNumberGenerator.GetBytes(32);
        await Add従業員Async(scope.App.DbContext, CreateVersion1Hash(PASSWORD, salt, CURRENT_ITERATIONS), salt);

        Assert.That(await TryLoginAsync(scope.App, "wrong-password"), Is.False);
    }

    private const string PASSWORD = "password123";
    private const string 従業員番号 = "PW_USER";
    /// <summary>現在のストレッチングの回数。パスワードのハッシュ値の形式（版1: [版][回数][鍵]）とともに、実装と合わせる</summary>
    private const int CURRENT_ITERATIONS = 600_000;

    private static byte[] CreateVersion1Hash(string password, byte[] salt, int iterations) {
        var hash = new byte[5 + 32];
        hash[0] = 0x01;
        BinaryPrimitives.WriteInt32BigEndian(hash.AsSpan(1, 4), iterations);
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32).CopyTo(hash.AsSpan(5));
        return hash;
    }

    private static async Task Add従業員Async(OverridedDbContext db, byte[] hash, byte[] salt) {
        db.従業員DbSet.Add(new 従業員DbEntity { 従業員番号 = 従業員番号, 氏名 = "パスワードの照合", パスワード = hash, SALT = salt, Version = 0 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear(); // ログインの処理は別のリクエストで行われるので、追跡を引き継がない
    }

    private static async Task<bool> TryLoginAsync(OverridedApplicationService app, string password) {
        var messages = new ログインParameterMessages([], new());
        var context = new OperationContext { ValidationOnly = false };
        var user = await app.ExecuteログインAsync(new() { 従業員番号 = 従業員番号, パスワード = password }, messages, context);
        return user != null && !messages.HasError();
    }

    private static async Task<byte[]> LoadHashAsync(OverridedDbContext db) {
        var employee = await db.従業員DbSet.AsNoTracking().SingleAsync(e => e.従業員番号 == 従業員番号);
        return employee.パスワード!;
    }

    private static async Task<int> LoadIterationsAsync(OverridedDbContext db) {
        var hash = await LoadHashAsync(db);
        Assert.That(hash[0], Is.EqualTo(0x01), "形式の版1で保存し直されていること");
        return BinaryPrimitives.ReadInt32BigEndian(hash.AsSpan(1, 4));
    }
}

using System.Reflection;

namespace MyApp.WebApi;

/// <summary>
/// このサーバーのバージョン。
/// バージョン番号とビルドしたときの git のコミットの識別子を「バージョン番号+コミットの識別子」の形でつないだもの。
/// git の管理の外でビルドしたときは、バージョン番号だけになる。
/// </summary>
internal static class AppVersion {
    /// <summary>バージョン</summary>
    public static string Value { get; } = typeof(AppVersion).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion
        ?? "";
}

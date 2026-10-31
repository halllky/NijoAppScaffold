namespace MyApp.Core.Authorization;

/// <summary>
/// ログイン中のユーザー。機能を使ってよいかの判定に必要な情報をもつ。
/// </summary>
public class LoginUser {
    public required string 従業員番号 { get; init; }
    public required string 氏名 { get; init; }
    public required IReadOnlyCollection<権限区分> 権限 { get; init; }

    /// <summary>
    /// システム管理者ならtrue
    /// </summary>
    public bool Isシステム管理者 => 権限.Contains(権限区分.システム管理者);

    /// <summary>
    /// 指定された権限があるか、システム管理者ならtrue
    /// </summary>
    public bool Has権限(権限区分 対象) => Isシステム管理者 || 権限.Contains(対象);

    /// <summary>
    /// 従業員のデータからログインユーザー情報を作る。従業員の権限が読み込まれている必要がある。
    /// </summary>
    public static LoginUser From(従業員DbEntity 従業員) => new() {
        従業員番号 = 従業員.従業員番号 ?? throw new InvalidOperationException(),
        氏名 = 従業員.氏名 ?? throw new InvalidOperationException(),
        権限 = 従業員.従業員の権限.Select(x => x.権限区分 ?? throw new InvalidOperationException()).ToArray(),
    };

    /// <summary>
    /// JavaScriptで使うログインユーザー情報に変換する。
    /// </summary>
    public ログインユーザー情報DisplayData ToDisplayData() => new() {
        従業員番号 = 従業員番号,
        氏名 = 氏名,
        ログインユーザー情報の権限 = 権限.Select(x => new ログインユーザー情報の権限DisplayData { 権限区分 = x }).ToList(),
    };
}

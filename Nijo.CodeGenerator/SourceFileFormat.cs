using System.Text;

namespace Nijo.CodeGenerator;

/// <summary>
/// 自動生成されるソースコードのファイルの出力形式。
/// どのファイルをどの形式で出力するかはこのライブラリでは決めず、利用側が決める。
/// </summary>
public sealed class SourceFileFormat {
    /// <summary>文字コード。BOMの有無もこれで決まる。</summary>
    public required Encoding Encoding { get; init; }
    /// <summary>改行コード</summary>
    public required string NewLine { get; init; }
    /// <summary>
    /// ファイル先頭に出力される文字列。null の場合は何も出力されない。
    /// 改行コードは <see cref="NewLine"/> に置き換えられる。
    /// </summary>
    public string? Header { get; init; }
}

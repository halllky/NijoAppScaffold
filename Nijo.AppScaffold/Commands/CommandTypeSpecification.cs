using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Nijo.Commands;

/// <summary>
/// コマンドの <see cref="ParameterOption"/>, <see cref="ReturnValueOption"/> 属性の値。
/// 「データ構造名」または「データ構造名:オブジェクトの種類」の形式で表される。
/// オブジェクトの種類が省略された場合は、IsStored 属性も IsQueryable 属性も指定されていないデータ構造（構造体）を表す。
/// </summary>
internal sealed record CommandTypeSpecification(string DataStructureName, string? ObjectKind) {

    /// <summary>
    /// 属性の値を解釈します。形式が不正な場合は false を返します。
    /// </summary>
    internal static bool TryParse(string? value, [NotNullWhen(true)] out CommandTypeSpecification? specification) {
        var splitted = (value ?? string.Empty).Split(':');
        if (splitted.Length > 2 || splitted.Any(string.IsNullOrWhiteSpace)) {
            specification = null;
            return false;
        }
        specification = new CommandTypeSpecification(splitted[0], splitted.Length == 2 ? splitted[1] : null);
        return true;
    }
}

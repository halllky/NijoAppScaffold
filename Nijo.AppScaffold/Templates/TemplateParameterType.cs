namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列の引数の型。
/// 種類ごとの変換は、この型ごとに宣言される。C# ではこの型を引数に取るオーバーロード、TypeScript では <see cref="Key"/> の名前のプロパティになる。
/// </summary>
/// <param name="CsType">C# の型名</param>
/// <param name="TsType">TypeScript の型名</param>
internal sealed record TemplateParameterType(string CsType, string TsType) {
    /// <summary>
    /// 種類ごとの変換を、引数の型で区別するときの名前。
    /// C# のオーバーロードと TypeScript のプロパティを1対1に対応させるため、C# の型名を使う。
    /// </summary>
    internal string Key => CsType;

    /// <summary>どの種類にも必ず宣言される型。引数の無いテンプレート文字列しか持たない種類でも、変換を書いておけるようにするため</summary>
    internal static TemplateParameterType String { get; } = new("string", "string");
}

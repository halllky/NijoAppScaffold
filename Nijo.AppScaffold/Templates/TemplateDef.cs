using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Nijo.Templates;

/// <summary>
/// テンプレート文字列1個分の定義。
/// 値の中の {名前} や {名前:型} を引数とし、引数を埋め込んだ文字列を返す関数として生成する。
/// 波括弧そのものは C# の書式指定文字列と同じく {{ }} と重ねて書く。
/// 引数の値は、文字列にしたうえでテンプレートの種類（<see cref="TemplateKindOption"/>）ごとの変換を通してから埋め込む。
/// 引数に null は渡せない。
/// </summary>
internal sealed class TemplateDef {
    internal const string PARAM_TYPE_STRING = "string";
    internal const string PARAM_TYPE_INT = "int";

    internal TemplateDef(XElement element) {
        Element = element;
        Name = element.Name.LocalName;
        Kind = TemplateKindOption.Resolve(element);
        Value = element.Attribute(TemplateOption.ATTRIBUTE_NAME)?.Value ?? string.Empty;
        (_parts, _hasUnescapedBrace) = Parse(Value);
    }

    internal XElement Element { get; }
    /// <summary>C# のメソッド名、TypeScript の関数名</summary>
    internal string Name { get; }
    /// <summary>テンプレートの種類。自身にも祖先のグループにも指定が無ければ null</summary>
    internal string? Kind { get; }
    private string Value { get; }

    private readonly IReadOnlyList<Part> _parts;
    private readonly bool _hasUnescapedBrace;

    /// <summary>
    /// 引数。同じ名前の引数が複数回登場するテンプレートでも、引数は1個になる。登場順に並ぶ。
    /// </summary>
    private IEnumerable<Placeholder> Parameters => _parts
        .OfType<Placeholder>()
        .DistinctBy(p => p.Name);

    /// <summary>
    /// スキーマ定義の誤りを検証します。
    /// </summary>
    internal void Validate(Action<string> addError) {
        if (Element.Attribute(TemplateOption.ATTRIBUTE_NAME) == null) {
            addError($"{TemplateOption.ATTRIBUTE_NAME} 属性でテンプレート文字列を指定してください。");
        }
        if (Kind == null) {
            addError($"自身かその親のグループに {TemplateKindOption.ATTRIBUTE_NAME} 属性を指定してください。");
        }
        if (_hasUnescapedBrace) {
            addError("引数は {売上SEQ:int} のように名前で書いてください（{0} のような番号は使えません）。波括弧そのものは {{ }} と重ねて書いてください。");
        }
        foreach (var placeholder in _parts.OfType<Placeholder>()) {
            if (!PARAM_TYPES.ContainsKey(placeholder.Type)) {
                addError($"引数「{placeholder.Name}」の型「{placeholder.Type}」は使えません。{string.Join(", ", PARAM_TYPES.Keys)} のいずれかを指定してください。");
            }
        }
        if (Parameters.Any(p => p.Name == TemplatesSection.TS_KIND_VARIABLE)) {
            addError($"引数の名前に「{TemplatesSection.TS_KIND_VARIABLE}」は使えません。");
        }
        foreach (var group in _parts.OfType<Placeholder>().GroupBy(p => p.Name)) {
            if (group.Select(p => p.Type).Distinct().Count() > 1) {
                addError($"引数「{group.Key}」に、異なる型が指定されています。");
            }
        }
    }

    /// <summary>
    /// 引数を埋め込んだ文字列を返す、C# の静的メソッドをレンダリングします。
    /// 種類ごとの変換は、テンプレートを入れるクラスの型引数から呼ぶ。
    /// </summary>
    internal string RenderCSharp(string kindTypeParameter) {
        var parameters = Parameters.Select(p => $"{PARAM_TYPES[p.Type].CsType} {p.Name}");
        var body = _parts.Select(part => part switch {
            Literal literal => literal.Text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("{", "{{")
                .Replace("}", "}}"),
            Placeholder p => $"{{{kindTypeParameter}.{Kind}({PARAM_TYPES[p.Type].CsToString(p.Name)})}}",
            _ => throw new InvalidOperationException(),
        });

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.CSharp)}}
            public static string {{Name}}({{string.Join(", ", parameters)}}) => $"{{string.Concat(body)}}";
            """;
    }

    /// <summary>
    /// 引数を埋め込んだ文字列を返す、TypeScript の関数をレンダリングします。
    /// </summary>
    /// <param name="kindVariable">種類ごとの変換を持つオブジェクトの変数名</param>
    internal string RenderTypeScriptFormat(string kindVariable) {
        return RenderTypeScriptFunction(
            p => PARAM_TYPES[p.Type].TsType,
            p => $"{kindVariable}.{Kind}({PARAM_TYPES[p.Type].TsToString(p.Name)})");
    }

    /// <summary>
    /// 引数の位置に、渡された文字列を変換せずにそのまま置く、TypeScript の関数をレンダリングします。
    /// React Router のルート定義（"/uriage/edit/:id"）のように、引数の位置に値以外のものを置くときに使う。
    /// </summary>
    internal string RenderTypeScriptPattern() {
        return RenderTypeScriptFunction(_ => "string", p => p.Name);
    }

    private string RenderTypeScriptFunction(Func<Placeholder, string> getParamType, Func<Placeholder, string> renderEmbedded) {
        var parameters = Parameters.ToArray();
        var args = parameters.Length == 0
            ? ""
            : $"{{ {string.Join(", ", parameters.Select(p => p.Name))} }}: {{ {string.Join(", ", parameters.Select(p => $"{p.Name}: {getParamType(p)}"))} }}";
        var body = _parts.Select(part => part switch {
            Literal literal => literal.Text
                .Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("$", "\\$"),
            Placeholder p => $"${{{renderEmbedded(p)}}}",
            _ => throw new InvalidOperationException(),
        });

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{Name}}: ({{args}}): string => `{{string.Concat(body)}}`,
            """;
    }

    #region 解析
    /// <summary>
    /// 値を先頭から区切る。{{ と }} は波括弧そのもの、{名前} と {名前:型} は引数、それ以外の波括弧は誤り。
    /// </summary>
    private static readonly Regex TOKEN = new(@"\{\{|\}\}|\{(?<name>[\p{L}_][\p{L}\p{Nd}_]*)(:(?<type>[^{}]*))?\}|[{}]");

    private static (IReadOnlyList<Part> Parts, bool HasUnescapedBrace) Parse(string value) {
        var parts = new List<Part>();
        var literal = new StringBuilder();
        var hasUnescapedBrace = false;
        var position = 0;
        foreach (Match match in TOKEN.Matches(value)) {
            literal.Append(value[position..match.Index]);
            position = match.Index + match.Length;

            if (match.Value == "{{" || match.Value == "}}") {
                literal.Append(match.Value[0]);
            } else if (match.Groups["name"].Success) {
                if (literal.Length > 0) parts.Add(new Literal(literal.ToString()));
                literal.Clear();
                var type = match.Groups["type"].Success ? match.Groups["type"].Value : PARAM_TYPE_STRING;
                parts.Add(new Placeholder(match.Groups["name"].Value, type));
            } else {
                literal.Append(match.Value);
                hasUnescapedBrace = true;
            }
        }
        literal.Append(value[position..]);
        if (literal.Length > 0) parts.Add(new Literal(literal.ToString()));
        return (parts, hasUnescapedBrace);
    }

    private abstract record Part;
    private sealed record Literal(string Text) : Part;
    private sealed record Placeholder(string Name, string Type) : Part;
    #endregion 解析

    #region 引数の型
    /// <summary>
    /// 引数の型ごとの、C# と TypeScript での型と、文字列にする式。
    /// 文字列にした結果が C# と TypeScript で一致するようにする。
    /// </summary>
    private static readonly IReadOnlyDictionary<string, ParamType> PARAM_TYPES = new Dictionary<string, ParamType> {
        [PARAM_TYPE_STRING] = new("string", name => name, "string", name => name),
        [PARAM_TYPE_INT] = new("int", name => $"{name}.ToString(System.Globalization.CultureInfo.InvariantCulture)", "number", name => $"String({name})"),
    };

    private sealed record ParamType(
        string CsType,
        Func<string, string> CsToString,
        string TsType,
        Func<string, string> TsToString);
    #endregion 引数の型
}

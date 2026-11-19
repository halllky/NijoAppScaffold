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
/// 値の中の {名前} を引数とし、引数を埋め込んだ文字列を返す関数として生成する。
/// 波括弧そのものは C# の書式指定文字列と同じく {{ }} と重ねて書く。
/// 引数はすべて文字列で、null は渡せない。値をどう見せるか（書式）と、値が無いときにどうするかは、呼び出し側が決める。
/// 引数の値は、テンプレートの種類（<see cref="TemplateKindOption"/>）の変換を通してから埋め込む。
/// 変換の中身（エスケープなど）はアプリ側に書くので、nijo は埋め込み先の文法について何も知らない。
/// </summary>
internal sealed class TemplateDef {

    internal TemplateDef(XElement element) {
        Element = element;
        Name = element.Name.LocalName;
        Kind = TemplateKindOption.Resolve(element);
        var value = element.Attribute(TemplateOption.ATTRIBUTE_NAME)?.Value ?? string.Empty;
        (_parts, _hasInvalidBrace) = Parse(value);
    }

    internal XElement Element { get; }
    /// <summary>C# のメソッド名、TypeScript の関数名</summary>
    internal string Name { get; }
    /// <summary>テンプレートの種類。自身にも祖先のグループにも指定が無ければ null</summary>
    internal string? Kind { get; }

    private readonly IReadOnlyList<Part> _parts;
    private readonly bool _hasInvalidBrace;

    /// <summary>
    /// 引数の名前。同じ名前の引数が複数回登場するテンプレートでも、引数は1個になる。登場順に並ぶ。
    /// </summary>
    private IEnumerable<string> Parameters => _parts
        .OfType<Placeholder>()
        .Select(p => p.Name)
        .Distinct();

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
        if (_hasInvalidBrace) {
            addError("引数は {売上SEQ} のように名前だけで書いてください（{0} のような番号や、型の指定は書けません）。波括弧そのものは {{ }} と重ねて書いてください。");
        }
        if (Parameters.Contains(TemplatesSection.TS_KIND_VARIABLE)) {
            addError($"引数の名前に「{TemplatesSection.TS_KIND_VARIABLE}」は使えません。");
        }
    }

    /// <summary>
    /// 引数を埋め込んだ文字列を返す、C# の静的メソッドをレンダリングします。
    /// 種類の変換は、テンプレートを入れるクラスの型引数から呼ぶ。
    /// </summary>
    internal string RenderCSharp(string kindTypeParameter) {
        return RenderCSharpMethod(name => $"{kindTypeParameter}.{Kind}({name})");
    }

    /// <summary>
    /// 引数の位置に、渡された文字列を変換せずにそのまま置く、C# の静的メソッドをレンダリングします。
    /// ASP.NET Core のルート定義（"/api/job/{jobId}"）のように、引数の位置に値以外のものを置くときに使う。
    /// </summary>
    internal string RenderCSharpPattern() {
        return RenderCSharpMethod(name => name);
    }

    private string RenderCSharpMethod(Func<string, string> renderEmbedded) {
        var body = _parts.Select(part => part switch {
            Literal literal => literal.Text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("{", "{{")
                .Replace("}", "}}"),
            Placeholder p => $"{{{renderEmbedded(p.Name)}}}",
            _ => throw new InvalidOperationException(),
        });

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.CSharp)}}
            public static string {{Name}}({{string.Join(", ", Parameters.Select(name => $"string {name}"))}}) => $"{{string.Concat(body)}}";
            """;
    }

    /// <summary>
    /// 引数を埋め込んだ文字列を返す、TypeScript の関数をレンダリングします。
    /// </summary>
    /// <param name="kindVariable">種類の変換を持つオブジェクトの変数名</param>
    internal string RenderTypeScriptFormat(string kindVariable) {
        return RenderTypeScriptFunction(name => $"{kindVariable}.{Kind}({name})");
    }

    /// <summary>
    /// 引数の位置に、渡された文字列を変換せずにそのまま置く、TypeScript の関数をレンダリングします。
    /// React Router のルート定義（"/uriage/edit/:id"）のように、引数の位置に値以外のものを置くときに使う。
    /// </summary>
    internal string RenderTypeScriptPattern() {
        return RenderTypeScriptFunction(name => name);
    }

    private string RenderTypeScriptFunction(Func<string, string> renderEmbedded) {
        var parameters = Parameters.ToArray();
        var args = parameters.Length == 0
            ? ""
            : $"{{ {string.Join(", ", parameters)} }}: {{ {string.Join(", ", parameters.Select(name => $"{name}: string"))} }}";
        var body = _parts.Select(part => part switch {
            Literal literal => literal.Text
                .Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("$", "\\$"),
            Placeholder p => $"${{{renderEmbedded(p.Name)}}}",
            _ => throw new InvalidOperationException(),
        });

        return $$"""
            {{Element.RenderXmlCommentOrJsDoc(E_CsTs.TypeScript)}}
            {{Name}}: ({{args}}): string => `{{string.Concat(body)}}`,
            """;
    }

    #region 解析
    /// <summary>
    /// 値を先頭から区切る。{{ と }} は波括弧そのもの、{名前} は引数、それ以外の波括弧は誤り。
    /// </summary>
    private static readonly Regex TOKEN = new(@"\{\{|\}\}|\{(?<name>[\p{L}_][\p{L}\p{Nd}_]*)\}|[{}]");

    private static (IReadOnlyList<Part> Parts, bool HasInvalidBrace) Parse(string value) {
        var parts = new List<Part>();
        var literal = new StringBuilder();
        var hasInvalidBrace = false;
        var position = 0;
        foreach (Match match in TOKEN.Matches(value)) {
            literal.Append(value[position..match.Index]);
            position = match.Index + match.Length;

            if (match.Value == "{{" || match.Value == "}}") {
                literal.Append(match.Value[0]);
            } else if (match.Groups["name"].Success) {
                if (literal.Length > 0) parts.Add(new Literal(literal.ToString()));
                literal.Clear();
                parts.Add(new Placeholder(match.Groups["name"].Value));
            } else {
                literal.Append(match.Value);
                hasInvalidBrace = true;
            }
        }
        literal.Append(value[position..]);
        if (literal.Length > 0) parts.Add(new Literal(literal.ToString()));
        return (parts, hasInvalidBrace);
    }

    private abstract record Part;
    private sealed record Literal(string Text) : Part;
    private sealed record Placeholder(string Name) : Part;
    #endregion 解析
}

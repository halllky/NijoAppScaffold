using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Nijo.Constants;

/// <summary>
/// Constants セクション。
/// 定数はC#とJavaScript/TypeScriptで常に値が同期される値で、string, int, decimal,
/// テンプレート文字列（引数を受け取って文字列を返す関数）の4種類を定義できる。ネストされた定数も定義できる。
/// </summary>
internal sealed class ConstantsSection : Section {
    public override E_Section Kind => E_Section.Constants;

    public override void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        ValidateConstants(rootAggregateElement, addError);

        void ValidateConstants(XElement element, Action<XElement, string> addError) {
            foreach (var constantElement in element.Elements()) {
                var constantType = constantElement.Attribute(ConstantTypeOption.ATTRIBUTE_NAME)?.Value;

                if (constantType == ConstantValueDef.CONSTTYPE_CHILD) {
                    // 子要素（ネストされた定数グループ）の場合は再帰的にバリデーション
                    ValidateConstants(constantElement, addError);
                } else {
                    // 定数要素のバリデーション
                    ValidateConstantElement(constantElement, addError);
                }
            }
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        RenderEach(ctx, ctx.Schema.GetConstants(), rootAggregate => RenderRootAggregate(ctx, rootAggregate));
    }

    private static void RenderRootAggregate(CodeRenderingContext ctx, SectionRoot rootAggregate) {
        var parser = new ConstantDefParser(rootAggregate.XElement, ctx.SchemaParser);

        // データ型: 定数クラス
        var constantDef = new ConstantDef(parser);

        // C#定数クラスの生成
        ctx.CoreLibrary(dir => {
            dir.Generate(new SourceFile {
                FileName = $"{constantDef.CsClassName}.cs",
                Contents = constantDef.RenderCSharp(ctx)
            });
        });

        // JavaScript/TypeScript定数の生成
        ctx.ReactProject(dir => {
            dir.Directory(TypeScriptAggregateModule.DIRECTORY, modelsDir => {
                modelsDir.Generate(constantDef.RenderTypeScript(ctx));
            });
        });
    }

    private static void ValidateConstantElement(XElement constantElement, Action<XElement, string> addError) {
        // 定数名の妥当性チェック（有効な識別子かどうか）
        var constantName = constantElement.Name.LocalName;
        if (!IsValidIdentifier(constantName)) {
            addError(constantElement, $"定数名「{constantName}」は有効な識別子ではありません。");
        }
    }

    private static bool IsValidIdentifier(string name) {
        if (string.IsNullOrEmpty(name)) return false;
        if (!char.IsLetter(name[0]) && name[0] != '_') return false;
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}

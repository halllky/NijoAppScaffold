using Nijo.CodeGenerating;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;
using System.IO;

namespace Nijo.Constants;

/// <summary>
/// Constants セクション。
/// 定数はC#とJavaScript/TypeScriptで常に値が同期される値で、string, int, decimal,
/// テンプレート文字列（引数を受け取って文字列を返す関数。<see cref="ConstantTemplate"/>）の4種類を定義できる。ネストされた定数も定義できる。
/// </summary>
internal sealed class ConstantsSection : Section {
    public override E_Section Kind => E_Section.Constants;

    public override void Validate(IReadOnlyList<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        foreach (var rootAggregateElement in rootAggregateElements) {
            ValidateGroup(new ConstantGroupDef(rootAggregateElement));
        }

        void ValidateGroup(ConstantGroupDef group) {
            foreach (var constant in group.GetConstants()) {
                ValidateConstantElement(constant.Element, errors.Add);
                constant.Template?.Validate(error => errors.Add(constant.Element, error));
            }
            foreach (var childGroup in group.GetChildGroups()) {
                ValidateGroup(childGroup);
            }
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        RenderEach(ctx, ctx.Schema.GetConstants(), rootAggregate => RenderRootAggregate(ctx, rootAggregate));

        // テンプレート文字列がまだ無いグループに指定された種類も宣言する。
        // 宣言が無いと、変換の中身だけを先に書いておくことができず、コンパイルエラーになるため。
        var templateKinds = ctx.Schema.GetConstants()
            .SelectMany(rootAggregate => rootAggregate.XElement.DescendantsAndSelf())
            .Select(el => el.Attribute(TemplateKindOption.ATTRIBUTE_NAME)?.Value)
            .OfType<string>();
        ConstantTemplateKind.Render(ctx, templateKinds);
    }

    private static void RenderRootAggregate(CodeRenderingContext ctx, SectionRoot rootAggregate) {
        var constants = new ConstantGroupDef(rootAggregate.XElement);
        var hasTemplate = constants.GetConstantsRecursively().Any(constant => constant.Template != null);

        // C#定数クラスの生成
        ctx.CoreLibrary(dir => {
            dir.Generate(new SourceFile {
                FileName = $"{rootAggregate.PhysicalName}.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    {{constants.RenderCSharpClass()}}
                    """,
            });
        });

        // JavaScript/TypeScript定数の生成
        ctx.ReactProject(dir => {
            dir.Directory(TypeScriptSectionRootModule.DIRECTORY, modelsDir => {
                modelsDir.Generate(new SourceFile {
                    FileName = $"{rootAggregate.PhysicalName}.ts",
                    Contents = hasTemplate
                        ? $$"""
                            import { {{ConstantTemplateKind.CLASS_NAME}} } from "../{{Path.GetFileNameWithoutExtension(ConstantTemplateKind.TS_FILENAME)}}"

                            {{constants.RenderTypeScriptObject(isRoot: true)}}
                            """
                        : constants.RenderTypeScriptObject(isRoot: true),
                });
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

    internal static bool IsValidIdentifier(string name) {
        if (string.IsNullOrEmpty(name)) return false;
        if (!char.IsLetter(name[0]) && name[0] != '_') return false;
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}

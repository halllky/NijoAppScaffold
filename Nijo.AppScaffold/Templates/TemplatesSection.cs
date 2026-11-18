using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nijo.Templates;

/// <summary>
/// Templates セクション。
/// テンプレート文字列（<see cref="TemplateDef"/>）を、C# と TypeScript で同じ文字列を組み立てる関数として生成する。
/// 引数を埋め込む前の変換は、テンプレートの種類（<see cref="TemplateKindOption"/>）ごとにアプリ側で書く。
/// 自動生成されたコードがアプリ側のコードを参照しないように、変換は外から受け取る形で生成する。
/// C# では型引数に変換を実装したクラスを渡し、TypeScript では変換を引数に渡す関数を呼ぶ。
/// </summary>
internal sealed class TemplatesSection : Section {
    public override E_Section Kind => E_Section.Templates;

    /// <summary>C# の、種類ごとの変換を表すインターフェースの名前</summary>
    internal const string CS_KIND_INTERFACE = "ITemplateKind";
    /// <summary>C# の、テンプレート文字列を入れるクラスの名前</summary>
    internal const string CS_CONTAINER_CLASS = "Templates";
    /// <summary>C# の、種類ごとの変換を受け取る型引数の名前</summary>
    private const string CS_KIND_TYPE_PARAMETER = "TKind";
    /// <summary>TypeScript の、種類ごとの変換の型の名前</summary>
    internal const string TS_KIND_TYPE = "TemplateKind";
    /// <summary>TypeScript の、種類ごとの変換を受け取ってテンプレート文字列を返す関数の名前</summary>
    internal const string TS_FACTORY_FUNCTION = "createTemplates";
    /// <summary>TypeScript の、引数の位置に渡された文字列をそのまま置く関数を集めたオブジェクトの名前</summary>
    internal const string TS_PATTERN_OBJECT = "TemplatePattern";
    /// <summary>TypeScript の、種類ごとの変換を受け取る引数の名前。テンプレート文字列の引数と同じ名前にはできない</summary>
    internal const string TS_KIND_VARIABLE = "templateKind";

    public override void Validate(IReadOnlyList<XElement> rootAggregateElements, SchemaParseContext context, SchemaValidationErrors errors) {
        foreach (var rootAggregateElement in rootAggregateElements) {
            ValidateGroup(new TemplateGroupDef(rootAggregateElement));
        }

        void ValidateGroup(TemplateGroupDef group) {
            ValidateName(group.Element);
            foreach (var template in group.GetTemplates()) {
                ValidateName(template.Element);
                template.Validate(error => errors.Add(template.Element, error));
            }
            foreach (var childGroup in group.GetChildGroups()) {
                ValidateGroup(childGroup);
            }
        }
        void ValidateName(XElement element) {
            if (!element.Name.LocalName.IsCSharpSafe()) {
                errors.Add(element, $"名前「{element.Name.LocalName}」は、C# や TypeScript の名前として使えません。");
            }
        }
    }

    public override void Render(CodeRenderingContext ctx) {
        var groups = ctx.Schema.GetTemplates().Select(root => new TemplateGroupDef(root.XElement)).ToArray();
        if (groups.Length == 0) return;

        var kinds = CollectKinds(groups);

        ctx.CoreLibrary(dir => {
            dir.Generate(new SourceFile {
                FileName = $"{CS_CONTAINER_CLASS}.cs",
                Contents = RenderCSharp(ctx, groups, kinds),
            });
        });
        ctx.ReactProject(dir => {
            dir.Generate(new SourceFile {
                FileName = "templates.ts",
                Contents = RenderTypeScript(groups, kinds),
            });
        });
    }

    /// <summary>
    /// 種類ごとに、変換を宣言する引数の型を集めます。
    /// 宣言するのは、その種類のテンプレート文字列で実際に使われている型と、文字列型。
    /// テンプレート文字列がまだ無いグループに指定された種類も、文字列型だけは宣言する。
    /// 宣言が無いと、変換の中身を先に書いておくことができないため。
    /// </summary>
    private static KindDeclaration[] CollectKinds(TemplateGroupDef[] groups) {
        var declaredKinds = groups
            .SelectMany(group => group.Element.DescendantsAndSelf())
            .Select(el => el.Attribute(TemplateKindOption.ATTRIBUTE_NAME)?.Value)
            .OfType<string>();
        var usedTypes = groups
            .SelectMany(group => group.GetTemplatesRecursively())
            .Where(template => template.Kind != null)
            .SelectMany(template => template.GetParameterTypes().Select(type => (Kind: template.Kind!, Type: type)));

        return declaredKinds
            .Concat(usedTypes.Select(x => x.Kind))
            .Distinct()
            .Order()
            .Select(kind => new KindDeclaration(kind, usedTypes
                .Where(x => x.Kind == kind)
                .Select(x => x.Type)
                .Prepend(TemplateParameterType.String)
                .DistinctBy(type => type.Key)
                .ToArray()))
            .ToArray();
    }
    /// <summary>種類1つ分の変換の宣言</summary>
    private sealed record KindDeclaration(string Kind, TemplateParameterType[] Types);

    private static string RenderCSharp(CodeRenderingContext ctx, TemplateGroupDef[] groups, KindDeclaration[] kinds) {
        return $$"""
            namespace {{ctx.Config.RootNamespace}};

            /// <summary>
            /// テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
            /// 種類は nijo.xml の {{TemplateKindOption.ATTRIBUTE_NAME}} 属性で指定したもの。引数の型ごとにオーバーロードがある。
            /// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="{{CS_CONTAINER_CLASS}}{{{CS_KIND_TYPE_PARAMETER}}}"/> の型引数に渡す。
            /// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
            /// </summary>
            public interface {{CS_KIND_INTERFACE}} {
            {{kinds.SelectMany(kind => kind.Types.Select(type => (kind.Kind, Type: type))).SelectTextTemplate(x => $$"""
                /// <summary>種類「{{x.Kind}}」の、{{x.Type.CsType}} の引数の変換</summary>
                static abstract string {{x.Kind}}({{x.Type.CsType}} value);
            """)}}
            }

            /// <summary>
            /// nijo.xml の {{E_Section.Templates.ToXmlElementName()}} セクションに定義したテンプレート文字列。
            /// 引数の値は、型引数に渡したクラスの変換を通してから埋め込む。
            /// アプリからは、型引数を決めた派生クラスを1つ定義し、それを通して使う。
            /// </summary>
            /// <example>
            /// public abstract class テンプレート : {{CS_CONTAINER_CLASS}}&lt;TemplateKind&gt;;
            /// </example>
            public abstract class {{CS_CONTAINER_CLASS}}<{{CS_KIND_TYPE_PARAMETER}}> where {{CS_KIND_TYPE_PARAMETER}} : {{CS_KIND_INTERFACE}} {
            {{groups.SelectTextTemplate(group => $$"""
                {{WithIndent(group.RenderCSharp(CS_KIND_TYPE_PARAMETER))}}
            """)}}
            }
            """;
    }

    private static string RenderTypeScript(TemplateGroupDef[] groups, KindDeclaration[] kinds) {
        return $$"""
            /**
             * テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
             * 種類は nijo.xml の {{TemplateKindOption.ATTRIBUTE_NAME}} 属性で指定したもの。
             * 引数の型ごとに、C# の型名の名前で変換を持つ。
             * サーバーと画面で同じ文字列になるように、C# 側の変換と同じ結果を返すこと。
             */
            export type {{TS_KIND_TYPE}} = {
            {{kinds.SelectTextTemplate(kind => $$"""
              /** 種類「{{kind.Kind}}」の変換 */
              {{kind.Kind}}: {
                {{WithIndent(kind.Types.Select(type => $"{type.Key}: (value: {type.TsType}) => string").Join("\n"))}}
              }
            """)}}
            }

            /**
             * nijo.xml の {{E_Section.Templates.ToXmlElementName()}} セクションに定義したテンプレート文字列を返します。
             * 引数の値は、渡された変換を通してから埋め込む。
             * アプリからは、この関数を1回だけ呼んだ結果を export し、それを通して使う。
             */
            export const {{TS_FACTORY_FUNCTION}} = ({{TS_KIND_VARIABLE}}: {{TS_KIND_TYPE}}) => ({
            {{groups.SelectTextTemplate(group => $$"""
              {{WithIndent(group.RenderTypeScriptFormat(TS_KIND_VARIABLE))}}
            """)}}
            })

            /**
             * nijo.xml の {{E_Section.Templates.ToXmlElementName()}} セクションに定義したテンプレート文字列の、
             * 引数の位置に渡された文字列を変換せずにそのまま置く関数。
             * React Router のルート定義（"/uriage/edit/:id"）のように、引数の位置に値以外のものを置くときに使う。
             */
            export const {{TS_PATTERN_OBJECT}} = {
            {{groups.SelectTextTemplate(group => $$"""
              {{WithIndent(group.RenderTypeScriptPattern())}}
            """)}}
            }
            """;
    }
}

using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.ValueMemberTypes;

/// <summary>
/// 真偽値型
/// </summary>
internal class BoolMember : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "Boolean";
    string IValueMemberType.SchemaTypeName => "bool";
    string IValueMemberType.CsDomainTypeName => "bool";
    string IValueMemberType.CsPrimitiveTypeName => "bool";
    string IValueMemberType.TsTypeName => "boolean";
    string IValueMemberType.DisplayName => "真偽値型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            真（true）または偽（false）の値を格納する型です。
            フラグ、有効/無効、完了/未完了などの二値データに適しています。
            検索時の挙動は真のみ、偽のみの条件指定が可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 真偽値型の検証
        // 真偽値型の場合は特別な検証は必要ありません
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = "BooleanSearchCondition",
        FilterTsTypeName = "{ trueのみ?: boolean | null; falseのみ?: boolean | null }",
        RenderTsNewObjectFunctionValue = () => "{ trueのみ: false, falseのみ: false }",
        RenderFiltering = ctx => {
            var query = ctx.Query.Root.Name;
            var cast = ctx.SearchCondition.Metadata.Type.RenderCastToPrimitiveType();

            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            return $$"""
                if ({{fullpathNullable}}?.AnyChecked() == true) {
                    if ({{fullpathNotNull}}.Trueのみ) {
                {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} == true));
                """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} == true);
                """)}}
                    } else {
                {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} != true));
                """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} != true);
                """)}}
                    }
                }
                """;
        }
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            return member.IsKey
                ? (context.GetNextSequence() % 2 == 0)
                : context.Random.Next(0, 2) == 0;
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {

        if (ctx.IsLegacyCompatibilityMode()) {

        } else {
            ctx.CoreLibrary(dir => {
                dir.Generate(new SourceFile {
                    FileName = "BooleanSearchCondition.cs",
                    Contents = $$"""
                    using System;
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}} {
                        public class BooleanSearchCondition {
                            [JsonPropertyName("trueのみ")]
                            public bool Trueのみ { get; set; }
                            [JsonPropertyName("falseのみ")]
                            public bool Falseのみ { get; set; }

                            public bool AnyChecked() => Trueのみ || Falseのみ;
                        }
                    }
                    """,
                });
            });
        }

    }
}

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

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => CodeRenderingContext.CurrentContext?.IsLegacyCompatibilityMode() == true
        ? new() {
            FilterCsTypeName = "E_BoolSearchCondition",
            FilterTsTypeName = "'指定なし' | 'Trueのみ' | 'Falseのみ'",
            RenderTsNewObjectFunctionValue = () => "'指定なし'",
            RenderFiltering = ctx => {
                var query = ctx.Query.Root.Name;
                var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
                var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
                var queryOwnerFullPath = queryFullPath.SkipLast(1);

                return $$"""
                    if ({{fullpathNullable}} == E_BoolSearchCondition.Trueのみ) {
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} == true));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} == true);
                    """)}}
                    } else if ({{fullpathNullable}} == E_BoolSearchCondition.Falseのみ) {
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} == false));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} == false);
                    """)}}
                    }
                    """;
            }
        }
        : new() {
            FilterCsTypeName = "BooleanSearchCondition",
            FilterTsTypeName = "{ trueのみ?: boolean | null; falseのみ?: boolean | null }",
            RenderTsNewObjectFunctionValue = () => "{ trueのみ: false, falseのみ: false }",
            RenderFiltering = ctx => {
                var query = ctx.Query.Root.Name;
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
        if (CodeRenderingContext.CurrentContext?.IsLegacyCompatibilityMode() == true
            && ctx.Schema.GetRootAggregates().Any(rootAggregate => {
                var requiresLegacyBoolSearchCondition = rootAggregate.Model is Models.ReadModel2
                    || rootAggregate.GenerateDefaultQueryModel;
                if (!requiresLegacyBoolSearchCondition) return false;

                var hasSchemaBoolMember = rootAggregate
                    .EnumerateThisAndDescendants()
                    .SelectMany(aggregate => aggregate.GetMembers())
                    .OfType<ValueMember>()
                    .Any(member => member.Type is BoolMember);
                if (!hasSchemaBoolMember) return false;

                var searchCondition = new Models.ReadModel2Modules.SearchCondition.Entry(rootAggregate);
                return searchCondition.EnumerateFilterMembersRecursively().Any(member => member.Member.Type is BoolMember && !member.Member.OnlySearchCondition);
            })) {
            ctx.Use<Parts.Common.EnumFile2>().AddSourceCode($$"""

                public enum E_BoolSearchCondition {
                    指定なし,
                    Trueのみ,
                    Falseのみ,
                }
                """);
        }
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

using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
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
/// 単語型
/// </summary>
internal class Word : IValueMemberType {
    string IValueMemberType.TypePhysicalName => "Word";
    string IValueMemberType.SchemaTypeName => "word";
    string IValueMemberType.CsDomainTypeName => "string";
    string IValueMemberType.CsPrimitiveTypeName => "string";
    string IValueMemberType.TsTypeName => "string";
    string IValueMemberType.DisplayName => "単語型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            文字列を格納する型です。
            名前などの、改行を含まない文字列データに適しています。
            検索時の挙動は完全一致・部分一致・前方一致・後方一致から選択可能です。
            """;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 特に追加の検証はありません。
        // 必要に応じて、属性の最大長や最小長などの制約を検証するコードをここに追加できます。
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = "string",
        FilterTsTypeName = "string",
        RenderTsNewObjectFunctionValue = () => "''",
        RenderFiltering = ctx => {
            var query = ctx.Query.Root.Name;
            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ctx.CodeRenderingContext.IsLegacyCompatibilityMode() ? "." : "!.");

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            var searchBehavior = ctx.Query.Metadata is IInstanceValuePropertyMetadata vm
                && vm.SchemaPathNode.XElement.Attribute(BasicNodeOptions.StringSearchBehavior.AttributeName)?.Value is string s
                ? s
                : BasicNodeOptions.STRING_SEARCH_BEHAVIOR_PARTIAL;

            string GetComparison(string target) {
                if (ctx.CodeRenderingContext.IsLegacyCompatibilityMode() && searchBehavior == BasicNodeOptions.STRING_SEARCH_BEHAVIOR_PARTIAL) {
                    return $"{target}.Contains(trimmed)";
                }
                return searchBehavior switch {
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_EXACT => $"{target} == trimmed",
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_FORWARD => $"Microsoft.EntityFrameworkCore.EF.Functions.Like({target}, $\"{{escaped}}%\", \"\\\\\")",
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_BACKWARD => $"Microsoft.EntityFrameworkCore.EF.Functions.Like({target}, $\"%{{escaped}}\", \"\\\\\")",
                    _ => $"Microsoft.EntityFrameworkCore.EF.Functions.Like({target}, $\"%{{escaped}}%\", \"\\\\\")",
                };
            }

            return $$"""
                if (!string.IsNullOrWhiteSpace({{fullpathNullable}})) {
                    var trimmed = {{fullpathNotNull}}.Trim();
                {{If(searchBehavior != BasicNodeOptions.STRING_SEARCH_BEHAVIOR_EXACT && !(ctx.CodeRenderingContext.IsLegacyCompatibilityMode() && searchBehavior == BasicNodeOptions.STRING_SEARCH_BEHAVIOR_PARTIAL), () => $$"""
                    var escaped = trimmed.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                """)}}
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(ctx.CodeRenderingContext.IsLegacyCompatibilityMode() ? "." : "!.")}}{{If(ctx.CodeRenderingContext.IsLegacyCompatibilityMode(), () => "").Else(() => "!")}}.Any(y => {{GetComparison($"y.{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}{(ctx.CodeRenderingContext.IsLegacyCompatibilityMode() ? "" : "!")}")}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{GetComparison($"x.{queryFullPath.Join(ctx.CodeRenderingContext.IsLegacyCompatibilityMode() ? "." : "!.")}{(ctx.CodeRenderingContext.IsLegacyCompatibilityMode() ? "" : "!")}")}});
                """)}}
                }
                """;
        },
    };

    void IValueMemberType.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
        // 特になし
    }

    string IValueMemberType.RenderCreateDummyDataValueBody(CodeRenderingContext ctx) {
        return $$"""
            return string
                .Concat(Enumerable.Range(0, member.MaxLength ?? 12)
                .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+-=[]{}\\\"|;:,.<>?"[context.Random.Next(0, 63)]));
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}

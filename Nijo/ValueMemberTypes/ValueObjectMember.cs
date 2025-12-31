using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models;
using Nijo.Models.QueryModelModules;
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
/// 値オブジェクト型
/// 値オブジェクトを表すC#のクラスを参照する型
/// </summary>
internal class ValueObjectMember : IValueMemberType {
    public string TypePhysicalName => _ctx.GetPhysicalName(_xElement);
    string IValueMemberType.SchemaTypeName => _ctx.GetPhysicalName(_xElement);
    string IValueMemberType.CsDomainTypeName => _ctx.GetPhysicalName(_xElement);
    string IValueMemberType.CsPrimitiveTypeName => "string";
    string IValueMemberType.TsTypeName => $"Util.{_ctx.GetPhysicalName(_xElement)}";
    UiConstraint.E_Type IValueMemberType.UiConstraintType => UiConstraint.E_Type.StringMemberConstraint;
    string IValueMemberType.DisplayName => "値オブジェクト型";

    string IValueMemberType.RenderSpecificationMarkdown() {
        return $$"""
            独自の値オブジェクトを参照する型です。
            通常の文字列型よりも特別な意味を持った値（「○○コード」など）を表現するのに適しています。
            検索時の挙動は完全一致・部分一致・前方一致・後方一致から選択可能です。

            予め nijo.xml で値オブジェクトの種類を `{{ValueObjectModel.SCHEMA_NAME}}` モデルとして定義しておく必要があります。
            """;
    }

    private readonly XElement _xElement;
    private readonly SchemaParseContext _ctx;

    public ValueObjectMember(XElement xElement, SchemaParseContext ctx) {
        _xElement = xElement;
        _ctx = ctx;
    }

    void IValueMemberType.Validate(XElement element, SchemaParseContext context, Action<XElement, string> addError) {
        // 値オブジェクト型の検証
        // 必要に応じて値オブジェクトの存在確認などを検証するコードをここに追加できます
    }

    ValueMemberSearchBehavior? IValueMemberType.SearchBehavior => new() {
        FilterCsTypeName = "string",
        FilterTsTypeName = "string",
        RenderTsNewObjectFunctionValue = () => "''",
        RenderFiltering = ctx => {
            var query = ctx.Query.Root.Name;
            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "!.");

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            var searchBehavior = ctx.Query.Metadata is IInstanceValuePropertyMetadata vm
                && vm.SchemaPathNode.XElement.Attribute(BasicNodeOptions.StringSearchBehavior.AttributeName)?.Value is string s
                ? s
                : BasicNodeOptions.STRING_SEARCH_BEHAVIOR_PARTIAL;

            string GetComparison(string target) {
                return searchBehavior switch {
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_EXACT => $"{target} == trimmed",
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_FORWARD => $"Microsoft.EntityFrameworkCore.EF.Functions.Like((string){target}, $\"{{escaped}}%\", \"\\\\\")",
                    BasicNodeOptions.STRING_SEARCH_BEHAVIOR_BACKWARD => $"Microsoft.EntityFrameworkCore.EF.Functions.Like((string){target}, $\"%{{escaped}}\", \"\\\\\")",
                    _ => $"Microsoft.EntityFrameworkCore.EF.Functions.Like((string){target}, $\"%{{escaped}}%\", \"\\\\\")",
                };
            }

            return $$"""
                if (!string.IsNullOrWhiteSpace({{fullpathNullable}})) {
                    var trimmed = {{fullpathNotNull}}!.Trim();
                {{If(searchBehavior != BasicNodeOptions.STRING_SEARCH_BEHAVIOR_EXACT, () => $$"""
                    var escaped = trimmed.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
                """)}}
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join("!.")}}.Any(y => {{GetComparison($"y.{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}!")}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{GetComparison($"x.{queryFullPath.Join("!.")}!")}});
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
            return member.IsKey
                ? new {{_ctx.GetPhysicalName(_xElement)}}($"VO_{context.GetNextSequence():D10}_{string.Concat(Enumerable.Range(0, 6).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[context.Random.Next(0, 36)]))}")
                : new {{_ctx.GetPhysicalName(_xElement)}}(string.Concat(Enumerable.Range(0, member.MaxLength ?? 12).Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[context.Random.Next(0, 36)])));
            """;
    }

    void IValueMemberType.RenderStaticSources(CodeRenderingContext ctx) {
        // 特になし
    }
}

using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;

namespace Nijo.Models.QueryModelModules {
    /// <summary>
    /// 範囲検索のレンダリングを共通化するユーティリティクラス
    /// </summary>
    public static class RangeSearchRenderer {
        /// <summary>
        /// 範囲検索のレンダリングを行う
        /// </summary>
        /// <param name="ctx">フィルタリングのレンダリングコンテキスト</param>
        /// <returns>レンダリングされたC#コード</returns>
        public static string RenderRangeSearchFiltering(FilterStatementRenderingContext ctx) {
            var query = ctx.Query.Root.Name;
            var cast = ctx.SearchCondition.Metadata.Type.RenderCastToPrimitiveType();
            var queryCast = ctx.Query.Metadata.Type.RenderCastToPrimitiveType(notNull: true);

            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            var manyTarget = RenderQueryTarget($"y.{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}");
            var singleTarget = RenderQueryTarget($"x.{queryFullPath.Join(".")}");

            var manyFromToCondition = RenderComparison(manyTarget, ">=", "from") + " && " + RenderComparison(manyTarget, "<=", "to");
            var singleFromToCondition = RenderComparison(singleTarget, ">=", "from") + " && " + RenderComparison(singleTarget, "<=", "to");
            var manyFromCondition = RenderComparison(manyTarget, ">=", "from");
            var singleFromCondition = RenderComparison(singleTarget, ">=", "from");
            var manyToCondition = RenderComparison(manyTarget, "<=", "to");
            var singleToCondition = RenderComparison(singleTarget, "<=", "to");

            return $$"""
                if ({{fullpathNullable}}?.From != null && {{fullpathNullable}}?.To != null) {
                    var from = {{cast}}{{fullpathNotNull}}!.From;
                    var to = {{cast}}{{fullpathNotNull}}!.To;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => {{manyFromToCondition}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{singleFromToCondition}});
                """)}}
                } else if ({{fullpathNullable}}?.From != null) {
                    var from = {{cast}}{{fullpathNotNull}}!.From;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => {{manyFromCondition}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{singleFromCondition}});
                """)}}
                } else if ({{fullpathNullable}}?.To != null) {
                    var to = {{cast}}{{fullpathNotNull}}!.To;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => {{manyToCondition}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{singleToCondition}});
                """)}}
                }
                """;

            string RenderQueryTarget(string target) {
                return string.IsNullOrEmpty(queryCast)
                    ? target
                    : $"{queryCast}{target}";
            }

            string RenderComparison(string target, string op, string value) {
                if (ctx.Query.Metadata.Type.CsPrimitiveTypeName == "string") {
                    return $"string.Compare({target}, {value}) {op} 0";
                }

                return $"{target} {op} {value}";
            }
        }
    }
}

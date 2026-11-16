using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Linq;
using Nijo.DataStructures.ValueMemberTypes;

namespace Nijo.DataStructures.IsQueryable {
    /// <summary>
    /// 範囲検索のレンダリングを共通化するユーティリティクラス
    /// </summary>
    public static class RangeSearchRenderer {
        /// <summary>
        /// 範囲検索のレンダリングを行う
        /// </summary>
        /// <param name="ctx">フィルタリングのレンダリングコンテキスト</param>
        /// <param name="includeWholeMinuteOfTo">
        /// 範囲の終わりに指定した分の終わりまでを含めるかどうか。日付時刻に使う。
        /// 画面からは分までしか指定できないが、保存されている日時は秒をもつため、終わりの次の分の始まり「未満」で比べる。
        /// </param>
        /// <returns>レンダリングされたC#コード</returns>
        public static string RenderRangeSearchFiltering(FilterStatementRenderingContext ctx, bool includeWholeMinuteOfTo = false) {
            var query = ctx.Query.Root.Name;
            var cast = ctx.SearchCondition.Metadata.Type.RenderCastToPrimitiveType();

            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            var manyTarget = $"y.{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}";
            var singleTarget = $"x.{queryFullPath.Join(".")}";

            var toOperator = includeWholeMinuteOfTo ? "<" : "<=";
            var manyFromToCondition = RenderComparison(manyTarget, ">=", "from") + " && " + RenderComparison(manyTarget, toOperator, "to");
            var singleFromToCondition = RenderComparison(singleTarget, ">=", "from") + " && " + RenderComparison(singleTarget, toOperator, "to");
            var manyFromCondition = RenderComparison(manyTarget, ">=", "from");
            var singleFromCondition = RenderComparison(singleTarget, ">=", "from");
            var manyToCondition = RenderComparison(manyTarget, toOperator, "to");
            var singleToCondition = RenderComparison(singleTarget, toOperator, "to");
            var toDeclaration = includeWholeMinuteOfTo
                ? $$"""
                    // 指定した分の終わりまで含めるよう、次の分の始まり「未満」で比べる
                    var toValue = {{fullpathNotNull}}!.To!.Value;
                    var to = new DateTime(toValue.Year, toValue.Month, toValue.Day, toValue.Hour, toValue.Minute, 0).AddMinutes(1);
                    """
                : $"var to = {cast}{fullpathNotNull}!.To;";

            return $$"""
                if ({{fullpathNullable}}?.From != null && {{fullpathNullable}}?.To != null) {
                    var from = {{cast}}{{fullpathNotNull}}!.From;
                    {{WithIndent(toDeclaration)}}
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
                    {{WithIndent(toDeclaration)}}
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => {{manyToCondition}}));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => {{singleToCondition}});
                """)}}
                }
                """;

            string RenderComparison(string target, string op, string value) {
                if (ctx.Query.Metadata.Type.CsPrimitiveTypeName == "string") {
                    return $"string.Compare({target}, {value}) {op} 0";
                }

                return $"{target} {op} {value}";
            }
        }
    }
}

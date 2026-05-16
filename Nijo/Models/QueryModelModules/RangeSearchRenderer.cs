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
            var isDateTime = ctx.SearchCondition.Metadata.Type.SchemaTypeName == "datetime";

            var fullpathNullable = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, "?.");
            var fullpathNotNull = ctx.SearchCondition.GetJoinedPathFromInstance(E_CsTs.CSharp, ".");
            var legacyToExpr = isDateTime
                ? $"{fullpathNotNull}.To?.AddSeconds(1)"
                : $"{fullpathNotNull}.To";
            var legacyToOperator = isDateTime ? "<" : "<=";
            var legacyToComment = isDateTime ? " // 1秒足している理由は後述Toの分岐のコメントを参照" : string.Empty;

            var queryFullPath = ctx.Query.GetFlattenArrayPath(E_CsTs.CSharp, out var isMany);
            var queryOwnerFullPath = queryFullPath.SkipLast(1);

            if (ctx.CodeRenderingContext.IsLegacyCompatibilityMode()) {
                return $$"""
                    if ({{fullpathNullable}}?.From != null
                     && {{fullpathNullable}}?.To != null) {
                        var from = {{fullpathNotNull}}.From;
                        var to = {{legacyToExpr}};{{legacyToComment}}
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} >= from && y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} {{legacyToOperator}} to));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} >= from && x.{{queryFullPath.Join(".")}} {{legacyToOperator}} to);
                    """)}}

                    } else if ({{fullpathNullable}}?.From != null) {
                        var from = {{fullpathNotNull}}.From;
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} >= from));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} >= from);
                    """)}}

                    } else if ({{fullpathNullable}}?.To != null) {
                    {{If(isDateTime, () => $$"""
                        // 日時がミリ秒まで登録されていた場合、検索条件は秒までしか指定できないため、正しく検索できない場合がある。
                        // 例:1999/01/01 09:12:34.5678と登録されていたら、検索条件(日時to)が1999/01/01 09:12:34ではヒットしない。
                        // 上記の例があるため、ミリ秒の考慮として1秒加算した値で検索を行う。
                    """)}}
                        var to = {{legacyToExpr}};
                    {{If(isMany, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} {{legacyToOperator}} to));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} {{legacyToOperator}} to);
                    """)}}
                    }
                    """;
            }

            return $$"""
                if ({{fullpathNullable}}?.From != null && {{fullpathNullable}}?.To != null) {
                    var from = {{cast}}{{fullpathNotNull}}!.From;
                    var to = {{cast}}{{fullpathNotNull}}!.To;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} >= from && y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} <= to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} >= from && x.{{queryFullPath.Join(".")}} <= to);
                """)}}
                } else if ({{fullpathNullable}}?.From != null) {
                    var from = {{cast}}{{fullpathNotNull}}!.From;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} >= from));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} >= from);
                """)}}
                } else if ({{fullpathNullable}}?.To != null) {
                    var to = {{cast}}{{fullpathNotNull}}!.To;
                {{If(isMany, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryOwnerFullPath.Join(".")}}.Any(y => y.{{ctx.Query.Metadata.GetPropertyName(E_CsTs.CSharp)}} <= to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{queryFullPath.Join(".")}} <= to);
                """)}}
                }
                """;
        }
    }
}

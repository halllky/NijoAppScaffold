using Nijo.Models.ReadModel2Features;
using Nijo.Models.RefTo;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts.BothOfClientAndServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class YearMonthDayTime : SchalarMemberType {
        public override string GetUiDisplayName() => "日付時刻";
        public override string GetHelpText() => $"日付時刻。";

        public override string GetCSharpTypeName() => "DateTime";
        public override string GetTypeScriptTypeName() => YearMonthDay.CurrentCodeRenderingContext?.Config.UseWijmo == true ? "Date | null" : "string";

        protected override string ComponentName => "Input.DateTime";
        private protected override IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {

            if (YearMonthDay.CurrentCodeRenderingContext?.Config.UseWijmo != true) {
                yield return $"className=\"w-48\"";
            }
        }

        public override WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "string",
            Format = "null",
        };

        private protected override string RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {

            if (ctx.CodeRenderingContext.Config.UseWijmo) {
                var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

                //Wijmo
                return $$"""
                <div className="flex flex-wrap items-center">
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.FROM_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                  <span className="select-none">～</span>
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.TO_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                </div>
                """;

            } else {
                // nijo標準
                return base.RenderSearchConditionVFormBody(vm, ctx);
            }


        }

        public override string DataTableColumnDefHelperName => "datetime";

        private protected override string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
                ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
                : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var nullableFullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}?.{FromTo.FROM}";
            var nullableFullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}?.{FromTo.TO}";
            var fullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.FROM}";
            var fullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.TO}";
            var whereFullpath = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetFullPathAsSearchResult(E_CsTs.CSharp, out var isArray)
                : member.GetFullPathAsDbEntity(E_CsTs.CSharp, out isArray);

            return $$"""
                if ({{nullableFullPathFrom}} != null
                 && {{nullableFullPathTo}} != null) {
                    var from = {{fullPathFrom}};
                    var to = {{fullPathTo}}?.AddSeconds(1); // 1秒足している理由は後述Toの分岐のコメントを参照
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} >= from && y.{{member.MemberName}} < to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} >= from && x.{{whereFullpath.Join(".")}} < to);
                """)}}

                } else if ({{nullableFullPathFrom}} != null) {
                    var from = {{fullPathFrom}};
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} >= from));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} >= from);
                """)}}

                } else if ({{nullableFullPathTo}} != null) {
                    // 日時がミリ秒まで登録されていた場合、検索条件は秒までしか指定できないため、正しく検索できない場合がある。
                    // 例:1999/01/01 09:12:34.5678と登録されていたら、検索条件(日時to)が1999/01/01 09:12:34ではヒットしない。
                    // 上記の例があるため、ミリ秒の考慮として1秒加算した値で検索を行う。
                    var to = {{fullPathTo}}?.AddSeconds(1);
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} < to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} < to);
                """)}}
                }
                """;
        }
    }
}

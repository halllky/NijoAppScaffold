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

namespace Nijo.Core {
    /// <summary>
    /// 数値や日付など連続した量をもつ値
    /// </summary>
    internal abstract class SchalarMemberType : IAggregateMemberType {
        public abstract string GetUiDisplayName();
        public abstract string GetHelpText();

        public virtual void GenerateCode(CodeRenderingContext context) { }

        public abstract string GetCSharpTypeName();
        public abstract string GetTypeScriptTypeName();

        public virtual string UiConstraintType => "MemberConstraintBase";
        public virtual IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            yield break;
        }

        public string GetSearchConditionCSharpType(AggregateMember.ValueMember vm) {
            var type = GetCSharpTypeName();
            return $"{FromTo.CLASSNAME}<{type}?>";
        }
        public string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm) {
            var type = GetTypeScriptTypeName();
            return $"{{ {FromTo.FROM_TS}?: {type}, {FromTo.TO_TS}?: {type} }}";
        }

        public abstract WijmoGridColumnSetting GetWijmoGridColumnSetting();

        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            return RenderFilteringStatement(member, query, searchCondition, searchConditionObject, searchQueryObject);
        }
        private protected virtual string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
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
                    var to = {{fullPathTo}};
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} >= from && y.{{member.MemberName}} <= to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} >= from && x.{{whereFullpath.Join(".")}} <= to);
                """)}}

                } else if ({{nullableFullPathFrom}} != null) {
                    var from = {{fullPathFrom}};
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} >= from));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} >= from);
                """)}}

                } else if ({{nullableFullPathTo}} != null) {
                    var to = {{fullPathTo}};
                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}} <= to));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}} <= to);
                """)}}
                }
                """;
        }

        /// <summary>コンポーネント名</summary>
        protected abstract string ComponentName { get; }
        /// <summary>レンダリングされるコンポーネントの属性をレンダリングします</summary>
        private protected abstract IEnumerable<string> RenderAttributes(AggregateMember.ValueMember vm, FormUIRenderingContext ctx);

        private protected virtual string RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

            return $$"""
                <div className="flex flex-nowrap items-center gap-1">
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.FROM_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                  <span className="select-none">～</span>
                  <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.TO_TS}}`)} {{RenderAttributes(vm, ctx).Select(x => $"{x} ").Join("")}}/>
                </div>
                """;
        }
        string IAggregateMemberType.RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) => RenderSearchConditionVFormBody(vm, ctx);

        string IAggregateMemberType.RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

            var attrs = RenderAttributes(vm, ctx).ToList();
            var readOnly = ctx.RenderReadOnlyStatement(vm.Declared);
            if (readOnly != null) attrs.Add(readOnly);

            if (vm.Options.UiWidth != null) {
                var rem = vm.Options.UiWidth.GetCssValue();
                attrs.Add($"className=\"min-w-[{rem}]\"");
                attrs.Add($"inputClassName=\"max-w-[{rem}] min-w-[{rem}]\"");
            }

            ctx.EditComponentAttributes?.Invoke(vm, attrs);

            return $$"""
                <{{ComponentName}} {...{{ctx.Register}}(`{{fullpath}}`)} {{attrs.Join(" ")}}/>
                {{ctx.RenderErrorMessage(vm)}}
                """;
        }

        public abstract string DataTableColumnDefHelperName { get; }
    }
}

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
    /// 文字列系メンバー型
    /// </summary>
    internal abstract class StringMemberType : IAggregateMemberType {
        public abstract string GetUiDisplayName();
        public abstract string GetHelpText();

        /// <summary>
        /// 検索時の挙動。
        /// 既定値は <see cref="E_SearchBehavior.PartialMatch"/>
        /// </summary>
        protected virtual E_SearchBehavior GetSearchBehavior(AggregateMember.ValueMember vm) => E_SearchBehavior.PartialMatch;
        /// <summary>
        /// 複数行にわたる文字列になる可能性があるかどうか
        /// </summary>
        protected virtual bool MultiLine => false;
        /// <summary>
        /// 入力に使われる React コンポーネントの名前
        /// </summary>
        protected virtual string ReactComponentName => MultiLine
            ? "Input.Description"
            : "Input.Word";

        public virtual string GetCSharpTypeName() => "string";
        public virtual string GetTypeScriptTypeName() => "string";

        public virtual string UiConstraintType => "StringMemberConstraint";
        public virtual IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            if (vm.Options.MaxLength.HasValue) {
                yield return $"maxLength: {vm.Options.MaxLength}";
            }
            if (vm.Options.CharacterType != null) {
                yield return $"characterType: '{vm.Options.CharacterType}'";
            }
        }

        public virtual string GetSearchConditionCSharpType(AggregateMember.ValueMember vm) {
            return vm.Options.SearchBehavior == E_SearchBehavior.Range
                ? $"{FromTo.CLASSNAME}<string>"
                : $"string";
        }
        public virtual string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm) {
            return vm.Options.SearchBehavior == E_SearchBehavior.Range
                ? $"{{ {FromTo.FROM_TS}?: string, {FromTo.TO_TS}?: string }}"
                : $"string";
        }

        public WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "String",
            Format = null,
        };
        private protected virtual string RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
                ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
                : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var whereFullpath = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetFullPathAsSearchResult(E_CsTs.CSharp, out var isArray)
                : member.GetFullPathAsDbEntity(E_CsTs.CSharp, out isArray);

            if (GetSearchBehavior(member) == E_SearchBehavior.Range) {
                var nullableFullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}?.{FromTo.FROM}";
                var nullableFullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}?.{FromTo.TO}";
                var fullPathFrom = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.FROM}";
                var fullPathTo = $"{searchCondition}.{pathFromSearchCondition.Join(".")}.{FromTo.TO}";
                return $$"""
                    if (!string.IsNullOrWhiteSpace({{nullableFullPathFrom}})
                     && !string.IsNullOrWhiteSpace({{nullableFullPathTo}})) {
                        var from = {{fullPathFrom}}.Trim();
                        var to = {{fullPathTo}}.Trim();
                    {{If(isArray, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => string.Compare(y.{{member.MemberName}}, from) >= 0 && string.Compare(y.{{member.MemberName}}, to) <= 0));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => string.Compare(x.{{whereFullpath.Join(".")}}, from) >= 0 && string.Compare(x.{{whereFullpath.Join(".")}}, to) <= 0);
                    """)}}

                    } else if (!string.IsNullOrWhiteSpace({{nullableFullPathFrom}})) {
                        var from = {{fullPathFrom}}.Trim();
                    {{If(isArray, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => string.Compare(y.{{member.MemberName}}, from) >= 0));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => string.Compare(x.{{whereFullpath.Join(".")}}, from) >= 0);
                    """)}}

                    } else if (!string.IsNullOrWhiteSpace({{nullableFullPathTo}})) {
                        var to = {{fullPathTo}}.Trim();
                    {{If(isArray, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => string.Compare(y.{{member.MemberName}}, to) <= 0));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => string.Compare(x.{{whereFullpath.Join(".")}}, to) <= 0);
                    """)}}
                    }
                    """;

            } else {
                var fullpathNullable = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}";
                var fullpathNotNull = $"{searchCondition}.{pathFromSearchCondition.Join(".")}";
                var method = GetSearchBehavior(member) switch {
                    E_SearchBehavior.PartialMatch => "Contains",
                    E_SearchBehavior.ForwardMatch => "StartsWith",
                    E_SearchBehavior.BackwardMatch => "EndsWith",
                    _ => "Equals",
                };
                return $$"""
                    if (!string.IsNullOrWhiteSpace({{fullpathNullable}})) {
                        var trimmed = {{fullpathNotNull}}.Trim();
                    {{If(isArray, () => $$"""
                        {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => y.{{member.MemberName}}.{{method}}(trimmed)));
                    """).Else(() => $$"""
                        {{query}} = {{query}}.Where(x => x.{{whereFullpath.Join(".")}}.{{method}}(trimmed));
                    """)}}
                    }
                    """;
            }
        }
        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            return RenderFilteringStatement(member, query, searchCondition, searchConditionObject, searchQueryObject);
        }

        string IAggregateMemberType.RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var attrs = new List<string>();
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

            if (vm.Options.UiWidth != null) {
                var rem = vm.Options.UiWidth.GetCssValue();
                attrs.Add($"className=\"min-w-[{rem}]\"");
                attrs.Add($"inputClassName=\"max-w-[{rem}] min-w-[{rem}]\"");
            }

            ctx.EditComponentAttributes?.Invoke(vm, attrs);

            if (GetSearchBehavior(vm) == E_SearchBehavior.Range) {
                return $$"""
                    <div className="flex flex-nowrap items-center gap-1">
                      <Input.Word {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.FROM_TS}}`)} {{attrs.Join(" ")}}/>
                      <span className="select-none">～</span>
                      <Input.Word {...{{ctx.Register}}(`{{fullpath}}.{{FromTo.TO_TS}}`)} {{attrs.Join(" ")}}/>
                    </div>
                    """;

            } else {
                return $$"""
                    <Input.Word {...{{ctx.Register}}(`{{fullpath}}`)} {{attrs.Join(" ")}}/>
                    """;
            }
        }

        string IAggregateMemberType.RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var attrs = new List<string>();
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");
            attrs.Add($"{{...{ctx.Register}(`{fullpath}`)}}");

            attrs.Add(ctx.RenderReadOnlyStatement(vm.Declared));

            if (vm.Options.UiWidth != null) {
                var rem = vm.Options.UiWidth.GetCssValue();
                attrs.Add($"className=\"min-w-[{rem}]\"");
                attrs.Add($"inputClassName=\"max-w-[{rem}] min-w-[{rem}]\"");
            }

            ctx.EditComponentAttributes?.Invoke(vm, attrs);

            return $$"""
                <{{ReactComponentName}} {{attrs.Join(" ")}}/>
                {{ctx.RenderErrorMessage(vm)}}
                """;
        }

        public string DataTableColumnDefHelperName => MultiLine
            ? "multiLineText"
            : "text";
    }

    /// <summary>
    /// 文字列検索の挙動
    /// </summary>
    public enum E_SearchBehavior {
        /// <summary>
        /// 完全一致。
        /// 発行されるSQL文: WHERE DBの値 = 検索条件
        /// </summary>
        Strict,
        /// <summary>
        /// 部分一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件%'
        /// </summary>
        PartialMatch,
        /// <summary>
        /// 前方一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '検索条件%'
        /// </summary>
        ForwardMatch,
        /// <summary>
        /// 後方一致。
        /// 発行されるSQL文: WHERE DBの値 LIKE '%検索条件'
        /// </summary>
        BackwardMatch,
        /// <summary>
        /// 範囲検索。
        /// 発行されるSQL文: WHERE DBの値 BETWEEN '検索条件1個目' AND '検索条件2個目'
        /// </summary>
        Range,
    }
}

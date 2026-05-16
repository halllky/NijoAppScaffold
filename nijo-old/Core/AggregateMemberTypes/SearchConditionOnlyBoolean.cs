using Nijo.Models.ReadModel2Features;
using Nijo.Models.RefTo;
using Nijo.Models.WriteModel2Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Core.AggregateMemberTypes {
    internal class SearchConditionOnlyBoolean : IAggregateMemberType {
        public string GetUiDisplayName() => "真偽値（検索条件欄のみに存在するもの）";
        public string GetHelpText() => $$"""
            検索条件欄のみに存在する真偽値。
            これがtrueの場合のWHERE句処理は自動生成されず、自前で定義する必要があるので注意。
            """;

        public string GetCSharpTypeName() => "bool";
        public string GetTypeScriptTypeName() => "boolean";

        public string UiConstraintType => "MemberConstraintBase";
        public virtual IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            yield break;
        }

        public string GetSearchConditionCSharpType(AggregateMember.ValueMember vm) => "bool";
        public string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm) => "boolean";

        void IAggregateMemberType.GenerateCode(CodeRenderingContext context) {
        }

        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            return $$"""
                // この項目のWHERE句の処理は Create...QuerySource メソッドで個別に実装してください。
                """;
        }

        string IAggregateMemberType.RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");
            return $$"""
                <Input.CheckBox {...{{ctx.Register}}(`{{fullpath}}`)} />
                """;
        }

        string IAggregateMemberType.RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

            var attrs = new List<string>();
            attrs.Add(ctx.RenderReadOnlyStatement(vm.Declared));
            ctx.EditComponentAttributes?.Invoke(vm, attrs);

            return $$"""
                <Input.CheckBox {...{{ctx.Register}}(`{{fullpath}}`)} {{attrs.Join(" ")}}/>
                {{ctx.RenderErrorMessage(vm)}}
                """;
        }

        public WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "",
            Format = null,
        };
        public string DataTableColumnDefHelperName => "";
    }
}

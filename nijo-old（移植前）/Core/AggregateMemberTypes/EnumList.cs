using Microsoft.EntityFrameworkCore.Metadata.Conventions;
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
    internal class EnumList : IAggregateMemberType {
        public string GetUiDisplayName() => "列挙体";
        public string GetHelpText() => $"列挙体。";

        public EnumList(EnumDefinition definition) {
            Definition = definition;
        }
        public EnumDefinition Definition { get; }

        public string GetCSharpTypeName() => Definition.Name;
        public string GetTypeScriptTypeName() {
            return Definition.Items.Select(x => $"'{(x.IsEmptyDisplayName ? string.Empty : x.DisplayName.Replace("'", "\\'"))}'").Join(" | ");
        }

        public string UiConstraintType => "MemberConstraintBase";
        public virtual IEnumerable<string> RenderUiConstraintValue(AggregateMember.ValueMember vm) {
            yield break;
        }

        private string SearchConditionEnum => $"{Definition.Name}SearchCondition";
        private const string ANY_CHECKED = "AnyChecked";

        public string GetSearchConditionCSharpType(AggregateMember.ValueMember vm) {
            return SearchConditionEnum;
        }
        public string GetSearchConditionTypeScriptType(AggregateMember.ValueMember vm) {
            return $"{{ {Definition.Items.Select(i => $"'{i.DisplayName.Replace("'", "\\'")}'?: boolean").Join(", ")} }}";
        }

        void IAggregateMemberType.GenerateCode(CodeRenderingContext context) {
            context.CoreLibrary.Enums.Add($$"""
                /// <summary>{{Definition.Name}}の検索条件クラス</summary>
                public class {{SearchConditionEnum}} {
                {{Definition.Items.SelectTextTemplate(item => $$"""
                    [System.Text.Json.Serialization.JsonPropertyName("{{item.DisplayName.Replace("\"", "\\\"")}}")]
                    public bool {{item.PhysicalName}} { get; set; }
                """)}}

                    /// <summary>いずれかの値が選択されているかを返します。</summary>
                    public bool {{ANY_CHECKED}}() {
                {{Definition.Items.SelectTextTemplate(item => $$"""
                        if ({{item.PhysicalName}}) return true;
                """)}}
                        return false;
                    }
                }
                """);
        }

        string IAggregateMemberType.RenderFilteringStatement(AggregateMember.ValueMember member, string query, string searchCondition, E_SearchConditionObject searchConditionObject, E_SearchQueryObject searchQueryObject) {
            var pathFromSearchCondition = searchConditionObject == E_SearchConditionObject.SearchCondition
                ? member.Declared.GetFullPathAsSearchConditionFilter(E_CsTs.CSharp)
                : member.Declared.GetFullPathAsRefSearchConditionFilter(E_CsTs.CSharp);
            var fullpathNullable = $"{searchCondition}.{pathFromSearchCondition.Join("?.")}";
            var fullpathNotNull = $"{searchCondition}.{pathFromSearchCondition.Join(".")}";

            var enumType = GetCSharpTypeName();
            string paramType;
            string cast;
            if (string.IsNullOrWhiteSpace(member.Options.EnumSqlParamType)) {
                paramType = enumType;
                cast = string.Empty;
            } else {
                paramType = member.Options.EnumSqlParamType;
                cast = $"({member.Options.EnumSqlParamType}?)";
            }

            var whereFullpath = searchQueryObject == E_SearchQueryObject.SearchResult
                ? member.GetFullPathAsSearchResult(E_CsTs.CSharp, out var isArray)
                : member.GetFullPathAsDbEntity(E_CsTs.CSharp, out isArray);

            return $$"""
                if ({{fullpathNullable}} != null && {{fullpathNotNull}}.{{ANY_CHECKED}}()) {
                    var array = new List<{{paramType}}?>();
                {{Definition.Items.SelectTextTemplate(item => $$"""
                    if ({{fullpathNotNull}}.{{item.PhysicalName}}) array.Add({{cast}}{{enumType}}.{{item.PhysicalName}});
                """)}}

                {{If(isArray, () => $$"""
                    {{query}} = {{query}}.Where(x => x.{{whereFullpath.SkipLast(1).Join(".")}}.Any(y => array.Contains({{cast}}y.{{member.MemberName}})));
                """).Else(() => $$"""
                    {{query}} = {{query}}.Where(x => array.Contains({{cast}}x.{{whereFullpath.Join(".")}}));
                """)}}
                }
                """;
        }

        string IAggregateMemberType.RenderSearchConditionVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");
            return $$"""
                <div className="flex flex-wrap gap-x-2 gap-y-1">
                {{Definition.Items.SelectTextTemplate(item => $$"""
                  <Input.CheckBox label="{{item.DisplayName.Replace("\"", "&quot;")}}" {...{{ctx.Register}}(`{{fullpath}}.{{item.DisplayName.Replace("`", "\\`")}}`)} />
                """)}}
                </div>
                """;
        }

        string IAggregateMemberType.RenderSingleViewVFormBody(AggregateMember.ValueMember vm, FormUIRenderingContext ctx) {
            var fullpath = ctx.GetReactHookFormFieldPath(vm.Declared).Join(".");

            var attrs = new List<string> {
                $"options={{[{Definition.Items.Select(x => $"'{x.DisplayName.Replace("'", "\\'")}' as const").Join(", ")}]}}",
                $"textSelector={{item => item}}",
                $"{ctx.RenderReadOnlyStatement(vm.Declared)}", // readonly
            };

            // ラジオボタンまたはコンボボックスどちらか決め打ちの場合
            if (vm.Options.IsCombo) {
                attrs.Add("combo");
            } else if (vm.Options.IsRadio) {
                attrs.Add("radio");
            }

            ctx.EditComponentAttributes?.Invoke(vm, attrs);

            return $$"""
                <Input.Selection {...{{ctx.Register}}(`{{fullpath}}`)} {{attrs.Join(" ")}}/>
                {{ctx.RenderErrorMessage(vm)}}
                """;
        }
        public WijmoGridColumnSetting GetWijmoGridColumnSetting() => new WijmoGridColumnSetting {
            DataType = "String",
            Format = null,
        };

        public string DataTableColumnDefHelperName => Definition.Name;
    }
}

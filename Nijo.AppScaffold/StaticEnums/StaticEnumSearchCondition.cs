using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.CodeGenerating;
using Nijo.SchemaParsing;

namespace Nijo.StaticEnums {
    /// <summary>
    /// 静的区分の検索条件オブジェクト
    /// </summary>
    internal class StaticEnumSearchCondition {

        internal StaticEnumSearchCondition(StaticEnumRoot staticEnum) {
            _staticEnum = staticEnum;
        }

        private readonly StaticEnumRoot _staticEnum;
        internal string CsClassName => _staticEnum.CsSearchConditionClassName;
        internal string TsTypeName => _staticEnum.CsSearchConditionClassName;

        internal const string ANY_CHECKED = "AnyChecked";

        internal string RenderCSharp() {
            return $$"""
                /// <summary>{{_staticEnum.DisplayName}}の検索条件クラス</summary>
                public class {{CsClassName}} {
                {{_staticEnum.GetValues().SelectTextTemplate(item => $$"""
                    public bool {{item.PhysicalName}} { get; set; }
                """)}}

                    /// <summary>いずれかの値が選択されているかを返します。</summary>
                    public bool {{ANY_CHECKED}}() {
                {{_staticEnum.GetValues().SelectTextTemplate(item => $$"""
                        if ({{item.PhysicalName}}) return true;
                """)}}
                        return false;
                    }
                }
                """;
        }

        internal string RenderTypeScript() {
            return $$"""
                /** {{_staticEnum.DisplayName}}の検索条件オブジェクト */
                export type {{TsTypeName}} = {
                {{_staticEnum.GetValues().SelectTextTemplate(item => $$"""
                  {{item.PhysicalName}}?: boolean
                """)}}
                }
                """;
        }
    }
}

using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.StaticEnums {
    /// <summary>
    /// 静的区分の種類
    /// </summary>
    internal class StaticEnumDef {

        internal StaticEnumDef(EnumDefParser parser, StaticEnumRoot rootAggregate) {
            _parser = parser;
            _rootAggregate = rootAggregate;
        }

        private readonly EnumDefParser _parser;
        private readonly StaticEnumRoot _rootAggregate;

        internal string DisplayName => _parser.DisplayName;
        internal string CsEnumName => _parser.CsEnumName;
        internal string TsTypeName => _parser.TsTypeName;

        internal IEnumerable<StaticEnumValueDef> GetValues() {
            return _rootAggregate.GetValues();
        }

        internal string RenderCSharp() {
            return $$"""
                /// <summary>
                /// {{DisplayName}}
                /// </summary>
                public enum {{CsEnumName}} {
                {{GetValues().SelectTextTemplate(item => $$"""
                    [Display(Name = "{{item.DisplayName.Replace("\"", "\\\"")}}")]
                    {{item.PhysicalName}} = {{item.EnumValue}},
                """)}}
                }
                """;
        }

        internal string RenderTypeScript() {
            return $$"""
                /** {{DisplayName}} */
                export type {{TsTypeName}} = {{GetValues().Select(v => $"'{v.DisplayName.Replace("'", "\'")}'").Join(" | ")}}
                """;
        }
    }
}

using Nijo.SchemaParsing;
using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.StaticEnums {
    /// <summary>
    /// 静的区分の値の定義
    /// </summary>
    internal class StaticEnumValueDef {

        internal StaticEnumValueDef(XElement xElement, SchemaParseContext ctx) {
            _xElement = xElement;
            _ctx = ctx;
        }
        private readonly XElement _xElement;
        private readonly SchemaParseContext _ctx;

        public string PhysicalName => _ctx.GetPhysicalName(_xElement);
        public string DisplayName => _xElement.GetDisplayName();
        public int EnumValue => int.Parse(_xElement.Attribute(StaticEnumKeyOption.ATTRIBUTE_NAME)?.Value ?? throw new InvalidOperationException());
    }
}

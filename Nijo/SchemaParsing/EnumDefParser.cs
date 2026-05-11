using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.SchemaParsing {
    /// <summary>
    /// スキーマ定義の列挙体定義を解釈する。
    /// スキーマ編集時とコード自動生成時の両方から利用される。
    /// </summary>
    internal class EnumDefParser {
        /// <summary>
        /// スキーマ定義XML内部でこの名前で指定されているルート集約は列挙体定義
        /// </summary>
        internal const string SCHEMA_NAME = "enum";

        internal EnumDefParser(XElement xElement, SchemaParseContext ctx) {
            _xElement = xElement;
            _ctx = ctx;
        }
        private readonly XElement _xElement;
        private readonly SchemaParseContext _ctx;

        internal string DisplayName => _xElement.GetDisplayName();
        internal string CsEnumName => _ctx.GetPhysicalName(_xElement);
        internal string TsTypeName => _ctx.GetPhysicalName(_xElement);

        internal string CsSearchConditionClassName => $"{CsEnumName}SearchCondition";

        internal string RenderTsSearchConditionType() {
            return $$"""
                { {{_xElement.Elements().Select(el => $"'{el.GetDisplayName().Replace("'", "\\'")}'?: boolean | null").Join(", ")}} }
                """;
        }
        internal string RenderTsSearchConditionInitializer() {
            return $$"""
                { {{_xElement.Elements().Select(el => $"'{el.GetDisplayName().Replace("'", "\\'")}': false").Join(", ")}} }
                """;
        }

        /// <summary>
        /// この列挙体に定義されている値の物理名の一覧を返します。
        /// </summary>
        internal IEnumerable<string> GetItemPhysicalNames() {
            foreach (var el in _xElement.Elements()) {
                yield return el.Name.LocalName;
            }
        }
    }
}

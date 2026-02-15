using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Nijo.SchemaParsing.BasicNodeOptions;

namespace Nijo.Models.ConstantModelModules {
    /// <summary>
    /// ネストされた定数グループの定義
    /// </summary>
    internal class ConstantGroupDef {
        internal ConstantGroupDef(XElement element, string path, SchemaParseContext schemaParser) {
            _element = element;
            Path = path;
            _schemaParser = schemaParser;

            Name = element.Name.LocalName;
            DisplayName = element.Attribute(BasicNodeOptions.DisplayName.AttributeName)?.Value ?? Name;
        }

        private readonly XElement _element;
        private readonly SchemaParseContext _schemaParser;

        internal string Name { get; }
        internal string Path { get; }
        internal string DisplayName { get; }
        internal XElement Element => _element;

        /// <summary>
        /// C#用のクラス名を取得
        /// </summary>
        internal string CsClassName => Path.Replace('.', '_');

        /// <summary>
        /// TypeScript用のオブジェクト名を取得
        /// </summary>
        internal string TsObjectName => Path.Replace('.', '_');

        /// <summary>
        /// このグループ内の直接の定数を取得
        /// </summary>
        internal IEnumerable<ConstantValueDef> GetDirectConstants() {
            foreach (var child in _element.Elements()) {
                var constantType = child.Attribute(ConstantType.AttributeName)?.Value;

                if (constantType != ConstantValueDef.CONSTTYPE_CHILD) {
                    // 定数要素
                    var constantPath = $"{Path}.{child.Name.LocalName}";
                    yield return new ConstantValueDef(child, constantPath, _schemaParser);
                }
            }
        }

        /// <summary>
        /// このグループ内の直接の子グループを取得
        /// </summary>
        internal IEnumerable<ConstantGroupDef> GetDirectChildGroups() {
            foreach (var child in _element.Elements()) {
                var constantType = child.Attribute(ConstantType.AttributeName)?.Value;

                if (constantType == ConstantValueDef.CONSTTYPE_CHILD) {
                    var childPath = $"{Path}.{child.Name.LocalName}";
                    yield return new ConstantGroupDef(child, childPath, _schemaParser);
                }
            }
        }
    }
}

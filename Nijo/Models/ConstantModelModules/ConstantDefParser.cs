using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Nijo.SchemaParsing.BasicNodeOptions;

namespace Nijo.Models.ConstantModelModules;

/// <summary>
/// 定数定義のパーサー
/// </summary>
internal class ConstantDefParser {
    internal ConstantDefParser(XElement rootAggregateElement, SchemaParseContext schemaParser) {
        _rootAggregateElement = rootAggregateElement;
        _schemaParser = schemaParser;
        DisplayName = rootAggregateElement.Attribute(BasicNodeOptions.DisplayName.AttributeName)?.Value ?? rootAggregateElement.Name.LocalName;
        PhysicalName = _schemaParser.GetPhysicalName(rootAggregateElement);
    }

    private readonly XElement _rootAggregateElement;
    private readonly SchemaParseContext _schemaParser;

    internal string DisplayName { get; }
    internal string PhysicalName { get; }
    internal string CsClassName => PhysicalName;
    internal string TsConstantsName => PhysicalName;
    internal XElement RootAggregateElement => _rootAggregateElement;

    /// <summary>
    /// 定数定義を取得する
    /// </summary>
    internal IEnumerable<ConstantValueDef> GetConstants() {
        return GetConstantsRecursive(_rootAggregateElement, string.Empty);
    }

    private IEnumerable<ConstantValueDef> GetConstantsRecursive(XElement element, string parentPath) {
        foreach (var child in element.Elements()) {
            var constantType = child.Attribute(ConstantType.AttributeName)?.Value;
            var currentPath = string.IsNullOrEmpty(parentPath)
                ? child.Name.LocalName
                : $"{parentPath}.{child.Name.LocalName}";

            if (constantType == ConstantValueDef.CONSTTYPE_CHILD) {
                // ネストされた定数グループ
                foreach (var nested in GetConstantsRecursive(child, currentPath)) {
                    yield return nested;
                }
            } else {
                // 定数要素
                yield return new ConstantValueDef(child, currentPath, _schemaParser);
            }
        }
    }

    /// <summary>
    /// ネストされた定数グループを取得する
    /// </summary>
    internal IEnumerable<ConstantGroupDef> GetConstantGroups() {
        return GetConstantGroupsRecursive(_rootAggregateElement, string.Empty);
    }

    private IEnumerable<ConstantGroupDef> GetConstantGroupsRecursive(XElement element, string parentPath) {
        foreach (var child in element.Elements()) {
            var constantType = child.Attribute(ConstantType.AttributeName)?.Value;

            if (constantType == ConstantValueDef.CONSTTYPE_CHILD) {
                var currentPath = string.IsNullOrEmpty(parentPath)
                    ? child.Name.LocalName
                    : $"{parentPath}.{child.Name.LocalName}";

                var group = new ConstantGroupDef(child, currentPath, _schemaParser);
                yield return group;

                // 再帰的にネストされたグループを取得
                foreach (var nested in GetConstantGroupsRecursive(child, currentPath)) {
                    yield return nested;
                }
            }
        }
    }
}

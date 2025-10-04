using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// スキーマ定義全体の情報。
/// データの持ち方こそ違うがデータの範囲は nijo.xml 1個分と対応する。
/// </summary>
public class ApplicationState {
    [JsonPropertyName("applicationName")]
    public string ApplicationName { get; set; } = "";
    [JsonPropertyName("xmlElementTrees")]
    public List<ModelPageForm> XmlElementTrees { get; set; } = [];
    [JsonPropertyName("attributeDefs")]
    public List<XmlElementAttribute> AttributeDefs { get; set; } = [];
    [JsonPropertyName("valueMemberTypes")]
    public List<ValueMemberType> ValueMemberTypes { get; set; } = [];

    /// <summary>
    /// 新しい <see cref="XDocument"/> インスタンスを構築して返す。
    /// ValueMemberやAttributeDefsは、暫定的に、画面上で編集できないものとし、
    /// <see cref="Nijo.SchemaParsing.SchemaParseRule.Default"/> から取得する。
    /// </summary>
    internal bool TryConvertToXDocument(
        XDocument original,
        ICollection<string> errors,
        [NotNullWhen(true)] out XDocument? xDocument,
        [NotNullWhen(true)] out IReadOnlyDictionary<XElement, string>? uuidToXmlElement) {

        xDocument = new XDocument(original);
        if (xDocument.Root == null) {
            errors.Add("XMLにルート要素がありません");
            xDocument = null;
            uuidToXmlElement = null;
            return false;
        }

        // クローンしたXDocumentのルート要素の子を削除する。
        // この後の処理で、ルート要素の子を追加していく。
        xDocument.Root.RemoveNodes();

        // ルート要素の子を追加していく過程で、XElementと、そのXElementの元となったJSONのIdを紐づける。
        var mapping = new Dictionary<XElement, string>();

        for (int i = 0; i < XmlElementTrees.Count; i++) {
            var aggregateTree = XmlElementTrees[i];
            var logName = aggregateTree.XmlElements.Count > 0
                ? $"{aggregateTree.XmlElements[0].LocalName}のツリー"
                : $"第{i + 1}番目の集約ツリー";
            if (XmlElementItem.TryConvertToRootAggregateXElement(
                aggregateTree.XmlElements,
                error => errors.Add($"{logName}: {error}"),
                (xElement, item) => mapping[xElement] = item.UniqueId,
                out var rootAggregate,
                out var commentToRootAggregate)) {

                if (commentToRootAggregate != null) xDocument.Root.Add(commentToRootAggregate);
                xDocument.Root.Add(rootAggregate);
            }
        }
        if (errors.Count > 0) {
            uuidToXmlElement = null;
            return false;
        }

        uuidToXmlElement = mapping;
        return true;
    }
}


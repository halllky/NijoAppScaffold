using System.Linq;
using System.Xml.Linq;

namespace Nijo.WebService.SchemaEditor;

/// <summary>
/// XML処理のヘルパーメソッド
/// </summary>
internal static class XmlHelper {

    /// <summary>
    /// XDocumentの全ての要素の属性を名前順にソートして、保存時の順序を一定にする
    /// </summary>
    internal static void SortXmlAttributes(XDocument document) {
        if (document.Root != null) {
            SortElementAttributes(document.Root);
        }
    }

    /// <summary>
    /// XML要素とその子要素の属性を再帰的に名前順にソートする。
    ///
    /// <see cref="SchemaParsing.SchemaParseContext.ATTR_UNIQUE_ID"/> だけは一番後ろ。
    /// バージョン管理でnijo.xmlの差分をとったとき、XMLの閉じ括弧と同じ行に「絶対に変わらない属性」があると差分が見やすくて嬉しいため。
    /// </summary>
    private static void SortElementAttributes(XElement element) {
        // 現在の要素の属性をソート
        var attributes = element.Attributes().ToList();
        if (attributes.Count > 1) {
            // 属性を名前順にソート
            var sortedAttributes = attributes
                .Where(attr => attr.Name.LocalName != SchemaParsing.SchemaParseContext.ATTR_UNIQUE_ID)
                .OrderBy(attr => attr.Name.LocalName)
                .ToList();

            // 既存の属性をすべて削除
            element.RemoveAttributes();

            // ソート済みの属性を再追加。ユニークIDは最後に追加
            foreach (var attr in sortedAttributes) {
                element.SetAttributeValue(attr.Name, attr.Value);
            }
            var uniqueId = attributes.SingleOrDefault(attr => attr.Name.LocalName == SchemaParsing.SchemaParseContext.ATTR_UNIQUE_ID);
            if (uniqueId != null) {
                element.SetAttributeValue(uniqueId.Name, uniqueId.Value);
            }
        }

        // 子要素を再帰的に処理
        foreach (var child in element.Elements()) {
            SortElementAttributes(child);
        }
    }
}


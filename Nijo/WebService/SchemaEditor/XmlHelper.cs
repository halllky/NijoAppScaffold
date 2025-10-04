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
    /// XML要素とその子要素の属性を再帰的に名前順にソートする
    /// </summary>
    private static void SortElementAttributes(XElement element) {
        // 現在の要素の属性をソート
        var attributes = element.Attributes().ToList();
        if (attributes.Count > 1) {
            // 属性を名前順にソート
            var sortedAttributes = attributes.OrderBy(attr => attr.Name.LocalName).ToList();

            // 既存の属性をすべて削除
            element.RemoveAttributes();

            // ソート済みの属性を再追加
            foreach (var attr in sortedAttributes) {
                element.SetAttributeValue(attr.Name, attr.Value);
            }
        }

        // 子要素を再帰的に処理
        foreach (var child in element.Elements()) {
            SortElementAttributes(child);
        }
    }
}


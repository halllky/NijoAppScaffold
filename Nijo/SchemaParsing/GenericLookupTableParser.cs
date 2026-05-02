using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.ImmutableSchema;

namespace Nijo.SchemaParsing;

/// <summary>
/// 汎用参照テーブルのスキーマ定義を解釈するクラス。
/// スキーマ編集時とコード自動生成時の両方から利用される。
/// </summary>
internal class GenericLookupTableParser {

    /// <summary>
    /// nijo.xml のルート要素直下の要素のうち、汎用参照テーブルのカテゴリが格納されるXML要素の名前。
    /// </summary>
    internal const string SECTION_NAME = SchemaParseContext.SECTION_GENERIC_LOOKUP_TABLES;
    /// <summary>
    /// <see cref="SECTION_NAME"/> 直下の要素
    /// </summary>
    internal const string CATEGORIES = "Categories";
    /// <summary>
    /// <see cref="CATEGORIES"/> の属性。どのテーブルのカテゴリーかを指定。
    /// DataModel のルート集約の <see cref="SchemaParseContext.ATTR_UNIQUE_ID"/> で指定する。
    /// </summary>
    internal const string FOR = "For";
    /// <summary>
    /// カテゴリ内のハードコードキーを定義する子要素名。
    /// </summary>
    internal const string KEY = "Key";
    /// <summary>
    /// <see cref="KEY"/> 要素のハードコード値属性名。
    /// </summary>
    internal const string KEY_VALUE = "Value";

    internal GenericLookupTableParser(SchemaParseContext ctx) {
        _ctx = ctx;
    }

    private readonly SchemaParseContext _ctx;

    /// <summary>
    /// nijo.xml の <see cref="SECTION_NAME"/> セクションの内容が正しいか検査します。
    /// 結果はエラーメッセージとしてGUI側に構造体などで返します。詳細は <see cref="SchemaParseContext"/> のバリデーションの仕様に従います。
    /// </summary>
    internal void ValidateCategoriesSection(/* TODO */) {
        throw new NotImplementedException(); // TODO
    }

    /// <summary>
    /// 指定のデータモデルのルート集約に対応する <see cref="SECTION_NAME"/> セクションの内容を解釈して、汎用参照テーブルのカテゴリの一覧を返します。
    /// </summary>
    internal IEnumerable<GenericLookupTableCategory> GetCategoriesOf(RootAggregate rootAggregate) {
        var uniqueId = rootAggregate.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
        if (string.IsNullOrEmpty(uniqueId)) yield break;

        var section = _ctx.Document.Root?.Element(SECTION_NAME);
        if (section == null) yield break;

        foreach (var categoriesElement in section.Elements(CATEGORIES)) {
            var forAttr = categoriesElement.Attribute(FOR)?.Value;
            if (forAttr != uniqueId) continue;

            // このテーブルに対応する Categories 要素が見つかった
            foreach (var categoryElement in categoriesElement.Elements()) {
                var keys = new List<GenericLookupTableCategory.HardCodedKeyEntry>();
                foreach (var keyElement in categoryElement.Elements(KEY)) {
                    var keyFor = keyElement.Attribute(FOR)?.Value;
                    var keyValue = keyElement.Attribute(KEY_VALUE)?.Value;
                    if (!string.IsNullOrEmpty(keyFor) && keyValue != null) {
                        keys.Add(new GenericLookupTableCategory.HardCodedKeyEntry {
                            UniqueId = keyFor,
                            Value = keyValue,
                        });
                    }
                }

                yield return new GenericLookupTableCategory {
                    Name = categoryElement.Name.LocalName,
                    DisplayName = categoryElement.Attribute(BasicNodeOptions.DisplayName.AttributeName)?.Value
                        ?? categoryElement.Name.LocalName,
                    HardCodedKeys = keys,
                };
            }

            // 同じテーブルの Categories 要素は1つだけのはず
            yield break;
        }
    }

    /// <summary>
    /// <see cref="CATEGORIES"/> 直下の要素1個と対応
    /// </summary>
    internal class GenericLookupTableCategory {
        /// <summary>カテゴリ名（XML要素名）。例: "Countries"</summary>
        public required string Name { get; init; }
        /// <summary>表示用名称。例: "国・地域区分"</summary>
        public required string DisplayName { get; init; }
        /// <summary>ハードコードされるキーの定義一覧</summary>
        public required IReadOnlyList<HardCodedKeyEntry> HardCodedKeys { get; init; }

        /// <summary>
        /// ハードコードされる主キーの1項目
        /// </summary>
        internal class HardCodedKeyEntry {
            /// <summary>対応するValueMemberのUniqueId</summary>
            public required string UniqueId { get; init; }
            /// <summary>ハードコードされる値</summary>
            public required string Value { get; init; }
        }
    }
}

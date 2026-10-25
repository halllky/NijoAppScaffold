using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nijo.DataStructures.Aggregates;
using Nijo.SchemaParsing;

namespace Nijo.DataStructures.IsGenericLookupTable;

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
    /// 永続化されるデータ構造 のルート集約の <see cref="SchemaParseContext.ATTR_UNIQUE_ID"/> で指定する。
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

    private readonly SchemaParseContext _ctx;

    internal GenericLookupTableParser(SchemaParseContext ctx) {
        _ctx = ctx;
    }

    /// <summary>
    /// 指定の永続化されるデータ構造のルート集約に対応する <see cref="SECTION_NAME"/> セクションの内容を解釈して、汎用参照テーブルのカテゴリの一覧を返します。
    /// </summary>
    internal IEnumerable<GenericLookupTableCategory> GetCategoriesOf(RootAggregate rootAggregate) {
        var uniqueId = rootAggregate.XElement.Attribute(SchemaParseContext.ATTR_UNIQUE_ID)?.Value;
        if (string.IsNullOrEmpty(uniqueId)) return Array.Empty<GenericLookupTableCategory>();

        var categoriesByUniqueId = CategoriesCache.GetValue(_ctx, ParseCategories);
        return categoriesByUniqueId.GetValueOrDefault(uniqueId) ?? [];
    }

    /// <summary>
    /// <see cref="SECTION_NAME"/> セクションの解釈結果。
    /// 汎用参照テーブルのコード生成の中で何度も参照されるため、スキーマ定義（<see cref="SchemaParseContext"/>）ごとに1回だけ解釈する。
    /// </summary>
    private static readonly ConditionalWeakTable<SchemaParseContext, IReadOnlyDictionary<string, IReadOnlyList<GenericLookupTableCategory>>> CategoriesCache = new();

    private static IReadOnlyDictionary<string, IReadOnlyList<GenericLookupTableCategory>> ParseCategories(SchemaParseContext ctx) {
        var categoriesByUniqueId = new Dictionary<string, IReadOnlyList<GenericLookupTableCategory>>(StringComparer.Ordinal);
        var section = ctx.Document.Root?.Element(SECTION_NAME);
        if (section == null) return categoriesByUniqueId;

        foreach (var categoriesElement in section.Elements(CATEGORIES)) {
            var uniqueId = categoriesElement.Attribute(FOR)?.Value;
            if (string.IsNullOrEmpty(uniqueId)) continue;

            var categories = new List<GenericLookupTableCategory>();
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

                categories.Add(new GenericLookupTableCategory {
                    Name = categoryElement.Name.LocalName,
                    DisplayName = ctx.GetDisplayName(categoryElement),
                    HardCodedKeys = keys,
                });
            }

            categoriesByUniqueId.TryAdd(uniqueId, categories);
        }

        return categoriesByUniqueId;
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

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
    private const string FOR = "For";

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
        throw new NotImplementedException(); // TODO
    }

    /// <summary>
    /// <see cref="CATEGORIES"/> 直下の要素1個と対応
    /// </summary>
    internal class GenericLookupTableCategory {
        // TODO
    }
}

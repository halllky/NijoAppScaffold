using System.Collections.Generic;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Models;
using Nijo.SchemaParsing;

/// <summary>
/// 値メンバー型の詳細な外部仕様を説明するマークダウンドキュメントを生成する。
/// </summary>
internal class ValueObjectTypesMd {

    internal static string FILE_NAME = "ValueMemberTypes.md";

    internal static string Render(IEnumerable<IValueMemberType> valueMemberTypes) {
        return $$"""
            ---
            title: nijo.xml 仕様 - 属性種類定義
            outline: [2, 4]  # `##` , `###` の見出しをページ内ナビゲーションに表示
            ---

            # nijo.xml 仕様 - 属性種類定義

            このページでは nijo.xml の定義の仕様のうちメンバーの属性として使用できる種類について説明します。

            [[toc]]

            ## Child（`{{SchemaParseContext.NODE_TYPE_CHILD}}`）

            親オブジェクトと1対1の多重度を持つ子オブジェクトを定義します。

            ## Children（`{{SchemaParseContext.NODE_TYPE_CHILDREN}}`）

            親オブジェクトと1対多の多重度を持つ子オブジェクトを定義します。

            ## 外部参照（`{{SchemaParseContext.NODE_TYPE_REFTO}}:参照先集約名`）

            他の集約を参照することができます。
            参照先がルート集約でなく子孫集約の場合は `{{SchemaParseContext.NODE_TYPE_REFTO}}:親集約/子集約1/子集約2` のようにスラッシュ区切りで指定してください。

            {{valueMemberTypes.SelectTextTemplate(type => $$"""
            ## {{type.DisplayName}}（`{{type.SchemaTypeName}}`）

            {{type.RenderSpecificationMarkdown()}}

            - **`{{SchemaParseContext.ATTR_NODE_TYPE}}` 属性の値**: `{{type.SchemaTypeName}}`
            - **C#型名（ビジネスロジック中で参照するときの型）**: `{{type.CsDomainTypeName}}`
            - **C#型名（EFCoreやHTTPなど外界とやりとりするときの型）**: `{{type.CsPrimitiveTypeName}}`
            - **TypeScript型名**: `{{type.TsTypeName}}`

            """)}}

            ## 静的区分型（`当該種類名`）

            スキーマ定義で `{{EnumDefParser.SCHEMA_NAME}}` モデルとして定義された種類をメンバー型として使用できます。
            {{SchemaParseContext.ATTR_NODE_TYPE}} 属性には当該種類の物理名を指定してください。

            ## 値オブジェクト型（`当該値オブジェクト名`）

            スキーマ定義で `{{ValueObjectModel.SCHEMA_NAME}}` モデルとして定義された値オブジェクトをメンバー型として使用できます。
            {{SchemaParseContext.ATTR_NODE_TYPE}} 属性には当該値オブジェクトの物理名を指定してください。

            ## コメント（`{{SchemaParseContext.NODE_TYPE_MEMO}}`）

            この型の要素はソースコード自動生成処理上では無視されます。
            例えば以下の例では、コメントは無視され、親集約自身が通常の属性1～3を持つものとしてソース生成されます。

            ```xml
            <親集約 Type="{{new DataModel().SchemaName}}">
              <コメント Type="{{SchemaParseContext.NODE_TYPE_MEMO}}">
                <通常の属性1 Type="word" />
                <通常の属性2 Type="int" />
              </コメント>
             <通常の属性3 Type="date" />
            </親集約>
            ```
            """;
    }
}

using System.Collections.Generic;
using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;

/// <summary>
/// モデルの詳細な外部仕様を説明するマークダウンドキュメントを生成する。
/// </summary>
internal class ModelsMd {

    internal static string FILE_NAME = "Models.md";

    internal static string Render(IEnumerable<IModel> models, SchemaParseRule rule) {
        return $$"""
            ---
            title: nijo.xml 仕様 - モデル定義
            outline: [2, 3]  # `##` , `###` の見出しをページ内ナビゲーションに表示
            ---

            # nijo.xml 仕様 - モデル定義

            このページでは nijo.xml の定義の仕様のうち各モデル単位の仕様について説明します。

            [[toc]]

            ## スキーマ定義全体にわたる仕様

            {{SchemaParseContext.RenderValidationSpecificationMarkdown()}}

            {{models.SelectTextTemplate(model => $$"""
            ## {{model.GetType().Name}}

            ### 制約事項

            {{model.RenderModelValidateSpecificationMarkdown()}}

            ### `{{SchemaParseContext.ATTR_NODE_TYPE}}` 属性の仕様

            ルート集約の {{SchemaParseContext.ATTR_NODE_TYPE}} には `{{model.SchemaName}}` を指定してください。

            {{model.RenderTypeAttributeSpecificationMarkdown()}}

            ### その他オプション

            {{rule.GetAvailableOptionsFor(model).SelectTextTemplate(opt => $$"""
            #### {{opt.AttributeName}}

            {{opt.DisplayName}}。

            {{opt.HelpText}}

            """)}}

            """)}}
            """;
    }
}
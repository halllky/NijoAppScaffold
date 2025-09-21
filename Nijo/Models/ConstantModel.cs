using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using Nijo.Models.ConstantModelModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models {
    /// <summary>
    /// 定数モデル。
    /// C#とJavaScript/TypeScriptで常に値が同期される定数のソースコードを生成する。
    /// string, int, decimal, テンプレート文字列（引数を受け取って文字列を返す関数）の4種類の定数を定義できる。
    /// Type="child"によってネストされた定数も定義可能。
    /// </summary>
    internal class ConstantModel : IModel {
        internal const string SCHEMA_NAME = "constant-model";

        public string SchemaName => SCHEMA_NAME;

        public string RenderModelValidateSpecificationMarkdown() {
            return $$"""
                #### 定数の種類

                定数モデルでは以下の4つの型の定数を定義できます：

                - **string**: 文字列定数
                - **int**: 整数定数
                - **decimal**: 小数定数
                - **template**: テンプレート文字列（引数を受け取って文字列を返す関数）

                #### 定数値の定義

                各定数要素には以下の属性を指定できます：

                - `ConstantType`: 定数の型（string, int, decimal, template）
                - `ConstantValue`: 定数の値

                #### テンプレート文字列

                `template` 型を使用すると、引数を受け取って文字列を返す関数が生成されます。
                使用できる変数の数は、ConstantValue中に含まれる `{0}` , `{1}` , ... から自動的に判定されます。
                プレースホルダーが0個でも構いません。

                #### ネストされた定数

                `{{SchemaParseContext.NODE_TYPE_CHILD}}` を使用してネストされた定数を定義できます。
                これにより階層構造を持つ定数グループを作成できます。

                #### 制約事項

                - すべての定数要素には `ConstantType` 属性が必須です（未指定の場合はstringとして扱われます）
                - 定数名は有効な識別子である必要があります
                - XMLコメント（<!-- コメント -->）は生成されるコードにも反映されます
                """;
        }

        public string RenderTypeAttributeSpecificationMarkdown() {
            return $$"""
                - ネストされた定数グループには `{{SchemaParseContext.NODE_TYPE_CHILD}}` を指定してください。
                - 定数要素自体に type 属性を指定することはできません（定数の型は `ConstantType` 属性で指定します）。
                """;
        }

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            ValidateConstants(rootAggregateElement, addError);
        }

        private void ValidateConstants(XElement element, Action<XElement, string> addError) {
            foreach (var constantElement in element.ElementsWithoutMemo()) {
                var constantType = constantElement.Attribute(BasicNodeOptions.ConstantType.AttributeName)?.Value;

                if (constantType == "child") {
                    // 子要素（ネストされた定数グループ）の場合は再帰的にバリデーション
                    ValidateConstants(constantElement, addError);
                } else {
                    // 定数要素のバリデーション
                    ValidateConstantElement(constantElement, addError);
                }
            }
        }

        private void ValidateConstantElement(XElement constantElement, Action<XElement, string> addError) {
            // 定数名の妥当性チェック（有効な識別子かどうか）
            var constantName = constantElement.Name.LocalName;
            if (!IsValidIdentifier(constantName)) {
                addError(constantElement, $"定数名「{constantName}」は有効な識別子ではありません。");
            }
        }

        private bool IsValidIdentifier(string name) {
            if (string.IsNullOrEmpty(name)) return false;
            if (!char.IsLetter(name[0]) && name[0] != '_') return false;
            return name.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var parser = new ConstantDefParser(((ISchemaPathNode)rootAggregate).XElement, ctx.SchemaParser);

            // データ型: 定数クラス
            var constantDef = new ConstantDef(parser, rootAggregate);

            // C#定数クラスの生成
            ctx.CoreLibrary(dir => {
                dir.Generate(new SourceFile {
                    FileName = $"{constantDef.CsClassName}.cs",
                    Contents = constantDef.RenderCSharp(ctx)
                });
            });

            // JavaScript/TypeScript定数の生成
            ctx.ReactProject(dir => {
                dir.Generate(constantDef.RenderTypeScript(ctx));
            });
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }
    }
}

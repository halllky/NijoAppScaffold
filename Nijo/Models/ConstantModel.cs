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

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            ValidateConstants(rootAggregateElement, addError);

            void ValidateConstants(XElement element, Action<XElement, string> addError) {
                foreach (var constantElement in element.Elements()) {
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
            var constantDef = new ConstantDef(parser);

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

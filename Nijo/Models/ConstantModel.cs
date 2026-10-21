using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.Parts.JavaScript;
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
    internal static class ConstantModel {

        /// <summary>
        /// ルート集約1個と対応するソースコードを生成します。
        /// </summary>
        internal static void Render(CodeRenderingContext ctx, RootAggregate rootAggregate) {
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
                dir.Directory(TypeScriptAggregateModule.DIRECTORY, modelsDir => {
                    modelsDir.Generate(constantDef.RenderTypeScript(ctx));
                });
            });
        }

    }
}

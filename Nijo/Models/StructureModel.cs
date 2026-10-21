using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Models.StructureModelModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.Parts.Common;
using Nijo.Parts.JavaScript;

namespace Nijo.Models {
    /// <summary>
    /// 単純な構造体モデル。
    /// サーバー(C#)とクライアント(TypeScript)の双方で共有するための入れ子可能なオブジェクトの定義を生成する。
    /// IsStored 属性も IsQueryable 属性も指定されていないデータ構造から生成される。
    /// </summary>
    internal static class StructureModel {

        /// <summary>
        /// ルート集約1個と対応するソースコードを生成します。
        /// </summary>
        internal static void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
            // C# クラス定義（ルートと子孫）
            aggregateFile.AddCSharpClass(PlainStructure.RenderCSharpRecursively(rootAggregate, ctx), "Class_Structure");

            // TypeScript 型定義（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTypeScriptRecursively(rootAggregate, ctx));

            // TypeScript 新規オブジェクト作成関数（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTsNewObjectFunctionRecursively(rootAggregate, ctx));

            // この構造体がいずれかのコマンドモデルの引数or戻り値として参照されている場合、
            // プレゼンテーション層での編集用のオブジェクト等を生成する。
            var refferdAsParameter = rootAggregate.EnumerateCommandModelsRefferingAsParameter().Any();
            var refferdAsReturnValue = rootAggregate.EnumerateCommandModelsRefferingAsReturnValue().Any();
            StructureDisplayData? displayData = null;
            if (refferdAsParameter || refferdAsReturnValue) {
                // 画面表示用データ
                displayData = new StructureDisplayData(rootAggregate);
                aggregateFile.AddCSharpClass(StructureDisplayData.RenderCSharpRecursively(rootAggregate, ctx), "Class_DisplayData");
                aggregateFile.AddTypeScriptTypeDef(StructureDisplayData.RenderTypeScriptRecursively(rootAggregate, ctx));
                aggregateFile.AddTypeScriptFunction(EditablePresentationObject.RenderTsNewObjectFunctionRecursively(displayData, ctx));
                aggregateFile.AddTypeScriptFunction(new DeepEqualFunction(displayData).Render(ctx));
            }
            if (refferdAsParameter) {
                // メッセージコンテナ
                var messageContainer = new StructureDisplayDataMessageContainer(rootAggregate);
                aggregateFile.AddCSharpClass(StructureDisplayDataMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_DisplayDataMessage");
                ctx.Use<Parts.CSharp.MessageContainer.BaseClass>().Register(messageContainer.CsClassName, messageContainer.CsClassName);
            }

            // TypeScriptのマッピングファイルへの登録
            ctx.Use<CommandQueryMappings>().AddStructureModel(rootAggregate);

            // 定数: メタデータ
            aggregateFile.AddTypeScriptFunction(AggregateMetadata.RenderTsConstant(rootAggregate, displayData, null, ctx));
        }

        /// <summary>
        /// すべての構造体で共通して使われる、ルート集約1個と対応しないソースコードを生成します。
        /// </summary>
        internal static void RenderCommonModules(CodeRenderingContext ctx) {
            ctx.Use<DeepEqualFunction.OptionType>();
        }
    }
}

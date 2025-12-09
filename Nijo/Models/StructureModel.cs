using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.SchemaParsing;
using Nijo.Models.StructureModelModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Nijo.Parts.Common;

namespace Nijo.Models {
    /// <summary>
    /// 単純な構造体モデル。
    /// サーバー(C#)とクライアント(TypeScript)の双方で共有するための入れ子可能なオブジェクトの定義を生成する。
    /// </summary>
    internal class StructureModel : IModel {
        internal const string SCHEMA_NAME = "structure-model";
        public string SchemaName => SCHEMA_NAME;

        public string RenderModelValidateSpecificationMarkdown() {
            return $$"""
                #### 外部参照 `{{SchemaParseContext.NODE_TYPE_REFTO}}` について

                - StructureModel から参照できるのは、クエリモデル、`{{BasicNodeOptions.GenerateDefaultQueryModel.AttributeName}}`属性が付与されたデータモデル、または他のStructureModelの集約のみです。
                - SearchCondition を参照する場合、ルート集約の検索条件オブジェクトのみ参照可能です。
                - 自身のツリーの集約を参照することはできません。
                - クエリモデルを参照する場合、`{{BasicNodeOptions.RefToObject.AttributeName}}`属性の指定が必須です。

                #### その他制約事項

                - 主キー属性（`{{BasicNodeOptions.IsKey.AttributeName}}`）には特別な意味はありません。
                """;
        }

        public string RenderTypeAttributeSpecificationMarkdown() {
            return $$"""
                - 入れ子になった子集約 `{{SchemaParseContext.NODE_TYPE_CHILD}}` を定義できます。
                - 子配列 `{{SchemaParseContext.NODE_TYPE_CHILDREN}}` を定義できます。
                - その他メンバーに定義できる属性については [属性種類定義](./{{ValueObjectTypesMd.FILE_NAME}}) を参照してください。
                """;
        }

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // 外部参照のチェック
            ValidateRefTo(rootAggregateElement, context, addError);
        }

        /// <summary>
        /// StructureModelの外部参照をチェックします
        /// </summary>
        private void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // 自身を起点とするすべての外部参照を取得
            var refElements = rootAggregateElement
                .Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
                .ToList();

            if (!refElements.Any()) return;

            foreach (var refElement in refElements) {
                var refTo = context.FindRefTo(refElement);
                if (refTo == null) {
                    addError(refElement, $"参照先の要素が見つかりません: {refElement.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value}");
                    continue;
                }

                // 自身のツリーの集約を参照していないかチェック
                var rootElement = refElement.GetRootAggregateElement();
                var refToRoot = refTo.GetRootAggregateElement();

                if (rootElement == refToRoot) {
                    addError(refElement, "自身のツリーの集約を参照することはできません。");
                    continue;
                }

                // 参照先がクエリモデル、GDQMデータモデル、またはStructureModelか確認
                var refToType = refToRoot.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;
                var isQueryModel = refToType == QueryModel.NODE_TYPE;
                var isGDQM = refToRoot.HasGenerateDefaultQueryModelAttribute();
                var isStructureModel = refToType == SCHEMA_NAME;

                if (!isQueryModel && !isGDQM && !isStructureModel) {
                    addError(refElement, $"StructureModelの集約からは、クエリモデル、{BasicNodeOptions.GenerateDefaultQueryModel.AttributeName}属性が付与されたデータモデル、またはStructureModelの集約しか参照できません。");
                    continue;
                }

                // クエリモデルまたはGDQMデータモデルを参照する場合はRefToObjectの指定が必須
                var refToObject = refElement.Attribute(BasicNodeOptions.RefToObject.AttributeName);
                if ((isQueryModel || isGDQM) && refToObject == null) {
                    addError(refElement, $"StructureModelからクエリモデルを外部参照する場合、{BasicNodeOptions.RefToObject.AttributeName}属性を指定する必要があります。");
                }

                // RefToObjectの値が不正かチェック
                if ((isQueryModel || isGDQM)
                        && refToObject != null
                        && !BasicNodeOptions.StructureRefToAvailable.ContainsKey(refToObject.Value)) {
                    addError(refElement, $"{BasicNodeOptions.RefToObject.AttributeName}属性の値は {string.Join(" または ", BasicNodeOptions.StructureRefToAvailable.Keys)} のいずれかを指定してください。");
                }

                // 検索条件を参照する場合はルート集約のみ参照可能
                if (refToObject?.Value == BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION
                        && refTo != refToRoot) {
                    addError(refElement, $"検索条件を参照する場合はルート集約のみ指定可能です。");
                }

                // StructureModelを参照する場合はRefToObjectの指定は不要（指定されていても無視）
            }
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var aggregateFile = new SourceFileByAggregate(rootAggregate);

            // C# クラス定義（ルートと子孫）
            aggregateFile.AddCSharpClass(PlainStructure.RenderCSharpRecursively(rootAggregate, ctx), "Class_Structure");

            // TypeScript 型定義（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTypeScriptRecursively(rootAggregate, ctx));

            // TypeScript 新規オブジェクト作成関数（ルートと子孫）
            aggregateFile.AddTypeScriptTypeDef(PlainStructure.RenderTsNewObjectFunctionRecursively(rootAggregate, ctx));

            // この構造体がいずれかのコマンドモデルの引数として参照されている場合、
            // プレゼンテーション層での編集用のオブジェクト等を生成する。
            if (rootAggregate.EnumerateCommandModelsRefferingAsParameter().Any()) {
                // 画面表示用データ
                var displayData = new StructureDisplayData(rootAggregate);
                aggregateFile.AddCSharpClass(StructureDisplayData.RenderCSharpRecursively(rootAggregate, ctx), "Class_DisplayData");
                aggregateFile.AddTypeScriptTypeDef(StructureDisplayData.RenderTypeScriptRecursively(rootAggregate, ctx));
                aggregateFile.AddTypeScriptTypeDef(displayData.RenderUiConstraintType(ctx));
                aggregateFile.AddTypeScriptTypeDef(displayData.RenderUiConstraintValue(ctx));
                aggregateFile.AddTypeScriptFunction(EditablePresentationObject.RenderTsNewObjectFunctionRecursively(displayData, ctx));

                // メッセージコンテナ
                var messageContainer = new StructureDisplayDataMessageContainer(rootAggregate);
                aggregateFile.AddCSharpClass(StructureDisplayDataMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_DisplayDataMessage");
                ctx.Use<Parts.CSharp.MessageContainer.BaseClass>().Register(messageContainer.CsClassName, messageContainer.CsClassName);
            }

            // TypeScriptのマッピングファイルへの登録
            ctx.Use<CommandQueryMappings>().AddStructureModel(rootAggregate);

            // 定数: メタデータ
            ctx.Use<Metadata>().Add(rootAggregate);
            ctx.Use<MetadataForPage>().Add(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }
    }
}

using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using Nijo.Models.CommandModelModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nijo.Models {
    internal class CommandModel : IModel {
        public string SchemaName => "command-model";

        public string RenderModelValidateSpecificationMarkdown() {
            return $$"""
                #### 引数、戻り値

                `{{BasicNodeOptions.Parameter.AttributeName}}` 属性と `{{BasicNodeOptions.ReturnValue.AttributeName}}` 属性で引数と戻り値の型を指定します。
                以下のいずれかを指定できます：

                * 何も指定しない（引数なし・戻り値なし）
                * 構造体モデルのルート集約名
                * クエリモデルのルート集約名:{{BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA}}
                * クエリモデルのルート集約名:{{BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION}}

                #### その他制約事項

                - コマンドモデルのルート集約は子孫XML要素を定義できません
                - コマンドモデルの集約には主キー属性を定義できません
                """;
        }

        public string RenderTypeAttributeSpecificationMarkdown() {
            return $$"""
                - 入れ子になった子集約 {{SchemaParseContext.NODE_TYPE_CHILD}} を定義できます。
                - 子配列 {{SchemaParseContext.NODE_TYPE_CHILDREN}} を定義できます。
                - その他メンバーに定義できる属性については [属性種類定義](./{{ValueObjectTypesMd.FILE_NAME}}) を参照してください。
                """;
        }

        public void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // コマンドモデルの物理名を取得
            var rootAggregateName = context.GetPhysicalName(rootAggregateElement);
            if (string.IsNullOrEmpty(rootAggregateName)) {
                addError(rootAggregateElement, "コマンドモデルの物理名が指定されていません。");
                return;
            }

            // 新仕様：コマンドモデルは子孫XML要素を定義できない
            var childElements = rootAggregateElement.ElementsWithoutMemo();
            if (childElements.Any()) {
                addError(rootAggregateElement, $"コマンドモデルのルート集約は子孫XML要素を定義できません。引数と戻り値の型は{BasicNodeOptions.Parameter.AttributeName}属性と{BasicNodeOptions.ReturnValue.AttributeName}属性で指定してください。");
            }

            // Parameter属性の検証
            var parameterAttr = rootAggregateElement.Attribute(BasicNodeOptions.Parameter.AttributeName)?.Value;
            if (!string.IsNullOrEmpty(parameterAttr)) {
                ValidateTypeSpecification(parameterAttr, "Parameter", rootAggregateElement, context, addError);
            }

            // ReturnValue属性の検証
            var returnValueAttr = rootAggregateElement.Attribute(BasicNodeOptions.ReturnValue.AttributeName)?.Value;
            if (!string.IsNullOrEmpty(returnValueAttr)) {
                ValidateTypeSpecification(returnValueAttr, "ReturnValue", rootAggregateElement, context, addError);
            }

            // コマンドモデルの集約には主キー属性を定義できない
            ValidateNoKeyAttributes(rootAggregateElement, context, addError);
        }

        /// <summary>
        /// コマンドモデルでは主キー属性を定義できないことをチェックします
        /// </summary>
        private void ValidateNoKeyAttributes(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            // すべての集約（ルート集約、子集約、子配列）をチェック
            var allAggregates = rootAggregateElement.DescendantsAndSelf()
                .Where(el => el == rootAggregateElement ||
                            el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD ||
                            el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);

            foreach (var aggregate in allAggregates) {
                var membersWithKey = aggregate.ElementsWithoutMemo()
                    .Where(member => member.Attribute(BasicNodeOptions.IsKey.AttributeName) != null).ToList();

                if (membersWithKey.Any()) {
                    addError(aggregate, "コマンドモデルの集約には主キー属性を定義できません。");
                    foreach (var member in membersWithKey) {
                        addError(member, "コマンドモデルのメンバーに主キー属性を定義することはできません。");
                    }
                }
            }
        }

        /// <summary>
        /// Parameter属性またはReturnValue属性の型指定を検証します
        /// </summary>
        private void ValidateTypeSpecification(string typeSpec, string attributeName, XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
            try {
                var (modelName, refToObject) = CommandModelExtensions.ParseTypeSpecification(typeSpec);

                if (string.IsNullOrEmpty(modelName)) {
                    addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {typeSpec}");
                    return;
                }

                // 参照先モデルの存在確認
                var targetModel = context.Document.Root?.ElementsWithoutMemo()
                    .FirstOrDefault(e => context.GetPhysicalName(e) == modelName);
                if (targetModel == null) {
                    addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」が見つからないか、ルート集約ではありません。");
                    return;
                }

                // 参照先モデルの種類チェック
                var targetModelType = targetModel.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value;

                if (string.IsNullOrEmpty(refToObject)) {
                    // 構造体モデルの場合
                    if (targetModelType != StructureModel.SCHEMA_NAME) {
                        addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」は構造体モデルではありません。");
                    }
                } else {
                    // クエリモデルの場合
                    var isQueryModel = targetModelType == QueryModel.NODE_TYPE;
                    var isGDQM = context.HasGenerateDefaultQueryModelAttribute(targetModel);

                    if (!isQueryModel && !isGDQM) {
                        addError(rootAggregateElement, $"{attributeName}属性で指定されたモデル「{modelName}」はクエリモデルまたは{BasicNodeOptions.GenerateDefaultQueryModel.AttributeName}属性が付与されたデータモデルではありません。");
                        return;
                    }

                    // RefToObjectの値チェック
                    if (refToObject != BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA && refToObject != BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION) {
                        addError(rootAggregateElement, $"{attributeName}属性のRefToObject指定「{refToObject}」は無効です。「{BasicNodeOptions.REF_TO_OBJECT_DISPLAY_DATA}」または「{BasicNodeOptions.REF_TO_OBJECT_SEARCH_CONDITION}」を指定してください。");
                    }
                }
            } catch (ArgumentException ex) {
                addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {ex.Message}");
            }
        }

        public void GenerateCode(CodeRenderingContext ctx, RootAggregate rootAggregate) {
            var aggregateFile = new SourceFileByAggregate(rootAggregate);

            // 処理: TypeScript用マッピング、Webエンドポイント、本処理抽象メソッド
            var commandProcessing = new CommandProcessing(rootAggregate);
            aggregateFile.AddWebapiControllerAction(commandProcessing.RenderAspNetCoreControllerAction(ctx));
            aggregateFile.AddAppSrvMethod(commandProcessing.RenderAppSrvMethods(ctx), "コマンド処理");

            // カスタムロジック用モジュール
            ctx.Use<CommandQueryMappings>().AddCommandModel(rootAggregate);

            // 定数: メタデータ（新仕様ではルート集約のみ）
            ctx.Use<MetadataForPage>().Add(rootAggregate);

            aggregateFile.ExecuteRendering(ctx);
        }

        public void GenerateCode(CodeRenderingContext ctx) {
            // 特になし
        }
    }


    internal static class CommandModelExtensions {

        internal static (string? ModelName, string? RefToObject) ParseTypeSpecification(string? typeSpec) {
            if (string.IsNullOrEmpty(typeSpec)) {
                return (null, null);
            }

            var parts = typeSpec.Split(':');
            if (parts.Length == 1) {
                // 構造体モデルの場合
                return (parts[0], null);
            } else if (parts.Length == 2) {
                // クエリモデルの場合
                return (parts[0], parts[1]);
            }

            throw new ArgumentException($"Invalid type specification: {typeSpec}");
        }
    }
}

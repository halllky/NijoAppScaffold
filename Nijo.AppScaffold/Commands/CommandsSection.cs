using Nijo.CodeGenerating;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Members;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Nijo.Commands;

/// <summary>
/// Commands セクション。
/// コマンドは引数を受け取り戻り値を返す処理で、Webサーバー・クライアント間で常に同期された型定義を提供する。
/// </summary>
internal sealed class CommandsSection : Section {
    public override E_Section Kind => E_Section.Commands;

    public override void Validate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // コマンドの物理名を取得
        var rootAggregateName = context.GetPhysicalName(rootAggregateElement);
        if (string.IsNullOrEmpty(rootAggregateName)) {
            addError(rootAggregateElement, "コマンドの物理名が指定されていません。");
            return;
        }

        // コマンドは子孫XML要素を定義できない
        var childElements = rootAggregateElement.Elements();
        if (childElements.Any()) {
            addError(rootAggregateElement, $"コマンドのルート集約は子孫XML要素を定義できません。引数と戻り値の型は{ParameterOption.ATTRIBUTE_NAME}属性と{ReturnValueOption.ATTRIBUTE_NAME}属性で指定してください。");
        }

        // Parameter属性の検証
        var parameterAttr = rootAggregateElement.Attribute(ParameterOption.ATTRIBUTE_NAME)?.Value;
        if (!string.IsNullOrEmpty(parameterAttr)) {
            ValidateTypeSpecification(parameterAttr, "Parameter", rootAggregateElement, context, addError);
        }

        // ReturnValue属性の検証
        var returnValueAttr = rootAggregateElement.Attribute(ReturnValueOption.ATTRIBUTE_NAME)?.Value;
        if (!string.IsNullOrEmpty(returnValueAttr)) {
            ValidateTypeSpecification(returnValueAttr, "ReturnValue", rootAggregateElement, context, addError);
        }

        // コマンドの集約には主キー属性を定義できない
        ValidateNoKeyAttributes(rootAggregateElement, context, addError);
    }

    public override void Render(CodeRenderingContext ctx) {
        RenderEach(ctx, ctx.Schema.GetCommands(), rootAggregate => RenderRootAggregate(ctx, rootAggregate));
    }

    private static void RenderRootAggregate(CodeRenderingContext ctx, RootAggregate rootAggregate) {
        var aggregateFile = new SourceFileByAggregate(rootAggregate);

        // 処理: TypeScript用マッピング、Webエンドポイント、本処理抽象メソッド
        var commandProcessing = new CommandProcessing(rootAggregate);
        aggregateFile.AddWebapiControllerAction(commandProcessing.RenderAspNetCoreControllerAction(ctx));
        aggregateFile.AddAppSrvMethod(commandProcessing.RenderAppSrvMethods(ctx), "コマンド処理");

        // カスタムロジック用モジュール
        ctx.Use<CommandQueryMappings>().AddCommandModel(rootAggregate);

        aggregateFile.ExecuteRendering(ctx);
    }

    /// <summary>
    /// コマンドでは主キー属性を定義できないことをチェックします
    /// </summary>
    private static void ValidateNoKeyAttributes(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // すべての集約（ルート集約、子集約、子配列）をチェック
        var allAggregates = rootAggregateElement.DescendantsAndSelf()
            .Where(el => el == rootAggregateElement ||
                        el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD ||
                        el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);

        foreach (var aggregate in allAggregates) {
            var membersWithKey = aggregate.Elements()
                .Where(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null).ToList();

            if (membersWithKey.Any()) {
                addError(aggregate, "コマンドの集約には主キー属性を定義できません。");
                foreach (var member in membersWithKey) {
                    addError(member, "コマンドのメンバーに主キー属性を定義することはできません。");
                }
            }
        }
    }

    /// <summary>
    /// Parameter属性またはReturnValue属性の型指定を検証します
    /// </summary>
    private static void ValidateTypeSpecification(string typeSpec, string attributeName, XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        try {
            var (modelName, refToObject) = ParseTypeSpecification(typeSpec);

            if (string.IsNullOrEmpty(modelName)) {
                addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {typeSpec}");
                return;
            }

            // 参照先のデータ構造の存在確認
            var targetModel = context.Document.Root
                ?.Element(SchemaParseContext.SECTION_DATA_STRUCTURES)
                ?.Elements()
                .FirstOrDefault(e => context.GetPhysicalName(e) == modelName);
            if (targetModel == null) {
                addError(rootAggregateElement, $"{attributeName}属性で指定されたデータ構造「{modelName}」が見つからないか、ルート集約ではありません。");
                return;
            }

            // 参照先のデータ構造の種類チェック
            var isStored = targetModel.IsStored();
            var isQueryable = targetModel.IsQueryable();

            if (string.IsNullOrEmpty(refToObject)) {
                // 構造体の場合
                if (isStored || isQueryable) {
                    addError(rootAggregateElement, $"{attributeName}属性で指定されたデータ構造「{modelName}」は{IsStoredOption.ATTRIBUTE_NAME}属性も{IsQueryableOption.ATTRIBUTE_NAME}属性も指定されていないデータ構造ではありません。");
                }
            } else {
                // 検索可能なデータ構造の場合
                if (!isQueryable) {
                    addError(rootAggregateElement, $"{attributeName}属性で指定されたデータ構造「{modelName}」は{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造ではありません。");
                    return;
                }

                // RefToObjectの値チェック
                if (!CommandTypeSpecificationOption.AvailableFromCommandToQuery.ContainsKey(refToObject)) {
                    addError(rootAggregateElement, $"{attributeName}属性のRefToObject指定「{refToObject}」は無効です。{string.Join(" または ", CommandTypeSpecificationOption.AvailableFromCommandToQuery.Keys)}のいずれかを指定してください。");
                }
            }
        } catch (ArgumentException ex) {
            addError(rootAggregateElement, $"{attributeName}属性の値が不正です: {ex.Message}");
        }
    }

    private static (string? ModelName, string? RefToObject) ParseTypeSpecification(string? typeSpec) {
        if (string.IsNullOrEmpty(typeSpec)) {
            return (null, null);
        }

        var parts = typeSpec.Split(':');
        if (parts.Length == 1) {
            // 構造体の場合
            return (parts[0], null);
        } else if (parts.Length == 2) {
            // 検索可能なデータ構造の場合
            return (parts[0], parts[1]);
        }

        throw new ArgumentException($"Invalid type specification: {typeSpec}");
    }
}

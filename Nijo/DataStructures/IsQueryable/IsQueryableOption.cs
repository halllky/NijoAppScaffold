using Nijo.CodeGenerating;
using Nijo.DataStructures.IsStored;
using Nijo.DataStructures.Members;
using Nijo.ImmutableSchema;
using Nijo.Parts.CSharp;
using Nijo.Parts.Common;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Nijo.DataStructures.IsQueryable;

/// <summary>
/// データ構造が画面や帳票から検索・照会されることを表す属性
/// </summary>
internal sealed class IsQueryableOption : FeatureOption {
    internal const string ATTRIBUTE_NAME = "IsQueryable";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "検索可能";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        このデータ構造が画面や帳票から検索・照会されることを表します。
        検索条件、検索処理、画面表示用データなどが生成されます。
        {{IsStoredOption.ATTRIBUTE_NAME}} が指定されていない場合、各項目をどのテーブルから取得するかを定義する処理は手動で実装する必要があります。
        {{IsStoredOption.ATTRIBUTE_NAME}} と同時に指定した場合の挙動は {{IsStoredOption.ATTRIBUTE_NAME}} の説明を参照してください。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.DataStructures && target.NodeType == E_NodeType.RootAggregate;
    }

    public override void ValidateRootAggregate(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // IsStored 属性と同時に指定された場合、データ構造の形はデータベース上のテーブルと同じになるため、IsStored 属性の側の検証に従う
        if (rootAggregateElement.IsStored()) return;

        // キーと ref-to のチェック
        var isView = rootAggregateElement.Attribute(MapToViewOption.ATTRIBUTE_NAME) != null;

        if (isView) {
            // ビューの場合でもChildrenがある場合はキーが必要
            var hasChildOrChildren = rootAggregateElement
                .Descendants()
                .Any(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD
                        || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);

            if (hasChildOrChildren) {
                // ルートとChildrenはキー必須
                var rootAndChildren = rootAggregateElement
                    .DescendantsAndSelf()
                    .Where(el => el.Parent?.Parent == el.Document?.Root
                              || el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);
                foreach (var el in rootAndChildren) {
                    var hasKey = el.Elements().Any(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null);

                    if (!hasKey) {
                        addError(el, "Child/Childrenがあるビューにマッピングされるデータ構造にはキーが必要です。");
                    }
                }
            }

            // キーなしのビューからのref-toを禁止
            var hasAnyKey = rootAggregateElement
                .DescendantsAndSelf()
                .Any(el => el.Elements().Any(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null));

            if (!hasAnyKey) {
                // 自身と子孫のすべてにキーがない場合、ref-toをチェック
                var refToMembers = rootAggregateElement
                    .Descendants()
                    .Where(member => member.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
                    .ToList();

                foreach (var refToMember in refToMembers) {
                    addError(refToMember, "キーが定義されていないビューからは他の集約を参照(ref-to)できません。EF Coreの制約により、キーレスエンティティからのナビゲーションプロパティはサポートされていません。");
                }
            }
        } else {
            // MapToViewが指定されていない通常のQueryModelの場合は子集約・子配列への主キー属性を禁止
            // 子集約には主キー属性を付与できない
            var childAggregates = rootAggregateElement.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILD);
            foreach (var childAggregate in childAggregates) {
                var membersWithKey = childAggregate.Elements()
                    .Where(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null).ToList();
                if (membersWithKey.Any()) {
                    addError(childAggregate, $"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造の子集約には主キー属性を付与することができません。");
                    foreach (var member in membersWithKey) {
                        addError(member, "この子集約のメンバーに主キー属性を付与することはできません。");
                    }
                }
            }

            // 子配列には主キー属性を付与できない
            var childrenAggregates = rootAggregateElement.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value == SchemaParseContext.NODE_TYPE_CHILDREN);
            foreach (var childrenAggregate in childrenAggregates) {
                var membersWithKey = childrenAggregate.Elements()
                    .Where(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null).ToList();
                if (membersWithKey.Any()) {
                    addError(childrenAggregate, $"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造の子配列には主キー属性を付与することができません。");
                    foreach (var member in membersWithKey) {
                        addError(member, "この子配列のメンバーに主キー属性を付与することはできません。");
                    }
                }
            }
        }

        // 外部参照のチェック
        ValidateRefTo(rootAggregateElement, context, addError);
    }

    public override void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
        // データ型: 検索条件クラス
        // - CS
        // - TS
        //   - export type 検索条件型
        //   - export type ソート可能メンバー型
        // - TS側オブジェクト作成関数
        var searchCondition = new SearchCondition.Entry(rootAggregate);
        aggregateFile.AddCSharpClass(SearchCondition.Entry.RenderCSharpRecursively(rootAggregate, ctx), "Class_SearchCondition");
        aggregateFile.AddTypeScriptTypeDef(SearchCondition.Entry.RenderTypeScriptRecursively(rootAggregate, ctx));
        aggregateFile.AddTypeScriptTypeDef(searchCondition.RenderTypeScriptSortableMemberType());
        aggregateFile.AddTypeScriptFunction(searchCondition.RenderNewObjectFunction());

        // データ型: 検索条件メッセージ
        var searchConditionMessages = new SearchConditionMessageContainer(rootAggregate);
        aggregateFile.AddCSharpClass(SearchConditionMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_SearchConditionMessage");
        ctx.Use<MessageContainer.BaseClass>().Register(searchConditionMessages.CsClassName, searchConditionMessages.CsClassName);

        // データ型: 検索結果クラス
        // ※ ビューにマッピングされる場合はSearchResultのEFCoreエンティティも生成する
        var searchResult = new SearchResult(rootAggregate);
        aggregateFile.AddCSharpClass(searchResult.RenderTree(ctx), "Class_SearchResult");
        if (rootAggregate.IsView) {
            ctx.Use<DbContextClass>().AddEntities(searchResult.EnumerateThisAndChildren());
        }

        // データ型: 画面表示用型 DisplayData
        // - 定義(CS, TS): 値 + 状態(existsInDB, willBeChanged, willBeDeleted)
        // - ディープイコール関数
        // - UIの制約定義オブジェクト（文字種、maxlength, 桁, required）
        // - TS側オブジェクト作成関数
        // - 変換処理: SearchResult => DisplayData
        // - 主キーの抽出・設定関数（URLなどのために使用）
        var displayData = new DisplayData(rootAggregate);
        aggregateFile.AddCSharpClass(DisplayData.RenderCSharpRecursively(rootAggregate, ctx), "Class_DisplayData");
        aggregateFile.AddTypeScriptTypeDef(DisplayData.RenderTypeScriptRecursively(rootAggregate, ctx));
        aggregateFile.AddTypeScriptFunction(EditablePresentationObject.RenderTsNewObjectFunctionRecursively(displayData, ctx));
        aggregateFile.AddTypeScriptFunction(new DeepEqualFunction(displayData).Render(ctx));

        // データ型: 画面表示用型メッセージ
        var displayDataMessages = new DisplayDataMessageContainer(rootAggregate);
        aggregateFile.AddCSharpClass(DisplayDataMessageContainer.RenderCSharpRecursively(rootAggregate), "Class_DisplayDataMessage");
        ctx.Use<MessageContainer.BaseClass>().Register(displayDataMessages.CsClassName, displayDataMessages.CsClassName);

        // 検索処理
        // - reactは型名マッピングのみ
        // - ASP.NET Core Controller Action
        // - AppSrv
        //   - CreateQuerySource
        //   - AppendWhereClause
        //   - Sort
        //   - Paging
        // - 以上がload, count それぞれ
        var searchProcessing = new SearchProcessing(rootAggregate);
        aggregateFile.AddWebapiControllerAction(searchProcessing.RenderAspNetCoreControllerAction(ctx));
        aggregateFile.AddAppSrvMethod(searchProcessing.RenderAppSrvMethods(ctx), "検索処理");

        // RefToモジュール
        // - データ型
        //   - RefDisplayData
        // - TS側オブジェクト作成関数
        // - 検索処理
        //   - Reactは型マッピングのみ
        //   - ASP.NET Core Controller Action
        //   - ApplicationService
        aggregateFile.AddCSharpClass(DisplayDataRef.RenderCSharpRecursively(rootAggregate, ctx), "Class_DisplayDataRef");
        aggregateFile.AddTypeScriptTypeDef(DisplayDataRef.RenderTypeScriptRecursively(rootAggregate, ctx));
        aggregateFile.AddTypeScriptFunction(DisplayDataRef.RenderTypeScriptFunctionsRecursively(rootAggregate, ctx));
        aggregateFile.AddAppSrvMethod(SearchProcessingRefs.RenderAppSrvMethodRecursively(rootAggregate, ctx), "参照検索処理");
        aggregateFile.AddWebapiControllerAction(SearchProcessingRefs.RenderAspNetCoreControllerActionRecursively(rootAggregate, ctx));

        // データ型: ほかの集約から参照されるときのキー
        if (rootAggregate.IsView) {
            aggregateFile.AddCSharpClass(KeyClass.KeyClassEntry.RenderClassDeclaringRecursively(rootAggregate, ctx), "Class_KeyClass");
        }

        // UI用モジュール
        // - DisplayData等のマッピングオブジェクト
        // - React Router のURL定義
        // - ナビゲーション用関数
        // など
        ctx.Use<CommandQueryMappings>()
            .AddQueryModel(rootAggregate);

        // 定数: メタデータ
        aggregateFile.AddTypeScriptFunction(AggregateMetadata.RenderTsConstant(rootAggregate, displayData, searchCondition.FilterRoot, ctx));
    }

    public override void RenderCommon(CodeRenderingContext ctx) {
        ctx.Use<DeepEqualFunction.OptionType>();

        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(SearchProcessingReturn.RenderCSharp(ctx)); // 一覧検索の戻り値の型
            });
        });
        ctx.ReactProject(dir => {
            dir.Generate(SearchCondition.Entry.RenderTsBaseType()); // 検索条件の基底型
            dir.Generate(SearchProcessingReturn.RenderTypeScript()); // 一覧検索の戻り値の型
        });
    }

    /// <summary>
    /// IsQueryable 属性のみが指定されたデータ構造の外部参照をチェックします
    /// </summary>
    private static void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith("ref-to:") == true)
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

            // 参照先が検索可能なデータ構造か確認
            if (!refToRoot.IsQueryable()) {
                addError(refElement, $"{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造からは、同じく{IsQueryableOption.ATTRIBUTE_NAME}属性が指定されたデータ構造しか参照できません。");
            }
        }

        // 循環参照チェック
        if (HasCircularReferences(rootAggregateElement, context)) {
            addError(rootAggregateElement, $"{IsQueryableOption.ATTRIBUTE_NAME}属性のみが指定されたデータ構造で循環参照を定義することはできません。");
        }
    }

    /// <summary>
    /// クエリモデルの循環参照をチェックします
    /// </summary>
    private static bool HasCircularReferences(XElement rootAggregateElement, SchemaParseContext context) {
        var visited = new HashSet<XElement>();
        var recStack = new HashSet<XElement>();

        // DFSで循環参照を検出
        bool HasCircular(XElement element) {
            if (recStack.Contains(element)) {
                return true; // 循環参照発見
            }

            if (visited.Contains(element)) {
                return false; // 既に訪問済みで循環なし
            }

            visited.Add(element);
            recStack.Add(element);

            // この要素からの参照をチェック
            var refElements = element.Descendants()
                .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true);

            foreach (var refElement in refElements) {
                var refTo = context.FindRefTo(refElement);
                if (refTo == null) continue;

                // 参照先のルート要素
                var refToRoot = refTo.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

                // 自身のツリー内の参照はスキップ
                var currentRoot = element.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);
                if (refToRoot == currentRoot) continue;

                if (HasCircular(refToRoot)) {
                    return true;
                }
            }

            recStack.Remove(element);
            return false;
        }

        return HasCircular(rootAggregateElement);
    }
}

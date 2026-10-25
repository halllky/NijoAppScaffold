using Nijo.CodeGenerating;
using Nijo.DataStructures.IsQueryable;
using Nijo.DataStructures.Members;
using Nijo.Parts.CSharp;
using Nijo.Parts.Common;
using Nijo.SchemaParsing;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System;
using Nijo.DataStructures.Aggregates;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// データ構造がデータベースに永続化されることを表す属性
/// </summary>
internal sealed class IsStoredOption : FeatureOption {
    internal const string ATTRIBUTE_NAME = "IsStored";

    public override string AttributeName => ATTRIBUTE_NAME;
    public override string DisplayName => "永続化する";
    public override E_NodeOptionType Type => E_NodeOptionType.Boolean;
    public override string HelpText => $$"""
        このデータ構造がデータベースに永続化されることを表します。
        RDBMSのテーブル定義やEntity Framework Coreのエンティティ、新規登録・更新・削除処理が生成されます。
        {{IsQueryableOption.ATTRIBUTE_NAME}} と同時に指定した場合、テーブルと全く同じ形の検索処理や画面表示用データも生成されます。
        これはきわめて単純なマスタデータなど、データベース上のデータ構造とそれを表示・編集する画面のデータ構造が完全一致する場合を想定しています。
        """;

    public override bool IsAvailable(NodeOptionTarget target) {
        return target.Section == E_Section.DataStructures && target.NodeType == E_NodeType.RootAggregate;
    }

    public override void ValidateRootAggregate(XElement rootAggregateElement, SchemaParseContext context, SchemaValidationErrors errors) {
        // ルートとChildrenはキー必須。子集約にキーを指定できないことは IsKey 属性の側で検証する
        var rootAndChildren = rootAggregateElement
            .DescendantsAndSelf()
            .Where(el => el == rootAggregateElement || context.GetNodeType(el) == E_NodeType.ChildrenAggregate);
        foreach (var el in rootAndChildren) {
            if (!el.Elements().Any(member => member.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null)) {
                errors.Add(el, "キーが指定されていません。");
            }
        }

        ValidateRefTo(rootAggregateElement, context, errors);

        // 循環参照のチェック（主キーや必須制約による閉路が生じないか）
        ValidateCircularReferences(rootAggregateElement, context, errors.Add);
    }

    /// <summary>
    /// 永続化されるデータ構造からの外部参照の制約を検証します。
    /// </summary>
    private static void ValidateRefTo(XElement rootAggregateElement, SchemaParseContext context, SchemaValidationErrors errors) {
        var isQueryable = rootAggregateElement.IsQueryable();
        foreach (var (refElement, _, refToRoot) in context.EnumerateRefsToOtherTrees(rootAggregateElement)) {
            var refToIsStored = refToRoot.IsStored();
            var refToIsQueryable = refToRoot.IsQueryable();
            var refToIsView = refToRoot.Attribute(MapToViewOption.ATTRIBUTE_NAME) != null;

            // 永続化されるデータ構造からは、永続化されるデータ構造またはビューにマッピングされるデータ構造しか参照できない
            if (!refToIsStored && !(refToIsQueryable && refToIsView)) {
                errors.Add(refElement, $"{ATTRIBUTE_NAME}属性が指定されたデータ構造からは、{ATTRIBUTE_NAME}属性が指定されたデータ構造、またはビューにマッピングされるデータ構造しか参照できません。");
                continue;
            }

            // 検索可能かつ永続化されるデータ構造から、検索可能でない永続化されるデータ構造への参照を禁止。RefTargetなどが生成されないので
            if (isQueryable && refToIsStored && !refToIsQueryable) {
                errors.Add(refElement, $"{ATTRIBUTE_NAME}属性と{IsQueryableOption.ATTRIBUTE_NAME}属性が両方指定されたデータ構造からは、{ATTRIBUTE_NAME}属性のみが指定されたデータ構造を参照できません。");
            }
        }
    }

    public override void Render(CodeRenderingContext ctx, RootAggregate rootAggregate, SourceFileByAggregate aggregateFile) {
        // データ型: EFCore Entity
        var efCoreEntity = new EFCoreEntity(rootAggregate);
        aggregateFile.AddCSharpClass(EFCoreEntity.RenderClassDeclaring(efCoreEntity, ctx), "Class_EFCoreEntity");
        ctx.Use<DbContextClass>().AddEntities(efCoreEntity.EnumerateThisAndDescendants());

        // データ型: SaveCommand
        aggregateFile.AddCSharpClass(SaveCommand.RenderAll(rootAggregate, ctx), "Class_SaveCommand");

        // データ型: SaveCommandメッセージ
        var saveCommandMessage = new SaveCommandMessageContainer(rootAggregate);
        aggregateFile.AddCSharpClass(SaveCommandMessageContainer.RenderTree(rootAggregate), "Class_SaveCommandMessage");
        ctx.Use<MessageContainer.BaseClass>()
            .Register(saveCommandMessage.InterfaceName, saveCommandMessage.CsClassName)
            .Register(saveCommandMessage.CsClassName, saveCommandMessage.CsClassName);

        // 処理: 新規登録、更新
        var descendantStructurePaths = efCoreEntity
            .CreateStructurePathsRecursively()
            .ToDictionary(x => x.Metadata.SchemaPathNode.ToMappingKey());
        var create = new CreateMethod(rootAggregate);
        var update = new UpdateMethod(rootAggregate);
        aggregateFile.AddAppSrvMethod(create.Render(ctx, descendantStructurePaths), "新規登録処理");
        aggregateFile.AddAppSrvMethod(update.Render(ctx, descendantStructurePaths), "更新処理");

        // 処理: 物理削除。論理削除する場合の削除処理は UseSoftDelete 属性が生成する
        if (!rootAggregate.UseSoftDelete) {
            var delete = new DeleteMethod(rootAggregate);
            aggregateFile.AddAppSrvMethod(delete.Render(ctx, descendantStructurePaths), "物理削除処理");
        }

        // 処理: 自動生成されるバリデーションエラーチェック
        aggregateFile.AddAppSrvMethod($$"""
            #region 自動生成されるバリデーション処理
            {{ValidatorBase.EnumerateAll(ctx).SelectTextTemplate(validator => $$"""
            {{validator.RenderDeclaring(rootAggregate, ctx)}}
            """)}}
            #endregion 自動生成されるバリデーション処理
            """, "バリデーション処理");

        // データ型: ほかの集約から参照されるときのキー
        aggregateFile.AddCSharpClass(KeyClass.KeyClassEntry.RenderClassDeclaringRecursively(rootAggregate, ctx), "Class_KeyClass");

        // カスタムロジック用モジュール
        ctx.Use<CommandQueryMappings>().AddDataModel(rootAggregate);
    }

    public override void RenderCommon(CodeRenderingContext ctx) {
        // 保存処理の結果クラス
        ctx.CoreLibrary(dir => {
            dir.Directory("Util", utilDir => {
                utilDir.Generate(DataModelSaveResult.Render(ctx));
            });
        });

        // メッセージコンテナ
        UpdateMethod.RegisterCommonParts(ctx);

        // バリデーターチェック違反時のメッセージ
        var msgFactory = ctx.Use<MsgFactory>();
        var validatorsForMessage = ValidatorBase.EnumerateAll(ctx)
            .GroupBy(v => v.MsgId)
            .Select(g => g.First());

        foreach (var validator in validatorsForMessage) {
            msgFactory.AddMessage(
                validator.MsgId,
                $"入力エラー時メッセージ（{validator.CommentName}）",
                validator.MsgTemplate);
        }

        // カスタム属性による入力検証のフック
        ValidateCustom.RegisterApplicationServiceHooks(ctx);
    }

    /// <summary>
    /// 永続化されるデータ構造の循環参照チェック
    /// 主キーや必須制約による閉路が生じないか確認します
    /// </summary>
    private static void ValidateCircularReferences(XElement rootAggregateElement, SchemaParseContext context, Action<XElement, string> addError) {
        // 自身を起点とするすべての外部参照を取得
        var refElements = rootAggregateElement
            .Descendants()
            .Where(el => el.Attribute(SchemaParseContext.ATTR_NODE_TYPE)?.Value?.StartsWith(SchemaParseContext.NODE_TYPE_REFTO + ":") == true)
            .ToList();

        if (refElements.Count == 0) {
            return; // 外部参照がなければ終了
        }

        // 循環参照チェック
        var graph = new Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>>();
        var allVertexes = new HashSet<XElement>();

        // グラフ構築
        foreach (var refElement in refElements) {
            var targetElement = context.FindRefTo(refElement);
            if (targetElement == null) continue;

            // 参照元の最上位ルート集約
            var sourceRoot = refElement.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

            // 参照先の最上位ルート集約
            var targetRoot = targetElement.AncestorsAndSelf().Last(e => e.Parent?.Parent == e.Document?.Root);

            // 自身のツリー内なら無視
            if (sourceRoot == targetRoot) continue;

            // 必須属性かどうか
            bool isRequired = refElement.Attribute(IsNotNullOption.ATTRIBUTE_NAME) != null;

            // 主キーかどうか
            bool isKey = refElement.Attribute(IsKeyOption.ATTRIBUTE_NAME) != null;

            // キーまたは必須の場合のみグラフに追加
            if (isKey || isRequired) {
                if (!graph.TryGetValue(sourceRoot, out var value)) {
                    value = [];
                    graph[sourceRoot] = value;
                }

                value.Add((targetRoot, isRequired));

                allVertexes.Add(sourceRoot);
                allVertexes.Add(targetRoot);
            }
        }

        // 循環検出
        if (HasCycle(graph, allVertexes, out var cycle)) {
            // 必須キーの循環依存がある場合はエラー
            var hasMandatoryCycle = cycle.All(node => {
                var sourceIdx = cycle.IndexOf(node);
                var targetIdx = (sourceIdx + 1) % cycle.Count;

                if (sourceIdx == -1 || targetIdx == -1) return false;

                var source = cycle[sourceIdx];
                var target = cycle[targetIdx];

                return graph.ContainsKey(source) &&
                       graph[source].Any(edge => edge.RefTo == target && edge.IsRequired);
            });

            if (hasMandatoryCycle) {
                string cycleStr = string.Join(" -> ", cycle.Select(context.GetPhysicalName));
                addError(rootAggregateElement, $"循環参照の中で全てのリンクが必須またはキーとなっており、データを登録できません。循環: {cycleStr}");
            }
        }
    }

    /// <summary>
    /// グラフ内での循環を検出します
    /// </summary>
    private static bool HasCycle(Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>> graph, HashSet<XElement> allVertexes, out List<XElement> cycle) {
        cycle = [];
        var visited = new Dictionary<XElement, bool>();
        var recStack = new HashSet<XElement>();

        foreach (var vertex in allVertexes) {
            if (!visited.ContainsKey(vertex)) {
                if (IsCyclicUtil(vertex, visited, recStack, graph, cycle)) {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsCyclicUtil(XElement vertex, Dictionary<XElement, bool> visited, HashSet<XElement> recStack,
                              Dictionary<XElement, List<(XElement RefTo, bool IsRequired)>> graph, List<XElement> cycle) {
        // 現在の頂点を訪問済みとマーク
        visited[vertex] = true;

        // 再帰スタックに追加
        recStack.Add(vertex);

        // この頂点から出ている辺を調べる
        if (graph.TryGetValue(vertex, out var value)) {
            foreach (var (RefTo, _) in value) {
                var neighbor = RefTo;

                // 未訪問の隣接頂点を再帰的に探索
                if (!visited.ContainsKey(neighbor)) {
                    if (IsCyclicUtil(neighbor, visited, recStack, graph, cycle)) {
                        // 循環が見つかった場合、循環リストに追加
                        if (cycle.Count == 0 || cycle[0] != neighbor) {
                            cycle.Add(vertex);
                        }
                        return true;
                    }
                }
                // 再帰スタック内に既に存在する場合は循環がある
                else if (recStack.Contains(neighbor)) {
                    // 循環の開始点として記録
                    cycle.Clear();
                    cycle.Add(neighbor);
                    cycle.Add(vertex);
                    return true;
                }
            }
        }

        // この頂点からの探索が終了したので再帰スタックから削除
        recStack.Remove(vertex);
        return false;
    }
}

using Nijo.Core;
using Nijo.Models.ReadModel2Features;
using Nijo.Models.RefTo;
using Nijo.Parts.WebClient;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models {
    /// <summary>
    /// 画面表示されるデータ型
    /// </summary>
    internal class ReadModel2 : IModel {
        void IModel.GenerateCode(CodeRenderingContext context, GraphNode<Aggregate> rootAggregate) {
            var allAggregates = rootAggregate.EnumerateThisAndDescendants();
            var aggregateFile = context.CoreLibrary.UseAggregateFile(rootAggregate);
            var rootDisplayData = new DataClassForDisplay(rootAggregate);
            var uiContext = context.UseSummarizedFile<UiContext>();

            // データ型: 検索条件クラス
            var condition = new SearchCondition(rootAggregate);
            aggregateFile.DataClassDeclaring.Add(condition.RenderCSharpDeclaringRecursively(context));
            aggregateFile.TypeScriptFile.Add(condition.RenderTypeScriptDeclaringRecursively(context));
            aggregateFile.TypeScriptFile.Add(condition.RenderCreateNewObjectFn(context));
            aggregateFile.TypeScriptFile.Add(condition.RenderParseQueryParameterFunction());
            aggregateFile.TypeScriptFile.Add(condition.RenderTypeScriptSortableMemberType());

            foreach (var agg in allAggregates) {
                // データ型: 検索結果クラス
                var searchResult = new SearchResult(agg);
                aggregateFile.DataClassDeclaring.Add(searchResult.RenderCSharpDeclaring(context));

                // データ型: ビュークラス
                var displayData = new DataClassForDisplay(agg);
                aggregateFile.DataClassDeclaring.Add(displayData.RenderCSharpDeclaring(context));
                aggregateFile.TypeScriptFile.Add(displayData.RenderTypeScriptDeclaring(context));
                aggregateFile.TypeScriptFile.Add(displayData.RenderTsNewObjectFunction(context));
            }

            // 処理: 検索処理
            var load = new LoadMethod(rootAggregate);
            aggregateFile.TypeScriptFile.Add(load.RenderReactHook(context));
            aggregateFile.ControllerActions.Add(load.RenderControllerAction(context));
            aggregateFile.AppServiceMethods.Add(load.RenderAppSrvBaseMethod(context));
            aggregateFile.AppServiceMethods.Add(load.RenderAppSrvAbstractMethod(context));

            // 処理: 検索処理の最後に読み取り専用を設定
            aggregateFile.AppServiceMethods.Add(rootDisplayData.RenderSetKeysReadOnly(context));

            // 処理: 一括更新処理
            if (context.Config.UseBatchUpdateVersion2) {
                if (!rootAggregate.Item.Options.IsReadOnlyAggregate) {
                    aggregateFile.TypeScriptFile.Add(new BatchUpdateReadModel().RenderFunction(context, rootAggregate));
                    aggregateFile.ControllerActions.Add(BatchUpdateReadModel.RenderControllerActionVersion2(context, rootAggregate));
                    aggregateFile.AppServiceMethods.Add(BatchUpdateReadModel.RenderAppSrvMethodVersion2(context, rootAggregate));
                }
            } else {
                context.UseSummarizedFile<BatchUpdateReadModel>().Register(rootAggregate);
            }

            // 処理: 一括更新処理前関数（ディープイコール比較関数、変更比較関数）
            aggregateFile.TypeScriptFile.Add(rootDisplayData.RenderDeepEqualFunctionRecursively(context));
            aggregateFile.TypeScriptFile.Add(rootDisplayData.RenderCheckChangesFunction(context));

            // UI: DisplayData型名一覧
            context.UseSummarizedFile<DisplayDataTypeList>().Add(rootDisplayData);

            // UI: 制約
            context.UseSummarizedFile<UiConstraintTypes>().Add(rootDisplayData);

            // UI: MultiView
            var multiView = new MultiView(rootAggregate);
            aggregateFile.TypeScriptFile.Add(multiView.RenderNavigationHook(context));
            aggregateFile.TypeScriptFile.Add(multiView.RenderExcelDownloadHook());
            context.CoreLibrary.AppSrvMethods.Add(multiView.RenderAppSrvGetUrlMethod());
            // MultiView 一覧検索結果Excel出力機能
            context.UseSummarizedFile<Features.Excel.OutputSearchResultMethod>().Use(rootAggregate);
            context.ReactProject.AddReactPage(multiView.Url, multiView.ComponentPhysicalName);

            // UI: SingleView
            var singleView = new SingleView(rootAggregate);
            aggregateFile.TypeScriptFile.Add(singleView.RenderPageFrameComponent(context));

            context.ReactProject.AddReactPage(singleView.Url, singleView.ComponentPhysicalName);

            // SingleView 初期表示時サーバー側処理
            aggregateFile.AppServiceMethods.Add(singleView.RenderSetSingleViewDisplayDataFn(context));
            aggregateFile.ControllerActions.Add(singleView.RenderSetSingleViewDisplayData(context));

            // UI: SingleViewナビゲーション用関数
            aggregateFile.TypeScriptFile.Add(singleView.RenderNavigateFn(context, SingleView.E_Type.New));
            aggregateFile.TypeScriptFile.Add(singleView.RenderNavigateFn(context, SingleView.E_Type.Edit)); // readonly, edit は関数共用
            context.CoreLibrary.AppSrvMethods.Add(singleView.RenderAppSrvGetUrlMethod()); // サーバー側は全モードで1つのメソッド

            // コマンドの処理結果でこの集約の詳細画面に遷移できるように登録する
            context.UseSummarizedFile<CommandModelFeatures.CommandResult>().Register(rootAggregate);

            // ---------------------------------------------
            // 他の集約から参照されるときのための部品

            foreach (var agg in allAggregates) {
                var asEntry = agg.AsEntry();

                // パフォーマンス改善のため、ほかの集約から参照されていない集約のRefTo部品は生成しない
                if (!agg.Item.Options.ForceGenerateRefToModules
                    && !context.Config.GenerateUnusedRefToModules
                    && !agg.GetReferedEdges().Any()) {
                    continue;
                }

                // データ型
                var refSearchCondition = new RefSearchCondition(asEntry, asEntry);
                var refSearchResult = new RefSearchResult(asEntry, asEntry);
                var refDisplayData = new RefDisplayData(asEntry, asEntry);
                aggregateFile.DataClassDeclaring.Add(refSearchCondition.RenderCSharpDeclaringRecursively(context));
                aggregateFile.DataClassDeclaring.Add(refSearchResult.RenderCSharp(context));
                aggregateFile.DataClassDeclaring.Add(refDisplayData.RenderCSharp(context));
                aggregateFile.TypeScriptFile.Add(refSearchCondition.RenderTypeScriptDeclaringRecursively(context));
                aggregateFile.TypeScriptFile.Add(refSearchCondition.RenderCreateNewObjectFn(context));
                aggregateFile.TypeScriptFile.Add(refDisplayData.RenderTypeScript(context));
                aggregateFile.TypeScriptFile.Add(refDisplayData.RenderTsNewObjectFunction(context));

                // 処理: 参照先検索
                var searchRef = new RefSearchMethod(asEntry, asEntry);
                aggregateFile.TypeScriptFile.Add(searchRef.RenderHook(context));
                aggregateFile.ControllerActions.Add(searchRef.RenderController(context));
                aggregateFile.AppServiceMethods.Add(searchRef.RenderAppSrvMethodOfReadModel(context));
            }

            // 権限
            foreach (var agg in allAggregates) {
                // どこからも参照されていない子孫集約の権限enumは作成しない
                if (agg.IsRoot() || agg.GetReferedEdges().Any()) {
                    context.UseSummarizedFile<AuthorizedAction>().Regeister(agg);
                }
            }
        }

        void IModel.GenerateCode(CodeRenderingContext context) {

            // ユーティリティクラス等
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(DataClassForDisplay.RenderBaseClass());
                dir.Generate(DisplayMessageContainer.RenderCSharp());
                dir.Generate(ISaveCommandConvertible.Render());
            });
            context.CoreLibrary.Enums.Add(SingleView.RenderSingleViewNavigationEnums());

            // 権限レベルを返すメソッド
            context.CoreLibrary.AppSrvMethods.Add($$"""
                /// <summary>
                /// 権限レベル取得メソッド
                /// ※詳細な処理はoverrideして実装する
                /// </summary>
                public virtual E_AuthLevel GetAuthorizedLevel({{AuthorizedAction.ENUM_Name}} auth) {
                    return E_AuthLevel.Write;
                }
                """);

            // SingleView, MultiViewのカスタマイズに用いる各種フック
            context.ReactProject.UtilDir(dir => {
                dir.Generate(SingleView.RenderSingleViewCommonHook());
            });
        }

        IEnumerable<string> IModel.ValidateAggregate(GraphNode<Aggregate> rootAggregate) {
            foreach (var agg in rootAggregate.EnumerateThisAndDescendants()) {

                // ルート集約またはChildrenはキー必須
                if (agg.IsRoot() || agg.IsChildrenMember()) {
                    var ownKeys = agg
                        .GetKeys()
                        .Where(m => m is AggregateMember.ValueMember vm && vm.DeclaringAggregate == vm.Owner
                                 || m is AggregateMember.Ref);
                    if (!ownKeys.Any()) {
                        yield return $"{agg.Item.DisplayName}にキーが1つもありません。";
                    }
                }

                foreach (var member in agg.GetMembers()) {

                    // WriteModelからReadModelへの参照は不可
                    if (member is AggregateMember.Ref @ref
                        && @ref.RefTo.GetRoot().Item.Options.Handler != NijoCodeGenerator.Models.ReadModel2.Key
                        && @ref.RefTo.GetRoot().Item.Options.Handler != NijoCodeGenerator.Models.WriteModel2.Key) {

                        yield return $"{agg.Item.DisplayName}.{member.MemberName}: {nameof(WriteModel2)}の参照先は{nameof(ReadModel2)}または{nameof(WriteModel2)}である必要があります。";
                    }
                }
            }
        }
    }
}

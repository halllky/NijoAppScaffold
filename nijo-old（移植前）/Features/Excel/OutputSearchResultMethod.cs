using Nijo.Core;
using Nijo.Models.ReadModel2Features;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Features.Excel {
    /// <summary>
    /// ReadModelの一覧検索結果をExcel出力するApplicationServiceのメソッド
    /// </summary>
    internal class OutputSearchResultMethod : ISummarizedFile {

        private const string CONTROLLER_ACTION_NAME = "excel";

        /// <summary>
        /// アプリケーションサービスのメソッドは引数の型でオーバーロードを用意するのでメソッド名は共通
        /// </summary>
        internal const string APP_SRV_METHOD_NAME = "CreateSearchResultExcelBook";

        /// <summary>
        /// ReadModelの一覧検索結果をExcel出力するよう登録します。
        /// </summary>
        internal void Use(GraphNode<Aggregate> aggregate) {
            _aggregates.Add(aggregate);
        }
        private readonly List<GraphNode<Aggregate>> _aggregates = new();

        int ISummarizedFile.RenderingOrder => -1; // ControllerActions（AggregateFileのレンダリング）より前

        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {

            // WebAPIのコントローラーのアクションを作成
            foreach (var rootAggregate in _aggregates) {
                var aggregateFile = context.CoreLibrary.UseAggregateFile(rootAggregate);
                aggregateFile.ControllerActions.Add(RenderWebApiControllerAction(rootAggregate));
            }

            // ApplicationServiceのExcel一覧化メソッドを作成
            context.CoreLibrary.AppSrvMethods.Add(RenderAppSrvMethods());

            // ユーティリティクラスを作成
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(ExcelBook.Render());
            });
        }


        /// <summary>
        /// サーバーのPOSTメソッドのAPI（クォートなし）
        /// </summary>
        internal static string GetApiEndpoint(GraphNode<Aggregate> rootAggregate) {
            var controller = new Parts.WebServer.Controller(rootAggregate.Item);
            return $"/{controller.SubDomain}/{CONTROLLER_ACTION_NAME}";
        }

        /// <summary>
        /// ASP.NET Core Web API のコントローラーをレンダリングします。
        /// </summary>
        private static string RenderWebApiControllerAction(GraphNode<Aggregate> rootAggregate) {
            var searchCondition = new SearchCondition(rootAggregate);

            return $$"""
                /// <summary>
                /// 一覧検索を行ない、結果をExcelファイルとしてクライアント側に返します。
                /// </summary>
                [HttpPost("{{CONTROLLER_ACTION_NAME}}")]
                public virtual IActionResult ExcelList(ComplexPostRequest<{{searchCondition.CsClassName}}> request) {
                    if (_applicationService.GetAuthorizedLevel(E_AuthorizedAction.{{rootAggregate.Item.PhysicalName}}) == E_AuthLevel.None) return Forbid();

                    /*********** 結合の不具合一覧のNo.114の件 **********/
                    // ページを跨いで全件出力する
                    request.Data.{{SearchCondition.SKIP_CS}} = null;
                    request.Data.{{SearchCondition.TAKE_CS}} = null;

                    // パフォーマンスのためExcel出力時は子孫テーブル（特に親と1対多のテーブル）をSELECTしない
                    request.Data.{{SearchCondition.EXCLUDE_CHILDREN_CS}} = true;

                    var context = new PresentationContext(new DisplayMessageContainer([]), new() { IgnoreConfirm = true }, _applicationService);
                    var excelBook = _applicationService.{{APP_SRV_METHOD_NAME}}(request.Data, context);
                    if (context.HasError()) {
                        return this.JsonContent(context.GetResult().ToJsonObject());
                    }
                    return File(excelBook.{{ExcelBook.TO_BYTE_ARRAY}}(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                }
                """;
        }

        private string RenderAppSrvMethods() {
            return $$"""
                #region 一覧検索結果Excel出力
                {{_aggregates.SelectTextTemplate(RenderAggregate)}}
                #endregion 一覧検索結果Excel出力
                """;

            static string RenderAggregate(GraphNode<Aggregate> rootAggregate) {
                var sc = new SearchCondition(rootAggregate);
                var sr = new DataClassForDisplay(rootAggregate);
                var load = new LoadMethod(rootAggregate);

                // 出力される列を定義する
                var columns = EnumerateColumnsRecursively(rootAggregate).Select(vm => new {
                    FullPath = vm.Declared.GetFullPathAsDataClassForDisplay(E_CsTs.CSharp),
                    OwnerName = vm.Owner.Item.DisplayName, // 列ヘッダのメンバーのオーナー名
                    vm.MemberName, // 列ヘッダのメンバー名
                });

                return $$"""
                    /// <summary>
                    /// {{rootAggregate.Item.DisplayName}}の一覧検索を行ない、その結果をレンダリングしたExcelブックオブジェクトを返します。
                    /// </summary>
                    /// <param name="searchCondition">一覧検索条件</param>
                    public virtual {{ExcelBook.BOOK_CLASS_NAME}} {{APP_SRV_METHOD_NAME}}({{sc.CsClassName}} searchCondition, IPresentationContext context) {
                        // TODO KR-0029: ExcelBookクラスの作成によって以下の処理が変更必要な場合は適宜変えてください。
                        //               変える必要がなければこのコメントを削除してください。
                        var book = new {{ExcelBook.BOOK_CLASS_NAME}}();

                        // シートにどう出力するかを定義する
                        var sheet = book.{{ExcelBook.ADD_SHEET}}<{{sr.CsClassName}}>("Sheet1");
                    {{columns.SelectTextTemplate(col => $$"""
                        sheet.{{ExcelBook.ADD_COLUMN}}(x => x.{{col.FullPath.Join("?.")}}, ["{{col.OwnerName.Replace("\"", "\\\"")}}", "{{col.MemberName}}"]);
                    """)}}

                        // 通常の一覧検索と同じ処理を流用して検索
                        var searchResult = {{load.AppSrvLoadMethod}}(searchCondition, context);

                        // 検索結果をExcelシートにレンダリング
                        sheet.{{ExcelBook.RENDER_ROWS}}(searchResult);

                        return book;
                    }
                    """;

                // 出力対象の列を再帰的に列挙します
                static IEnumerable<AggregateMember.ValueMember> EnumerateColumnsRecursively(GraphNode<Aggregate> agg) {
                    foreach (var m in agg.GetMembers()) {
                        if (m is AggregateMember.ValueMember vm) {
                            if (vm.DeclaringAggregate != agg) continue; // 親や参照先のメンバーは除外
                            if (vm.Options.InvisibleInGui) continue; // 非表示メンバーは出力対象外
                            yield return vm;

                        } else if (m is AggregateMember.Child child) {
                            foreach (var vm2 in EnumerateColumnsRecursively(child.ChildAggregate)) {
                                yield return vm2;
                            }
                        } else if (m is AggregateMember.Ref @ref) {
                            foreach (var vm2 in EnumerateColumnsRecursively(@ref.RefTo)) {
                                yield return vm2;
                            }
                        } else if (m is AggregateMember.Parent parent) {
                            // 無限ループ回避
                            if (parent.MemberAggregate == agg.Source?.Source.As<Aggregate>()) continue;
                            // 参照先の親ならば出力
                            if (agg.IsOutOfEntryTree()) {
                                foreach (var vm2 in EnumerateColumnsRecursively(parent.ParentAggregate)) {
                                    yield return vm2;
                                }
                            }
                        } else {
                            // Children, VariationItem はExcel上で表現できないので出力対象外。
                            continue;
                        }
                    }
                }
            }
        }
    }
}

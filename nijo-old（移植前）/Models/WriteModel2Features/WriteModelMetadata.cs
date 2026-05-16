using Nijo.Core;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.WriteModel2Features;

/// <summary>
/// WriteModelのメタデータ。
/// ソース生成後でリフレクション的な処理をしたいときに使用する。
/// </summary>
internal class WriteModelMetadata : ISummarizedFile {

    internal WriteModelMetadata Register(GraphNode<Aggregate> rootAggregate) {
        _rootAggregates.Add(rootAggregate);
        return this;
    }
    private readonly List<GraphNode<Aggregate>> _rootAggregates = [];

    void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
        context.CoreLibrary.UtilDir(utilDir => {
            utilDir.Generate(new SourceFile {
                FileName = "WriteModelMetadata.cs",
                RenderContent = RenderContent,
            });
        });
    }

    private string RenderContent(CodeRenderingContext context) {

        // 全集約をデータフローの順番（他のテーブルから参照される方がより先）で列挙する
        var allAggregatesOrderByDataFlow = new List<GraphNode<Aggregate>>();
        foreach (var root in _rootAggregates.OrderByDataFlow()) {

            // 集約ツリーを深さ優先探索で走査しリストに追加する
            void PushRecursive(GraphNode<Aggregate> agg) {
                allAggregatesOrderByDataFlow.Add(agg);

                foreach (var member in agg.GetMembersOrderByStrict()) {
                    if (member is AggregateMember.Child child) {
                        PushRecursive(child.ChildAggregate);

                    } else if (member is AggregateMember.Children children) {
                        PushRecursive(children.ChildrenAggregate);
                    }
                }
            }
            PushRecursive(root);
        }

        return $$"""
            using System;
            using System.Collections.Generic;
            using System.Linq;

            namespace {{context.Config.RootNamespace}};

            /// <summary>
            /// WriteModelのメタデータ。
            /// ソース生成後でリフレクション的な処理をしたいときに使用する。
            /// 理論上は各カラムの桁数の定数などより細かい情報も生成可能だが、2025-06-27現在はそこまで必要なかったので生成していない。
            /// </summary>
            public static class WriteModelMetadata {

                /// <summary>
                /// 全ての集約の情報を、データフローの順番で返す。
                /// 例えば、テーブルAがBへの、BがCへの外部キーを持つとき、 C, B, A の順番で列挙する。
                /// バックアップデータを外部キー制約に引っかからないようにしつつ全テーブルへリストアしたいような状況で使用する。
                /// </summary>
                public static IEnumerable<AggregateInfo> EnumerateAllAggregatesOrderByDataFlow() {
            {{allAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                    yield return {{agg.Item.PhysicalName}};
            """)}}
                }

            {{allAggregatesOrderByDataFlow.SelectTextTemplate(agg => $$"""
                public static AggregateInfo {{agg.Item.PhysicalName}} => new() {
                    DisplayName = "{{agg.Item.DisplayName.Replace("\"", "\\\"")}}",
                    TableName = "{{agg.Item.Options.DbName ?? agg.Item.PhysicalName}}",
                };
            """)}}


                #region 型定義
                /// <summary>
                /// 集約1個分の情報。
                /// RDBMSで言うならばテーブル1個と対応する。
                /// </summary>
                public class AggregateInfo {
                    /// <summary>
                    /// 表示用名称
                    /// </summary>
                    public required string DisplayName { get; init; }
                    /// <summary>
                    /// テーブル物理名
                    /// </summary>
                    public required string TableName { get; init; }
                }
                #endregion 型定義
            }
            """;
    }
}

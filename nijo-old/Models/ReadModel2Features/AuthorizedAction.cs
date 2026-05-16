using Castle.Core.Internal;
using Nijo.Core;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.ReadModel2Features {
    /// <summary>
    /// 要権限アクション
    /// ReadModel1つにつき、参照権限と更新権限の2種類が生成される
    /// </summary>
    internal class AuthorizedAction : ISummarizedFile {

        private readonly List<GraphNode<Aggregate>> _aggregates = new();

        public void Regeister(GraphNode<Aggregate> aggreate) {
            _aggregates.Add(aggreate);
        }

        public const string ENUM_Name = "E_AuthorizedAction";

        public void OnEndGenerating(CodeRenderingContext context) {

            context.ReactProject.UtilDir(dir => {
                dir.Generate(new SourceFile {
                    FileName = "auth-util.ts",
                    RenderContent = ctx => {
                        return $$"""
                            /** 権限レベル */
                            export const E_AuthLevel = {
                              /** 権限なし */
                              None: 0,
                              /** ReadModelの場合は「閲覧権限あり（制限付き）」。CommandModelの場合は「実行権限なし」 */
                              Read_RESTRICT: 1,
                              /** ReadModelの場合は「閲覧権限あり（制限なし）」。CommandModelの場合は「実行権限なし」 */
                              Read: 2,
                              /** ReadModelの場合は「閲覧権限（制限なし）と更新権限あり（制限付き）」。CommandModelの場合は「実行権限あり」 */
                              Write_RESTRICT: 3,
                              /** ReadModelの場合は「閲覧権限（制限なし）と更新権限あり（制限なし）」。CommandModelの場合は「実行権限あり」 */
                              Write: 4,
                            } as const

                            /** 権限レベルの数値 */
                            export type E_AuthLevelNumber = 0 | 1 | 2 | 3 | 4
                            """;
                    },
                });
            });

            context.CoreLibrary.Enums.Add($$"""
                /// <summary>
                /// 権限レベル
                /// </summary>
                public enum E_AuthLevel {
                    /// <summary>
                    /// 権限なし
                    /// </summary>
                    None = 0,
                    /// <summary>
                    /// ReadModelの場合は「閲覧権限あり（制限付き）」。CommandModelの場合は「実行権限なし」
                    /// </summary>
                    Read_RESTRICT = 1,
                    /// <summary>
                    /// ReadModelの場合は「閲覧権限あり（制限なし）」。CommandModelの場合は「実行権限なし」
                    /// </summary>
                    Read = 2,
                    /// <summary>
                    /// ReadModelの場合は「閲覧権限（制限なし）と更新権限あり（制限付き）」。CommandModelの場合は「実行権限あり」
                    /// </summary>
                    Write_RESTRICT = 3,
                    /// <summary>
                    /// ReadModelの場合は「閲覧権限（制限なし）と更新権限あり（制限なし）」。CommandModelの場合は「実行権限あり」
                    /// </summary>
                    Write = 4,
                }

                /// <summary>
                /// 要権限アクション
                /// 参照権限と更新権限
                /// </summary>
                public enum {{ENUM_Name}} {
                {{_aggregates.Distinct().SelectTextTemplate(agg => $$"""
                    {{agg.Item.PhysicalName}},
                """)}}
                }

                """);
        }

    }
}

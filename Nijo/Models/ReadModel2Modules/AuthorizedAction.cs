using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Parts.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Models.ReadModel2Modules {
    internal class AuthorizedAction : IMultiAggregateSourceFile {
        internal const string ENUM_Name = "E_AuthorizedAction";

        private readonly Lock _lock = new();
        private readonly List<AggregateBase> _aggregates = [];

        internal AuthorizedAction Register(AggregateBase aggregate) {
            lock (_lock) {
                _aggregates.Add(aggregate);
                return this;
            }
        }

        public void RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            ctx.Use<EnumFile2>().AddSourceCode(RenderEnumSourceCode());
        }

        public void Render(CodeRenderingContext ctx) {
            ctx.ReactProject(dir => {
                dir.Directory("util", utilDir => {
                    utilDir.Generate(new SourceFile {
                        FileName = "auth-util.ts",
                        Contents = """
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
                            """,
                    });
                });
            });
        }

        private string RenderEnumSourceCode() {
            var orderedAggregates = _aggregates
                .Distinct()
                .OrderBy(aggregate => aggregate.GetRoot().GetIndexOfDataFlow())
                .ThenBy(aggregate => aggregate.GetOrderInTree())
                .ToArray();

            return $$"""
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
                {{orderedAggregates.SelectTextTemplate(aggregate => $$"""
                    {{aggregate.PhysicalName}},
                """)}}
                }

                """;
        }
    }
}

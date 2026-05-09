using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Parts.JavaScript {
    /// <summary>
    /// 旧版互換モードで生成する React / TypeScript 関連ファイル。
    /// </summary>
    internal static class LegacyCompatibilityJavaScript {
        internal static void RenderReact(CodeRenderingContext ctx) {
            ctx.ReactProject(dir => {
                dir.Generate(RenderVForm2ContainerQueryCss(ctx));
                dir.Directory("util", utilDir => {
                    utilDir.Generate(RenderTypeScriptUtilIndex());
                    utilDir.Generate(RenderPresentationContextTypeScript());
                    utilDir.Generate(RenderSpaceFinderConstTypeScript(ctx));
                });
            });
        }

        private static SourceFile RenderVForm2ContainerQueryCss(CodeRenderingContext ctx) {
            const int maxColumn = 5;
            const int maxMember = 100;
            var threshold = ctx.Config.VFormThreshold ?? 320;

            var sections = Enumerable.Range(1, maxColumn).Select(col => {
                var minmax = new List<string>();
                if (col > 1) minmax.Add($"(min-width: {(col - 1) * threshold + threshold}px)");
                if (col < maxColumn) minmax.Add($"(max-width: {col * threshold + threshold}px)");

                return $$"""
                    /* VForm2: 横{{col}}列の場合のレイアウト */
                    @container {{string.Join(" and ", minmax)}} {
                      .vform-template-column {
                        grid-template-columns: calc((1px * var(--vform-max-depth)) + var(--vform-label-width)) 1fr{{(col >= 2 ? $" repeat({col - 1}, var(--vform-label-width) 1fr)" : string.Empty)}};
                      }
                    {{Enumerable.Range(1, maxMember).SelectTextTemplate(i => $$"""

                      .vform-vertical-{{i}}-items {
                        grid-template-rows: repeat({{Math.Ceiling((decimal)i / col)}}, auto);
                      }
                    """)}}
                    }
                    """;
            });

            return new SourceFile {
                FileName = "vform2-container-query.css",
                Contents = $$"""
                    {{sections.SelectTextTemplate(source => $$"""

                    {{source}}
                    """)}}
                    """,
            };
        }

        private static SourceFile RenderTypeScriptUtilIndex() {
            return new SourceFile {
                FileName = "index.ts",
                Contents = """
                    export * from "./batch-update"
                    export * from "./presentation-context"
                    export * from "./space-finder-const"
                    export * from "./MSG"
                    export * from "./index"
                    """,
            };
        }

        private static SourceFile RenderPresentationContextTypeScript() {
            return new SourceFile {
                FileName = "presentation-context.ts",
                Contents = """
                    /** 更新系処理結果後のプレゼンテーション側の状態。このオブジェクトの型はサーバー側と合わせる必要がある */
                    export type PresentationContextState<T = object> = {
                      /** 処理全体の成否 */
                      ok: boolean
                      /** 処理結果の概要。「処理成功しました」など */
                      summary?: string
                      /** 確認メッセージ */
                      confirms?: string[]
                      /** 処理結果の詳細。項目ごとにメッセージが格納される。 */
                      detail?: [string, ReactHookFormMultipleErrorObject][]
                      /** アプリケーション側からプレゼンテーション側に返す任意の値 */
                      returnValue?: T
                    }

                    /**
                     * react-hook-form で1つのフィールドに複数のエラーを表示させる場合のエラーメッセージのオブジェクトの型。
                     * ただしキー名を "ERROR-" 等で始めるのは react-hook-forms ではなくこのプロジェクトの都合（警告やインフォメーションで色を分けるため）
                     */
                    export type ReactHookFormMultipleErrorObject = {
                      types: {
                        [key in `ERROR-${number | string}` | `WARN-${number | string}` | `INFO-${number | string}`]: string
                      }
                    }
                    """,
            };
        }

        private static SourceFile RenderSpaceFinderConstTypeScript(CodeRenderingContext ctx) {
            var maxFileSize = ctx.Config.MaxFileSizeMB?.ToString() ?? "undefined";
            var maxTotalFileSize = ctx.Config.MaxTotalFileSizeMB?.ToString() ?? "undefined";
            var extensions = string.IsNullOrWhiteSpace(ctx.Config.AttachmentFileExtensions)
                ? "undefined"
                : $"[{string.Join(", ", ctx.Config.AttachmentFileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ext => $"'.{ext}'"))}]";

            return new SourceFile {
                FileName = "space-finder-const.ts",
                Contents = $$"""
                    /** Webブラウザから添付ファイルとして添付可能なファイルの制限を表す定数。 */
                    export const AttachmentFileConst = {
                      /** 添付可能なファイルの上限サイズ（メガバイト） */
                      MaxFileSizeMB: {{maxFileSize}},
                      /** 一度に複数ファイル添付する際のトータルの上限サイズ（メガバイト） */
                      MaxTotalFileSizeMB: {{maxTotalFileSize}},
                      /** 添付可能なファイルの拡張子 */
                      AttachmentFileExtensions: {{extensions}},
                    }
                    """,
            };
        }
    }
}

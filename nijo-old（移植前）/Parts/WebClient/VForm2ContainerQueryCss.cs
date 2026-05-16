using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebClient {
    /// <summary>
    /// vform2-container-query.css ファイル
    /// </summary>
    internal class VForm2ContainerQueryCss {

        /// <summary>
        /// VForm2では技術的な理由によりその実装の一部をコンテナクエリ(@container)で実装している。
        /// またその中身もCSSファイルに直で書くよりループを回して自動生成した方が楽な箇所があり、
        /// それをこのメソッドでレンダリングしている。
        /// </summary>
        internal static SourceFile RenderVFormContainerQuery() {
            return new SourceFile {
                FileName = "vform2-container-query.css",
                RenderContent = ctx => {

                    // 最大列数。通常のデスクトップPCならこの列数まで用意しておけば足りるだろうという数
                    var MAX_COLUMN = ctx.Config.VFormMaxColumnCount ?? 5;

                    // CSSクラスを生成する数。1つの親要素の直下に並ぶメンバーの限界値。
                    // メンバーの数がこれを超えるとレイアウトが崩れる。
                    var MAX_MEMBER = ctx.Config.VFormMaxMemberCount ?? 100;

                    // 列数が切り替わる閾値（px）
                    var THRESHOLD = ctx.Config.VFormThreshold ?? 320;

                    return Enumerable.Range(1, MAX_COLUMN).SelectTextTemplate(col => {

                        // minmaxはそのレイアウトが適用されるコンテナ横幅の最小から最大までの幅。
                        // 例えば閾値が400pxの場合、各列数ごとの具体的な値は以下
                        // 
                        // COL: 1      2       3       4       5
                        // -------------------------------------------
                        // MIN: -    ,  800px, 1200px, 1600px, 2000px
                        // MAX: 800px, 1200px, 1600px, 2000px, -

                        var minmax = new List<string>();
                        if (col > 1) minmax.Add($"(min-width: {(col - 1) * THRESHOLD + THRESHOLD}px)");
                        if (col < MAX_COLUMN) minmax.Add($"(max-width: {col * THRESHOLD + THRESHOLD}px)");

                        return $$"""

                            /* VForm2: 横{{col}}列の場合のレイアウト */
                            @container {{minmax.Join(" and ")}} {
                              .vform-template-column {
                                grid-template-columns: calc((1px * var(--vform-max-depth)) + var(--vform-label-width)) 1fr{{(col >= 2 ? $" repeat({col - 1}, var(--vform-label-width) 1fr)" : "")}};
                              }
                            {{Enumerable.Range(1, MAX_MEMBER).SelectTextTemplate(i => $$"""

                              .vform-vertical-{{i}}-items {
                                grid-template-rows: repeat({{Math.Ceiling((decimal)i / col)}}, auto);
                              }
                            """)}}
                            }
                            """;
                    });
                },
            };
        }

        /// <summary>
        /// 既定のボタン色。nijo.xmlでボタン色が指定されている場合、cssファイル名のこの文字列に対する置換が行われる。
        /// </summary>
        internal const string DEFAULT_BUTTON_COLOR = "cyan";
    }
}

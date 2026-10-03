using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.CodeGenerating {
    /// <summary>
    /// 自動生成されるソースコードのファイル1個分
    /// </summary>
    public class SourceFile {
        /// <summary>
        /// 自動生成されるソースコードのファイル1個分を表すクラスを作成します。
        /// </summary>
        /// <param name="callerFilePath">このソースファイルがどのファイルから生成されたか（コンパイル時に自動的に設定される）</param>
        /// <param name="callerMemberName">このソースファイルがどのメソッドから生成されたか（コンパイル時に自動的に設定される）</param>
        public SourceFile([CallerFilePath] string? callerFilePath = null, [CallerMemberName] string? callerMemberName = null) {
            CallerFilePath = callerFilePath;
            CallerMemberName = callerMemberName;
        }

        /// <summary>このソースファイルがどのファイルから生成されたか</summary>
        public string? CallerFilePath { get; }
        /// <summary>このソースファイルがどのメソッドから生成されたか</summary>
        public string? CallerMemberName { get; }

        /// <summary>生成されるファイルの名前</summary>
        public required string FileName { get; init; }
        /// <summary>生成されるファイルの内容</summary>
        public required string Contents { get; init; }

        internal void Render(string filepath) {

            // 出力ファイルの形式を決める
            var ext = Path.GetExtension(filepath).ToLower();
            var encoding = ext == ".cs" || ext == ".sql"
                ? Encoding.UTF8 // With BOM
                : new UTF8Encoding(false);
            var newLine = ext == ".cs" || ext == ".sql"
                ? "\r\n"
                : "\n";
            var comment = ext switch {
                ".sql" => "--",
                ".css" => "",
                _ => "//",
            };
            using var sw = new StreamWriter(filepath, append: false, encoding) {
                NewLine = newLine,
            };

            // ファイル先頭の注意書き
            if (ext != ".md") {
                sw.WriteLine($$"""
                    {{(ext == ".css" ? "/*" : "")}}
                    {{comment}} このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
                    {{(CallerFilePath != null ? $"{comment} ※ このファイルを生成したクラス  : {Path.GetFileName(CallerFilePath)}" : "")}}
                    {{(CallerMemberName != null ? $"{comment} ※ このファイルを生成したメソッド: {CallerMemberName}" : "")}}
                    {{(ext == ".css" ? "*/" : "")}}
                    """.ReplaceLineEndings(newLine));
            }

            // WithIndent 用のスタック
            var indentStack = new Stack<string>();

            foreach (var rawLine in Contents.Split(["\r\n", "\n"], StringSplitOptions.None)) {
                var containsSkipMarker = rawLine.Contains(SKIP_MARKER);
                var currentIndent = string.Concat(indentStack.Reverse());
                var line = RemoveMarkers(rawLine);

                // ループや条件分岐の空展開で発生する余計な空行だけ抑止し、通常の空行は保持する
                if (!containsSkipMarker || !string.IsNullOrWhiteSpace(line)) {
                    sw.WriteLine(currentIndent + line);
                }
                UpdateIndentStack(indentStack, rawLine);
            }
        }

        /// <summary>
        /// <see cref="TemplateTextHelper.WithIndent"/> の仕組みを実現するための仕組み。
        /// インデントスタックを更新します。
        /// 1引数版のインデント開始マーカーが見つかったら、インデント計測起点からマーカーまでの文字列をスタックに積みます。
        /// インデント計測起点は、この行の中で開始されてまだ終了していない直近の開始マーカーの直後で、無ければ行頭です。
        /// 2引数版のインデント開始マーカーが見つかったら、マーカーに埋め込まれたインデント文字列をスタックに積みます。
        /// インデント終了マーカーが見つかったら、スタックから1つ取り出します。
        /// </summary>
        private static void UpdateIndentStack(Stack<string> indentStack, string line) {
            // この行の中で開始された開始マーカーについて、そのマーカーより前のインデント計測起点を退避しておくスタック
            var segmentStartStack = new Stack<int>();
            var segmentStart = 0;
            var searchIndex = 0;

            while (searchIndex < line.Length) {
                var nextStart = line.IndexOf(INDENT_START_MARKER, searchIndex, StringComparison.Ordinal);
                var nextExplicitStart = line.IndexOf(EXPLICIT_INDENT_START_MARKER, searchIndex, StringComparison.Ordinal);
                var nextEnd = line.IndexOf(INDENT_END_MARKER, searchIndex, StringComparison.Ordinal);
                var next = new[] { nextStart, nextExplicitStart, nextEnd }.Where(i => i >= 0).DefaultIfEmpty(-1).Min();

                if (next < 0) {
                    break;
                }

                if (next == nextEnd) {
                    if (indentStack.Count == 0) {
                        throw new InvalidOperationException("インデント終了マーカーに対応する開始マーカーがありません。");
                    }

                    indentStack.Pop();
                    if (segmentStartStack.Count > 0) {
                        segmentStart = segmentStartStack.Pop();
                    }
                    searchIndex = nextEnd + INDENT_END_MARKER.Length;

                } else if (next == nextStart) {
                    indentStack.Push(RemoveMarkers(line[segmentStart..nextStart]));
                    segmentStartStack.Push(segmentStart);
                    segmentStart = nextStart + INDENT_START_MARKER.Length;
                    searchIndex = segmentStart;

                } else {
                    var indentBegin = nextExplicitStart + EXPLICIT_INDENT_START_MARKER.Length;
                    var delimiter = line.IndexOf(EXPLICIT_INDENT_DELIMITER, indentBegin, StringComparison.Ordinal);
                    if (delimiter < 0) {
                        throw new InvalidOperationException("インデント開始マーカーに対応するインデント文字列の終端がありません。");
                    }

                    indentStack.Push(line[indentBegin..delimiter]);
                    segmentStartStack.Push(segmentStart);
                    segmentStart = delimiter + EXPLICIT_INDENT_DELIMITER.Length;
                    searchIndex = segmentStart;
                }
            }
        }

        /// <summary>
        /// <see cref="TemplateTextHelper"/> のマーカーを文字列から除去します。
        /// 2引数版のインデント開始マーカーは、埋め込まれたインデント文字列ごと除去します。
        /// </summary>
        private static string RemoveMarkers(string text) {
            if (!text.Contains('\0')) {
                return text;
            }

            var result = new StringBuilder();
            var index = 0;

            while (true) {
                var explicitStart = text.IndexOf(EXPLICIT_INDENT_START_MARKER, index, StringComparison.Ordinal);
                if (explicitStart < 0) {
                    break;
                }

                var delimiter = text.IndexOf(EXPLICIT_INDENT_DELIMITER, explicitStart, StringComparison.Ordinal);
                if (delimiter < 0) {
                    throw new InvalidOperationException("インデント開始マーカーに対応するインデント文字列の終端がありません。");
                }

                result.Append(text, index, explicitStart - index);
                index = delimiter + EXPLICIT_INDENT_DELIMITER.Length;
            }
            result.Append(text, index, text.Length - index);

            return result
                .Replace(INDENT_START_MARKER, string.Empty)
                .Replace(INDENT_END_MARKER, string.Empty)
                .Replace(SKIP_MARKER, string.Empty)
                .ToString();
        }
    }
}

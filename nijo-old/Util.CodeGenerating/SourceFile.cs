using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Util.CodeGenerating {
    public class SourceFile {
        public required string FileName { get; init; }
        public required Func<CodeRenderingContext, string> RenderContent { get; init; }

        public static StreamWriter GetStreamWriter(string filepath) {
            var ext = Path.GetExtension(filepath).ToLower();
            var encoding = ext == ".cs" || ext == ".sql"
                ? Encoding.UTF8 // With BOM
                : new UTF8Encoding(false);
            var newLine = ext == ".cs" || ext == ".sql"
                ? "\r\n"
                : "\n";
            return new StreamWriter(filepath, append: false, encoding) {
                NewLine = newLine,
            };
        }

        internal void Render(string filepath, CodeRenderingContext ctx) {
            var ext = Path.GetExtension(filepath).ToLower();
            using var sw = GetStreamWriter(filepath);

            if (ext != ".md") {
                var comment = ext switch {
                    ".sql" => "--",
                    ".css" => "",
                    _ => "//",
                };
                sw.WriteLine($$"""
                    {{(ext == ".css" ? "/*" : "")}}
                    {{comment}} このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
                    {{(ext == ".css" ? "*/" : "")}}
                    """.ReplaceLineEndings(sw.NewLine));
            }

            var content = RenderContent(ctx).ReplaceLineEndings(sw.NewLine);
            foreach (var line in content.Split(sw.NewLine)) {
                if (line.Contains(SKIP_MARKER)) continue;
                sw.WriteLine(string.IsNullOrWhiteSpace(line) ? string.Empty : line);
            }
        }
    }
}

using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class SpaceFinderConst {
    internal static SourceFile RenderTypeScript() {
        return new SourceFile {
            FileName = "space-finder-const.ts",
            RenderContent = ctx => $$"""
                /** Webブラウザから添付ファイルとして添付可能なファイルの制限を表す定数。 */
                export const AttachmentFileConst = {
                  /** 添付可能なファイルの上限サイズ（メガバイト） */
                  MaxFileSizeMB: {{(ctx.Config.MaxFileSizeMB?.ToString() ?? "undefined")}},
                  /** 一度に複数ファイル添付する際のトータルの上限サイズ（メガバイト） */
                  MaxTotalFileSizeMB: {{(ctx.Config.MaxTotalFileSizeMB?.ToString() ?? "undefined")}},
                  /** 添付可能なファイルの拡張子 */
                  AttachmentFileExtensions: {{(ctx.Config.AttachmentFileExtensions != null ? $"[{ctx.Config.AttachmentFileExtensions.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ext => $"'.{ext}'").Join(", ")}]" : "undefined")}},
                }
                """,
        };
    }

    internal static SourceFile RenderCSharp() {
        return new SourceFile {
            FileName = "SpaceFinderConst.cs",
            RenderContent = ctx => $$"""
using System.Collections.Generic;

namespace {{ctx.Config.RootNamespace}} {
    public class SpaceFinderConst {
{{If(ctx.Config.MaxFileSizeMB.HasValue, () => $$"""
        /// <summary>添付可能なファイルの上限サイズ（メガバイト）</summary>
        public const int MAX_FILE_SIZE_MB = {{ctx.Config.MaxFileSizeMB!.Value}};
""")}}
{{If(ctx.Config.MaxTotalFileSizeMB.HasValue, () => $$"""
        /// <summary>一度に複数ファイル添付する際のトータルの上限サイズ（メガバイト）</summary>
        public const int MAX_TOTAL_FILE_SIZE_MB = {{ctx.Config.MaxTotalFileSizeMB!.Value}};
""")}}
{{If(ctx.Config.AttachmentFileExtensions != null, () => $$"""
        /// <summary>添付可能なファイルの拡張子</summary>
        public static IEnumerable<string> ATTACHMENT_FILE_EXTENSIONS => [{{ctx.Config.AttachmentFileExtensions!.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ext => $"\".{ext}\"").Join(", ")}}];
""")}}
    }
}
""",
        };
    }
}

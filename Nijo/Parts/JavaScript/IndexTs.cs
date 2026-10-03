using Nijo.CodeGenerating;
using Nijo.Parts.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.JavaScript {
    /// <summary>
    /// TypeScriptの "index.ts" ファイル
    /// </summary>
    internal class IndexTs {
        private const string FILE_NAME = "index.ts";

        /// <summary>
        /// TypeScriptの "index.ts" ファイルをレンダリングします。
        /// 同ディレクトリ内の他のソースファイルが全てレンダリングされた後に呼ばれる必要があります。
        /// </summary>
        internal static void Render(DirectorySetupper dir, CodeRenderingContext ctx) {
            var modules = ctx
                .GetGeneratedFileNames(dir)
                .Where(path => Path.GetExtension(path) == ".ts")
                .Select(path => Path.GetFileName(path))
                .Where(fileName => fileName != FILE_NAME && fileName != EnumFile.TS_FILENAME)
                .Select(fileName => Path.GetFileNameWithoutExtension(fileName))
                .OrderBy(fileName => fileName)
                .ToArray();

            // "import * as" で読み込んで再エクスポートするもの
            var namespaceModules = modules
                .Where(CommandQueryMappings.TsNamespaceModules.ContainsKey)
                .Select(fileName => (FileName: fileName, Alias: CommandQueryMappings.TsNamespaceModules[fileName]))
                .ToArray();

            // そのまま再エクスポートするもの
            var otherModules = modules
                .Where(fileName => !CommandQueryMappings.TsNamespaceModules.ContainsKey(fileName))
                .ToArray();

            dir.Generate(new SourceFile {
                FileName = FILE_NAME,
                Contents = $$"""
                    {{otherModules.SelectTextTemplate(fileName => $$"""
                    export * from "./{{fileName.Replace("\"", "\\\"")}}"
                    """)}}

                    {{namespaceModules.SelectTextTemplate(x => $$"""
                    import * as {{x.Alias}} from "./{{x.FileName}}"
                    """)}}
                    export {
                    {{namespaceModules.SelectTextTemplate(x => $$"""
                      {{x.Alias}},
                    """)}}
                    }
                    """,
            });
        }

    }
}

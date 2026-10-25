using Nijo.CodeGenerating;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Parts.JavaScript {
    /// <summary>
    /// TypeScriptの自動生成ディレクトリ直下の "index.ts" ファイル。
    /// 自動生成ディレクトリ直下のモジュールのうち index.ts から再エクスポートするものは、そのモジュールを生成する側が登録する。
    /// </summary>
    internal class IndexTs : ISharedSourceFile<CodeRenderingContext> {
        private const string FILE_NAME = "index.ts";

        private readonly Lock _lock = new();
        private readonly HashSet<string> _modules = [];
        private readonly Dictionary<string, string> _namespaceModules = [];

        /// <summary>
        /// そのまま再エクスポートするモジュールを登録します。
        /// </summary>
        /// <param name="fileNameWithoutExtension">自動生成ディレクトリ直下のモジュールのファイル名（拡張子なし）</param>
        internal IndexTs AddModule(string fileNameWithoutExtension) {
            lock (_lock) {
                _modules.Add(fileNameWithoutExtension);
                return this;
            }
        }
        /// <summary>
        /// "import * as" で読み込み、別名をつけて再エクスポートするモジュールを登録します。
        /// </summary>
        /// <param name="fileNameWithoutExtension">自動生成ディレクトリ直下のモジュールのファイル名（拡張子なし）</param>
        /// <param name="alias">再エクスポートするときの別名</param>
        internal IndexTs AddNamespaceModule(string fileNameWithoutExtension, string alias) {
            lock (_lock) {
                _namespaceModules[fileNameWithoutExtension] = alias;
                return this;
            }
        }

        void ISharedSourceFile<CodeRenderingContext>.RegisterDependencies(CodeRenderingContext ctx) {
            // 特になし
        }

        void ISharedSourceFile<CodeRenderingContext>.Render(CodeRenderingContext ctx) {
            var modules = _modules
                .OrderBy(fileName => fileName)
                .ToArray();
            var namespaceModules = _namespaceModules
                .OrderBy(x => x.Key)
                .Select(x => (FileName: x.Key, Alias: x.Value))
                .ToArray();

            ctx.ReactProject(dir => {
                dir.Generate(new SourceFile {
                    FileName = FILE_NAME,
                    Contents = $$"""
                        {{modules.SelectTextTemplate(fileName => $$"""
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
            });
        }
    }
}

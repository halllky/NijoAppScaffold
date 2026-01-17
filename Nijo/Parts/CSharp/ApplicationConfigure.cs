using Nijo.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// アプリケーション起動時に実行される設定処理。
    /// </summary>
    public class ApplicationConfigure : IMultiAggregateSourceFile {

        public const string ABSTRACT_CLASS_CORE = "DefaultConfiguration";

        #region Add
        private readonly Lock _lock = new();

        private readonly List<Func<string, string>> _coreConfigureServices = [];
        private readonly List<string> _coreMethods = [];

        /// <summary>
        /// ConfigureServicesに生成されるソースコード。
        /// 引数は <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> のインスタンスの名前。
        /// </summary>
        public ApplicationConfigure AddCoreConfigureServices(Func<string, string> render) {
            lock (_lock) {
                _coreConfigureServices.Add(render);
                return this;
            }
        }
        /// <summary>
        /// Coreプロジェクトのアプリケーション起動時に実行される設定処理にメソッド等を追加します。
        /// </summary>
        /// <param name="configureServices">
        /// </param>
        /// <param name="abstractMethodSource">クラス直下にレンダリングされるソースコード</param>
        public ApplicationConfigure AddCoreMethod(string sourceCode) {
            lock (_lock) {
                _coreMethods.Add(sourceCode);
                return this;
            }
        }
        #endregion Add


        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // 特になし
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Generate(RenderCore(ctx));
            });
        }

        private SourceFile RenderCore(CodeRenderingContext ctx) {
            var coreConfigureServices = new List<Func<string, string>>(_coreConfigureServices);
            var coreMethods = new List<string>(_coreMethods);

            // ApplicationServiceを登録
            coreConfigureServices.Add(services => $$"""
                // アプリケーションサービス
                {{services}}.AddScoped(ConfigureApplicationService);
                """);
            coreMethods.Add($$"""
                /// <summary>
                /// アプリケーションサービスのインスタンスを定義する
                /// </summary>
                protected abstract {{ApplicationService.ABSTRACT_CLASS}} ConfigureApplicationService(IServiceProvider services);
                """);

            return new SourceFile {
                FileName = "DefaultConfiguration.cs",
                Contents = $$"""
                    using Microsoft.Extensions.DependencyInjection;
                    using Microsoft.Extensions.Logging;
                    using System.Text.Json;
                    using System.Text.Json.Nodes;
                    using System.Text.Json.Serialization;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 自動生成されたアプリケーション実行時設定。
                    /// 設定の一部を変更したい場合はこのクラスをオーバーライドしたクラスを作る。
                    /// </summary>
                    public abstract partial class {{ABSTRACT_CLASS_CORE}} {

                        #region DI設定
                        /// <summary>
                        /// DI設定。
                        /// このメソッドをオーバーライドするときは必ずbaseを呼び出すこと。
                        /// </summary>
                        public virtual void ConfigureServices(IServiceCollection services) {
                    {{coreConfigureServices.Select(render => $$"""

                            {{WithIndent(render("services"), "        ")}}
                    """).OrderBy(source => source).SelectTextTemplate(source => source)}}
                        }
                        #endregion DI設定

                    {{coreMethods.OrderBy(source => source).SelectTextTemplate(source => $$"""

                        {{WithIndent(source, "    ")}}
                    """)}}
                    }
                    """,
            };
        }
    }
}

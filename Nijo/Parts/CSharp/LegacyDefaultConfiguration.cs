using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// 旧版互換モードの DefaultConfiguration.cs を集約横断で構築する。
    /// </summary>
    internal class LegacyDefaultConfiguration : IMultiAggregateSourceFile {
        private readonly List<string> _valueObjectClassNames = [];
        private readonly List<string> _characterTypes = [];
        private readonly Lock _lock = new();
        private bool _renderDbContextCustomizationMethods;

        internal LegacyDefaultConfiguration AddValueObject(string className) {
            lock (_lock) {
                if (!_valueObjectClassNames.Contains(className)) {
                    _valueObjectClassNames.Add(className);
                }
                return this;
            }
        }

        internal LegacyDefaultConfiguration AddCharacterType(string characterType) {
            lock (_lock) {
                if (!_characterTypes.Contains(characterType)) {
                    _characterTypes.Add(characterType);
                }
                return this;
            }
        }

        internal LegacyDefaultConfiguration EnableDbContextCustomizationMethods() {
            lock (_lock) {
                _renderDbContextCustomizationMethods = true;
                return this;
            }
        }

        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            // 特になし
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
            var valueObjectClassNames = GetValueObjectClassNames();
            var characterTypes = GetCharacterTypes();
            var renderDbContextCustomizationMethods = ShouldRenderDbContextCustomizationMethods();

            ctx.CoreLibrary(dir => {
                dir.Generate(new SourceFile {
                    FileName = "DefaultConfiguration.cs",
                    Contents = $$"""
                        namespace {{ctx.Config.RootNamespace}} {
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.Extensions.Configuration;
                            using Microsoft.Extensions.DependencyInjection;
                            using NLog;
                            using NLog.Web;

                            /// <summary>
                            /// 自動生成されたアプリケーション実行時設定。
                            /// 設定の一部を変更したい場合は <see cref="CustomizedConfiguration"/> で該当の設定をオーバーライドしてください。
                            /// </summary>
                            public abstract partial class DefaultConfiguration {

                                /// <summary>
                                /// DI設定
                                /// </summary>
                                /// <param name="appSettingsNijoSection">appsettings.jsonのうちNijoセクション部分</param>
                                public virtual void ConfigureServices(IServiceCollection services, IConfigurationSection appSettingsNijoSection) {

                                    // アプリケーションサービス
                                    ConfigureApplicationService(services);

                                    // 実行時設定ファイル
                                    ConfigureRuntimeSetting(services);

                                    // DB接続
                                    services.AddScoped<Microsoft.EntityFrameworkCore.DbContext, {{ctx.Config.DbContextName}}>();
                                    ConfigureDbContext(services);

                                    // Encodingで Shift-JIS が使えるようにする
                                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                                }

                                /// <summary>
                                /// <see cref="OverridedApplicationService"/> をDIに登録します。
                                /// </summary>
                                protected abstract void ConfigureApplicationService(IServiceCollection services);

                                /// <summary>
                                /// 実行時設定をどこから参照するかの処理をDIに登録します。
                                /// <see cref="IRuntimeSetting"/> 型を登録してください。
                                /// </summary>
                                protected abstract void ConfigureRuntimeSetting(IServiceCollection services);

                                /// <summary>
                                /// Entity Framework Core のDbContextをDIに登録します。
                                /// </summary>
                                protected abstract void ConfigureDbContext(IServiceCollection services);

                        {{valueObjectClassNames.SelectTextTemplate(className => $$"""

                                /// <summary>
                                /// <see cref="{{className}}"/> クラスのプロパティがDBとC#の間で変換されるときの処理を定義するクラスを返します。
                                /// </summary>
                                public virtual Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter GetEFCoreValueConverterOf{{className}}() {
                                    return new {{className}}.EFCoreValueConverter();
                                }
                        """)}}
                        {{characterTypes.SelectTextTemplate(characterType => $$"""

                                /// <summary>
                                /// データ登録更新前の、文字列が{{characterType}}か否かを判定するロジック
                                /// </summary>
                                public abstract bool {{GetCharacterTypeMethodName(characterType)}}(string value, int? maxLength);
                        """)}}
                        {{If(renderDbContextCustomizationMethods, () => $$"""

                                /// <summary>
                                /// Entity Framework Core の定義にカスタマイズを加えます。
                                /// 既定のモデル定義処理の一番最後に呼ばれます。
                                /// データベース全体に対する設定を行うことを想定しています。（例えば、全テーブルの列挙体のDB保存される型を数値ではなく文字列にする、など）
                                /// </summary>
                                /// <param name="modelBuilder">モデルビルダー。Entity Framework Core 公式の解説を参照のこと。</param>
                                public virtual void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) {
                                }

                                /// <summary>
                                /// Entity Framework Core の <see cref="DbContext.ConfigureConventions(ModelConfigurationBuilder)"/> メソッドから呼ばれます。
                                /// 主にC#の値とDBのカラムの値の変換処理を定義します。
                                /// </summary>
                                /// <param name="configurationBuilder"></param>
                                public virtual void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
                                }
                        """)}}
                            }
                        }
                        """,
                });
            });
        }

        internal string[] GetValueObjectClassNames() {
            lock (_lock) {
                return _valueObjectClassNames.ToArray();
            }
        }

        internal string[] GetCharacterTypes() {
            lock (_lock) {
                return _characterTypes.OrderBy(x => x).ToArray();
            }
        }

        internal bool ShouldRenderDbContextCustomizationMethods() {
            lock (_lock) {
                return _renderDbContextCustomizationMethods;
            }
        }

        internal static string GetCharacterTypeMethodName(string characterType) {
            return "CheckIfStringIs" + characterType
                .Replace(" ", string.Empty)
                .Replace("　", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("（", string.Empty)
                .Replace("）", string.Empty)
                .Replace("-", string.Empty)
                .Replace("ー", string.Empty);
        }
    }
}

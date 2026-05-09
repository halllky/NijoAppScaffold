using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// 旧版互換モードの EFCoreDbContext.cs を集約横断で構築する。
    /// </summary>
    internal class LegacyDbContextClass : IMultiAggregateSourceFile {
        private readonly List<string> _valueObjectClassNames = [];
        private readonly Lock _lock = new();

        internal LegacyDbContextClass AddValueObject(string className) {
            lock (_lock) {
                if (!_valueObjectClassNames.Contains(className)) {
                    _valueObjectClassNames.Add(className);
                }
                return this;
            }
        }

        internal string[] GetValueObjectClassNames() {
            lock (_lock) {
                return _valueObjectClassNames.ToArray();
            }
        }

        void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
            ctx.Use<LegacyDefaultConfiguration>().EnableDbContextCustomizationMethods();
        }

        void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
            ctx.CoreLibrary(dir => {
                dir.Directory("EntityFramework", efcoreDir => {
                    efcoreDir.Generate(new SourceFile {
                        FileName = "EFCoreDbContext.cs",
                        Contents = $$"""
                            using Microsoft.EntityFrameworkCore;
                            using Microsoft.Extensions.Logging;

                            namespace {{ctx.Config.RootNamespace}} {

                                /// <summary>
                                /// DBコンテキスト。データベース全体と対応する抽象。
                                /// 詳しくは Entity Framework Core で調べてください。
                                /// </summary>
                                public partial class {{ctx.Config.DbContextName}} : DbContext {
                            #pragma warning disable CS8618 // DbSetはEFCore側で自動的に設定されるため問題なし
                                    public {{ctx.Config.DbContextName}}(DbContextOptions<{{ctx.Config.DbContextName}}> options, DefaultConfiguration nijoConfig, NLog.Logger logger) : base(options) {
                                        _nijoConfig = nijoConfig;
                                        _logger = logger;
                                    }
                            #pragma warning restore CS8618 // DbSetはEFCore側で自動的に設定されるため問題なし

                                    private readonly DefaultConfiguration _nijoConfig;
                                    private readonly NLog.Logger _logger;


                                    /// <summary>
                                    /// ログテーブル。このDbSetを直に参照する使い方は想定されていない。
                                    /// ログ出力はアプリケーションサービスのログプロパティ経由で行う想定
                                    /// </summary>
                                    public DbSet<LogEntity> LogEntity { get; set; }

                                    /// <inheritdoc />
                                    protected override void OnModelCreating(ModelBuilder modelBuilder) {
                                        try {
                                            // 集約ごとのモデル定義

                                            // モデル定義に変更を加えたい場合は CustomizedConfiguration クラスでこのメソッドをオーバーライドしてください。
                                            _nijoConfig.OnModelCreating(modelBuilder);

                                        } catch (Exception ex) {
                                            _logger.Error(ex);
                                            throw;
                                        }
                                    }

                                    /// <inheritdoc/>
                                    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
                                        _nijoConfig.ConfigureConventions(configurationBuilder);
                                    }

                                    /// <inheritdoc />
                                    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
                                        optionsBuilder.LogTo(sql => {
                                            _logger.Debug(sql);
                                            if (OutSqlToVisualStudio) {
                                                System.Diagnostics.Debug.WriteLine("---------------------");
                                                System.Diagnostics.Debug.WriteLine(sql);
                                            }
                                        },
                                        LogLevel.Information,
                                        Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.SingleLine);
                                    }
                                    /// <summary>デバッグ用</summary>
                                    public static bool OutSqlToVisualStudio { get; set; } = false;
                                }

                            }
                            """,
                    });
                });
            });
        }
    }
}

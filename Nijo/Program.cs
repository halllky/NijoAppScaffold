using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nijo.Util.DotnetEx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using Nijo.SchemaParsing;
using System.Xml.Linq;
using Nijo.Models.DataModelModules;
using Nijo.Models.QueryModelModules;
using Nijo.ImmutableSchema;
using Nijo.CodeGenerating;
using Nijo.Models;
using System.CommandLine.Invocation;
using System.CommandLine.Binding;

[assembly: InternalsVisibleTo("Nijo.IntegrationTest")]

namespace Nijo {
    public class Program {

        static async Task<int> Main(string[] args) {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, e) => {
                cancellationTokenSource.Cancel();

                // キャンセル時のリソース解放を適切に行うために既定の動作（アプリケーション終了）を殺す
                e.Cancel = true;
            };

            var rootCommand = DefineCommand();

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()

                // 例外処理
                .UseExceptionHandler((ex, _) => {
                    if (ex is OperationCanceledException) {
                        Console.Error.WriteLine("キャンセルされました。");
                    } else {
                        cancellationTokenSource.Cancel();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine(ex.ToString());
                        Console.ResetColor();
                    }
                })
                .Build();

            return await parser.InvokeAsync(args);
        }

        private static RootCommand DefineCommand() {
            var rootCommand = new RootCommand("nijo");

            // ---------------------------------------------------
            // ** 引数定義 **

            // プロジェクト相対パス
            var path = new Argument<string?>(
                name: "project path",
                getDefaultValue: () => string.Empty,
                description: "カレントディレクトリから操作対象のnijoプロジェクトへの相対パス");

            // ビルドスキップ
            var noBuild = new Option<bool>(
                ["-n", "--no-build"],
                description: "デバッグ開始時にコード自動生成をせず、アプリケーションの起動のみ行います。");

            // デバッグ実行時、ブラウザを立ち上げない
            var noBrowser = new Option<bool>(
                ["-b", "--no-browser"],
                description: "デバッグ開始時にブラウザを立ち上げません。");

            // 未実装を許可
            var allowNotImplemented = new Option<bool>(
                ["-a", "--allow-not-implemented"],
                description: "QueryModelのデータ構造定義などの必ず実装しなければならないメソッドは通常abstractでレンダリングされるが、コンパイルエラーの確認などのためにあえてvirtualでレンダリングする。");

            // デバッグ実行キャンセルファイル
            var cancelFile = new Option<string?>(
                ["-c", "--cancel-file"],
                description: "デバッグ実行の終了のトリガーは、通常はユーザーからのキー入力ですが、これを指定したときはこのファイルが存在したら終了と判定します。");

            // GUI用のサービスが実行されるURL
            var url = new Option<string?>(
                ["-u", "--url"],
                description: "GUI用のサービスが実行されるURLを明示的に指定します。");

            // ---------------------------------------------------
            // ** コマンド **

            // 新規プロジェクト作成
            var newProject = new Command(
                name: "new",
                description: "新規プロジェクトを作成します。")
                { path };
            newProject.SetHandler(NewProject(path));
            rootCommand.AddCommand(newProject);

            // 検証
            var validate = new Command(
                name: "validate",
                description: "スキーマ定義の検証を行ないます。")
                { path };
            validate.SetHandler(Validate(path));
            rootCommand.AddCommand(validate);

            // コード自動生成
            var generate = new Command(
                name: "generate",
                description: "ソースコードの自動生成を実行します。")
                { path, allowNotImplemented };
            generate.SetHandler(Generate(path, allowNotImplemented));
            rootCommand.AddCommand(generate);

            // デバッグ実行開始
            var run = new Command(
                name: "run",
                description: "プロジェクトのデバッグを開始します。")
                { path, noBuild, noBrowser, allowNotImplemented, cancelFile };
            run.SetHandler(Run, path, noBuild, noBrowser, allowNotImplemented, cancelFile);
            rootCommand.AddCommand(run);

            // スキーマダンプ
            var dump = new Command(
                name: "dump",
                description: "スキーマ定義とプロパティパスの情報をMarkdown形式で出力します。")
                { path };
            dump.SetHandler(Dump, path);
            rootCommand.AddCommand(dump);

            // GUI用のサービスを展開する
            var serve = new Command(
                name: "serve",
                description: "GUI用のサービスを展開します。")
                { path, url, noBrowser };
            serve.SetHandler(Serve, path, url, noBrowser);
            rootCommand.AddCommand(serve);

            // リファレンスドキュメント生成
            var outOption = new Option<string>(
                ["-o", "--out"],
                description: "出力先ディレクトリのパス");
            var generateReference = new Command(
                name: "generate-reference",
                description: "各モデルで利用可能なNodeOptionのHelpTextを.mdファイルとして出力します。")
                { outOption };
            generateReference.SetHandler(GenerateReference, outOption);
            rootCommand.AddCommand(generateReference);

            return rootCommand;
        }


        /// <summary>
        /// 新規プロジェクトを作成します。
        /// </summary>
        /// <param name="argPath">対象フォルダまでの相対パス</param>
        private static Action<InvocationContext> NewProject(Argument<string?> argPath) {

            return context => {
                try {
                    var path = GetValueForHandlerParameter(argPath, context);

                    var projectRoot = path == null
                        ? Directory.GetCurrentDirectory()
                        : Path.Combine(Directory.GetCurrentDirectory(), path);
                    var logger = ILoggerExtension.CreateConsoleLogger();

                    if (Directory.Exists(projectRoot)) {
                        logger.LogError("既にプロジェクトが存在します: {projectRoot}", projectRoot);
                        context.ExitCode = 1;
                        return;
                    }

                    var (success, errorMessage) = GeneratedProject.CreatePhysicalProjectAndInstallDependenciesAsync(projectRoot, logger);

                    if (success) {
                        logger.LogInformation("プロジェクトの作成が完了しました: {projectRoot}", projectRoot);
                        context.ExitCode = 0;
                    } else {
                        logger.LogError(errorMessage ?? "プロジェクトの作成に失敗しました。");
                        // 作成途中のディレクトリが残っている可能性があるので削除を試みる
                        if (Directory.Exists(projectRoot)) {
                            try {
                                Directory.Delete(projectRoot, recursive: true);
                            } catch (Exception ex) {
                                logger.LogWarning($"作成失敗したプロジェクトディレクトリの削除に失敗しました: {projectRoot}, {ex.Message}");
                            }
                        }
                        context.ExitCode = 1;
                    }
                } catch {
                    context.ExitCode = 1;
                    throw;
                }
            };
        }


        /// <summary>
        /// スキーマ定義の検証を行ないます。
        /// </summary>
        /// <param name="argPath">対象フォルダまでの相対パス</param>
        private static Action<InvocationContext> Validate(Argument<string?> argPath) {

            return context => {
                try {
                    var path = GetValueForHandlerParameter(argPath, context);

                    var projectRoot = path == null
                        ? Directory.GetCurrentDirectory()
                        : Path.Combine(Directory.GetCurrentDirectory(), path);
                    var logger = ILoggerExtension.CreateConsoleLogger();

                    if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                        logger.LogError(error);
                        context.ExitCode = 1;
                        return;
                    }
                    var rule = SchemaParseRule.Default();
                    var xDocument = XDocument.Load(project.SchemaXmlPath);
                    var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));

                    if (!project.ValidateSchema(parseContext, logger)) {
                        context.ExitCode = 1;
                        return;
                    }
                    context.ExitCode = 0;
                } catch {
                    context.ExitCode = 1;
                    throw;
                }
            };
        }


        /// <summary>
        /// ソースコードの自動生成を実行します。
        /// </summary>
        /// <param name="path">対象フォルダまでの相対パス</param>
        /// <param name="allowNotImplemented">抽象メソッドをabstractでなくvirtualで生成</param>
        private static Action<InvocationContext> Generate(Argument<string?> argPath, Option<bool> optAllowNotImplemented) {

            return context => {
                try {
                    var path = GetValueForHandlerParameter(argPath, context);
                    var allowNotImplemented = GetValueForHandlerParameter(optAllowNotImplemented, context);

                    var projectRoot = path == null
                        ? Directory.GetCurrentDirectory()
                        : Path.Combine(Directory.GetCurrentDirectory(), path);
                    var logger = ILoggerExtension.CreateConsoleLogger();

                    if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                        logger.LogError(error);
                        context.ExitCode = 1;
                        return;
                    }
                    var rule = SchemaParseRule.Default();
                    var xDocument = XDocument.Load(project.SchemaXmlPath);
                    var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));
                    var renderingOptions = new CodeRenderingOptions {
                        AllowNotImplemented = allowNotImplemented,
                    };

                    if (project.GenerateCode(parseContext, renderingOptions, logger)) {
                        context.ExitCode = 0;
                    } else {
                        context.ExitCode = 1;
                    }
                } catch {
                    context.ExitCode = 1;
                    throw;
                }
            };
        }


        /// <summary>
        /// 対象プロジェクトのデバッグ実行を開始します。
        /// </summary>
        /// <param name="path">対象フォルダまでの相対パス</param>
        /// <param name="noBuild">ソースコードの自動生成をスキップする場合はtrue</param>
        /// <param name="noBrowser">デバッグ開始時にブラウザを立ち上げない</param>
        /// <param name="allowNotImplemented">抽象メソッドをabstractでなくvirtualで生成</param>
        /// <param name="cancelFile">デバッグ実行を終了するトリガー。このファイルが存在したら終了する。</param>
        private static async Task Run(string? path, bool noBuild, bool noBrowser, bool allowNotImplemented, string? cancelFile) {
            var projectRoot = path == null
                ? Directory.GetCurrentDirectory()
                : Path.Combine(Directory.GetCurrentDirectory(), path);
            var cancelFileFullPath = cancelFile == null
                ? null
                : Path.GetFullPath(cancelFile);
            var logger = ILoggerExtension.CreateConsoleLogger();

            if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                logger.LogError(error);
                return;
            }

            var firstLaunch = true;
            while (true) {
                logger.LogInformation("-----------------------------------------------");
                if (cancelFileFullPath == null) {
                    logger.LogInformation("デバッグを開始します。キーボードのQで終了します。それ以外のキーでリビルドします。");
                } else {
                    logger.LogInformation("デバッグを開始します。右記パスにファイルが存在したら終了します: {cancelFile}", cancelFileFullPath);
                }

                var config = project.GetConfig();
                using var launcher = new Runtime.GeneratedProjectLauncher(
                    project.WebapiProjectRoot,
                    Path.Combine(project.ReactProjectRoot, ".."),
                    new Uri(config.DotnetDebuggingUrl),
                    new Uri(config.ReactDebuggingUrl),
                    logger);
                try {
                    if (!noBuild) {
                        var rule = SchemaParseRule.Default();
                        var xDocument = XDocument.Load(project.SchemaXmlPath);
                        var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));
                        var renderingOptions = new CodeRenderingOptions {
                            AllowNotImplemented = allowNotImplemented,
                        };

                        project.GenerateCode(parseContext, renderingOptions, logger);
                    }

                    launcher.Launch();
                    launcher.WaitForReady();

                    // 初回ビルド時はブラウザ立ち上げ
                    if (firstLaunch && !noBrowser) {
                        try {
                            var launchBrowser = new Process();
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                                launchBrowser.StartInfo.FileName = "cmd";
                                launchBrowser.StartInfo.Arguments = $"/c \"start {config.ReactDebuggingUrl}\"";
                            } else {
                                launchBrowser.StartInfo.FileName = "open";
                                launchBrowser.StartInfo.Arguments = config.ReactDebuggingUrl;
                            }
                            launchBrowser.Start();
                            launchBrowser.WaitForExit();
                        } catch (Exception ex) {
                            logger.LogError("Fail to launch browser: {msg}", ex.Message);
                        }
                        firstLaunch = false;
                    }
                } catch (Exception ex) {
                    logger.LogError("{msg}", ex.ToString());
                }

                // 待機。breakで終了。continueでリビルド
                if (cancelFileFullPath == null) {
                    // キー入力待機
                    var input = Console.ReadKey(true);
                    if (input.Key == ConsoleKey.Q) break;

                } else {
                    // キャンセルファイル監視
                    while (!File.Exists(cancelFileFullPath)) {
                        await Task.Delay(500);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// スキーマ定義とプロパティパスの情報をMarkdown形式で出力します。
        /// </summary>
        /// <param name="path">対象フォルダまでの相対パス</param>
        private static void Dump(string? path) {
            var projectRoot = path == null
                ? Directory.GetCurrentDirectory()
                : Path.Combine(Directory.GetCurrentDirectory(), path);
            var logger = ILoggerExtension.CreateConsoleLogger();

            if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                logger.LogError(error);
                return;
            }

            var rule = SchemaParseRule.Default();
            var xDocument = XDocument.Load(project.SchemaXmlPath);
            var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));

            // TryBuildSchemaメソッドを使用してApplicationSchemaのインスタンスを生成
            if (parseContext.TryBuildSchema(xDocument, out var appSchema, out var errors)) {
                // ApplicationSchemaクラスのGenerateMarkdownDumpメソッドを使用
                var markdownContent = appSchema.GenerateMarkdownDump();

                // 標準出力に出力
                Console.WriteLine(markdownContent);
            } else {
                logger.LogError("スキーマのビルドに失敗したため、ダンプを生成できませんでした。");
                foreach (var err in errors) {
                    var xmlPath = err.XElement
                        .AncestorsAndSelf()
                        .Reverse()
                        .Skip(1)
                        .Select(el => el.Name.LocalName)
                        .Join("/");

                    var errorMessages = err.OwnErrors
                        .Concat(err.AttributeErrors.SelectMany(x => x.Value, (p, v) => $"[{p.Key}] {v}"))
                        .ToArray();
                    var summary = errorMessages.Length >= 2
                        ? $"{errorMessages.Length}件のエラー（{errorMessages.Join(", ")}）"
                        : errorMessages.Single();
                    logger.LogError("  * {xmlPath}: {summary}", xmlPath, summary);
                }
            }
        }

        /// <summary>
        /// Nijo自体のドキュメント生成
        /// </summary>
        private static void GenerateReference(string outPath) {
            var logger = ILoggerExtension.CreateConsoleLogger();
            var rule = SchemaParseRule.Default();

            // 出力ディレクトリが存在しない場合は作成（親ディレクトリも含めて作成）
            var outDirFullPath = Path.Combine(Directory.GetCurrentDirectory(), outPath);
            if (!Directory.Exists(outDirFullPath)) {
                Directory.CreateDirectory(outDirFullPath);
                logger.LogInformation("出力ディレクトリを作成しました: {outPath}", outDirFullPath);
            } else {
                // 既存のディレクトリ内の古い.mdファイルを削除
                var existingFiles = Directory.GetFiles(outDirFullPath, "*.md");
                foreach (var file in existingFiles) {
                    File.Delete(file);
                    logger.LogInformation("古いファイルを削除しました: {file}", file);
                }
            }

            // 値メンバー型のドキュメントを生成
            var schemaContext = new SchemaParseContext(new XDocument(), rule, GeneratedProjectOptions.Parse(null, true));
            var valueMemberTypes = schemaContext.GetValueMemberTypes().ToArray();
            var valueMemberTypesPath = Path.Combine(outDirFullPath, ValueObjectTypesMd.FILE_NAME_WITHOUT_EXT + ".md");
            File.WriteAllText(valueMemberTypesPath, ValueObjectTypesMd.Render(valueMemberTypes), new UTF8Encoding(false, false));
            logger.LogInformation("ValueMemberTypes.mdファイルを生成しました: {valueMemberTypesPath}", valueMemberTypesPath);

            // CLIのドキュメントを生成
            var rootCommand = DefineCommand();
            var cliDocPath = Path.Combine(outDirFullPath, CliDocumentMd.FILE_NAME);
            File.WriteAllText(cliDocPath, CliDocumentMd.Render(rootCommand), new UTF8Encoding(false, false));
            logger.LogInformation("CLI.mdファイルを生成しました: {cliDocPath}", cliDocPath);

            logger.LogInformation("リファレンスドキュメントの生成が完了しました。");
        }

        /// <summary>
        /// GUI用のサービスを展開する
        /// </summary>
        private static async Task Serve(string? path, string? optUrl, bool noBrowser) {
            var logger = ILoggerExtension.CreateConsoleLogger();

            // サービス内容定義
            var nijoUi = new WebService.NijoWebServiceBuilder();
            var app = nijoUi.BuildWebApplication(logger);

            // 起動
            var url = optUrl ?? $"http://localhost:5000";
            logger.LogInformation("GUI用のサービスを起動します: {url}", url);

            // ブラウザを立ち上げる
            if (!noBrowser) {
                string browserUrl;
                if (string.IsNullOrWhiteSpace(path)) {
                    browserUrl = url;
                } else {
                    var param = System.Web.HttpUtility.ParseQueryString(string.Empty);
                    param.Add(WebService.Common.ProjectHelper.PROJECT_DIR_PARAMETER, path);
                    browserUrl = $"{url}/?{param}";
                }

                app.Lifetime.ApplicationStarted.Register(() => {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        Process.Start(new ProcessStartInfo {
                            FileName = "cmd",
                            Arguments = $"/c \"start {browserUrl}\"",
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        });
                    } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                        Process.Start(new ProcessStartInfo {
                            FileName = "open",
                            Arguments = browserUrl,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                        });
                    } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BROWSER"))) {
                        Process.Start(new ProcessStartInfo {
                            FileName = Environment.GetEnvironmentVariable("BROWSER")!,
                            Arguments = browserUrl,
                            UseShellExecute = false,
                        });
                    } else {
                        Console.Error.WriteLine(
                            $"このOSではブラウザを自動起動できません。" +
                            $"手動で次のURLを開いてください: {browserUrl}");
                    }
                });
            }

            await app.RunAsync(url);
        }


        /// <summary>
        /// CommandLineParser ライブラリのための補助メソッド。
        /// ハンドラパラメーターに対応する値を取得します。
        /// </summary>
        private static T? GetValueForHandlerParameter<T>(IValueDescriptor<T> symbol, InvocationContext context) {
            if (symbol is IValueSource source && source.TryGetValue(symbol, context.BindingContext, out var ret) && ret is T value)
                return value;

            return symbol switch {
                Argument<T> argument => context.ParseResult.GetValueForArgument(argument),
                Option<T> option => context.ParseResult.GetValueForOption(option),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}

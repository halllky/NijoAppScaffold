using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nijo.Util.DotnetEx;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Nijo.Reference;
using Nijo.SchemaParsing;
using System.Xml.Linq;
using Nijo.CodeGenerating;
using Nijo.WebService.Previewing;

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

            try {
                return await DefineCommand()
                    .Parse(args)
                    .InvokeAsync(new InvocationConfiguration {
                        EnableDefaultExceptionHandler = false,
                    }, cancellationTokenSource.Token);

            } catch (OperationCanceledException) {
                Console.Error.WriteLine("キャンセルされました。");
                return 1;
            } catch (Exception ex) {
                cancellationTokenSource.Cancel();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(ex.ToString());
                Console.ResetColor();
                return 1;
            }
        }

        private static RootCommand DefineCommand() {
            var rootCommand = new RootCommand("nijo");

            // ---------------------------------------------------
            // ** 引数定義 **

            // プロジェクト相対パス
            var path = new Argument<string?>("project path") {
                DefaultValueFactory = _ => string.Empty,
                Description = "カレントディレクトリから操作対象のnijoプロジェクトへの相対パス",
            };

            // nijo serve の編集対象フォルダ
            var serveTarget = new Argument<string?>("project path") {
                DefaultValueFactory = _ => string.Empty,
                Description = "カレントディレクトリから編集対象フォルダへの相対パス。未指定の場合はカレントディレクトリが編集対象になる。nijoプロジェクトでないフォルダを指定した場合はブラウザ上でそのフォルダの直下にプロジェクトを新規作成できる。",
            };

            // GUIブラウザを立ち上げない
            var noBrowser = new Option<bool>("--no-browser", "-b") {
                Description = "GUIブラウザを立ち上げません。",
            };

            // 未実装を許可
            var allowNotImplemented = new Option<bool>("--allow-not-implemented", "-a") {
                Description = "IsQueryable 属性が指定されたデータ構造の検索処理などの必ず実装しなければならないメソッドは通常abstractでレンダリングされるが、コンパイルエラーの確認などのためにあえてvirtualでレンダリングする。",
            };

            // GUI用のサービスが実行されるURL
            var url = new Option<string?>("--url", "-u") {
                Description = "GUI用のサービスが実行されるURLを明示的に指定します。",
            };

            // 新規プロジェクト作成時のテンプレート
            var template = new Option<string?>("--template", "-t") {
                Description = "新規プロジェクト作成時に使用するテンプレート名。"
                    + $"指定可能な値: {string.Join(", ", GeneratedProject.Templates.Keys)}",
            };

            // ---------------------------------------------------
            // ** コマンド **

            // 新規プロジェクト作成
            var newProject = new Command("new", "新規プロジェクトを作成します。") { path, template };
            newProject.SetAction(NewProject(path, template));
            rootCommand.Add(newProject);

            // 検証
            var validate = new Command("validate", "スキーマ定義の検証を行ないます。") { path };
            validate.SetAction(Validate(path));
            rootCommand.Add(validate);

            // コード自動生成
            var generate = new Command("generate", "ソースコードの自動生成を実行します。") { path, allowNotImplemented };
            generate.SetAction(Generate(path, allowNotImplemented));
            rootCommand.Add(generate);

            // GUI用のサービスを展開する
            var serve = new Command("serve", "GUI用のサービスを展開します。") { serveTarget, url, noBrowser };
            serve.SetAction((parseResult, ct) => Serve(
                parseResult.GetValue(serveTarget),
                parseResult.GetValue(url),
                parseResult.GetValue(noBrowser),
                ct));
            rootCommand.Add(serve);

            // リファレンスドキュメント生成
            var outOption = new Option<string>("--out", "-o") {
                Description = "出力先ディレクトリのパス",
                Required = true,
            };
            var generateReference = new Command("generate-reference", "値メンバーの型とコマンドラインAPIのリファレンスを.mdファイルとして出力します。") { outOption };
            generateReference.SetAction(parseResult => GenerateReference(parseResult.GetValue(outOption)!));
            rootCommand.Add(generateReference);

            return rootCommand;
        }


        /// <summary>
        /// 新規プロジェクトを作成します。
        /// </summary>
        /// <param name="argPath">対象フォルダまでの相対パス</param>
        /// <param name="optTemplate">使用するテンプレート名</param>
        private static Func<ParseResult, int> NewProject(Argument<string?> argPath, Option<string?> optTemplate) {
            return parseResult => {
                var path = parseResult.GetValue(argPath);
                var templateName = parseResult.GetValue(optTemplate);
                var logger = ILoggerExtension.CreateConsoleLogger();

                if (templateName == null) {
                    logger.LogError("テンプレートが指定されていません。 --template オプションで以下のいずれかを指定してください。");
                    foreach (var (name, (_, description)) in GeneratedProject.Templates) {
                        logger.LogInformation("  * {name}: {description}", name, description);
                    }
                    return 1;
                }

                var projectRoot = path == null
                    ? Directory.GetCurrentDirectory()
                    : Path.Combine(Directory.GetCurrentDirectory(), path);

                var (success, errorMessage) = GeneratedProject.CreateProjectFromTemplate(projectRoot, templateName);

                if (success) {
                    logger.LogInformation("プロジェクトの作成が完了しました: {projectRoot}", projectRoot);
                    return 0;
                } else {
                    logger.LogError(errorMessage ?? "プロジェクトの作成に失敗しました。");
                    return 1;
                }
            };
        }


        /// <summary>
        /// スキーマ定義の検証を行ないます。
        /// </summary>
        /// <param name="argPath">対象フォルダまでの相対パス</param>
        private static Func<ParseResult, int> Validate(Argument<string?> argPath) {
            return parseResult => {
                var path = parseResult.GetValue(argPath);

                var projectRoot = path == null
                    ? Directory.GetCurrentDirectory()
                    : Path.Combine(Directory.GetCurrentDirectory(), path);
                var logger = ILoggerExtension.CreateConsoleLogger();

                if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                    logger.LogError(error);
                    return 1;
                }
                var rule = SchemaParseRule.Default();
                var xDocument = XDocument.Load(project.SchemaXmlPath);
                var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));

                return project.ValidateSchema(parseContext, logger) ? 0 : 1;
            };
        }


        /// <summary>
        /// ソースコードの自動生成を実行します。
        /// </summary>
        /// <param name="path">対象フォルダまでの相対パス</param>
        /// <param name="allowNotImplemented">抽象メソッドをabstractでなくvirtualで生成</param>
        private static Func<ParseResult, int> Generate(Argument<string?> argPath, Option<bool> optAllowNotImplemented) {
            return parseResult => {
                var path = parseResult.GetValue(argPath);
                var allowNotImplemented = parseResult.GetValue(optAllowNotImplemented);

                var projectRoot = path == null
                    ? Directory.GetCurrentDirectory()
                    : Path.Combine(Directory.GetCurrentDirectory(), path);
                var logger = ILoggerExtension.CreateConsoleLogger();

                if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error)) {
                    logger.LogError(error);
                    return 1;
                }
                var rule = SchemaParseRule.Default();
                var xDocument = XDocument.Load(project.SchemaXmlPath);
                var parseContext = new SchemaParseContext(xDocument, rule, GeneratedProjectOptions.Parse(xDocument, true));
                var renderingOptions = new CodeRenderingOptions {
                    AllowNotImplemented = allowNotImplemented,
                };

                return project.GenerateCode(parseContext, renderingOptions, logger, out _) ? 0 : 1;
            };
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
            var valueMemberTypesPath = Path.Combine(outDirFullPath, ValueMemberTypesMd.FILE_NAME_WITHOUT_EXT + ".md");
            File.WriteAllText(valueMemberTypesPath, ValueMemberTypesMd.Render(rule.ValueMemberTypes), new UTF8Encoding(false, false));
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
        /// <param name="path">カレントディレクトリから編集対象フォルダへの相対パス</param>
        private static async Task<int> Serve(string? path, string? optUrl, bool noBrowser, CancellationToken cancellationToken) {
            var logger = ILoggerExtension.CreateConsoleLogger();

            var projectRoot = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path ?? string.Empty)));
            if (File.Exists(projectRoot)) {
                logger.LogError("編集対象にはフォルダを指定してください。指定されたパスはファイルです: {projectRoot}", projectRoot);
                return 1;
            }

            // サービス内容定義
            using var nijoWebService = new WebService.NijoWebService(projectRoot);
            var app = nijoWebService.BuildWebApplication(logger);

            // 起動
            var url = optUrl ?? $"http://localhost:5000";
            logger.LogInformation("GUI用のサービスを起動します: {url}", url);
            logger.LogInformation("編集対象フォルダ: {projectRoot}", projectRoot);

            app.Lifetime.ApplicationStarted.Register(() => {
                // nijo.preview.json で自動起動が有効ならプレビューを開始する
                if (GeneratedProject.TryOpen(projectRoot, out var project, out _)) {
                    var previewSetting = PreviewSetting.Load(project);
                    if (previewSetting.StartOnNijoServe) {
                        nijoWebService.Preview.Start(previewSetting, logger);
                    }
                }

                // ブラウザを立ち上げる
                if (!noBrowser) {
                    ProcessExtension.OpenBrowser(url);
                }
            });

            // アプリケーション終了時（Ctrl+Cを含む）は稼働中のプレビュープロセスを確実に停止する
            app.Lifetime.ApplicationStopping.Register(() => nijoWebService.Preview.Stop(logger));

            using var registration = cancellationToken.Register(() => app.Lifetime.StopApplication());
            await app.RunAsync(url);
            return 0;
        }
    }
}

using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Nijo.IntegrationTest;

/// <summary>
/// テスト実行1回分のコンテキスト情報。
/// <para>
/// 実行1回ごとにワークフォルダが作成され、
/// その中にテストケース単位のサブフォルダが作成される。
/// サブフォルダ内に、 nijo.xml と、自動生成されたコードが出力される。
/// </para>
/// <para>
/// dotnet や npm のコマンドラインツールによる操作も提供する。
/// </para>
/// </summary>
[SetUpFixture]
public class NijoTestUtil {

#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。
    /// <summary>
    /// テスト実行1回ごとに作成されるワークフォルダのパス。
    /// </summary>
    public static string BaseDirectory { get; private set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。'required' 修飾子を追加するか、Null 許容として宣言することを検討してください。

    [OneTimeSetUp]
    public void Setup() {
        // Nijo用のワークフォルダを作成
        BaseDirectory = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, // net9.0
            $"Nijo.IntegrationTest.Log",
            $"テスト結果_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}"));

        // カレントディレクトリをワークフォルダに移動
        Directory.CreateDirectory(BaseDirectory);
        Directory.SetCurrentDirectory(BaseDirectory);
    }


    /// <summary>
    /// ワークフォルダ内部に、新しいNijoプロジェクトを作成して返します。
    /// </summary>
    /// <param name="nijoXmlContent">nijo.xml の内容。</param>
    public static async Task<GeneratedProjectWithTestUtil> CreateNewProjectAsync(string nijoXmlContent) {

        // テストケースごとのサブフォルダを作成
        var testName = TestContext.CurrentContext.Test.Name;
        var guid = Guid.NewGuid().ToString().Substring(0, 8);
        var projectRoot = Path.Combine(BaseDirectory, $"{testName}_{guid}");
        Directory.CreateDirectory(projectRoot);

        // nijo.xml を作成
        await File.WriteAllTextAsync(
            Path.Combine(projectRoot, "nijo.xml"),
            nijoXmlContent.ReplaceLineEndings("\n"),
            new UTF8Encoding(false));

        if (!GeneratedProject.TryOpen(projectRoot, out var project, out var errors)) {
            // ありえない
            throw new InvalidOperationException($"プロジェクトのオープンに失敗しました。エラー内容: {errors}");
        }
        return new GeneratedProjectWithTestUtil {
            Project = project,
            Logger = new FileLogger(Path.Combine(projectRoot, "RESULT.LOG")),
        };
    }

    /// <summary>
    /// Nijo プロジェクトを表すインスタンスと、
    /// そのテストユーティリティをまとめたクラス。
    /// </summary>
    public class GeneratedProjectWithTestUtil : IDisposable {
        /// <summary>
        /// Nijo プロジェクトのインスタンス。
        /// コード自動生成の実行などはこのインスタンスを通じて行う。
        /// </summary>
        public required GeneratedProject Project { get; init; }
        /// <summary>
        /// コード自動生成処理などの実行時には <see cref="ILogger"/> のインスタンスが必要になるが、
        /// その結果をテスト毎に作成されるワークフォルダに出力するためのもの。
        /// </summary>
        public required ILogger Logger { get; init; }

        void IDisposable.Dispose() {
            var result = TestContext.CurrentContext.Result;
            if (result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed) {
                Logger.LogError("TEST FAILED: {Message}\n{StackTrace}", result.Message, result.StackTrace);
            }
        }

        /// <summary>
        /// スキーマ検証エラーを列挙する。
        /// </summary>
        public async Task<SchemaParsing.SchemaParseContext.ValidationError[]> EnumerateValidationErrorsAsync() {
            try {
                using var reader = File.OpenRead(Project.SchemaXmlPath);
                var document = await XDocument.LoadAsync(reader, LoadOptions.None, CancellationToken.None);
                var rule = SchemaParsing.SchemaParseRule.Default();
                var parseContext = new SchemaParsing.SchemaParseContext(document, rule);

                parseContext.TryBuildSchema(document, out var _, out var errors);
                return errors;
            } catch (Exception ex) {
                Logger.LogError(ex, "EnumerateValidationErrorsAsyncで例外が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// <see cref="GeneratedProject.GenerateCode"/> の呼び出し方が少々煩雑なのでラップしたもの
        /// </summary>
        public async Task<bool> GenerateCodeAsync() {
            try {
                using var reader = File.OpenRead(Project.SchemaXmlPath);
                var document = await XDocument.LoadAsync(reader, LoadOptions.None, CancellationToken.None);
                var rule = SchemaParsing.SchemaParseRule.Default();
                var parseContext = new SchemaParsing.SchemaParseContext(document, rule);
                var renderingOptions = new CodeGenerating.CodeRenderingOptions {
                    AllowNotImplemented = false,
                };
                return Project.GenerateCode(parseContext, renderingOptions, Logger);
            } catch (Exception ex) {
                Logger.LogError(ex, "GenerateCodeAsyncで例外が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// コンパイルエラーが発生しないかを確認する。詳細な結果はログファイルに出力される。
        ///
        /// C#: dotnet build の実行
        /// TypeScript: npm run check の実行
        /// </summary>
        public async Task<bool> CheckCompileAsync() {
            try {
                // C# や TypeScript のビルド確認用のファイルを作成
                await File.WriteAllTextAsync(
                    Path.Combine(Project.ProjectRoot, "NijoGeneratedCode.csproj"),
                    DEFAULT_CSPROJ,
                    new UTF8Encoding(false));
                await File.WriteAllTextAsync(
                    Path.Combine(Project.ProjectRoot, "package.json"),
                    DEFAULT_PACKAGE_JSON,
                    new UTF8Encoding(false));
                await File.WriteAllTextAsync(
                    Path.Combine(Project.ProjectRoot, "tsconfig.json"),
                    DEFAULT_TSCONFIG_JSON,
                    new UTF8Encoding(false));

                var csLogPath = Path.Combine(Project.ProjectRoot, "RESULT_CS.log");
                var tsLogPath = Path.Combine(Project.ProjectRoot, "RESULT_TS.log");

                // 2つを並列実行して待機
                var results = await Task.WhenAll(
                    RunCommandAsync("dotnet", "build", csLogPath),
                    RunCommandAsync("npm", "run check", tsLogPath)
                );

                if (results.All(ok => ok)) {
                    return true;
                }

                Logger.LogError(
                    "コンパイル確認に失敗しました。C# ログ: {CsLogPath}, TypeScript ログ: {TsLogPath}",
                    csLogPath,
                    tsLogPath);
                return false;
            } catch (Exception ex) {
                Logger.LogError(ex, "CheckCompileAsyncで例外が発生しました。");
                throw;
            }
        }

        private async Task<bool> RunCommandAsync(string fileName, string arguments, string logFilePath) {
            var psi = new ProcessStartInfo {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = Project.ProjectRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using var process = Process.Start(psi);
            if (process == null) throw new InvalidOperationException($"プロセスを開始できませんでした: {fileName} {arguments}");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var sb = new StringBuilder();
            sb.AppendLine($"COMMAND: {fileName} {arguments}");
            sb.AppendLine($"EXIT_CODE: {process.ExitCode}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("STDOUT:");
            sb.AppendLine(stdout);
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("STDERR:");
            sb.AppendLine(stderr);

            await File.WriteAllTextAsync(logFilePath, sb.ToString(), new UTF8Encoding(false));

            if (process.ExitCode != 0) {
                var summary = SummarizeCommandFailure(stdout, stderr);
                Logger.LogError(
                    "Command failed: {FileName} {Arguments} (ExitCode: {ExitCode})\nLog: {LogFilePath}\nSummary:\n{Summary}",
                    fileName,
                    arguments,
                    process.ExitCode,
                    logFilePath,
                    summary);
            } else {
                Logger.LogInformation("Command success: {FileName} {Arguments}", fileName, arguments);
            }

            return process.ExitCode == 0;
        }

        private static string SummarizeCommandFailure(string stdout, string stderr) {
            var combined = $"{stdout}\n{stderr}";
            var lines = combined
                .Split('\n', StringSplitOptions.TrimEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            var errorLines = lines
                .Where(line => Regex.IsMatch(line, @"\berror\b|\bBuild FAILED\b|\bfailed\b", RegexOptions.IgnoreCase))
                .Distinct()
                .Take(12)
                .ToArray();

            if (errorLines.Length > 0) {
                return string.Join(Environment.NewLine, errorLines);
            }

            return string.Join(Environment.NewLine, lines.TakeLast(Math.Min(20, lines.Length)));
        }
    }

    /// <summary>
    /// ファイルにログを書き込む単純なロガー。
    /// </summary>
    private class FileLogger : ILogger {
        private readonly string _logFilePath;
        private readonly Lock _lock = new();

        public FileLogger(string logFilePath) {
            _logFilePath = logFilePath;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var logRecord = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] {message}";
            if (exception != null) {
                logRecord += Environment.NewLine + exception.ToString();
            }
            logRecord += Environment.NewLine;

            lock (_lock) {
                File.AppendAllText(_logFilePath, logRecord);
            }

            if (logLevel >= LogLevel.Error) {
                TestContext.Progress.Write(logRecord);
            }
        }
    }

    /// <summary>
    /// C# のコンパイルエラーを確認するための最低限の .csproj ファイルの内容。
    /// </summary>
    private const string DEFAULT_CSPROJ = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Library</OutputType>
            <TargetFramework>net9.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
            <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="9.*" />
            <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.*" />
          </ItemGroup>
        </Project>
        """;

    /// <summary>
    /// TypeScript のコンパイルエラーを確認するための最低限の package.json ファイルの内容。
    /// </summary>
    private const string DEFAULT_PACKAGE_JSON = """
        {
          "name": "nijo-generated-code-check",
          "private": true,
          "version": "0.0.0",
          "type": "module",
          "scripts": {
            "check": "tsc --noEmit"
          },
          "devDependencies": {
            "typescript": "^5.0.0"
          }
        }
        """;

    /// <summary>
    /// TypeScript のコンパイルエラーを確認するための最低限の tsconfig.json ファイルの内容。
    /// </summary>
    private const string DEFAULT_TSCONFIG_JSON = """
        {
          "compilerOptions": {
            "target": "ES2020",
            "useDefineForClassFields": true,
            "lib": ["ES2020", "DOM", "DOM.Iterable"],
            "module": "ESNext",
            "skipLibCheck": true,
            "moduleResolution": "bundler",
            "allowImportingTsExtensions": true,
            "resolveJsonModule": true,
            "isolatedModules": true,
            "noEmit": true,
            "jsx": "react-jsx",
            "strict": true,
            "noUnusedLocals": false,
            "noUnusedParameters": false,
            "noFallthroughCasesInSwitch": true
          },
          "include": ["**/*.ts", "**/*.tsx"],
          "exclude": ["node_modules"]
        }
        """;
}

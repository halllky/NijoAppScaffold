using System.Text;
using System.Xml.Linq;
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
    public class GeneratedProjectWithTestUtil {
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

        /// <summary>
        /// <see cref="GeneratedProject.GenerateCode"/> の呼び出し方が少々煩雑なのでラップしたもの
        /// </summary>
        public async Task<bool> GenerateCodeAsync() {
            using var reader = File.OpenRead(Project.SchemaXmlPath);
            var document = await XDocument.LoadAsync(reader, LoadOptions.None, CancellationToken.None);
            var rule = SchemaParsing.SchemaParseRule.Default();
            var parseContext = new SchemaParsing.SchemaParseContext(document, rule);
            var renderingOptions = new CodeGenerating.CodeRenderingOptions {
                AllowNotImplemented = false,
            };
            return Project.GenerateCode(parseContext, renderingOptions, Logger);
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
        }
    }
}

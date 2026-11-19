using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.UnitTest;

namespace MyApp;

/// <summary>
/// 1回のテスト全体のコンテキスト情報。
/// 一度に複数のテストを実行する場合で共通の情報を提供する。
/// 例えば、DB参照系のテストが複数あり、同じSQLiteファイルに対して問い合わせを行うとき、
/// それら複数のテストに同じSQLiteファイルを参照させるときに共通の情報を提供する。
/// </summary>
[SetUpFixture]
public class TestUtilImpl {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static TestUtilImpl Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void Setup() {
        Instance = new() {
            BaseWorkDirectory = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.WorkDirectory, // net10.0
                "..", // Debug
                "..", // bin
                "..", // UnitTest
                "..", // Demo200.sln があるフォルダ
                $"UnitTest.Log",
                $"テスト結果_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}"))
        };

        // カレントディレクトリをワークフォルダに移動
        Directory.CreateDirectory(Instance.BaseWorkDirectory);
        Directory.SetCurrentDirectory(Instance.BaseWorkDirectory);
    }

    [OneTimeTearDown]
    public void Dispose() {
        // テストケースごとに作った DI コンテナを破棄し、ログのファイルなどを閉じる。
        // 閉じないと、テストの実行が終わるまでファイルが開いたままになり、テストの結果のフォルダを消せないことがある
        while (_providers.TryTake(out var provider)) {
            provider.Dispose();
        }

        // ログの場所を標準出力に表示する
        TestContext.Out.WriteLine($$"""
            テストが実行されました。ログは以下の場所に出力されました。
            {{BaseWorkDirectory}}
            """);
    }

    /// <summary>
    /// このコンストラクタはNUnitのランナーまたはこのクラス内部でのみ呼ばれる想定です。
    /// 各テストケースからは <see cref="Instance"/> を参照してください。
    /// </summary>
    public TestUtilImpl() { }

    /// <summary>
    /// テスト実行全体で共有されるログのベースディレクトリ。
    /// テスト実行1回ごとに異なるフォルダが出力される。
    /// </summary>
    private string BaseWorkDirectory { get; set; } = "";

    /// <summary>テストケースごとに作った DI コンテナ。テストは並列に動くので、スレッドセーフな入れ物に控える</summary>
    private static readonly System.Collections.Concurrent.ConcurrentBag<ServiceProvider> _providers = [];

    /// <summary>
    /// テスト用の新しいサービスプロバイダのインスタンスを構成する。
    /// ワークフォルダとデータベースもテストケースごとに新しく作る。
    /// </summary>
    public TestScopeImpl CreateScope(string testCaseName, Action<IServiceCollection>? configureServices = null) {

        // ワークフォルダ作成
        var currentTestWorkDirectory = Path.Combine(BaseWorkDirectory, testCaseName);
        Directory.CreateDirectory(currentTestWorkDirectory);

        // DI機構
        var services = new ServiceCollection();
        var dbFilePath = Path.Combine(currentTestWorkDirectory, "UNITTEST.sqlite3");
        OverridedApplicationService.ConfigureServices(services, $"Data Source={dbFilePath};Pooling=False");

        // DI機構: ログ
        var logFilePath = Path.Combine(currentTestWorkDirectory, "result.log");
        services.AddLogging(logging => {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddProvider(new FileLoggerProvider(logFilePath));
        });

        // DI機構: テストケースごとのカスタマイズ
        configureServices?.Invoke(services);

        var provider = services.BuildServiceProvider();
        _providers.Add(provider);

        CreateDatabase(provider.GetRequiredService<OverridedDbContext>());

        return new TestScopeImpl(provider, currentTestWorkDirectory);
    }

    /// <summary>
    /// テーブルを Entity Framework Core のモデルから作り、続けて Core/Views フォルダのビューを作る。
    /// ビューは別のビューを参照することがあるので、ファイル名の昇順に作る。
    /// </summary>
    private void CreateDatabase(OverridedDbContext dbContext) {
        dbContext.Database.EnsureCreated();

        var viewsFolder = Path.Combine(
            BaseWorkDirectory,
            "..", // UnitTest.Log
            "..", // Demo200.sln があるフォルダ
            "Core",
            "Views");
        foreach (var sqlFile in Directory.GetFiles(viewsFolder, "*.sql").Order()) {
            dbContext.Database.ExecuteSqlRaw(File.ReadAllText(sqlFile, Encoding.UTF8));
        }
    }

    /// <summary>
    /// テスト用のファイルロガー。ログをテキストファイルに出力する。
    /// テストケースごとに異なるファイルに出力される想定。
    /// </summary>
    private sealed class FileLoggerProvider : ILoggerProvider {
        private readonly StreamWriter _writer;
        private readonly object _lock = new();
        private bool _disposed;

        public FileLoggerProvider(string filePath) {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            _writer = new StreamWriter(File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read)) {
                AutoFlush = true
            };
        }

        public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _writer, _lock);

        public void Dispose() {
            if (_disposed) return;
            _disposed = true;
            _writer.Dispose();
        }
    }

    /// <inheritdoc cref="FileLoggerProvider" />
    private sealed class FileLogger : ILogger {
        private readonly string _categoryName;
        private readonly StreamWriter _writer;
        private readonly object _lock;

        public FileLogger(string categoryName, StreamWriter writer, object writeLock) {
            _categoryName = categoryName;
            _writer = writer;
            _lock = writeLock;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            var message = formatter(state, exception);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {_categoryName} {message}";
            if (exception != null) {
                line += $"{Environment.NewLine}{exception}";
            }
            lock (_lock) {
                _writer.WriteLine(line);
            }
        }
    }

    private sealed class NullScope : IDisposable {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}

/// <summary>
/// ユニットテスト用のユーティリティクラス。
/// このインスタンスの生存期間は <see cref="OverridedApplicationService"/> のライフサイクル（webapiの場合はHTTPリクエスト1回分）に相当する。
/// </summary>
public class TestScopeImpl {
    internal TestScopeImpl(IServiceProvider serviceProvider, string workingDirectory) {
        ServiceProvider = serviceProvider;
        App = serviceProvider.GetRequiredService<OverridedApplicationService>();
        WorkDirectory = workingDirectory;
    }

    public IServiceProvider ServiceProvider { get; }
    public OverridedApplicationService App { get; }
    public string WorkDirectory { get; }
}

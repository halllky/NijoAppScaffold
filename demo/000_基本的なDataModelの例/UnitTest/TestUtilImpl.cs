using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MyApp.UnitTest;
using MyApp.Debugging;

namespace MyApp;

/// <summary>
/// 1回のテスト全体のコンテキスト情報。
/// 一度に複数のテストを実行する場合で共通の情報を提供する。
/// 例えば、DB参照系のテストが複数あり、同じSQLiteファイルに対して問い合わせを行うとき、
/// それら複数のテストに同じSQLiteファイルを参照させるときに共通の情報を提供する。
/// </summary>
[SetUpFixture]
public partial class TestUtilImpl {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static TestUtilImpl Instance { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    [OneTimeSetUp]
    public void Setup() {
        Instance = new() {
            BaseWorkDirectory = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.WorkDirectory, // net9.0
                "..", // Debug
                "..", // bin
                "..", // UnitTest
                "..", // MyApp.sln があるフォルダ
                $"UnitTest.Log",
                $"テスト結果_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}"))
        };

        // カレントディレクトリをワークフォルダに移動
        Directory.CreateDirectory(Instance.BaseWorkDirectory);
        Directory.SetCurrentDirectory(Instance.BaseWorkDirectory);
    }

    [OneTimeTearDown]
    public void Dispose() {
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

    /// <summary>
    /// テスト用の新しいサービスプロバイダのインスタンスを構成する。
    /// ワークフォルダの作成も行う。
    /// データベースの再作成もここで行なう。
    /// </summary>
    public TestScopeImpl CreateScope(string testCaseName, Action<IServiceCollection>? configureServices = null) {

        // ワークフォルダ作成
        var currentTestWorkDirectory = Path.Combine(BaseWorkDirectory, testCaseName);
        Directory.CreateDirectory(currentTestWorkDirectory);

        // appsettings.json はWebApiのそれを流用する
        var webapiDir = Path.Combine(
            BaseWorkDirectory,
            "..", // UnitTest.Log
            "..", // MyApp.sln があるフォルダ
            "WebApi");
        var appSettingsJson = Path.Combine(webapiDir, "appsettings.json");
        var developmentJson = Path.Combine(webapiDir, "appsettings.Development.json");
        if (File.Exists(appSettingsJson)) {
            File.Copy(appSettingsJson, Path.Combine(currentTestWorkDirectory, "appsettings.json"));
        }
        if (File.Exists(developmentJson)) {
            File.Copy(developmentJson, Path.Combine(currentTestWorkDirectory, "appsettings.Development.json"));
        }

        // DI機構
        var services = new ServiceCollection();
        OverridedApplicationService.ConfigureServices(services, currentTestWorkDirectory);
        ConfigureServicesスキーマ定義依存(services);

        // DI機構: ログ
        var logFilePath = Path.Combine(currentTestWorkDirectory, "result.log");
        services.AddLogging(logging => {
            logging.SetMinimumLevel(LogLevel.Trace);

            // コンソール出力
            logging.AddFilter<ConsoleLoggerProvider>((_, level) => level >= LogLevel.Warning);
            logging.AddConsole();

            // ファイル出力
            logging.AddFilter<FileLoggerProvider>((_, level) => level != LogLevel.None);
            logging.AddProvider(new FileLoggerProvider(logFilePath));
        });

        // DI機構: テストケースごとのカスタマイズ
        configureServices?.Invoke(services);

        // DI機構: 実行時設定
        services.AddScoped(provider => {
            var solutionRoot = Path.Combine(
                Instance.BaseWorkDirectory,
                "..",  // UnitTest.Log
                ".."); // MyApp.sln があるフォルダ

            var settings = new RuntimeSetting();

            // appsettings.json から読み取る設定を適用
            new ConfigurationBuilder()
                .SetBasePath(currentTestWorkDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true) // 後にAddされたファイルが優先される
                .Build()
                .GetSection(RuntimeSetting.MY_APP_SECTION)
                .Bind(settings);

            // ユニットテスト実行毎のログフォルダに出力されるべき項目の書き換え
            settings.LogDirectory = currentTestWorkDirectory; // テストケースごとのディレクトリ
            settings.CurrentDbProfileName = "SQLITE001";
            settings.MigrationsScriptFolder = Path.Combine(
                solutionRoot,
                "Core",
                "MigrationsScript");

            var dbFileName = $"./{testCaseName}/UNITTEST.sqlite3";
            settings.DbProfiles.Add(new() {
                Name = "SQLITE001",
                ConnStr = $"Data Source={dbFileName};Pooling=False",
            });
            return settings;
        });

        var provider = services.BuildServiceProvider();

        // DB作成
        var dbContext = provider.GetRequiredService<OverridedDbContext>();
        dbContext.EnsureCreatedAsyncEx(provider.GetRequiredService<RuntimeSetting>()).GetAwaiter().GetResult();

        return new TestScopeImpl(provider, currentTestWorkDirectory);
    }

    partial void ConfigureServicesスキーマ定義依存(IServiceCollection services);

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
    public AutoGeneratedApplicationService App { get; }
    public string WorkDirectory { get; }
}

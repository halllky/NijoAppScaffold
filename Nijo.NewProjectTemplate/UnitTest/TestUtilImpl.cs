using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
public class TestUtilImpl {

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

    public TestScopeImpl<TMessageRoot> CreateScope<TMessageRoot>(string testCaseName, Action<IServiceCollection>? configureServices = null, IPresentationContextOptions? options = null) where TMessageRoot : IMessageSetter {
        var (currentTestWorkDirectory, provider) = SetupEnvironments(testCaseName, configureServices);
        var messageRoot = MessageSetter.GetDefaultClass<TMessageRoot>([], new PresentationMessageContext());
        var contextOptions = options ?? new PresentationContextOptionsImpl();
        var presentationContext = new PresentationContextInUnitTest<TMessageRoot>(messageRoot, contextOptions);

        return new TestScopeImpl<TMessageRoot>(provider, presentationContext, currentTestWorkDirectory);
    }

    public TestScopeImpl CreateScope(string testCaseName, Action<IServiceCollection>? configureServices = null, IPresentationContextOptions? options = null) {
        return CreateScope(testCaseName, typeof(MessageSetter), configureServices, options);
    }

    public TestScopeImpl CreateScope(string testCaseName, Type messageRootType, Action<IServiceCollection>? configureServices = null, IPresentationContextOptions? options = null) {
        var (currentTestWorkDirectory, provider) = SetupEnvironments(testCaseName, configureServices);
        var contextOptions = options ?? new PresentationContextOptionsImpl();
        var presentationContext = new PresentationContextInUnitTest(messageRootType, contextOptions);

        return new TestScopeImpl(provider, presentationContext, currentTestWorkDirectory);
    }

    /// <summary>
    /// テスト用の新しいサービスプロバイダのインスタンスを構成する。
    /// ワークフォルダの作成も行う。
    /// データベースの再作成もここで行なう。
    /// </summary>
    private (string CurrentTestWorkDirectory, IServiceProvider Services) SetupEnvironments(string testCaseName, Action<IServiceCollection>? configureServices) {

        // ワークフォルダ作成
        var currentTestWorkDirectory = Path.Combine(BaseWorkDirectory, testCaseName);
        Directory.CreateDirectory(currentTestWorkDirectory);

        // DI機構
        var configure = new OverridedApplicationConfigureForTest();
        var services = new ServiceCollection();
        configure.ConfigureServices(services);

        // DI機構: テストケースごとのカスタマイズ
        configureServices?.Invoke(services);

        // DI機構: 実行時設定
        services.AddScoped(provider => {
            var solutionRoot = Path.Combine(
                Instance.BaseWorkDirectory,
                "..",  // UnitTest.Log
                ".."); // MyApp.sln があるフォルダ

            var settings = new RuntimeSetting();

            // appsettings.json から読み取る設定を適用。WebApiのそれを流用する
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Path.Combine(solutionRoot, "WebApi"))
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
        var dbContext = provider.GetRequiredService<MyDbContext>();
        dbContext.EnsureCreatedAsyncEx(provider.GetRequiredService<RuntimeSetting>()).GetAwaiter().GetResult();

        return (currentTestWorkDirectory, provider);
    }

    #region
    /// <summary>
    /// <see cref="OverridedApplicationConfigure"/> のうちユニットテストの時だけ変更したい初期設定処理を変更したもの
    /// </summary>
    public class OverridedApplicationConfigureForTest : OverridedApplicationConfigure {
        // ログファイル名、Webアプリケーションの方では日付毎などだが、テストの場合は毎回別のフォルダに出力されるので、決め打ち
        protected override string LogFileNameRule => "テスト中に出力されたログ.log";
    }
    /// <summary>
    /// <see cref="IPresentationContextOptions"/> のユニットテスト用の実装。
    /// </summary>
    public class PresentationContextOptionsImpl : IPresentationContextOptions {
        public bool IgnoreConfirm { get; init; }
    }
    #endregion
}

/// <summary>
/// ユニットテスト用のユーティリティクラス。
/// このインスタンスの生存期間は <see cref="OverridedApplicationService"/> のライフサイクル（webapiの場合はHTTPリクエスト1回分）に相当する。
/// </summary>
public class TestScopeImpl {
    internal TestScopeImpl(IServiceProvider serviceProvider, IPresentationContext presentationContext, string workingDirectory) {
        ServiceProvider = serviceProvider;
        App = serviceProvider.GetRequiredService<OverridedApplicationService>();
        PresentationContext = presentationContext;
        WorkDirectory = workingDirectory;
    }

    public IServiceProvider ServiceProvider { get; }
    public AutoGeneratedApplicationService App { get; }
    public IPresentationContext PresentationContext { get; }
    public string WorkDirectory { get; }
}

/// <inheritdoc cref="TestScopeImpl"/>
public class TestScopeImpl<TMessage> : TestScopeImpl where TMessage : IMessageSetter {
    internal TestScopeImpl(IServiceProvider serviceProvider, IPresentationContext<TMessage> presentationContext, string workDirectory)
        : base(serviceProvider, presentationContext, workDirectory) { }

    public new IPresentationContext<TMessage> PresentationContext => (IPresentationContext<TMessage>)base.PresentationContext;
}

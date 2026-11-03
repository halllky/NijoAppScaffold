using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class TestUtilImpl {

    partial void ConfigureServicesスキーマ定義依存(IServiceCollection services) {
        services.AddSingleton<ISessionKeyProvider, SessionKeyProviderInUnitTest>();
        services.AddSingleton<ICurrentProcess, CurrentProcessInUnitTest>();
    }

    /// <summary>
    /// 単体テストにおける <see cref="ICurrentProcess"/>。処理名は実行中のテストケースの名前とする。
    /// </summary>
    public class CurrentProcessInUnitTest : ICurrentProcess {
        public string Name => TestContext.CurrentContext.Test.Name;
    }


    public class SessionKeyProviderInUnitTest : ISessionKeyProvider {

        public const string SESSION_KEY = "xxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

        void ISessionKeyProvider.ClearSessionKey() {
            // テスト用なので何もしない
        }

        string? ISessionKeyProvider.GetSessionKey() {
            return SESSION_KEY;
        }

        void ISessionKeyProvider.ReturnSessionKeyToClient(string sessionKey) {
            // テスト用なので何もしない
        }
    }
}

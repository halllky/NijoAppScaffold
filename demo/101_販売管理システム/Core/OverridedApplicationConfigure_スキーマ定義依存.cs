using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.外部システム.商品管理システム;
using NLog.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationConfigure {

    /// <summary>ログ出力用キー名: ログインユーザーID</summary>
    public const string LOG_SCOPEPROP_LOGIN_USERID = "LoginUserId";
    /// <summary>ログ出力用キー名: 処理名</summary>
    public const string LOG_SCOPEPROP_PROCESS_NAME = "ProcessName";
    /// <summary>ログ出力用キー名: 処理1回分の識別子</summary>
    public const string LOG_SCOPEPROP_SCOPE_ID = "ScopeId";

    partial void ConfigureDemoService(IServiceCollection services, IConfigurationSection myAppSection) {

        // 商品管理システムの設定をバインド。
        // appsettings.json の設定に従い、モック/実際の外部システムクラスを切り替える。
        if (myAppSection.GetValue<bool>(nameof(商品管理システムSettings.UseMock))) {
            services.AddTransient<I商品管理システム, 商品管理システムMock>();
        } else {
            services.AddTransient<I商品管理システム, 商品管理システム本番>();
        }

        // NLog を用いたログ設定を Microsoft.Extensions.Logging に統合する
        services.AddLogging(logging => {

            // ログのファイル出力。
            // 出力先ディレクトリは設定ファイルから取得する
            var logDir = myAppSection.GetValue<string?>(nameof(RuntimeSetting.LogDirectory));
            if (string.IsNullOrWhiteSpace(logDir)) logDir = "Log";
            if (!Path.IsPathRooted(logDir)) {
                logDir = Path.Combine(Directory.GetCurrentDirectory(), logDir);
            }
            var fileTarget = new NLog.Targets.FileTarget("logfile") {
                FileName = Path.Combine(logDir, "${shortdate}.log"),
                Layout = "${longdate}"             // ログ時刻
                       + "\t${uppercase:${level}}" // ログレベル
                       + "\t${scopeproperty:item=" + LOG_SCOPEPROP_LOGIN_USERID + "}" // ログインユーザーID（未ログイン時は空文字）
                       + "\t${scopeproperty:item=" + LOG_SCOPEPROP_PROCESS_NAME + "}" // 処理名。
                                                                                      // Webの場合はURLのパス部分（ドメイン名とクエリを除いた部分）。
                                                                                      // バッチの場合はコマンドライン引数で指定された処理名。
                       + "\t${scopeproperty:item=" + LOG_SCOPEPROP_SCOPE_ID + "}" // 処理1回分の識別子。
                                                                                  // Webの場合はHTTPリクエスト1回毎。
                                                                                  // バッチの場合はバッチ処理1回毎。
                                                                                  // これがあるとログの追跡が大いに助かる
                       + "\t${logger}"  // ロガー名（通常はクラス名）
                       + "\t${message}" // ログ本文
                       + "\t${exception:format=tostring}", // 例外情報
                Encoding = Encoding.UTF8,
                ArchiveEvery = NLog.Targets.FileArchivePeriod.Day,
                MaxArchiveFiles = 30,
            };

            // コンソールへのログ出力
            var consoleTarget = new NLog.Targets.ConsoleTarget("console");

            // ----------------

            var config = new NLog.Config.LoggingConfiguration();
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);

            // EFCore や ASP.NET Core が標準で出力するログはコンソールに出力する。
            // それ以外はファイルに出力。（final: true をつけるとそれ以降のルールは無視される）
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget, "Microsoft.EntityFrameworkCore.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget, "Microsoft.AspNetCore.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, fileTarget);

            // ファイル出力自体の失敗などは、別途Fatalログとして出力する
            NLog.Common.InternalLogger.LogFile = Path.Combine(logDir, "NLog.Fatal.log");
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Fatal;

            logging.AddNLog(config);
        });
    }
}

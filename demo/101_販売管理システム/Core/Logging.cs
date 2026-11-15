using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.運用担当への通知;
using MyApp.NijoAttr;
using NLog.Extensions.Logging;

namespace MyApp;

internal static class LogSettings {

    /// <summary>
    /// ログ時刻の書式。サーバーの OS のタイムゾーンの時刻に、そのオフセットを付ける。
    /// OS のタイムゾーンが業務のタイムゾーンと違っても、データの日時と突き合わせられるようにするため
    /// </summary>
    internal const string TIME_LAYOUT = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.ffffzzz}";

    #region DI設定
    /// <summary>
    /// <see cref="OverridedApplicationService.ConfigureServices"/> のうちログ出力設定に関する部分。
    /// 処理が多いので別メソッドとして切り出している。
    /// </summary>
    /// <param name="errorNotifier">Error 以上のログを渡す、エラーの通知</param>
    /// <param name="logFactory">ログの設定を行う NLog の LogFactory。NLog 全体の設定は使わない</param>
    public static void ConfigureServices(IServiceCollection services, IConfigurationSection myAppSection, string basePath, ErrorNotifier errorNotifier, NLog.LogFactory logFactory) {

        // NLog を用いたログ設定を Microsoft.Extensions.Logging に統合する
        services.AddLogging(logging => {

            // ログの出力は NLog だけで行う。
            // ASP.NET Core が既定で登録するコンソールなどへの出力を残すと、NLog のコンソール出力と二重になるので消す
            logging.ClearProviders();

            // ログのファイル出力。
            // 出力先ディレクトリは設定ファイルから取得する
            var logDir = myAppSection.GetValue<string?>(nameof(RuntimeSetting.LogDirectory));
            if (string.IsNullOrWhiteSpace(logDir)) logDir = "Log";
            if (!Path.IsPathRooted(logDir)) {
                logDir = Path.Combine(basePath, logDir);
            }
            var fileTarget = new NLog.Targets.FileTarget("logfile") {
                FileName = Path.Combine(logDir, "${shortdate}.log"),
                Layout = TIME_LAYOUT                  // ログ時刻
                       + "\t${uppercase:${level}}" // ログレベル

                       // ログインユーザーID（未ログイン時は空文字）
                       + "\t${scopeproperty:item=" + OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID + "}"

                       // 処理名。
                       // Webの場合はURLのパス部分（ドメイン名とクエリを除いた部分）。
                       // バッチの場合はコマンドライン引数で指定された処理名。
                       + "\t${scopeproperty:item=" + OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME + "}"

                       // 処理1回分の識別子。
                       // Webの場合はHTTPリクエスト1回毎。
                       // バッチの場合はバッチ処理1回毎。
                       // これがあるとログの追跡が大いに助かる
                       + "\t${scopeproperty:item=" + OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID + "}"

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

            // エラーの通知。Error 以上のログを運用担当者にメールで知らせる
            var errorNotificationTarget = new ErrorNotificationTarget(errorNotifier) { Name = "errorNotification" };

            var config = new NLog.Config.LoggingConfiguration();
            config.AddTarget(fileTarget);
            config.AddTarget(consoleTarget);
            config.AddTarget(errorNotificationTarget);

            // Error 以上のログは、フレームワークのものも含めてすべて運用担当へのエラーの通知に渡す。
            // 下の final: true の規則より先に置かないと、フレームワークのログが渡らない
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, errorNotificationTarget);

            // すべてのログをコンソールに出力する。コンテナなど、コンソールの出力を集める動かし方ではこれを使う。
            // ファイルには、アプリケーションのログと、EFCore や ASP.NET Core のログのうち Warning 以上を出力する。
            // Windows のサービスなど、コンソールの出力が残らない動かし方でも、
            // 起動の失敗や DB のエラーを後から調べられるようにするため。
            // （final: true をつけると、そのロガーについてはそれ以降のルールは無視される）
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, fileTarget, "Microsoft.EntityFrameworkCore.*");
            config.AddRule(NLog.LogLevel.Warn, NLog.LogLevel.Fatal, fileTarget, "Microsoft.AspNetCore.*");
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget, "Microsoft.EntityFrameworkCore.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget, "Microsoft.AspNetCore.*", final: true);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, fileTarget);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);

            // ファイル出力自体の失敗などは、別途Fatalログとして出力する
            NLog.Common.InternalLogger.LogFile = Path.Combine(logDir, "NLog.Fatal.log");
            NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Fatal;

            // DI コンテナができる前（起動の途中）からこの設定でログを出力できるよう、この場で LogFactory に設定する
            logFactory.Configuration = config;
            logging.AddNLog(_ => logFactory);
        });
    }
    #endregion DI設定


    #region オブジェクトのログ出力

    /// <summary>
    /// ログ出力用の JSON シリアライザーオプション。
    /// パスワードなどの機微な情報をログ出力せずマスキングするなどの考慮を行う。
    /// </summary>
    public static readonly JsonSerializerOptions LogSerializerOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        TypeInfoResolver = new MaskingTypeResolver(),
    };

    /// <summary>
    /// オブジェクト型がもつプロパティのうち、
    /// <see cref="MaskingAttribute"/> がついているものをログ出力時にマスキングするための <see cref="JsonTypeInfoResolver"/>。
    /// </summary>
    private sealed class MaskingTypeResolver : DefaultJsonTypeInfoResolver {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
            var typeInfo = base.GetTypeInfo(type, options);

            if (typeInfo.Kind == JsonTypeInfoKind.Object) {
                foreach (var prop in typeInfo.Properties) {
                    var provider = prop.AttributeProvider;
                    if (provider == null) continue;

                    if (provider.IsDefined(typeof(MaskingAttribute), inherit: true)) {
                        prop.CustomConverter = _maskingJsonConverterCache.GetOrAdd(prop.PropertyType, static t => {
                            var converterType = typeof(MaskingJsonConverter<>).MakeGenericType(t);
                            return (JsonConverter)Activator.CreateInstance(converterType)!;
                        });
                    }
                }
            }

            return typeInfo;
        }

        private static readonly ConcurrentDictionary<Type, JsonConverter> _maskingJsonConverterCache = new();
    }

    private sealed class MaskingJsonConverter<T> : JsonConverter<T> {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            throw new NotSupportedException("This converter is intended for serialization only.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) {
            writer.WriteStringValue("***");
        }
    }
    #endregion オブジェクトのログ出力
}

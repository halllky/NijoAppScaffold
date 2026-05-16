using System.Collections.Concurrent;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.NijoAttr;
using NLog.Extensions.Logging;

namespace MyApp;

internal static class LogSettings {

    #region DI設定
    /// <summary>
    /// <see cref="OverridedApplicationService.ConfigureServices"/> のうちログ出力設定に関する部分。
    /// 処理が多いので別メソッドとして切り出している。
    /// </summary>
    public static void ConfigureServices(IServiceCollection services, IConfigurationSection myAppSection, string basePath) {

        // NLog を用いたログ設定を Microsoft.Extensions.Logging に統合する
        services.AddLogging(logging => {

            // ログのファイル出力。
            // 出力先ディレクトリは設定ファイルから取得する
            var logDir = myAppSection.GetValue<string?>(nameof(RuntimeSetting.LogDirectory));
            if (string.IsNullOrWhiteSpace(logDir)) logDir = "Log";
            if (!Path.IsPathRooted(logDir)) {
                logDir = Path.Combine(basePath, logDir);
            }
            var fileTarget = new NLog.Targets.FileTarget("logfile") {
                FileName = Path.Combine(logDir, "${shortdate}.log"),
                Layout = "${longdate}"             // ログ時刻
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

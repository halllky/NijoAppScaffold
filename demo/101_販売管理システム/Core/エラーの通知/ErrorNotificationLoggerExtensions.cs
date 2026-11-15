using Microsoft.Extensions.Logging;

namespace MyApp.Core.エラーの通知;

/// <summary>
/// エラーの通知の宛先の区分をログに付ける。
/// </summary>
public static class ErrorNotificationLoggerExtensions {

    /// <summary>ログ出力用キー名: エラーの通知の宛先の区分</summary>
    internal const string LOG_SCOPEPROP_CATEGORY = "ErrorNotificationCategory";

    /// <summary>
    /// 戻り値を破棄するまでの間に出力した Error 以上のログを、既定の宛先に加えて、指定した区分の宛先にも知らせる。
    /// 区分の宛先は設定で決める。設定にない区分は、既定の宛先にだけ知らせる。
    /// <code>
    /// using (Log.BeginErrorNotificationCategory("会計システム")) {
    ///     Log.LogError(ex, "会計システムとの連携に失敗しました。");
    /// }
    /// </code>
    /// </summary>
    public static IDisposable? BeginErrorNotificationCategory(this ILogger logger, string category) {
        return logger.BeginScope(new Dictionary<string, object?> { [LOG_SCOPEPROP_CATEGORY] = category });
    }
}

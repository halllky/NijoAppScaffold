using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace MyApp.Core.エラーの通知;

/// <summary>
/// NLog の出力先のうち、受け取ったログをエラーの通知に渡すもの。
/// Error 以上のログだけを受け取るよう、出力の規則で指定すること。
/// ログを出力した時点でログファイルの1行と同じ情報を文字列にして渡すので、ログのマスキングはメールにも適用される。
/// この出力先が閉じられると、エラーの通知も止める（まだ送っていないメールは送る）。
/// </summary>
public sealed class ErrorNotificationTarget(ErrorNotifier notifier) : Target {

    private static readonly Layout CATEGORY = ScopeProperty(ErrorNotificationLoggerExtensions.LOG_SCOPEPROP_CATEGORY);
    private static readonly Layout TIME = LogSettings.TIME_LAYOUT;
    private static readonly Layout LEVEL = "${uppercase:${level}}";
    private static readonly Layout LOGIN_USER_ID = ScopeProperty(OverridedApplicationService.LOG_SCOPEPROP_LOGIN_USERID);
    private static readonly Layout PROCESS_NAME = ScopeProperty(OverridedApplicationService.LOG_SCOPEPROP_PROCESS_NAME);
    private static readonly Layout SCOPE_ID = ScopeProperty(OverridedApplicationService.LOG_SCOPEPROP_SCOPE_ID);
    private static readonly Layout LOGGER = "${logger}";
    private static readonly Layout MESSAGE = "${message}";
    private static readonly Layout EXCEPTION = "${exception:format=tostring}";

    protected override void Write(LogEventInfo logEvent) {
        notifier.Enqueue(new ErrorLogEntry(
            Category: CATEGORY.Render(logEvent),
            Time: TIME.Render(logEvent),
            Level: LEVEL.Render(logEvent),
            LoginUserId: LOGIN_USER_ID.Render(logEvent),
            ProcessName: PROCESS_NAME.Render(logEvent),
            ScopeId: SCOPE_ID.Render(logEvent),
            LoggerName: LOGGER.Render(logEvent),
            Message: MESSAGE.Render(logEvent),
            Exception: EXCEPTION.Render(logEvent)));
    }

    protected override void CloseTarget() {
        notifier.Dispose();
        base.CloseTarget();
    }

    private static Layout ScopeProperty(string name) => "${scopeproperty:item=" + name + "}";
}

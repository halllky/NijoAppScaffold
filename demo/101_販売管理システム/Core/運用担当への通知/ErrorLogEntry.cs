namespace MyApp.Core.運用担当への通知;

/// <summary>
/// 通知の対象になった、Error 以上のログ1件。
/// ログを出力した時点で文字列にしたもので、ログファイルの1行と同じ情報をもつ。
/// </summary>
/// <param name="Category">宛先の区分。付けられていなければ空文字</param>
public sealed record ErrorLogEntry(
    string Category,
    string Time,
    string Level,
    string LoginUserId,
    string ProcessName,
    string ScopeId,
    string LoggerName,
    string Message,
    string Exception);

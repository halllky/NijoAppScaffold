namespace MyApp.Core.運用担当への通知;

/// <summary>
/// エラーの通知のメールを送る部品。実機とモックを設定で切り替える。
/// 送信に失敗したときは例外を投げてよい。呼び出し側がログに出し、送り直さない。
/// </summary>
public interface IErrorMailSender {
    Task SendAsync(ErrorMail mail, CancellationToken cancellationToken);
}

using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.運用担当への通知;

/// <summary>
/// appsettings.json のエラーの通知の設定セクションに対応する設定クラス。
/// エラーの通知は、Error 以上のログを運用担当者にメールで知らせる。
/// </summary>
public class ErrorNotificationSettings {
    /// <summary>
    /// メールの送信にモックを使う場合は true。実機を使う場合は false。
    /// 省略を許さないので既定値をもたない。省略されていると起動しない。
    /// 実機の送信は実装していないので、false を指定しても起動しない。
    /// </summary>
    public bool? UseMock { get; set; }
    /// <summary>
    /// モックがメールの代わりにテキストファイルを作るフォルダ。相対パスの場合は <see cref="RuntimeSetting.LogDirectory"/> からの相対パス。
    /// </summary>
    [Required]
    public string MockOutputDirectory { get; set; } = string.Empty;
    /// <summary>
    /// 既定の宛先。すべての通知のメールが届く。1件以上指定する。
    /// 誰にも届かない状態で運用するのを防ぐため。設定ファイルの空の配列は省略と同じに扱われる。
    /// </summary>
    [Required, MinLength(1)]
    public List<string> DefaultRecipients { get; set; } = null!;
    /// <summary>
    /// 宛先の区分ごとの宛先。ログを出力するときに宛先の区分を付けた通知は、既定の宛先とその区分の宛先に届く。
    /// 宛先の区分を使わないなら省略してよい。
    /// </summary>
    public Dictionary<string, List<string>> RecipientsByCategory { get; set; } = [];
    /// <summary>
    /// WebApi で、宛先の区分ごとに送るメールの数を抑える間隔（分）。
    /// この間隔の中で2通目以降になるログは貯めておき、間隔が過ぎたときに1通にまとめて送る。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ThrottleMinutes { get; set; }
}

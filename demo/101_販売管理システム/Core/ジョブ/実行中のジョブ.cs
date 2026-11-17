namespace MyApp.Core.ジョブ;

/// <summary>
/// 実行中のジョブ。ジョブの本体に渡す。
/// </summary>
public sealed class 実行中のジョブ {

    internal 実行中のジョブ(string ジョブID, string? 引数, string 作業フォルダ, CancellationToken 停止の合図) {
        this.ジョブID = ジョブID;
        this.引数 = 引数;
        this.作業フォルダ = 作業フォルダ;
        this.停止の合図 = 停止の合図;
    }

    /// <summary>ジョブの識別子</summary>
    public string ジョブID { get; }

    /// <summary>
    /// このジョブだけが使えるサーバー上のフォルダの絶対パス。実行の前に作られる。一時ファイルなどを自由に置いてよい。
    /// 終わってから一定期間が経ったら、クリーンナップで中身ごと削除される。
    /// </summary>
    public string 作業フォルダ { get; }

    /// <summary>
    /// 作業フォルダの中の、利用者がダウンロードできるファイルを置くフォルダ（スプール）の絶対パス。実行の前に作られる。
    /// ここに置いたファイルは、ジョブを起動したユーザーとシステム管理者が、ジョブの一覧からダウンロードできる。
    /// </summary>
    public string スプールフォルダ => Path.Combine(作業フォルダ, スプールフォルダの名前);

    /// <summary>
    /// サーバーの停止の合図。合図があったら、なるべく早く処理を打ち切って例外を投げること。
    /// 一定時間のうちに終わらなければ、ジョブは実行中のままプロセスが止まり、次の起動のときに中断として異常終了になる。
    /// </summary>
    public CancellationToken 停止の合図 { get; }

    /// <summary>
    /// 利用者に見せる結果。ジョブの一覧に表示する。
    /// 本体が例外を投げて異常終了したときも、ここに設定してあればそれを表示する。設定していなければ決まった文言を表示する。
    /// </summary>
    public string? 結果メッセージ { get; set; }

    /// <summary>ジョブの登録のときに渡された引数の JSON</summary>
    internal string? 引数 { get; }

    /// <summary>作業フォルダの中の、スプールのフォルダの名前</summary>
    internal const string スプールフォルダの名前 = "spool";
}

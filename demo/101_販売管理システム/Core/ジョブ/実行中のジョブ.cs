namespace MyApp.Core.ジョブ;

/// <summary>
/// 実行中のジョブの情報。ジョブの本体に渡される。
/// </summary>
public sealed class 実行中のジョブ {

    internal 実行中のジョブ(string ジョブID, string? 引数, string 作業フォルダ, CancellationToken cancellationToken) {
        this.ジョブID = ジョブID;
        this.引数 = 引数;
        this.作業フォルダ = 作業フォルダ;
        CancellationToken = cancellationToken;
    }

    /// <summary>ジョブ ID</summary>
    public string ジョブID { get; }

    /// <summary>
    /// このジョブ専用の、サーバー上の作業フォルダの絶対パス。実行前に作成される。一時ファイルなどを自由に置いてよい。
    /// ジョブの終了から一定期間が経つと、クリーンナップで中身ごと削除される。
    /// </summary>
    public string 作業フォルダ { get; }

    /// <summary>
    /// 作業フォルダ内にある、利用者がダウンロードできるファイルを置くフォルダ（スプール）の絶対パス。実行前に作成される。
    /// ここに置いたファイルは、ジョブを起動したユーザーとシステム管理者が、ジョブ一覧画面からダウンロードできる。
    /// </summary>
    public string スプールフォルダ => Path.Combine(作業フォルダ, スプールフォルダの名前);

    /// <summary>
    /// サーバーの停止時にキャンセルされるトークン。キャンセルされたら、なるべく早く処理を打ち切って <see cref="OperationCanceledException"/> を投げること。
    /// 一定時間内に終わらなければ、ジョブは実行中のままプロセスが終了し、次回の起動時に中断として異常終了になる。
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// ジョブ一覧画面に表示する、利用者向けの結果のメッセージ。
    /// 本体が例外を投げて異常終了した場合も、設定してあればそれを表示する。設定していなければ既定の文言を表示する。
    /// </summary>
    public string? 結果メッセージ { get; set; }

    /// <summary>ジョブの登録時に渡された引数の JSON</summary>
    internal string? 引数 { get; }

    /// <summary>作業フォルダ内のスプールフォルダの名前</summary>
    internal const string スプールフォルダの名前 = "spool";
}

using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.ジョブ;

/// <summary>
/// appsettings.json のジョブの設定セクションに対応する設定クラス。
/// </summary>
public class JobSettings {
    /// <summary>
    /// ジョブごとの作業フォルダを作るフォルダ。相対パスの場合は appsettings.json のあるフォルダからの相対パス。
    /// サーバーが複数台のときも、台ごとのフォルダでよい。作業フォルダは、ジョブを実行した台にだけ作られる。
    /// </summary>
    [Required]
    public string WorkDirectory { get; set; } = string.Empty;
    /// <summary>
    /// 1台のサーバーで同時に実行するジョブの数。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxConcurrency { get; set; }
    /// <summary>
    /// サーバーの停止のとき、実行中のジョブに停止を伝えてから、終わるのを待つ時間（秒）。
    /// 過ぎても終わらないジョブは、次の起動のときに中断として異常終了にする。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int ShutdownTimeoutSeconds { get; set; }

    /// <summary>
    /// ジョブの作業フォルダの絶対パスを返す。
    /// </summary>
    public string Get作業フォルダ(string ジョブID) => Path.Combine(WorkDirectory, ジョブID);

    /// <summary>
    /// 相対パスで指定された項目を、appsettings.json のあるフォルダを基準にした絶対パスにする。
    /// 業務処理の中では基準のフォルダが分からないので、設定を読み込んだときに呼ぶ。
    /// </summary>
    internal void ResolvePaths(string basePath) {
        if (!string.IsNullOrWhiteSpace(WorkDirectory)) {
            WorkDirectory = Path.GetFullPath(WorkDirectory, basePath);
        }
    }
}

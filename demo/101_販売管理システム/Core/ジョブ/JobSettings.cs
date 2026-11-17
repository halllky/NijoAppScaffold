using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.ジョブ;

/// <summary>
/// appsettings.json のジョブに関する設定。
/// </summary>
public class JobSettings {
    /// <summary>
    /// ジョブごとの作業フォルダを作るフォルダ。相対パスの場合は appsettings.json のあるフォルダからの相対パス。
    /// サーバーが複数台の場合も、共有フォルダにする必要はない。作業フォルダは、ジョブを実行したサーバーにだけ作られる。
    /// </summary>
    [Required]
    public string WorkDirectory { get; set; } = string.Empty;
    /// <summary>
    /// 1台のサーバーで同時に実行するジョブの数。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MaxConcurrency { get; set; }
    /// <summary>
    /// サーバーの停止時に、実行中のジョブをキャンセルしてから終了を待つ時間（秒）。
    /// この時間内に終わらなかったジョブは、次回の起動時に中断として異常終了にする。
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

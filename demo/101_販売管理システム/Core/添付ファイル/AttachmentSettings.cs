using System.ComponentModel.DataAnnotations;

namespace MyApp.Core.添付ファイル;

/// <summary>
/// appsettings.json の添付ファイルに関する設定。
/// </summary>
public class AttachmentSettings {
    /// <summary>
    /// 既定の保存先（サーバー上のフォルダ）のパス。相対パスの場合は appsettings.json のあるフォルダからの相対パス。
    /// サーバーが複数台の場合は、すべてのサーバーから同じ場所を指すようにするか、保存先を差し替える。
    /// </summary>
    [Required]
    public string StorageDirectory { get; set; } = string.Empty;
    /// <summary>
    /// 1ファイルのサイズの上限（MB）。
    /// </summary>
    [Range(1, 2047)]
    public int MaxFileSizeMegabytes { get; set; }
    /// <summary>
    /// どのデータからも参照されなくなったファイルを、削除するまで残しておく日数。
    /// DB をバックアップから戻したときにファイルが無くなっているのを防ぐため、DB のバックアップを残す日数より長くする。
    /// </summary>
    [Range(1, int.MaxValue)]
    public int UnreferencedRetentionDays { get; set; }

    /// <summary>
    /// 1ファイルのサイズの上限（バイト）。
    /// </summary>
    public long MaxFileSizeBytes => MaxFileSizeMegabytes * 1024L * 1024L;

    /// <summary>
    /// 相対パスで指定された項目を、appsettings.json のあるフォルダを基準にした絶対パスにする。
    /// 業務処理の中では基準のフォルダが分からないので、設定を読み込んだときに呼ぶ。
    /// </summary>
    internal void ResolvePaths(string basePath) {
        if (!string.IsNullOrWhiteSpace(StorageDirectory)) {
            StorageDirectory = Path.GetFullPath(StorageDirectory, basePath);
        }
    }
}

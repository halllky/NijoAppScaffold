using Microsoft.EntityFrameworkCore;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// ログインユーザーがダウンロードできる、スプールのファイルの絶対パスを返す。
    /// ダウンロードできるのは、ジョブを起動したユーザーとシステム管理者だけ。
    /// 権限のないジョブのファイルは、存在しないファイルと同じように扱い、存在すること自体を知らせない。
    /// </summary>
    /// <param name="ジョブID">ジョブ ID</param>
    /// <param name="ファイル名">スプールフォルダ内のファイル名。パスの区切り文字を含むものは受け付けない</param>
    /// <returns>ファイルの絶対パス。ダウンロードできるファイルが無ければ null</returns>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    public async Task<string?> FindスプールのファイルAsync(string ジョブID, string ファイル名) {
        if (!Isスプールの中のファイル名(ファイル名)) return null;

        var visible = await Filter見てよいジョブ(DbContext.ジョブDbSet).AnyAsync(j => j.ジョブID == ジョブID);
        if (!visible) return null;

        var path = Path.Combine(Getスプールのフォルダ(ジョブID), ファイル名);
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// ジョブのスプールフォルダにあるファイルの名前を返す。
    /// 作業フォルダはジョブを実行したサーバーにしかないので、別のサーバーで実行したジョブでは空になる。
    /// </summary>
    private IEnumerable<string> Getスプールのファイル名(string ジョブID) {
        var spool = new DirectoryInfo(Getスプールのフォルダ(ジョブID));
        return spool.Exists
            ? spool.GetFiles().Select(f => f.Name).Order()
            : [];
    }

    private string Getスプールのフォルダ(string ジョブID) {
        return Path.Combine(Settings.Job.Get作業フォルダ(ジョブID), 実行中のジョブ.スプールフォルダの名前);
    }

    /// <summary>
    /// スプールフォルダ直下のファイルを指す名前かどうか。
    /// ".." やパスの区切り文字で、スプールフォルダの外のファイルを指定されるのを防ぐため。
    /// </summary>
    private static bool Isスプールの中のファイル名(string ファイル名) {
        return !string.IsNullOrWhiteSpace(ファイル名)
            && ファイル名 == Path.GetFileName(ファイル名)
            && ファイル名.IndexOfAny(['/', '\\']) < 0
            && ファイル名 != "."
            && ファイル名 != "..";
    }
}

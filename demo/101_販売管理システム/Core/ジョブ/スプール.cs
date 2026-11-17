using Microsoft.EntityFrameworkCore;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// ログインユーザーがダウンロードしてよい、スプールのファイルの絶対パスを返す。
    /// ダウンロードしてよいのは、ジョブを起動したユーザーとシステム管理者だけ。
    /// 見てよくないジョブのファイルは、存在しないファイルと同じに扱い、存在することも知らせない。
    /// </summary>
    /// <param name="ジョブID">ジョブの識別子</param>
    /// <param name="ファイル名">スプールのフォルダの中のファイル名。フォルダの区切りを含むものは受け付けない</param>
    /// <returns>ファイルの絶対パス。ダウンロードしてよいファイルが無ければ null</returns>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    public async Task<string?> FindスプールのファイルAsync(string ジョブID, string ファイル名) {
        if (!Isスプールの中のファイル名(ファイル名)) return null;

        var visible = await Filter見てよいジョブ(DbContext.ジョブDbSet).AnyAsync(j => j.ジョブID == ジョブID);
        if (!visible) return null;

        var path = Path.Combine(Getスプールのフォルダ(ジョブID), ファイル名);
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// ジョブのスプールのフォルダにあるファイルの名前を返す。
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
    /// スプールのフォルダの直下のファイルを指す名前かどうか。
    /// ".." やフォルダの区切りで、スプールの外のファイルを指定されないようにするため。
    /// </summary>
    private static bool Isスプールの中のファイル名(string ファイル名) {
        return !string.IsNullOrWhiteSpace(ファイル名)
            && ファイル名 == Path.GetFileName(ファイル名)
            && ファイル名.IndexOfAny(['/', '\\']) < 0
            && ファイル名 != "."
            && ファイル名 != "..";
    }
}

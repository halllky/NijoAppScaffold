namespace MyApp.Core.添付ファイル;

/// <summary>
/// サーバー上のフォルダに、識別子をファイル名にして保存する。既定の保存先。
/// </summary>
public class フォルダの添付ファイル保存先(string フォルダ) : I添付ファイル保存先 {

    public async Task SaveAsync(string 識別子, Stream 中身, CancellationToken ct) {
        Directory.CreateDirectory(フォルダ);
        var path = GetPath(識別子);

        // 同じ名前のファイルがあれば、ここで例外になる。そのファイルには触れない
        var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        try {
            await using (file) {
                await 中身.CopyToAsync(file, ct);
            }
        } catch {
            // 書きかけのファイルを残さない
            File.Delete(path);
            throw;
        }
    }

    public Task<Stream?> OpenReadAsync(string 識別子, CancellationToken ct) {
        var path = GetPath(識別子);
        Stream? stream = File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true)
            : null;
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string 識別子, CancellationToken ct) {
        File.Delete(GetPath(識別子));
        return Task.CompletedTask;
    }

    /// <summary>
    /// 識別子からファイルのパスを作る。識別子はアプリケーションが採番した値だが、フォルダの外を指さないことを念のため確かめる。
    /// </summary>
    private string GetPath(string 識別子) {
        if (string.IsNullOrWhiteSpace(識別子) || 識別子 != Path.GetFileName(識別子) || 識別子 is "." or "..") {
            throw new ArgumentException($"添付ファイルの識別子として使えない値です: {識別子}", nameof(識別子));
        }
        return Path.Combine(フォルダ, 識別子);
    }
}

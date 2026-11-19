namespace MyApp.Core.添付ファイル;

/// <summary>
/// 添付ファイルの中身を置く場所。
/// ファイルの名前や状態は DB の管理テーブルが持つので、ここでは識別子をキーにした中身の出し入れだけを行う。
/// S3 などに変えるときは、このインターフェースの実装を作り、DI の設定で登録し直す。
/// </summary>
public interface I添付ファイル保存先 {
    /// <summary>
    /// ファイルの中身を書き込む。同じ識別子のファイルが既にあれば例外を投げる。
    /// </summary>
    Task SaveAsync(string 識別子, Stream 中身, CancellationToken ct);
    /// <summary>
    /// ファイルの中身を読み出す。呼び出し元が閉じる。
    /// </summary>
    /// <returns>ファイルが無ければ null</returns>
    Task<Stream?> OpenReadAsync(string 識別子, CancellationToken ct);
    /// <summary>
    /// ファイルを削除する。ファイルが無くてもエラーにしない。
    /// </summary>
    Task DeleteAsync(string 識別子, CancellationToken ct);
}

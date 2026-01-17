namespace MyApp;

/// <summary>
/// <para>
/// Nijo自動生成処理内部で付加されたメッセージの構造体からエラー等のメッセージを取り出し、
/// エンドユーザーに知らせるための画面表示用のリスト等に変換するための拡張メソッド群。
/// </para>
/// <para>
/// Nijoが行うのは「各種エラーメッセージがどの項目で発生したか」を表すパス情報付きで集めることまでであり、
/// それをエンドユーザーにどういう形で見せるかは個別のアプリケーションの仕様に依存するため、
/// ここでは個別のアプリケーションの仕様に合わせた形でのメッセージ取り扱い処理を定義する。
/// </para>
/// <para>
/// アプリケーション新規作成時点では各種拡張メソッド（エラー有無チェック、リスト化、JsonObject型への変換）
/// を定義しているが、あくまで例であり、必要に応じて削除してかまわない。
/// </para>
/// </summary>
public static class MessageContainerExtension {

    /// <summary>
    /// このオブジェクトおよび子孫オブジェクトが持っているメッセージのうち、エラーが1件以上あるかどうかを返します。
    /// </summary>
    /// <param name="messages"></param>
    /// <returns></returns>
    public static bool HasError<T>(this T messages) where T : IMessageSetter {
        return messages.GetState()?.DescendantsAndSelf().Any(c => c.Errors.Any()) == true;
    }

    /// <summary>
    /// このオブジェクトおよび子孫オブジェクトが持っているメッセージを再帰的に列挙します。
    /// メッセージは、パスとメッセージの組み合わせで返されます。
    /// パスは、このオブジェクトから子孫オブジェクトまでのパスをピリオドで繋いだものになります。
    /// </summary>
    /// <param name="messageContainer">メッセージコンテナ</param>
    /// <returns>メッセージ</returns>
    public static IEnumerable<string> GetAllMessages<T>(this T messages) where T : IMessageSetter {
        var messageContainer = messages.GetState();
        if (messageContainer == null) yield break;

        foreach (var container in messageContainer.DescendantsAndSelf()) {
            foreach (var err in container.Errors) {
                yield return $"{PathToString(container.Path)}: {err}";
            }
            foreach (var warn in container.Warns) {
                yield return $"{PathToString(container.Path)}: {warn}";
            }
            foreach (var info in container.Infos) {
                yield return $"{PathToString(container.Path)}: {info}";
            }
        }

        // パスをピリオドで繋いだ文字列に変換する。
        // 半角数値のみから構成されるキー名は配列インデックスを意味するので「(i + 1)行目」というように変換する。
        static string PathToString(IEnumerable<string> path) {
            var result = new List<string>();
            foreach (var strKey in path) {
                if (int.TryParse(strKey, out var intKey)) {
                    result.Add($"{intKey + 1}行目");
                } else {
                    result.Add(strKey);
                }
            }
            return string.Join(".", result);
        }
    }
}

namespace MyApp;

// このファイルは自動生成されたオブジェクトに任意の部分クラス宣言を付与するためのファイルです。
// 例えば DbContext に任意の DbSet を追加したり、
// 自動生成された基底クラスに任意のヘルパーメソッドを追加したりする際に使用してください。

// -------------------------------------

/// <summary>
/// エラーメッセージなどの入れ物の基底クラス。
/// 自動生成された各オブジェクトのメッセージコンテナは、このクラスを継承しています。
///
/// このオブジェクトは、単にメッセージの文言を保持するだけでなく、それがどの項目で発生したかを示す情報も持っています。
/// 具体的にエラーをどう見せたいかはアプリケーションの要件により千差万別なので、
/// ここで partial 宣言でメソッドを追加するなどして、要件にあわせたエラー表示を実現してください。
/// </summary>
partial class MessageContainer {

    /// <summary>
    /// 構造化されたメッセージを、単なるstringのリストに変換して返す。
    /// エラーがどの項目で発生したかにそれほど関心がなく、とにかくただメッセージをどこかに一覧表示したい場合に使用します。
    /// </summary>
    public IEnumerable<string> GetAllMessages() {
        foreach (var err in _errors) {
            yield return $"{string.Join(".", _path)}: {err}";
        }
        foreach (var warn in _warnings) {
            yield return $"{string.Join(".", _path)}: {warn}";
        }
        foreach (var info in _informations) {
            yield return $"{string.Join(".", _path)}: {info}";
        }

        foreach (var child in ((IMessageContainer)this).EnumerateDescendants()) {
            foreach (var msg in ((MessageContainer)child).GetAllMessages()) {
                yield return msg;
            }
        }
    }

    /// <summary>
    /// <para>
    /// 現時点でこのインスタンスが持っているメッセージをすべて引数のメッセージコンテナのルートに移動させます。
    /// このインスタンスが持っているメッセージはクリアされます。
    /// </para>
    /// <para>
    /// 主に、 CommandModel の中で DataModel の更新を行う場合に使用します。
    /// </para>
    /// <para>
    /// （CommandModel のパラメータと DataModel のデータ構造は異なることが多いため、
    /// CommandModel のメッセージコンテナをそのまま DataModel の更新処理に渡すことができない。
    /// そこで、まず DataModel のメッセージコンテナを作成し、それを更新処理にわたしてメッセージを収集し、
    /// その後に DataModel のメッセージコンテナから CommandModel のメッセージコンテナにメッセージを移動させる。）
    /// </para>
    /// </summary>
    /// <param name="target">移動先</param>
    public void TransferToRootOf(IMessageContainer target) {
        foreach (var err in _errors) target.AddError(err);
        foreach (var warn in _warnings) target.AddWarn(warn);
        foreach (var info in _informations) target.AddInfo(info);
        _errors.Clear();
        _warnings.Clear();
        _informations.Clear();

        foreach (var child in ((IMessageContainer)this).EnumerateDescendants()) ((MessageContainer)child).TransferToRootOf(target);
    }
}

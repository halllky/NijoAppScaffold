namespace MyApp;

// このファイルは自動生成されたオブジェクトに任意の部分クラス宣言を付与するためのファイルです。
// 例えば DbContext に任意の DbSet を追加したり、
// 自動生成された基底クラスに任意のヘルパーメソッドを追加したりする際に使用してください。

// -------------------------------------

/// <summary>
/// エラーメッセージなどの入れ物
/// </summary>
partial class MessageContainer {

    /// <summary>
    /// 構造化されたメッセージを、単なるstringのリストに変換して返す。
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
}

/// <summary>
/// アカウントDbEntityの拡張
/// </summary>
partial class アカウントDbEntity {
    /// <summary>
    /// パスワードをハッシュ化して設定する
    /// </summary>
    public void SetPassword(string plainPassword) {
        パスワードハッシュ = System.Text.Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(plainPassword));
    }

    /// <summary>
    /// パスワードを検証する
    /// </summary>
    public bool VerifyPassword(string plainPassword) {
        if (パスワードハッシュ == null) return false;
        var hashedPassword = System.Text.Encoding.UTF8.GetString(パスワードハッシュ);
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}

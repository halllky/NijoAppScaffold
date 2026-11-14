
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : CurrentUser.cs
// ※ このファイルを生成したメソッド: Render

namespace MyApp;

/// <summary>
/// 現在操作中のユーザー。データ更新時の作成者・更新者の記録などに使用。
/// </summary>
public interface ICurrentUser {
    /// <summary>ユーザーを識別する名前</summary>
    string Name { get; }
}

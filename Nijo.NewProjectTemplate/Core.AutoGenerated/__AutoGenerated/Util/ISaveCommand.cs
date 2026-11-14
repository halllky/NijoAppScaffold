
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : SaveCommand.cs
// ※ このファイルを生成したメソッド: RenderInterface

using System.Diagnostics.CodeAnalysis;

namespace MyApp;

/// <summary>
/// 新規登録・更新・削除のコマンド。
/// 集約の種類を問わずにコマンドを扱う共通処理を書くときに使う。
/// </summary>
/// <typeparam name="TValidated">検証済みコマンドの型</typeparam>
/// <typeparam name="TMessages">検証エラーのメッセージの入れ物の型</typeparam>
public interface ISaveCommand<TValidated, TMessages>
    where TValidated : ValidatedCommand
    where TMessages : MessageSetter {

    /// <summary>
    /// このコマンドの現在の値を検証します。このコマンドの状態は変更しません。
    /// 必須チェックなどスキーマ定義から自動生成されるチェックと、楽観排他のバージョンの食い違いのチェックが行なわれます。
    /// </summary>
    /// <param name="validated">
    /// エラーが無い場合のみ、現時点の値を固定した検証済みコマンド。
    /// 以降にこのコマンドを書き換えても影響を受けない。
    /// </param>
    /// <param name="errors">検証エラー。エラーが無い場合は空。</param>
    /// <returns>エラーが無ければ true</returns>
    bool TryValidate([NotNullWhen(true)] out TValidated? validated, out TMessages errors);
}

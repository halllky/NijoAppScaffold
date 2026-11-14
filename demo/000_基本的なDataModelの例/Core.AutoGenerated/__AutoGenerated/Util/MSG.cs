
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : MsgFactory.cs
// ※ このファイルを生成したメソッド: Render

namespace MyApp;

/// <summary>
/// 自動生成されるコードの中で登場する、ユーザーに表示するメッセージ。
/// 具体的な文言はDI経由で入れ替えることができる。
/// </summary>
public interface IDisplayMessageFactory {
    /// <summary>
    /// 楽観排他制御に引っかかったときのメッセージ
    /// </summary>
    string ConcurrencyError();
    /// <summary>
    /// 更新対象・削除対象のデータがデータベース上で見つからなかったときのメッセージ
    /// </summary>
    string DataNotFoundError();
    /// <summary>
    /// 入力エラー時メッセージ（数値項目の桁数チェック）
    /// </summary>
    string DigitsError(string arg0, string arg1, string arg2);
    /// <summary>
    /// 入力エラー時メッセージ（汎用参照テーブル存在チェック）
    /// </summary>
    string GenericLookupValueNotFound(string arg0);
    /// <summary>
    /// 入力エラー時メッセージ（文字列最大長チェック）
    /// </summary>
    string MaxLengthError(string arg0, string arg1);
    /// <summary>
    /// 入力エラー時メッセージ（必須入力チェック）
    /// </summary>
    string RequiredError(string arg0);
    /// <summary>
    /// 一意制約に違反したときのメッセージ
    /// </summary>
    string UniqueConstraintError();
    /// <summary>
    /// 登録/更新/削除のタイミングでRDBMS上で何らかのエラーが生じた場合のメッセージ
    /// </summary>
    string UnknownUpdateError();
}

/// <summary>
/// <see cref="IDisplayMessageFactory"/> の既定の実装。
/// </summary>
public class DefaultMessageFactory : IDisplayMessageFactory {
    public string ConcurrencyError() => $"ほかのユーザーが更新しました。";
    public string DataNotFoundError() => $"更新対象のデータが見つかりません。";
    public string DigitsError(string arg0, string arg1, string arg2) => $"{arg0} は整数部 {arg1} 桁、小数部 {arg2} 桁以内で入力してください。";
    public string GenericLookupValueNotFound(string arg0) => $"{arg0} に指定された区分値が存在しません。";
    public string MaxLengthError(string arg0, string arg1) => $"{arg0} は {arg1} 文字以内で入力してください。";
    public string RequiredError(string arg0) => $"{arg0} を入力してください。";
    public string UniqueConstraintError() => $"同じ値のデータが既に存在します。";
    public string UnknownUpdateError() => $"登録処理でエラーが発生しました。";
}

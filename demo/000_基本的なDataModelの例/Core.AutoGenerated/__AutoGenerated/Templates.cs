
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数の値を埋め込む前の変換（エスケープなど）。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。
/// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="Templates{TKind}"/> の型引数に渡す。
/// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
/// </summary>
public interface ITemplateKind {
    /// <summary>種類「文言」の変換</summary>
    static abstract string 文言(string value);
}

/// <summary>
/// nijo.xml の Templates セクションに定義したテンプレート文字列。
/// 引数の値は、型引数に渡したクラスの変換を通してから埋め込む。
/// アプリからは、型引数を決めたこのクラスを using static し、グループのクラスを直接参照して使う。
/// </summary>
/// <example>
/// global using static MyApp.Templates&lt;MyApp.TemplateKind&gt;;
/// </example>
public abstract class Templates<TKind> where TKind : ITemplateKind {
    /// <summary>
    /// メッセージ
    /// <para>
    /// 利用者に見せるメッセージ。名前がメッセージの ID になる。
    /// 入力チェックや保存処理の共通部分で使うもの。業務ごとのメッセージは、別のグループに定義する。
    /// </para>
    /// </summary>
    public static class MSG {
        /// <summary>
        /// 必須入力エラー
        /// </summary>
        public static string RequiredError(string 項目名) => $"{TKind.文言(項目名)} を入力してください。";
        /// <summary>
        /// 文字数超過エラー
        /// </summary>
        public static string MaxLengthError(string 項目名, string 最大文字数) => $"{TKind.文言(項目名)} は {TKind.文言(最大文字数)} 文字以内で入力してください。";
        /// <summary>
        /// 桁数超過エラー
        /// </summary>
        public static string DigitsError(string 項目名, string 整数部の桁数, string 小数部の桁数) => $"{TKind.文言(項目名)} は整数部 {TKind.文言(整数部の桁数)} 桁、小数部 {TKind.文言(小数部の桁数)} 桁以内で入力してください。";
        /// <summary>
        /// 区分値不在エラー
        /// </summary>
        public static string GenericLookupValueNotFound(string 項目名) => $"{TKind.文言(項目名)} に指定された区分値が存在しません。";
        /// <summary>
        /// 楽観排他エラー
        /// <para>
        /// データを読み込んだ後に、ほかのユーザーが同じデータを更新していたとき
        /// </para>
        /// </summary>
        public static string ConcurrencyError() => $"ほかのユーザーが更新しました。";
        /// <summary>
        /// データ不在エラー
        /// <para>
        /// 更新対象・削除対象のデータがデータベース上で見つからなかったとき
        /// </para>
        /// </summary>
        public static string DataNotFoundError() => $"更新対象のデータが見つかりません。";
        /// <summary>
        /// 一意制約違反エラー
        /// </summary>
        public static string UniqueConstraintError() => $"同じ値のデータが既に存在します。";
        /// <summary>
        /// 保存時エラー
        /// <para>
        /// 登録・更新・削除のときに、データベースで何らかのエラーが起きたとき。エラーの内容は利用者に見せない
        /// </para>
        /// </summary>
        public static string UnknownUpdateError() => $"登録処理でエラーが発生しました。";
    }
}

/// <summary>
/// nijo.xml の Templates セクションに定義したテンプレート文字列の、
/// 引数の位置に渡された文字列を変換せずにそのまま置くメソッド。引数はすべて文字列で受け取る。
/// ASP.NET Core のルート定義（"/api/job/{jobId}"）のように、引数の位置に値以外のものを置くときに使う。
/// 変換しないので、利用者が入力した値など、任意の値を渡す用途には使わないこと。
/// </summary>
public static class TemplatePattern {
    /// <summary>
    /// メッセージ
    /// <para>
    /// 利用者に見せるメッセージ。名前がメッセージの ID になる。
    /// 入力チェックや保存処理の共通部分で使うもの。業務ごとのメッセージは、別のグループに定義する。
    /// </para>
    /// </summary>
    public static class MSG {
        /// <summary>
        /// 必須入力エラー
        /// </summary>
        public static string RequiredError(string 項目名) => $"{項目名} を入力してください。";
        /// <summary>
        /// 文字数超過エラー
        /// </summary>
        public static string MaxLengthError(string 項目名, string 最大文字数) => $"{項目名} は {最大文字数} 文字以内で入力してください。";
        /// <summary>
        /// 桁数超過エラー
        /// </summary>
        public static string DigitsError(string 項目名, string 整数部の桁数, string 小数部の桁数) => $"{項目名} は整数部 {整数部の桁数} 桁、小数部 {小数部の桁数} 桁以内で入力してください。";
        /// <summary>
        /// 区分値不在エラー
        /// </summary>
        public static string GenericLookupValueNotFound(string 項目名) => $"{項目名} に指定された区分値が存在しません。";
        /// <summary>
        /// 楽観排他エラー
        /// <para>
        /// データを読み込んだ後に、ほかのユーザーが同じデータを更新していたとき
        /// </para>
        /// </summary>
        public static string ConcurrencyError() => $"ほかのユーザーが更新しました。";
        /// <summary>
        /// データ不在エラー
        /// <para>
        /// 更新対象・削除対象のデータがデータベース上で見つからなかったとき
        /// </para>
        /// </summary>
        public static string DataNotFoundError() => $"更新対象のデータが見つかりません。";
        /// <summary>
        /// 一意制約違反エラー
        /// </summary>
        public static string UniqueConstraintError() => $"同じ値のデータが既に存在します。";
        /// <summary>
        /// 保存時エラー
        /// <para>
        /// 登録・更新・削除のときに、データベースで何らかのエラーが起きたとき。エラーの内容は利用者に見せない
        /// </para>
        /// </summary>
        public static string UnknownUpdateError() => $"登録処理でエラーが発生しました。";
    }
}

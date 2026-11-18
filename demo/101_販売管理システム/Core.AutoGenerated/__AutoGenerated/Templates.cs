
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。引数の型ごとにオーバーロードがある。
/// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="Templates{TKind}"/> の型引数に渡す。
/// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
/// </summary>
public interface ITemplateKind {
    /// <summary>種類「URL」の、string の引数の変換</summary>
    static abstract string URL(string value);
    /// <summary>種類「URL」の、int の引数の変換</summary>
    static abstract string URL(int value);
    /// <summary>種類「文言」の、string の引数の変換</summary>
    static abstract string 文言(string value);
    /// <summary>種類「文言」の、int の引数の変換</summary>
    static abstract string 文言(int value);
}

/// <summary>
/// nijo.xml の Templates セクションに定義したテンプレート文字列。
/// 引数の値は、型引数に渡したクラスの変換を通してから埋め込む。
/// アプリからは、型引数を決めた派生クラスを1つ定義し、それを通して使う。
/// </summary>
/// <example>
/// public abstract class テンプレート : Templates&lt;TemplateKind&gt;;
/// </example>
public abstract class Templates<TKind> where TKind : ITemplateKind {
    /// <summary>
    /// 画面URL一覧
    /// <para>
    /// 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
    /// 値はオリジンを除いた、"/" で始まるパス。
    /// </para>
    /// </summary>
    public static class PAGES_URL {
        /// <summary>
        /// P400 従業員
        /// </summary>
        public static string P400() => $"/jugyoin";
        /// <summary>
        /// P004 ジョブ一覧
        /// </summary>
        public static string P004() => $"/job";
    }
    /// <summary>
    /// 画面URL一覧(デモ101専用)
    /// <para>
    /// 販売管理の画面の URL。値の決まりは「PAGES_URL」と同じ。
    /// </para>
    /// </summary>
    public static class PAGES_URL_DEMO101 {
        /// <summary>
        /// P101 売上詳細
        /// </summary>
        public static string P101(int 売上SEQ) => $"/uriage/edit/{TKind.URL(売上SEQ)}";
        /// <summary>
        /// P301 商品詳細
        /// </summary>
        public static string P301(int 商品SEQ) => $"/shohin/{TKind.URL(商品SEQ)}";
    }
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
        public static string MaxLengthError(string 項目名, int 最大文字数) => $"{TKind.文言(項目名)} は {TKind.文言(最大文字数)} 文字以内で入力してください。";
        /// <summary>
        /// 桁数超過エラー
        /// </summary>
        public static string DigitsError(string 項目名, int 整数部の桁数, int 小数部の桁数) => $"{TKind.文言(項目名)} は整数部 {TKind.文言(整数部の桁数)} 桁、小数部 {TKind.文言(小数部の桁数)} 桁以内で入力してください。";
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

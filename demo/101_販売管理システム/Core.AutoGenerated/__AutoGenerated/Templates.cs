
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
    /// 入力チェックや保存処理の共通部分と、デモ101 テンプレートに残る機能で使うもの。販売管理のメッセージは MSG_DEMO101 に定義する。
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
        /// <summary>
        /// 未選択エラー
        /// </summary>
        public static string SelectRequiredError(string 項目名) => $"{TKind.文言(項目名)} を選択してください。";
        /// <summary>
        /// 未ログインエラー
        /// </summary>
        public static string NotLoggedInError() => $"ログインしていません。";
        /// <summary>
        /// ログイン一時停止
        /// </summary>
        public static string LoginLockedError() => $"ログインの失敗が続いたため、しばらくの間ログインできません。時間をおいてから、もう一度お試しください。";
        /// <summary>
        /// ログイン失敗
        /// <para>
        /// 従業員番号とパスワードのどちらが間違っているかは伝えない
        /// </para>
        /// </summary>
        public static string LoginFailedError() => $"従業員番号またはパスワードが間違っています。";
        /// <summary>
        /// パスワード未設定
        /// </summary>
        public static string PasswordNotSetError() => $"パスワードが設定されていません。";
        /// <summary>
        /// パスワード不一致
        /// </summary>
        public static string PasswordConfirmationMismatchError() => $"パスワードが一致しません。";
        /// <summary>
        /// 現在のパスワード誤り
        /// </summary>
        public static string CurrentPasswordIncorrectError() => $"現在のパスワードが正しくありません。";
        /// <summary>
        /// 従業員番号重複
        /// </summary>
        public static string DuplicateEmployeeNumberError(string 従業員番号) => $"従業員番号 '{TKind.文言(従業員番号)}' は既に存在します。";
        /// <summary>
        /// 自分自身の削除
        /// </summary>
        public static string CannotDeleteSelfError() => $"自分自身は削除できません。";
        /// <summary>
        /// 自分自身の管理者権限の削除
        /// </summary>
        public static string CannotRemoveOwnAdminPermissionError() => $"自分自身のシステム管理者の権限は外せません。";
        /// <summary>
        /// 従業員一括更新の確認
        /// </summary>
        public static string EmployeeUpdateConfirm() => $"更新しますか？";
        /// <summary>
        /// 変更なし
        /// </summary>
        public static string NothingChangedInfo() => $"更新された内容はありません。";
        /// <summary>
        /// 保存完了
        /// </summary>
        public static string SavedInfo() => $"保存しました。";
        /// <summary>
        /// 既読・未読の未指定
        /// </summary>
        public static string ReadStatusRequiredError() => $"既読にするか未読にするかを指定してください。";
        /// <summary>
        /// 通知不在
        /// </summary>
        public static string NotificationNotFoundError() => $"通知が見つかりません。";
        /// <summary>
        /// スプールのファイル名での絞り込み
        /// </summary>
        public static string SpoolFileNameFilterError() => $"スプールのファイルの名前では絞り込めません。";
        /// <summary>
        /// ジョブ正常終了の通知
        /// </summary>
        public static string JobSucceededNotification(string ジョブの種類) => $"ジョブ「{TKind.文言(ジョブの種類)}」が正常終了しました。";
        /// <summary>
        /// ジョブ異常終了の通知
        /// </summary>
        public static string JobFailedNotification(string ジョブの種類) => $"ジョブ「{TKind.文言(ジョブの種類)}」が異常終了しました。";
        /// <summary>
        /// ジョブ異常終了の結果
        /// </summary>
        public static string JobFailedResult() => $"エラーが発生したため、異常終了しました。";
        /// <summary>
        /// ジョブ中断の結果
        /// <para>
        /// ジョブの実行中に、サーバーの停止の合図を受け取ったとき
        /// </para>
        /// </summary>
        public static string JobInterruptedResult() => $"サーバーの停止により、中断しました。";
        /// <summary>
        /// 停止前に実行中だったジョブの結果
        /// <para>
        /// サーバーの起動時に、前回の停止のときに実行中だったジョブを見つけたとき
        /// </para>
        /// </summary>
        public static string JobInterruptedBeforeRestartResult() => $"サーバーが停止したため、中断しました。";
        /// <summary>
        /// 未定義の種類のジョブの結果
        /// </summary>
        public static string JobKindUndefinedResult() => $"ジョブの種類が定義されていないため、実行できませんでした。";
        /// <summary>
        /// クリーンナップの結果
        /// </summary>
        public static string CleanUpResult(int セッション, int 冪等キー, int 通知, int ジョブ) => $"セッション {TKind.文言(セッション)} 件、冪等キー {TKind.文言(冪等キー)} 件、通知 {TKind.文言(通知)} 件、ジョブ {TKind.文言(ジョブ)} 件を削除しました。";
    }
}

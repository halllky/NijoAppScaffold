
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
    /// <summary>種類「URL」の変換</summary>
    static abstract string URL(string value);
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
    /// API URL一覧
    /// <para>
    /// 画面から呼ぶ API の URL のうち、自動生成された処理のエンドポイント以外のもの。サーバーのルート定義と画面からの呼び出しで同じ URL を使うため。
    /// 値はオリジンを除いた、"/" で始まるパス。
    /// </para>
    /// </summary>
    public static class API_URL {
        /// <summary>
        /// ジョブのスプールのファイルのダウンロード
        /// </summary>
        public static string JobSpoolFile(string ジョブID, string ファイル名) => $"/api/job/{TKind.URL(ジョブID)}/spool/{TKind.URL(ファイル名)}";
        /// <summary>
        /// 添付ファイルの一時保存
        /// </summary>
        public static string AttachmentUpload() => $"/api/attachment";
        /// <summary>
        /// 添付ファイルのダウンロード
        /// </summary>
        public static string AttachmentDownload(string 識別子) => $"/api/attachment/{TKind.URL(識別子)}";
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
        public static string P101(string 売上SEQ) => $"/uriage/edit/{TKind.URL(売上SEQ)}";
        /// <summary>
        /// P301 商品詳細
        /// </summary>
        public static string P301(string 商品SEQ) => $"/shohin/{TKind.URL(商品SEQ)}";
    }
    /// <summary>
    /// メッセージ
    /// <para>
    /// 利用者に見せるメッセージ。名前がメッセージの ID になる。
    /// 入力チェックや保存処理の共通部分と、ログインや通知などの業務によらない機能で使うもの。業務ごとのメッセージは、業務ごとのグループに定義する。
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
        /// 添付できないファイルのエラー
        /// <para>
        /// データに添付しようとしたファイルが、存在しないか、他のユーザーが一時保存したものだったとき
        /// </para>
        /// </summary>
        public static string AttachmentNotAvailableError() => $"添付できないファイルが指定されています。ファイルを選び直してください。";
        /// <summary>
        /// 添付ファイルのサイズ超過エラー
        /// </summary>
        public static string AttachmentTooLargeError(string 上限) => $"ファイルのサイズが上限（{TKind.文言(上限)}）を超えています。";
        /// <summary>
        /// ジョブ異常終了の結果
        /// </summary>
        public static string JobFailedResult() => $"エラーが発生したため、異常終了しました。";
        /// <summary>
        /// ジョブ中断の結果
        /// <para>
        /// ジョブの実行中に、サーバーの停止でキャンセルされたとき
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
        public static string CleanUpResult(string セッション, string 冪等キー, string 通知, string ジョブ) => $"セッション {TKind.文言(セッション)} 件、冪等キー {TKind.文言(冪等キー)} 件、通知 {TKind.文言(通知)} 件、ジョブ {TKind.文言(ジョブ)} 件を削除しました。";
    }
    /// <summary>
    /// メッセージ(デモ101専用)
    /// <para>
    /// 販売管理のメッセージ。値の決まりは「MSG」と同じ。
    /// </para>
    /// </summary>
    public static class MSG_DEMO101 {
        /// <summary>
        /// 未指定エラー
        /// </summary>
        public static string NotSpecifiedError(string 項目名) => $"{TKind.文言(項目名)}が指定されていません。";
        /// <summary>
        /// 売上不在
        /// </summary>
        public static string SalesNotFoundError() => $"指定された売上データが見つかりません。";
        /// <summary>
        /// 売上明細なし（登録）
        /// </summary>
        public static string SalesDetailRequiredError() => $"売上明細を1件以上登録してください。";
        /// <summary>
        /// 売上明細なし
        /// </summary>
        public static string SalesDetailMinCountError() => $"売上明細は1件以上必要です。";
        /// <summary>
        /// 既存の売上明細の編集
        /// </summary>
        public static string ExistingSalesDetailNotEditableError() => $"既存の売上明細の編集・削除はできません。新規追加のみ可能です。";
        /// <summary>
        /// 新規登録時の取消明細
        /// </summary>
        public static string CancelDetailOnCreateError() => $"新規登録時に取消明細を追加することはできません。";
        /// <summary>
        /// 売上数量が0
        /// </summary>
        public static string SalesQuantityNonZeroError() => $"売上数量は0以外の整数を入力してください。";
        /// <summary>
        /// 売上数量が1未満
        /// </summary>
        public static string SalesQuantityPositiveError() => $"売上数量は1以上の整数を入力してください。";
        /// <summary>
        /// 明細の個数が負
        /// </summary>
        public static string DetailQuantityNegativeError() => $"明細の個数は0以上でなければなりません。";
        /// <summary>
        /// 金額が0以外
        /// </summary>
        public static string AmountMustBeZeroError() => $"金額は0のみ指定可能です。";
        /// <summary>
        /// 売上明細の金額が負
        /// </summary>
        public static string SalesAmountNegativeError() => $"売上明細の金額は0以上でなければなりません。";
        /// <summary>
        /// 取消明細の金額が正
        /// </summary>
        public static string CancelAmountPositiveError() => $"取消明細の金額は0以下でなければなりません。";
        /// <summary>
        /// 在庫引当済みの明細の削除
        /// </summary>
        public static string AllocatedDetailDeleteError() => $"在庫引当済みのため、既存の明細を削除することはできません。取消明細を追加して調整してください。";
        /// <summary>
        /// 在庫引当済みの明細の変更
        /// </summary>
        public static string AllocatedDetailChangeError() => $"在庫引当済みのため変更できません。新しい明細を追加して調整してください。";
        /// <summary>
        /// 在庫不足
        /// </summary>
        public static string InsufficientStockError(string 現在在庫) => $"在庫不足です。（現在在庫: {TKind.文言(現在在庫)}）";
        /// <summary>
        /// 取消可能な在庫引当の不足
        /// </summary>
        public static string InsufficientReturnableStockError(string 取消可能数) => $"取消可能な在庫引当履歴が不足しています。（取消可能数: {TKind.文言(取消可能数)}）";
        /// <summary>
        /// 売上登録の確認
        /// </summary>
        public static string SalesRegisterConfirm() => $"登録を確定します。よろしいですか？";
        /// <summary>
        /// 売上登録完了
        /// </summary>
        public static string SalesRegisteredInfo() => $"売上登録が完了しました。";
        /// <summary>
        /// 売上修正の確認
        /// </summary>
        public static string SalesUpdateConfirm() => $"修正を確定します。よろしいですか？";
        /// <summary>
        /// 売上修正完了
        /// </summary>
        public static string SalesUpdatedInfo() => $"売上修正が完了しました。";
        /// <summary>
        /// 担当の売上の修正の通知
        /// </summary>
        public static string SalesUpdatedNotification(string 売上SEQ, string 修正者) => $"担当の売上 {TKind.文言(売上SEQ)} が {TKind.文言(修正者)} さんにより修正されました。";
        /// <summary>
        /// 在庫調整の数量未指定
        /// </summary>
        public static string StockAdjustmentAmountRequiredError() => $"増減数または絶対数のいずれかを指定してください。";
        /// <summary>
        /// 在庫調整の数量の重複指定
        /// </summary>
        public static string StockAdjustmentAmountConflictError() => $"増減数と絶対数の両方を指定することはできません。";
        /// <summary>
        /// 在庫数の変更なし
        /// </summary>
        public static string StockNotChangedError() => $"在庫数に変更がありません。";
        /// <summary>
        /// 減算時の在庫不足
        /// </summary>
        public static string InsufficientStockForSubtractionError(string 現在在庫, string 減算指定) => $"在庫不足のため減算できません。（現在在庫: {TKind.文言(現在在庫)}, 減算指定: {TKind.文言(減算指定)}）";
        /// <summary>
        /// 在庫調整の通知
        /// <para>
        /// 増減数は、符号を付けて渡す（+3、-2 など）
        /// </para>
        /// </summary>
        public static string StockAdjustedNotification(string 商品名, string 調整者, string 増減数) => $"商品「{TKind.文言(商品名)}」の在庫が {TKind.文言(調整者)} さんにより調整されました（{TKind.文言(増減数)}個）。";
        /// <summary>
        /// 入荷商品なし
        /// </summary>
        public static string ArrivalItemRequiredError() => $"入荷商品を1件以上登録してください。";
        /// <summary>
        /// 入荷不在
        /// </summary>
        public static string ArrivalNotFoundError(string 入荷ID) => $"入荷データが見つかりません。(ID: {TKind.文言(入荷ID)})";
        /// <summary>
        /// 入荷明細不在
        /// </summary>
        public static string ArrivalDetailNotFoundError(string 入荷明細ID) => $"指定された入荷明細が存在しません。(ID: {TKind.文言(入荷明細ID)})";
        /// <summary>
        /// 出荷引当済みの入荷の商品の変更
        /// </summary>
        public static string AllocatedArrivalProductChangeError() => $"既に出荷引当が行われているため、商品は変更できません。";
        /// <summary>
        /// 出荷引当済みの入荷の削除
        /// </summary>
        public static string AllocatedArrivalDeleteError(string 商品名) => $"既に出荷引当が行われているため、削除できません。(商品: {TKind.文言(商品名)})";
        /// <summary>
        /// 使用済みの数量を下回る入荷数量
        /// </summary>
        public static string ArrivalQuantityBelowUsedError(string 使用済みの数量) => $"入荷数量を既に使用されている数量({TKind.文言(使用済みの数量)})未満にすることはできません。";
        /// <summary>
        /// 入荷修正の確認
        /// </summary>
        public static string ArrivalUpdateConfirm() => $"入荷データを修正します。よろしいですか？";
        /// <summary>
        /// 入荷登録完了
        /// </summary>
        public static string ArrivalRegisteredInfo() => $"入荷登録が完了しました。";
        /// <summary>
        /// 商品データ取込の確認
        /// </summary>
        public static string ProductImportConfirm() => $"商品データの取込を開始します。終わったら通知でお知らせします。よろしいですか？";
        /// <summary>
        /// 商品データ取込の結果
        /// </summary>
        public static string ProductImportResult(string 件数, string 取込件数) => $"{TKind.文言(件数)} 件のうち、{TKind.文言(取込件数)} 件を取り込みました。";
        /// <summary>
        /// 商品データ取込の結果（失敗あり）
        /// </summary>
        public static string ProductImportPartiallyFailedResult(string 件数, string 取込件数, string 失敗件数) => $"{TKind.文言(件数)} 件のうち、{TKind.文言(取込件数)} 件を取り込みました。{TKind.文言(失敗件数)} 件は取り込めませんでした。理由は取込結果のファイルを参照してください。";
        /// <summary>
        /// 不正な消費税区分
        /// <para>
        /// 商品データ取込の取込結果のファイルに出す、取り込めなかった理由
        /// </para>
        /// </summary>
        public static string InvalidTaxTypeReason(string 消費税区分) => $"不正な消費税区分です: {TKind.文言(消費税区分)}";
        /// <summary>
        /// 負の値
        /// <para>
        /// カスタム属性「0以上のみ」の入力チェック
        /// </para>
        /// </summary>
        public static string NegativeValueError() => $"負の値は許可されていません。";
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
    /// API URL一覧
    /// <para>
    /// 画面から呼ぶ API の URL のうち、自動生成された処理のエンドポイント以外のもの。サーバーのルート定義と画面からの呼び出しで同じ URL を使うため。
    /// 値はオリジンを除いた、"/" で始まるパス。
    /// </para>
    /// </summary>
    public static class API_URL {
        /// <summary>
        /// ジョブのスプールのファイルのダウンロード
        /// </summary>
        public static string JobSpoolFile(string ジョブID, string ファイル名) => $"/api/job/{ジョブID}/spool/{ファイル名}";
        /// <summary>
        /// 添付ファイルの一時保存
        /// </summary>
        public static string AttachmentUpload() => $"/api/attachment";
        /// <summary>
        /// 添付ファイルのダウンロード
        /// </summary>
        public static string AttachmentDownload(string 識別子) => $"/api/attachment/{識別子}";
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
        public static string P101(string 売上SEQ) => $"/uriage/edit/{売上SEQ}";
        /// <summary>
        /// P301 商品詳細
        /// </summary>
        public static string P301(string 商品SEQ) => $"/shohin/{商品SEQ}";
    }
    /// <summary>
    /// メッセージ
    /// <para>
    /// 利用者に見せるメッセージ。名前がメッセージの ID になる。
    /// 入力チェックや保存処理の共通部分と、ログインや通知などの業務によらない機能で使うもの。業務ごとのメッセージは、業務ごとのグループに定義する。
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
        /// <summary>
        /// 未選択エラー
        /// </summary>
        public static string SelectRequiredError(string 項目名) => $"{項目名} を選択してください。";
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
        public static string DuplicateEmployeeNumberError(string 従業員番号) => $"従業員番号 '{従業員番号}' は既に存在します。";
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
        public static string JobSucceededNotification(string ジョブの種類) => $"ジョブ「{ジョブの種類}」が正常終了しました。";
        /// <summary>
        /// ジョブ異常終了の通知
        /// </summary>
        public static string JobFailedNotification(string ジョブの種類) => $"ジョブ「{ジョブの種類}」が異常終了しました。";
        /// <summary>
        /// 添付できないファイルのエラー
        /// <para>
        /// データに添付しようとしたファイルが、存在しないか、他のユーザーが一時保存したものだったとき
        /// </para>
        /// </summary>
        public static string AttachmentNotAvailableError() => $"添付できないファイルが指定されています。ファイルを選び直してください。";
        /// <summary>
        /// 添付ファイルのサイズ超過エラー
        /// </summary>
        public static string AttachmentTooLargeError(string 上限) => $"ファイルのサイズが上限（{上限}）を超えています。";
        /// <summary>
        /// ジョブ異常終了の結果
        /// </summary>
        public static string JobFailedResult() => $"エラーが発生したため、異常終了しました。";
        /// <summary>
        /// ジョブ中断の結果
        /// <para>
        /// ジョブの実行中に、サーバーの停止でキャンセルされたとき
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
        public static string CleanUpResult(string セッション, string 冪等キー, string 通知, string ジョブ) => $"セッション {セッション} 件、冪等キー {冪等キー} 件、通知 {通知} 件、ジョブ {ジョブ} 件を削除しました。";
    }
    /// <summary>
    /// メッセージ(デモ101専用)
    /// <para>
    /// 販売管理のメッセージ。値の決まりは「MSG」と同じ。
    /// </para>
    /// </summary>
    public static class MSG_DEMO101 {
        /// <summary>
        /// 未指定エラー
        /// </summary>
        public static string NotSpecifiedError(string 項目名) => $"{項目名}が指定されていません。";
        /// <summary>
        /// 売上不在
        /// </summary>
        public static string SalesNotFoundError() => $"指定された売上データが見つかりません。";
        /// <summary>
        /// 売上明細なし（登録）
        /// </summary>
        public static string SalesDetailRequiredError() => $"売上明細を1件以上登録してください。";
        /// <summary>
        /// 売上明細なし
        /// </summary>
        public static string SalesDetailMinCountError() => $"売上明細は1件以上必要です。";
        /// <summary>
        /// 既存の売上明細の編集
        /// </summary>
        public static string ExistingSalesDetailNotEditableError() => $"既存の売上明細の編集・削除はできません。新規追加のみ可能です。";
        /// <summary>
        /// 新規登録時の取消明細
        /// </summary>
        public static string CancelDetailOnCreateError() => $"新規登録時に取消明細を追加することはできません。";
        /// <summary>
        /// 売上数量が0
        /// </summary>
        public static string SalesQuantityNonZeroError() => $"売上数量は0以外の整数を入力してください。";
        /// <summary>
        /// 売上数量が1未満
        /// </summary>
        public static string SalesQuantityPositiveError() => $"売上数量は1以上の整数を入力してください。";
        /// <summary>
        /// 明細の個数が負
        /// </summary>
        public static string DetailQuantityNegativeError() => $"明細の個数は0以上でなければなりません。";
        /// <summary>
        /// 金額が0以外
        /// </summary>
        public static string AmountMustBeZeroError() => $"金額は0のみ指定可能です。";
        /// <summary>
        /// 売上明細の金額が負
        /// </summary>
        public static string SalesAmountNegativeError() => $"売上明細の金額は0以上でなければなりません。";
        /// <summary>
        /// 取消明細の金額が正
        /// </summary>
        public static string CancelAmountPositiveError() => $"取消明細の金額は0以下でなければなりません。";
        /// <summary>
        /// 在庫引当済みの明細の削除
        /// </summary>
        public static string AllocatedDetailDeleteError() => $"在庫引当済みのため、既存の明細を削除することはできません。取消明細を追加して調整してください。";
        /// <summary>
        /// 在庫引当済みの明細の変更
        /// </summary>
        public static string AllocatedDetailChangeError() => $"在庫引当済みのため変更できません。新しい明細を追加して調整してください。";
        /// <summary>
        /// 在庫不足
        /// </summary>
        public static string InsufficientStockError(string 現在在庫) => $"在庫不足です。（現在在庫: {現在在庫}）";
        /// <summary>
        /// 取消可能な在庫引当の不足
        /// </summary>
        public static string InsufficientReturnableStockError(string 取消可能数) => $"取消可能な在庫引当履歴が不足しています。（取消可能数: {取消可能数}）";
        /// <summary>
        /// 売上登録の確認
        /// </summary>
        public static string SalesRegisterConfirm() => $"登録を確定します。よろしいですか？";
        /// <summary>
        /// 売上登録完了
        /// </summary>
        public static string SalesRegisteredInfo() => $"売上登録が完了しました。";
        /// <summary>
        /// 売上修正の確認
        /// </summary>
        public static string SalesUpdateConfirm() => $"修正を確定します。よろしいですか？";
        /// <summary>
        /// 売上修正完了
        /// </summary>
        public static string SalesUpdatedInfo() => $"売上修正が完了しました。";
        /// <summary>
        /// 担当の売上の修正の通知
        /// </summary>
        public static string SalesUpdatedNotification(string 売上SEQ, string 修正者) => $"担当の売上 {売上SEQ} が {修正者} さんにより修正されました。";
        /// <summary>
        /// 在庫調整の数量未指定
        /// </summary>
        public static string StockAdjustmentAmountRequiredError() => $"増減数または絶対数のいずれかを指定してください。";
        /// <summary>
        /// 在庫調整の数量の重複指定
        /// </summary>
        public static string StockAdjustmentAmountConflictError() => $"増減数と絶対数の両方を指定することはできません。";
        /// <summary>
        /// 在庫数の変更なし
        /// </summary>
        public static string StockNotChangedError() => $"在庫数に変更がありません。";
        /// <summary>
        /// 減算時の在庫不足
        /// </summary>
        public static string InsufficientStockForSubtractionError(string 現在在庫, string 減算指定) => $"在庫不足のため減算できません。（現在在庫: {現在在庫}, 減算指定: {減算指定}）";
        /// <summary>
        /// 在庫調整の通知
        /// <para>
        /// 増減数は、符号を付けて渡す（+3、-2 など）
        /// </para>
        /// </summary>
        public static string StockAdjustedNotification(string 商品名, string 調整者, string 増減数) => $"商品「{商品名}」の在庫が {調整者} さんにより調整されました（{増減数}個）。";
        /// <summary>
        /// 入荷商品なし
        /// </summary>
        public static string ArrivalItemRequiredError() => $"入荷商品を1件以上登録してください。";
        /// <summary>
        /// 入荷不在
        /// </summary>
        public static string ArrivalNotFoundError(string 入荷ID) => $"入荷データが見つかりません。(ID: {入荷ID})";
        /// <summary>
        /// 入荷明細不在
        /// </summary>
        public static string ArrivalDetailNotFoundError(string 入荷明細ID) => $"指定された入荷明細が存在しません。(ID: {入荷明細ID})";
        /// <summary>
        /// 出荷引当済みの入荷の商品の変更
        /// </summary>
        public static string AllocatedArrivalProductChangeError() => $"既に出荷引当が行われているため、商品は変更できません。";
        /// <summary>
        /// 出荷引当済みの入荷の削除
        /// </summary>
        public static string AllocatedArrivalDeleteError(string 商品名) => $"既に出荷引当が行われているため、削除できません。(商品: {商品名})";
        /// <summary>
        /// 使用済みの数量を下回る入荷数量
        /// </summary>
        public static string ArrivalQuantityBelowUsedError(string 使用済みの数量) => $"入荷数量を既に使用されている数量({使用済みの数量})未満にすることはできません。";
        /// <summary>
        /// 入荷修正の確認
        /// </summary>
        public static string ArrivalUpdateConfirm() => $"入荷データを修正します。よろしいですか？";
        /// <summary>
        /// 入荷登録完了
        /// </summary>
        public static string ArrivalRegisteredInfo() => $"入荷登録が完了しました。";
        /// <summary>
        /// 商品データ取込の確認
        /// </summary>
        public static string ProductImportConfirm() => $"商品データの取込を開始します。終わったら通知でお知らせします。よろしいですか？";
        /// <summary>
        /// 商品データ取込の結果
        /// </summary>
        public static string ProductImportResult(string 件数, string 取込件数) => $"{件数} 件のうち、{取込件数} 件を取り込みました。";
        /// <summary>
        /// 商品データ取込の結果（失敗あり）
        /// </summary>
        public static string ProductImportPartiallyFailedResult(string 件数, string 取込件数, string 失敗件数) => $"{件数} 件のうち、{取込件数} 件を取り込みました。{失敗件数} 件は取り込めませんでした。理由は取込結果のファイルを参照してください。";
        /// <summary>
        /// 不正な消費税区分
        /// <para>
        /// 商品データ取込の取込結果のファイルに出す、取り込めなかった理由
        /// </para>
        /// </summary>
        public static string InvalidTaxTypeReason(string 消費税区分) => $"不正な消費税区分です: {消費税区分}";
        /// <summary>
        /// 負の値
        /// <para>
        /// カスタム属性「0以上のみ」の入力チェック
        /// </para>
        /// </summary>
        public static string NegativeValueError() => $"負の値は許可されていません。";
    }
}

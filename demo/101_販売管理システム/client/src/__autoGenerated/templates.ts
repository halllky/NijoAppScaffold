
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

/**
 * テンプレート文字列の種類ごとの、引数の値を埋め込む前の変換（エスケープなど）。
 * 種類は nijo.xml の TemplateKind 属性で指定したもの。
 * サーバーと画面で同じ文字列になるように、C# 側の変換と同じ結果を返すこと。
 */
export type TemplateKind = {
  /** 種類「URL」の変換 */
  URL: (value: string) => string
  /** 種類「文言」の変換 */
  文言: (value: string) => string
}

/**
 * nijo.xml の Templates セクションに定義したテンプレート文字列を返します。
 * 引数の値は、渡された変換を通してから埋め込む。
 * アプリからは、この関数を1回だけ呼んだ結果を export し、それを通して使う。
 * 宣言されていない変換を余分に渡してもエラーにしない。C# で余分なメソッドがあってもエラーにならないのと同じく、
 * nijo.xml から無くなった種類の変換を、消さずに残しておけるようにするため。
 */
export const createTemplates = <TKind extends TemplateKind>(templateKind: TKind) => ({
  /**
   * 画面URL一覧
   *
   * 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  PAGES_URL: {
    /**
     * P400 従業員
     */
    P400: (): string => `/jugyoin`,
    /**
     * P004 ジョブ一覧
     */
    P004: (): string => `/job`,
  },
  /**
   * API URL一覧
   *
   * 画面から呼ぶ API の URL のうち、自動生成された処理のエンドポイント以外のもの。サーバーのルート定義と画面からの呼び出しで同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  API_URL: {
    /**
     * ジョブのスプールのファイルのダウンロード
     */
    JobSpoolFile: ({ ジョブID, ファイル名 }: { ジョブID: string, ファイル名: string }): string => `/api/job/${templateKind.URL(ジョブID)}/spool/${templateKind.URL(ファイル名)}`,
  },
  /**
   * 画面URL一覧(デモ101専用)
   *
   * 販売管理の画面の URL。値の決まりは「PAGES_URL」と同じ。
   */
  PAGES_URL_DEMO101: {
    /**
     * P101 売上詳細
     */
    P101: ({ 売上SEQ }: { 売上SEQ: string }): string => `/uriage/edit/${templateKind.URL(売上SEQ)}`,
    /**
     * P301 商品詳細
     */
    P301: ({ 商品SEQ }: { 商品SEQ: string }): string => `/shohin/${templateKind.URL(商品SEQ)}`,
  },
  /**
   * メッセージ
   *
   * 利用者に見せるメッセージ。名前がメッセージの ID になる。
   * 入力チェックや保存処理の共通部分と、ログインや通知などの業務によらない機能で使うもの。業務ごとのメッセージは、業務ごとのグループに定義する。
   */
  MSG: {
    /**
     * 必須入力エラー
     */
    RequiredError: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言(項目名)} を入力してください。`,
    /**
     * 文字数超過エラー
     */
    MaxLengthError: ({ 項目名, 最大文字数 }: { 項目名: string, 最大文字数: string }): string => `${templateKind.文言(項目名)} は ${templateKind.文言(最大文字数)} 文字以内で入力してください。`,
    /**
     * 桁数超過エラー
     */
    DigitsError: ({ 項目名, 整数部の桁数, 小数部の桁数 }: { 項目名: string, 整数部の桁数: string, 小数部の桁数: string }): string => `${templateKind.文言(項目名)} は整数部 ${templateKind.文言(整数部の桁数)} 桁、小数部 ${templateKind.文言(小数部の桁数)} 桁以内で入力してください。`,
    /**
     * 区分値不在エラー
     */
    GenericLookupValueNotFound: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言(項目名)} に指定された区分値が存在しません。`,
    /**
     * 楽観排他エラー
     *
     * データを読み込んだ後に、ほかのユーザーが同じデータを更新していたとき
     */
    ConcurrencyError: (): string => `ほかのユーザーが更新しました。`,
    /**
     * データ不在エラー
     *
     * 更新対象・削除対象のデータがデータベース上で見つからなかったとき
     */
    DataNotFoundError: (): string => `更新対象のデータが見つかりません。`,
    /**
     * 一意制約違反エラー
     */
    UniqueConstraintError: (): string => `同じ値のデータが既に存在します。`,
    /**
     * 保存時エラー
     *
     * 登録・更新・削除のときに、データベースで何らかのエラーが起きたとき。エラーの内容は利用者に見せない
     */
    UnknownUpdateError: (): string => `登録処理でエラーが発生しました。`,
    /**
     * 未選択エラー
     */
    SelectRequiredError: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言(項目名)} を選択してください。`,
    /**
     * 未ログインエラー
     */
    NotLoggedInError: (): string => `ログインしていません。`,
    /**
     * ログイン一時停止
     */
    LoginLockedError: (): string => `ログインの失敗が続いたため、しばらくの間ログインできません。時間をおいてから、もう一度お試しください。`,
    /**
     * ログイン失敗
     *
     * 従業員番号とパスワードのどちらが間違っているかは伝えない
     */
    LoginFailedError: (): string => `従業員番号またはパスワードが間違っています。`,
    /**
     * パスワード未設定
     */
    PasswordNotSetError: (): string => `パスワードが設定されていません。`,
    /**
     * パスワード不一致
     */
    PasswordConfirmationMismatchError: (): string => `パスワードが一致しません。`,
    /**
     * 現在のパスワード誤り
     */
    CurrentPasswordIncorrectError: (): string => `現在のパスワードが正しくありません。`,
    /**
     * 従業員番号重複
     */
    DuplicateEmployeeNumberError: ({ 従業員番号 }: { 従業員番号: string }): string => `従業員番号 '${templateKind.文言(従業員番号)}' は既に存在します。`,
    /**
     * 自分自身の削除
     */
    CannotDeleteSelfError: (): string => `自分自身は削除できません。`,
    /**
     * 自分自身の管理者権限の削除
     */
    CannotRemoveOwnAdminPermissionError: (): string => `自分自身のシステム管理者の権限は外せません。`,
    /**
     * 従業員一括更新の確認
     */
    EmployeeUpdateConfirm: (): string => `更新しますか？`,
    /**
     * 変更なし
     */
    NothingChangedInfo: (): string => `更新された内容はありません。`,
    /**
     * 保存完了
     */
    SavedInfo: (): string => `保存しました。`,
    /**
     * 既読・未読の未指定
     */
    ReadStatusRequiredError: (): string => `既読にするか未読にするかを指定してください。`,
    /**
     * 通知不在
     */
    NotificationNotFoundError: (): string => `通知が見つかりません。`,
    /**
     * スプールのファイル名での絞り込み
     */
    SpoolFileNameFilterError: (): string => `スプールのファイルの名前では絞り込めません。`,
    /**
     * ジョブ正常終了の通知
     */
    JobSucceededNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${templateKind.文言(ジョブの種類)}」が正常終了しました。`,
    /**
     * ジョブ異常終了の通知
     */
    JobFailedNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${templateKind.文言(ジョブの種類)}」が異常終了しました。`,
    /**
     * ジョブ異常終了の結果
     */
    JobFailedResult: (): string => `エラーが発生したため、異常終了しました。`,
    /**
     * ジョブ中断の結果
     *
     * ジョブの実行中に、サーバーの停止でキャンセルされたとき
     */
    JobInterruptedResult: (): string => `サーバーの停止により、中断しました。`,
    /**
     * 停止前に実行中だったジョブの結果
     *
     * サーバーの起動時に、前回の停止のときに実行中だったジョブを見つけたとき
     */
    JobInterruptedBeforeRestartResult: (): string => `サーバーが停止したため、中断しました。`,
    /**
     * 未定義の種類のジョブの結果
     */
    JobKindUndefinedResult: (): string => `ジョブの種類が定義されていないため、実行できませんでした。`,
    /**
     * クリーンナップの結果
     */
    CleanUpResult: ({ セッション, 冪等キー, 通知, ジョブ }: { セッション: string, 冪等キー: string, 通知: string, ジョブ: string }): string => `セッション ${templateKind.文言(セッション)} 件、冪等キー ${templateKind.文言(冪等キー)} 件、通知 ${templateKind.文言(通知)} 件、ジョブ ${templateKind.文言(ジョブ)} 件を削除しました。`,
  },
  /**
   * メッセージ(デモ101専用)
   *
   * 販売管理のメッセージ。値の決まりは「MSG」と同じ。
   */
  MSG_DEMO101: {
    /**
     * 未指定エラー
     */
    NotSpecifiedError: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言(項目名)}が指定されていません。`,
    /**
     * 売上不在
     */
    SalesNotFoundError: (): string => `指定された売上データが見つかりません。`,
    /**
     * 売上明細なし（登録）
     */
    SalesDetailRequiredError: (): string => `売上明細を1件以上登録してください。`,
    /**
     * 売上明細なし
     */
    SalesDetailMinCountError: (): string => `売上明細は1件以上必要です。`,
    /**
     * 既存の売上明細の編集
     */
    ExistingSalesDetailNotEditableError: (): string => `既存の売上明細の編集・削除はできません。新規追加のみ可能です。`,
    /**
     * 新規登録時の取消明細
     */
    CancelDetailOnCreateError: (): string => `新規登録時に取消明細を追加することはできません。`,
    /**
     * 売上数量が0
     */
    SalesQuantityNonZeroError: (): string => `売上数量は0以外の整数を入力してください。`,
    /**
     * 売上数量が1未満
     */
    SalesQuantityPositiveError: (): string => `売上数量は1以上の整数を入力してください。`,
    /**
     * 明細の個数が負
     */
    DetailQuantityNegativeError: (): string => `明細の個数は0以上でなければなりません。`,
    /**
     * 金額が0以外
     */
    AmountMustBeZeroError: (): string => `金額は0のみ指定可能です。`,
    /**
     * 売上明細の金額が負
     */
    SalesAmountNegativeError: (): string => `売上明細の金額は0以上でなければなりません。`,
    /**
     * 取消明細の金額が正
     */
    CancelAmountPositiveError: (): string => `取消明細の金額は0以下でなければなりません。`,
    /**
     * 在庫引当済みの明細の削除
     */
    AllocatedDetailDeleteError: (): string => `在庫引当済みのため、既存の明細を削除することはできません。取消明細を追加して調整してください。`,
    /**
     * 在庫引当済みの明細の変更
     */
    AllocatedDetailChangeError: (): string => `在庫引当済みのため変更できません。新しい明細を追加して調整してください。`,
    /**
     * 在庫不足
     */
    InsufficientStockError: ({ 現在在庫 }: { 現在在庫: string }): string => `在庫不足です。（現在在庫: ${templateKind.文言(現在在庫)}）`,
    /**
     * 取消可能な在庫引当の不足
     */
    InsufficientReturnableStockError: ({ 取消可能数 }: { 取消可能数: string }): string => `取消可能な在庫引当履歴が不足しています。（取消可能数: ${templateKind.文言(取消可能数)}）`,
    /**
     * 売上登録の確認
     */
    SalesRegisterConfirm: (): string => `登録を確定します。よろしいですか？`,
    /**
     * 売上登録完了
     */
    SalesRegisteredInfo: (): string => `売上登録が完了しました。`,
    /**
     * 売上修正の確認
     */
    SalesUpdateConfirm: (): string => `修正を確定します。よろしいですか？`,
    /**
     * 売上修正完了
     */
    SalesUpdatedInfo: (): string => `売上修正が完了しました。`,
    /**
     * 担当の売上の修正の通知
     */
    SalesUpdatedNotification: ({ 売上SEQ, 修正者 }: { 売上SEQ: string, 修正者: string }): string => `担当の売上 ${templateKind.文言(売上SEQ)} が ${templateKind.文言(修正者)} さんにより修正されました。`,
    /**
     * 在庫調整の数量未指定
     */
    StockAdjustmentAmountRequiredError: (): string => `増減数または絶対数のいずれかを指定してください。`,
    /**
     * 在庫調整の数量の重複指定
     */
    StockAdjustmentAmountConflictError: (): string => `増減数と絶対数の両方を指定することはできません。`,
    /**
     * 在庫数の変更なし
     */
    StockNotChangedError: (): string => `在庫数に変更がありません。`,
    /**
     * 減算時の在庫不足
     */
    InsufficientStockForSubtractionError: ({ 現在在庫, 減算指定 }: { 現在在庫: string, 減算指定: string }): string => `在庫不足のため減算できません。（現在在庫: ${templateKind.文言(現在在庫)}, 減算指定: ${templateKind.文言(減算指定)}）`,
    /**
     * 在庫調整の通知
     *
     * 増減数は、符号を付けて渡す（+3、-2 など）
     */
    StockAdjustedNotification: ({ 商品名, 調整者, 増減数 }: { 商品名: string, 調整者: string, 増減数: string }): string => `商品「${templateKind.文言(商品名)}」の在庫が ${templateKind.文言(調整者)} さんにより調整されました（${templateKind.文言(増減数)}個）。`,
    /**
     * 入荷商品なし
     */
    ArrivalItemRequiredError: (): string => `入荷商品を1件以上登録してください。`,
    /**
     * 入荷不在
     */
    ArrivalNotFoundError: ({ 入荷ID }: { 入荷ID: string }): string => `入荷データが見つかりません。(ID: ${templateKind.文言(入荷ID)})`,
    /**
     * 入荷明細不在
     */
    ArrivalDetailNotFoundError: ({ 入荷明細ID }: { 入荷明細ID: string }): string => `指定された入荷明細が存在しません。(ID: ${templateKind.文言(入荷明細ID)})`,
    /**
     * 出荷引当済みの入荷の商品の変更
     */
    AllocatedArrivalProductChangeError: (): string => `既に出荷引当が行われているため、商品は変更できません。`,
    /**
     * 出荷引当済みの入荷の削除
     */
    AllocatedArrivalDeleteError: ({ 商品名 }: { 商品名: string }): string => `既に出荷引当が行われているため、削除できません。(商品: ${templateKind.文言(商品名)})`,
    /**
     * 使用済みの数量を下回る入荷数量
     */
    ArrivalQuantityBelowUsedError: ({ 使用済みの数量 }: { 使用済みの数量: string }): string => `入荷数量を既に使用されている数量(${templateKind.文言(使用済みの数量)})未満にすることはできません。`,
    /**
     * 入荷修正の確認
     */
    ArrivalUpdateConfirm: (): string => `入荷データを修正します。よろしいですか？`,
    /**
     * 入荷登録完了
     */
    ArrivalRegisteredInfo: (): string => `入荷登録が完了しました。`,
    /**
     * 商品データ取込の確認
     */
    ProductImportConfirm: (): string => `商品データの取込を開始します。終わったら通知でお知らせします。よろしいですか？`,
    /**
     * 商品データ取込の結果
     */
    ProductImportResult: ({ 件数, 取込件数 }: { 件数: string, 取込件数: string }): string => `${templateKind.文言(件数)} 件のうち、${templateKind.文言(取込件数)} 件を取り込みました。`,
    /**
     * 商品データ取込の結果（失敗あり）
     */
    ProductImportPartiallyFailedResult: ({ 件数, 取込件数, 失敗件数 }: { 件数: string, 取込件数: string, 失敗件数: string }): string => `${templateKind.文言(件数)} 件のうち、${templateKind.文言(取込件数)} 件を取り込みました。${templateKind.文言(失敗件数)} 件は取り込めませんでした。理由は取込結果のファイルを参照してください。`,
    /**
     * 不正な消費税区分
     *
     * 商品データ取込の取込結果のファイルに出す、取り込めなかった理由
     */
    InvalidTaxTypeReason: ({ 消費税区分 }: { 消費税区分: string }): string => `不正な消費税区分です: ${templateKind.文言(消費税区分)}`,
    /**
     * 負の値
     *
     * カスタム属性「0以上のみ」の入力チェック
     */
    NegativeValueError: (): string => `負の値は許可されていません。`,
  },
})

/**
 * nijo.xml の Templates セクションに定義したテンプレート文字列の、
 * 引数の位置に渡された文字列を変換せずにそのまま置く関数。
 * React Router のルート定義（"/uriage/edit/:id"）のように、引数の位置に値以外のものを置くときに使う。
 */
export const TemplatePattern = {
  /**
   * 画面URL一覧
   *
   * 画面の URL。サーバーで組み立てて通知のリンク先に使い、画面のルーティングと他の画面からのリンクもここから組み立てる。サーバーと画面で同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  PAGES_URL: {
    /**
     * P400 従業員
     */
    P400: (): string => `/jugyoin`,
    /**
     * P004 ジョブ一覧
     */
    P004: (): string => `/job`,
  },
  /**
   * API URL一覧
   *
   * 画面から呼ぶ API の URL のうち、自動生成された処理のエンドポイント以外のもの。サーバーのルート定義と画面からの呼び出しで同じ URL を使うため。
   * 値はオリジンを除いた、"/" で始まるパス。
   */
  API_URL: {
    /**
     * ジョブのスプールのファイルのダウンロード
     */
    JobSpoolFile: ({ ジョブID, ファイル名 }: { ジョブID: string, ファイル名: string }): string => `/api/job/${ジョブID}/spool/${ファイル名}`,
  },
  /**
   * 画面URL一覧(デモ101専用)
   *
   * 販売管理の画面の URL。値の決まりは「PAGES_URL」と同じ。
   */
  PAGES_URL_DEMO101: {
    /**
     * P101 売上詳細
     */
    P101: ({ 売上SEQ }: { 売上SEQ: string }): string => `/uriage/edit/${売上SEQ}`,
    /**
     * P301 商品詳細
     */
    P301: ({ 商品SEQ }: { 商品SEQ: string }): string => `/shohin/${商品SEQ}`,
  },
  /**
   * メッセージ
   *
   * 利用者に見せるメッセージ。名前がメッセージの ID になる。
   * 入力チェックや保存処理の共通部分と、ログインや通知などの業務によらない機能で使うもの。業務ごとのメッセージは、業務ごとのグループに定義する。
   */
  MSG: {
    /**
     * 必須入力エラー
     */
    RequiredError: ({ 項目名 }: { 項目名: string }): string => `${項目名} を入力してください。`,
    /**
     * 文字数超過エラー
     */
    MaxLengthError: ({ 項目名, 最大文字数 }: { 項目名: string, 最大文字数: string }): string => `${項目名} は ${最大文字数} 文字以内で入力してください。`,
    /**
     * 桁数超過エラー
     */
    DigitsError: ({ 項目名, 整数部の桁数, 小数部の桁数 }: { 項目名: string, 整数部の桁数: string, 小数部の桁数: string }): string => `${項目名} は整数部 ${整数部の桁数} 桁、小数部 ${小数部の桁数} 桁以内で入力してください。`,
    /**
     * 区分値不在エラー
     */
    GenericLookupValueNotFound: ({ 項目名 }: { 項目名: string }): string => `${項目名} に指定された区分値が存在しません。`,
    /**
     * 楽観排他エラー
     *
     * データを読み込んだ後に、ほかのユーザーが同じデータを更新していたとき
     */
    ConcurrencyError: (): string => `ほかのユーザーが更新しました。`,
    /**
     * データ不在エラー
     *
     * 更新対象・削除対象のデータがデータベース上で見つからなかったとき
     */
    DataNotFoundError: (): string => `更新対象のデータが見つかりません。`,
    /**
     * 一意制約違反エラー
     */
    UniqueConstraintError: (): string => `同じ値のデータが既に存在します。`,
    /**
     * 保存時エラー
     *
     * 登録・更新・削除のときに、データベースで何らかのエラーが起きたとき。エラーの内容は利用者に見せない
     */
    UnknownUpdateError: (): string => `登録処理でエラーが発生しました。`,
    /**
     * 未選択エラー
     */
    SelectRequiredError: ({ 項目名 }: { 項目名: string }): string => `${項目名} を選択してください。`,
    /**
     * 未ログインエラー
     */
    NotLoggedInError: (): string => `ログインしていません。`,
    /**
     * ログイン一時停止
     */
    LoginLockedError: (): string => `ログインの失敗が続いたため、しばらくの間ログインできません。時間をおいてから、もう一度お試しください。`,
    /**
     * ログイン失敗
     *
     * 従業員番号とパスワードのどちらが間違っているかは伝えない
     */
    LoginFailedError: (): string => `従業員番号またはパスワードが間違っています。`,
    /**
     * パスワード未設定
     */
    PasswordNotSetError: (): string => `パスワードが設定されていません。`,
    /**
     * パスワード不一致
     */
    PasswordConfirmationMismatchError: (): string => `パスワードが一致しません。`,
    /**
     * 現在のパスワード誤り
     */
    CurrentPasswordIncorrectError: (): string => `現在のパスワードが正しくありません。`,
    /**
     * 従業員番号重複
     */
    DuplicateEmployeeNumberError: ({ 従業員番号 }: { 従業員番号: string }): string => `従業員番号 '${従業員番号}' は既に存在します。`,
    /**
     * 自分自身の削除
     */
    CannotDeleteSelfError: (): string => `自分自身は削除できません。`,
    /**
     * 自分自身の管理者権限の削除
     */
    CannotRemoveOwnAdminPermissionError: (): string => `自分自身のシステム管理者の権限は外せません。`,
    /**
     * 従業員一括更新の確認
     */
    EmployeeUpdateConfirm: (): string => `更新しますか？`,
    /**
     * 変更なし
     */
    NothingChangedInfo: (): string => `更新された内容はありません。`,
    /**
     * 保存完了
     */
    SavedInfo: (): string => `保存しました。`,
    /**
     * 既読・未読の未指定
     */
    ReadStatusRequiredError: (): string => `既読にするか未読にするかを指定してください。`,
    /**
     * 通知不在
     */
    NotificationNotFoundError: (): string => `通知が見つかりません。`,
    /**
     * スプールのファイル名での絞り込み
     */
    SpoolFileNameFilterError: (): string => `スプールのファイルの名前では絞り込めません。`,
    /**
     * ジョブ正常終了の通知
     */
    JobSucceededNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${ジョブの種類}」が正常終了しました。`,
    /**
     * ジョブ異常終了の通知
     */
    JobFailedNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${ジョブの種類}」が異常終了しました。`,
    /**
     * ジョブ異常終了の結果
     */
    JobFailedResult: (): string => `エラーが発生したため、異常終了しました。`,
    /**
     * ジョブ中断の結果
     *
     * ジョブの実行中に、サーバーの停止でキャンセルされたとき
     */
    JobInterruptedResult: (): string => `サーバーの停止により、中断しました。`,
    /**
     * 停止前に実行中だったジョブの結果
     *
     * サーバーの起動時に、前回の停止のときに実行中だったジョブを見つけたとき
     */
    JobInterruptedBeforeRestartResult: (): string => `サーバーが停止したため、中断しました。`,
    /**
     * 未定義の種類のジョブの結果
     */
    JobKindUndefinedResult: (): string => `ジョブの種類が定義されていないため、実行できませんでした。`,
    /**
     * クリーンナップの結果
     */
    CleanUpResult: ({ セッション, 冪等キー, 通知, ジョブ }: { セッション: string, 冪等キー: string, 通知: string, ジョブ: string }): string => `セッション ${セッション} 件、冪等キー ${冪等キー} 件、通知 ${通知} 件、ジョブ ${ジョブ} 件を削除しました。`,
  },
  /**
   * メッセージ(デモ101専用)
   *
   * 販売管理のメッセージ。値の決まりは「MSG」と同じ。
   */
  MSG_DEMO101: {
    /**
     * 未指定エラー
     */
    NotSpecifiedError: ({ 項目名 }: { 項目名: string }): string => `${項目名}が指定されていません。`,
    /**
     * 売上不在
     */
    SalesNotFoundError: (): string => `指定された売上データが見つかりません。`,
    /**
     * 売上明細なし（登録）
     */
    SalesDetailRequiredError: (): string => `売上明細を1件以上登録してください。`,
    /**
     * 売上明細なし
     */
    SalesDetailMinCountError: (): string => `売上明細は1件以上必要です。`,
    /**
     * 既存の売上明細の編集
     */
    ExistingSalesDetailNotEditableError: (): string => `既存の売上明細の編集・削除はできません。新規追加のみ可能です。`,
    /**
     * 新規登録時の取消明細
     */
    CancelDetailOnCreateError: (): string => `新規登録時に取消明細を追加することはできません。`,
    /**
     * 売上数量が0
     */
    SalesQuantityNonZeroError: (): string => `売上数量は0以外の整数を入力してください。`,
    /**
     * 売上数量が1未満
     */
    SalesQuantityPositiveError: (): string => `売上数量は1以上の整数を入力してください。`,
    /**
     * 明細の個数が負
     */
    DetailQuantityNegativeError: (): string => `明細の個数は0以上でなければなりません。`,
    /**
     * 金額が0以外
     */
    AmountMustBeZeroError: (): string => `金額は0のみ指定可能です。`,
    /**
     * 売上明細の金額が負
     */
    SalesAmountNegativeError: (): string => `売上明細の金額は0以上でなければなりません。`,
    /**
     * 取消明細の金額が正
     */
    CancelAmountPositiveError: (): string => `取消明細の金額は0以下でなければなりません。`,
    /**
     * 在庫引当済みの明細の削除
     */
    AllocatedDetailDeleteError: (): string => `在庫引当済みのため、既存の明細を削除することはできません。取消明細を追加して調整してください。`,
    /**
     * 在庫引当済みの明細の変更
     */
    AllocatedDetailChangeError: (): string => `在庫引当済みのため変更できません。新しい明細を追加して調整してください。`,
    /**
     * 在庫不足
     */
    InsufficientStockError: ({ 現在在庫 }: { 現在在庫: string }): string => `在庫不足です。（現在在庫: ${現在在庫}）`,
    /**
     * 取消可能な在庫引当の不足
     */
    InsufficientReturnableStockError: ({ 取消可能数 }: { 取消可能数: string }): string => `取消可能な在庫引当履歴が不足しています。（取消可能数: ${取消可能数}）`,
    /**
     * 売上登録の確認
     */
    SalesRegisterConfirm: (): string => `登録を確定します。よろしいですか？`,
    /**
     * 売上登録完了
     */
    SalesRegisteredInfo: (): string => `売上登録が完了しました。`,
    /**
     * 売上修正の確認
     */
    SalesUpdateConfirm: (): string => `修正を確定します。よろしいですか？`,
    /**
     * 売上修正完了
     */
    SalesUpdatedInfo: (): string => `売上修正が完了しました。`,
    /**
     * 担当の売上の修正の通知
     */
    SalesUpdatedNotification: ({ 売上SEQ, 修正者 }: { 売上SEQ: string, 修正者: string }): string => `担当の売上 ${売上SEQ} が ${修正者} さんにより修正されました。`,
    /**
     * 在庫調整の数量未指定
     */
    StockAdjustmentAmountRequiredError: (): string => `増減数または絶対数のいずれかを指定してください。`,
    /**
     * 在庫調整の数量の重複指定
     */
    StockAdjustmentAmountConflictError: (): string => `増減数と絶対数の両方を指定することはできません。`,
    /**
     * 在庫数の変更なし
     */
    StockNotChangedError: (): string => `在庫数に変更がありません。`,
    /**
     * 減算時の在庫不足
     */
    InsufficientStockForSubtractionError: ({ 現在在庫, 減算指定 }: { 現在在庫: string, 減算指定: string }): string => `在庫不足のため減算できません。（現在在庫: ${現在在庫}, 減算指定: ${減算指定}）`,
    /**
     * 在庫調整の通知
     *
     * 増減数は、符号を付けて渡す（+3、-2 など）
     */
    StockAdjustedNotification: ({ 商品名, 調整者, 増減数 }: { 商品名: string, 調整者: string, 増減数: string }): string => `商品「${商品名}」の在庫が ${調整者} さんにより調整されました（${増減数}個）。`,
    /**
     * 入荷商品なし
     */
    ArrivalItemRequiredError: (): string => `入荷商品を1件以上登録してください。`,
    /**
     * 入荷不在
     */
    ArrivalNotFoundError: ({ 入荷ID }: { 入荷ID: string }): string => `入荷データが見つかりません。(ID: ${入荷ID})`,
    /**
     * 入荷明細不在
     */
    ArrivalDetailNotFoundError: ({ 入荷明細ID }: { 入荷明細ID: string }): string => `指定された入荷明細が存在しません。(ID: ${入荷明細ID})`,
    /**
     * 出荷引当済みの入荷の商品の変更
     */
    AllocatedArrivalProductChangeError: (): string => `既に出荷引当が行われているため、商品は変更できません。`,
    /**
     * 出荷引当済みの入荷の削除
     */
    AllocatedArrivalDeleteError: ({ 商品名 }: { 商品名: string }): string => `既に出荷引当が行われているため、削除できません。(商品: ${商品名})`,
    /**
     * 使用済みの数量を下回る入荷数量
     */
    ArrivalQuantityBelowUsedError: ({ 使用済みの数量 }: { 使用済みの数量: string }): string => `入荷数量を既に使用されている数量(${使用済みの数量})未満にすることはできません。`,
    /**
     * 入荷修正の確認
     */
    ArrivalUpdateConfirm: (): string => `入荷データを修正します。よろしいですか？`,
    /**
     * 入荷登録完了
     */
    ArrivalRegisteredInfo: (): string => `入荷登録が完了しました。`,
    /**
     * 商品データ取込の確認
     */
    ProductImportConfirm: (): string => `商品データの取込を開始します。終わったら通知でお知らせします。よろしいですか？`,
    /**
     * 商品データ取込の結果
     */
    ProductImportResult: ({ 件数, 取込件数 }: { 件数: string, 取込件数: string }): string => `${件数} 件のうち、${取込件数} 件を取り込みました。`,
    /**
     * 商品データ取込の結果（失敗あり）
     */
    ProductImportPartiallyFailedResult: ({ 件数, 取込件数, 失敗件数 }: { 件数: string, 取込件数: string, 失敗件数: string }): string => `${件数} 件のうち、${取込件数} 件を取り込みました。${失敗件数} 件は取り込めませんでした。理由は取込結果のファイルを参照してください。`,
    /**
     * 不正な消費税区分
     *
     * 商品データ取込の取込結果のファイルに出す、取り込めなかった理由
     */
    InvalidTaxTypeReason: ({ 消費税区分 }: { 消費税区分: string }): string => `不正な消費税区分です: ${消費税区分}`,
    /**
     * 負の値
     *
     * カスタム属性「0以上のみ」の入力チェック
     */
    NegativeValueError: (): string => `負の値は許可されていません。`,
  },
}

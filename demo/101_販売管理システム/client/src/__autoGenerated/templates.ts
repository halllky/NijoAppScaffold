
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev


/**
 * テンプレート文字列の種類ごとの、引数の値を埋め込む文字列にする変換。
 * 種類は nijo.xml の TemplateKind 属性で指定したもの。
 * 引数の型ごとに、C# の型名の名前で変換を持つ。
 * サーバーと画面で同じ文字列になるように、C# 側の変換と同じ結果を返すこと。
 */
export type TemplateKind = {
  /** 種類「URL」の変換 */
  URL: {
    string: (value: string) => string
    int: (value: string) => string
  }
  /** 種類「文言」の変換 */
  文言: {
    string: (value: string) => string
    int: (value: string) => string
  }
}

/**
 * nijo.xml の Templates セクションに定義したテンプレート文字列を返します。
 * 引数の値は、渡された変換を通してから埋め込む。
 * アプリからは、この関数を1回だけ呼んだ結果を export し、それを通して使う。
 * 宣言されていない変換を余分に渡してもエラーにしない。C# で余分なオーバーロードがあってもエラーにならないのと同じく、
 * テンプレート文字列の引数で使われなくなった型の変換を、消さずに残しておけるようにするため。
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
   * 画面URL一覧(デモ101専用)
   *
   * 販売管理の画面の URL。値の決まりは「PAGES_URL」と同じ。
   */
  PAGES_URL_DEMO101: {
    /**
     * P101 売上詳細
     */
    P101: ({ 売上SEQ }: { 売上SEQ: string }): string => `/uriage/edit/${templateKind.URL.int(売上SEQ)}`,
    /**
     * P301 商品詳細
     */
    P301: ({ 商品SEQ }: { 商品SEQ: string }): string => `/shohin/${templateKind.URL.int(商品SEQ)}`,
  },
  /**
   * メッセージ
   *
   * 利用者に見せるメッセージ。名前がメッセージの ID になる。
   * 入力チェックや保存処理の共通部分と、デモ101 テンプレートに残る機能で使うもの。販売管理のメッセージは MSG_DEMO101 に定義する。
   */
  MSG: {
    /**
     * 必須入力エラー
     */
    RequiredError: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言.string(項目名)} を入力してください。`,
    /**
     * 文字数超過エラー
     */
    MaxLengthError: ({ 項目名, 最大文字数 }: { 項目名: string, 最大文字数: string }): string => `${templateKind.文言.string(項目名)} は ${templateKind.文言.int(最大文字数)} 文字以内で入力してください。`,
    /**
     * 桁数超過エラー
     */
    DigitsError: ({ 項目名, 整数部の桁数, 小数部の桁数 }: { 項目名: string, 整数部の桁数: string, 小数部の桁数: string }): string => `${templateKind.文言.string(項目名)} は整数部 ${templateKind.文言.int(整数部の桁数)} 桁、小数部 ${templateKind.文言.int(小数部の桁数)} 桁以内で入力してください。`,
    /**
     * 区分値不在エラー
     */
    GenericLookupValueNotFound: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言.string(項目名)} に指定された区分値が存在しません。`,
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
    SelectRequiredError: ({ 項目名 }: { 項目名: string }): string => `${templateKind.文言.string(項目名)} を選択してください。`,
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
    DuplicateEmployeeNumberError: ({ 従業員番号 }: { 従業員番号: string }): string => `従業員番号 '${templateKind.文言.string(従業員番号)}' は既に存在します。`,
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
    JobSucceededNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${templateKind.文言.string(ジョブの種類)}」が正常終了しました。`,
    /**
     * ジョブ異常終了の通知
     */
    JobFailedNotification: ({ ジョブの種類 }: { ジョブの種類: string }): string => `ジョブ「${templateKind.文言.string(ジョブの種類)}」が異常終了しました。`,
    /**
     * ジョブ異常終了の結果
     */
    JobFailedResult: (): string => `エラーが発生したため、異常終了しました。`,
    /**
     * ジョブ中断の結果
     *
     * ジョブの実行中に、サーバーの停止の合図を受け取ったとき
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
    CleanUpResult: ({ セッション, 冪等キー, 通知, ジョブ }: { セッション: string, 冪等キー: string, 通知: string, ジョブ: string }): string => `セッション ${templateKind.文言.int(セッション)} 件、冪等キー ${templateKind.文言.int(冪等キー)} 件、通知 ${templateKind.文言.int(通知)} 件、ジョブ ${templateKind.文言.int(ジョブ)} 件を削除しました。`,
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
   * 入力チェックや保存処理の共通部分と、デモ101 テンプレートに残る機能で使うもの。販売管理のメッセージは MSG_DEMO101 に定義する。
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
     * ジョブの実行中に、サーバーの停止の合図を受け取ったとき
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
}

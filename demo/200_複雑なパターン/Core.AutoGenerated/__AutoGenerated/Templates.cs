
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : TemplatesSection.cs
// ※ このファイルを生成したメソッド: Render
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// テンプレート文字列の種類ごとの、引数を埋め込む前の変換。
/// 種類は nijo.xml の TemplateKind 属性で指定したもの。
/// 変換の中身は、このインターフェースを実装するクラスに書き、そのクラスを <see cref="Templates{TKind}"/> の型引数に渡す。
/// サーバーと画面で同じ文字列になるように、TypeScript 側の変換と同じ結果を返すこと。
/// </summary>
public interface ITemplateKind {
    /// <summary>種類「JSON」の引数を埋め込む前の変換</summary>
    static abstract string JSON(string value);
    /// <summary>種類「文言」の引数を埋め込む前の変換</summary>
    static abstract string 文言(string value);
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
    /// アプリケーションのテンプレート文字列
    /// <para>
    /// アプリケーションのテンプレート文字列。入れ子のグループのテスト
    /// </para>
    /// </summary>
    public static class ApplicationTemplates {
        /// <summary>
        /// メッセージ
        /// </summary>
        public static class Messages {
            /// <summary>
            /// 成功メッセージ
            /// <para>
            /// 引数が0個の場合のテスト
            /// </para>
            /// </summary>
            public static string Success() => $"正常に処理されました";
            /// <summary>
            /// エラーメッセージ
            /// <para>
            /// コロンの後ろには開発者向けの情報ではなくアプリケーションのユーザーに向けた文字にすること。
            /// </para>
            /// </summary>
            public static string Error(string 詳細) => $"エラーが発生しました: {TKind.文言(詳細)}";
            /// <summary>
            /// バリデーションエラー
            /// <para>
            /// 引数が複数ある場合のテスト
            /// </para>
            /// </summary>
            public static string ValidationError(string フィールド名, string エラーの種類, string 詳細) => $"{TKind.文言(フィールド名)}フィールドで{TKind.文言(エラーの種類)}エラーが発生しました。詳細: {TKind.文言(詳細)}";
            /// <summary>
            /// メンテナンスメッセージ
            /// </summary>
            public static string SystemMaintenance() => $"システムメンテナンス中です。しばらくお待ちください。";
        }
        public static class API {
            /// <summary>
            /// 認証ヘッダー
            /// </summary>
            public static string AuthHeader(string トークン) => $"Bearer {TKind.文言(トークン)}";
            /// <summary>
            /// JSONフォーマット
            /// <para>
            /// 引用符と波括弧そのものを含む文字列、外側のグループのテンプレートの種類の上書きのテスト
            /// </para>
            /// </summary>
            public static string JsonFormat(string メッセージ, string 時刻) => $"{{\"message\": \"{TKind.JSON(メッセージ)}\", \"timestamp\": \"{TKind.JSON(時刻)}\"}}";
        }
    }
    /// <summary>
    /// システムメッセージ
    /// <para>
    /// システムメッセージ
    /// </para>
    /// </summary>
    public static class SystemMessages {
        /// <summary>
        /// 患者未発見エラー
        /// <para>
        /// 患者管理関連
        /// </para>
        /// </summary>
        public static string PatientNotFound(string 患者ID) => $"患者ID「{TKind.文言(患者ID)}」が見つかりません";
        /// <summary>
        /// 患者登録完了
        /// </summary>
        public static string PatientRegistered(string 患者名) => $"患者「{TKind.文言(患者名)}」を正常に登録しました";
        /// <summary>
        /// 機器メンテナンス通知
        /// <para>
        /// 機器管理関連
        /// </para>
        /// </summary>
        public static string EquipmentMaintenance(string 機器名, string 最終点検日) => $"機器「{TKind.文言(機器名)}」のメンテナンスが必要です。最終点検日: {TKind.文言(最終点検日)}";
        /// <summary>
        /// 在庫アラート
        /// <para>
        /// 整数の引数のテスト
        /// </para>
        /// </summary>
        public static string StockAlert(string 品目名, int 残数) => $"在庫不足: {TKind.文言(品目名)}の残り数量が{TKind.文言(残数.ToString(System.Globalization.CultureInfo.InvariantCulture))}個です";
        /// <summary>
        /// 予約確認
        /// <para>
        /// 予約関連。同じ引数を2回使う場合のテスト
        /// </para>
        /// </summary>
        public static string ReservationConfirmed(string 予約番号, string 日時) => $"予約を確定しました。予約番号: {TKind.文言(予約番号)}、日時: {TKind.文言(日時)}（予約番号 {TKind.文言(予約番号)} は控えておいてください）";
        /// <summary>
        /// 予約キャンセル
        /// </summary>
        public static string ReservationCancelled() => $"予約がキャンセルされました";
        /// <summary>
        /// スクリプトテンプレート
        /// <para>
        /// バックティック（`）と、波括弧そのものを含むテンプレート文字列のテスト
        /// </para>
        /// </summary>
        public static string ScriptTemplate(string 名前) => $"console.log(`患者ID: ${{patientId}}, 名前: {TKind.文言(名前)}`);";
        /// <summary>
        /// JSONテンプレート
        /// <para>
        /// ダブルクォート（"）を含むテンプレート文字列のテスト
        /// </para>
        /// </summary>
        public static string JsonTemplate(string 患者名) => $"{{\"patientName\": \"{TKind.JSON(患者名)}\", \"status\": \"active\"}}";
    }
}

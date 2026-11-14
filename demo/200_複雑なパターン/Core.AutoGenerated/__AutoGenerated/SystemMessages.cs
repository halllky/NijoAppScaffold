
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate

namespace MyApp;

/// <summary>
/// システムメッセージ
/// <para>
/// システムメッセージの定数定義
/// </para>
/// </summary>
public static class SystemMessages {
    /// <summary>
    /// 予約キャンセル
    /// </summary>
    public const string ReservationCancelled = "予約がキャンセルされました";
    /// <summary>
    /// 患者未発見エラー
    /// <para>
    /// 患者管理関連
    /// </para>
    /// </summary>
    public static string PatientNotFound(string arg0) => $"患者ID「{arg0}」が見つかりません";
    /// <summary>
    /// 患者登録完了
    /// </summary>
    public static string PatientRegistered(string arg0) => $"患者「{arg0}」を正常に登録しました";
    /// <summary>
    /// 機器メンテナンス通知
    /// <para>
    /// 機器管理関連
    /// </para>
    /// </summary>
    public static string EquipmentMaintenance(string arg0, string arg1) => $"機器「{arg0}」のメンテナンスが必要です。最終点検日: {arg1}";
    /// <summary>
    /// 在庫アラート
    /// </summary>
    public static string StockAlert(string arg0, string arg1) => $"在庫不足: {arg0}の残り数量が{arg1}個です";
    /// <summary>
    /// 予約確認
    /// <para>
    /// 予約関連
    /// </para>
    /// </summary>
    public static string ReservationConfirmed(string arg0, string arg1) => $"予約を確定しました。予約番号: {arg0}、日時: {arg1}";
    /// <summary>
    /// スクリプトテンプレート
    /// <para>
    /// バックティック（`）を含むテンプレート文字列のテスト
    /// </para>
    /// </summary>
    public static string ScriptTemplate(string arg0) => $"console.log(`患者ID: ${{patientId}}, 名前: {arg0}`);";
    /// <summary>
    /// JSONテンプレート
    /// <para>
    /// ダブルクォート（"）を含むテンプレート文字列のテスト
    /// </para>
    /// </summary>
    public static string JsonTemplate(string arg0) => $"{{\"patientName\": \"{arg0}\", \"status\": \"active\"}}";
}

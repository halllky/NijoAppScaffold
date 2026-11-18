
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ConstantsSection.cs
// ※ このファイルを生成したメソッド: RenderRootAggregate
// ※ このファイルを生成した nijo のバージョン: 0.0.0-dev

namespace MyApp;

/// <summary>
/// システムメッセージ
/// <para>
/// システムメッセージの定数定義
/// </para>
/// </summary>
public static class SystemMessages {
    /// <summary>
    /// 患者未発見エラー
    /// <para>
    /// 患者管理関連
    /// </para>
    /// </summary>
    public static string PatientNotFound(string 患者ID) => $"患者ID「{ConstantTemplateKind.文言(患者ID)}」が見つかりません";
    /// <summary>
    /// 患者登録完了
    /// </summary>
    public static string PatientRegistered(string 患者名) => $"患者「{ConstantTemplateKind.文言(患者名)}」を正常に登録しました";
    /// <summary>
    /// 機器メンテナンス通知
    /// <para>
    /// 機器管理関連
    /// </para>
    /// </summary>
    public static string EquipmentMaintenance(string 機器名, string 最終点検日) => $"機器「{ConstantTemplateKind.文言(機器名)}」のメンテナンスが必要です。最終点検日: {ConstantTemplateKind.文言(最終点検日)}";
    /// <summary>
    /// 在庫アラート
    /// <para>
    /// 整数の引数のテスト
    /// </para>
    /// </summary>
    public static string StockAlert(string 品目名, int 残数) => $"在庫不足: {ConstantTemplateKind.文言(品目名)}の残り数量が{ConstantTemplateKind.文言(残数.ToString(System.Globalization.CultureInfo.InvariantCulture))}個です";
    /// <summary>
    /// 予約確認
    /// <para>
    /// 予約関連
    /// </para>
    /// </summary>
    public static string ReservationConfirmed(string 予約番号, string 日時) => $"予約を確定しました。予約番号: {ConstantTemplateKind.文言(予約番号)}、日時: {ConstantTemplateKind.文言(日時)}";
    /// <summary>
    /// 予約キャンセル
    /// </summary>
    public const string ReservationCancelled = "予約がキャンセルされました";
    /// <summary>
    /// スクリプトテンプレート
    /// <para>
    /// バックティック（`）と、波括弧そのものを含むテンプレート文字列のテスト
    /// </para>
    /// </summary>
    public static string ScriptTemplate(string 名前) => $"console.log(`患者ID: ${{patientId}}, 名前: {ConstantTemplateKind.文言(名前)}`);";
    /// <summary>
    /// JSONテンプレート
    /// <para>
    /// ダブルクォート（"）を含むテンプレート文字列のテスト
    /// </para>
    /// </summary>
    public static string JsonTemplate(string 患者名) => $"{{\"patientName\": \"{ConstantTemplateKind.JSON(患者名)}\", \"status\": \"active\"}}";
}

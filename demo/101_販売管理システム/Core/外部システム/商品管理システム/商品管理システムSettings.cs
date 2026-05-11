namespace MyApp.Core.外部システム.商品管理システム;

/// <summary>
/// appsettings.json の商品管理システム設定セクションに対応する設定クラス
/// </summary>
public class 商品管理システムSettings {
    /// <summary>
    /// モックを使う場合は true。本番環境では false。
    /// この値をもとにDIコンテナで登録するクラスを切り替える。
    /// </summary>
    public bool UseMock { get; set; } = true;
    /// <summary>
    /// モックJSONデータのパス
    /// </summary>
    public string MockJsonPath { get; set; } = string.Empty;
    /// <summary>
    /// 本番環境用の接続文字列
    /// （このアプリはデモなので未使用。あくまで本番用の設定を加える場合はどうすべきかをイメージしやすくするための例）
    /// </summary>
    public string HonbanConnectionString { get; set; } = string.Empty;
}

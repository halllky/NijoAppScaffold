namespace MyApp.Core.外部システム.商品管理システム;

/// <summary>
/// appsettings.json の商品管理システム設定セクションに対応する設定クラス。
/// appsettings.json に設定が無い項目は、各プロパティの既定値になる。
/// </summary>
public class 商品管理システムSettings {
    /// <summary>
    /// モックを使う場合は true、本番の商品管理システムを使う場合は false。
    /// この値をもとにDIコンテナで登録するクラスを切り替える。
    /// 省略を許さないので既定値をもたない。省略されていると起動しない。
    /// </summary>
    public bool? UseMock { get; set; }
    /// <summary>
    /// モックJSONデータのパス。相対パスの場合は <see cref="RuntimeSetting.LogDirectory"/> からの相対パス。
    /// </summary>
    public string MockJsonPath { get; set; } = "商品管理システム（モック）.json";
    /// <summary>
    /// 本番環境用の接続文字列
    /// （このアプリはデモなので未使用。あくまで本番用の設定を加える場合はどうすべきかをイメージしやすくするための例）
    /// </summary>
    public string HonbanConnectionString { get; set; } = string.Empty;
}

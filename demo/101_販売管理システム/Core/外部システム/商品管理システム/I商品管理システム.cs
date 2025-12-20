namespace MyApp.Core.外部システム.商品管理システム;

/// <summary>
/// 商品管理システム インターフェース。
/// 開発環境ではモック、本番環境では実際の外部システム連携クラスを使う。
/// （このアプリケーションはデモなので本番環境は無い。あくまでイメージ）
///
/// 切り替えは appsettings.json の設定値を介してDIコンテナで行う。
/// </summary>
public interface I商品管理システム {
    /// <summary>
    /// 最新の商品データを取得して返します。
    /// </summary>
    IEnumerable<ProductImportData> Enumerate商品データ();
}

/// <summary>
/// 商品管理システムから取込む商品データの形式
/// </summary>
public class ProductImportData {
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string TaxType { get; set; } = string.Empty;
}

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
}

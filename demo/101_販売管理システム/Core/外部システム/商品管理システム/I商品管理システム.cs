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
    /// <summary>
    /// 在庫数量を更新します。
    /// </summary>
    /// <param name="externalProductId">外部システムの商品ID</param>
    /// <param name="newStockQuantity">新しい在庫数量</param>
    Task Update在庫数量Async(string externalProductId, int newStockQuantity);
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
/// こちらのシステムが在庫数量更新を通知する際のインターフェースの型
/// </summary>
public class QuantityUpdateInterfaceType {
    public int NewQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}

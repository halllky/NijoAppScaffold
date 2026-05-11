namespace MyApp.Core.外部システム.商品管理システム;

public class 商品管理システム本番 : I商品管理システム {

    public IEnumerable<ProductImportData> Enumerate商品データ() {
        // ここに実際の外部システムから商品データを取得するロジックを実装する。
        // 例えば、Web APIを呼び出したり、データベースに接続したりする。
        // デモなので、未実装とする。

        throw new NotImplementedException("実際の外部システム連携ロジックは未実装です。");
    }

    public void Update在庫数量(string externalProductId, int newStockQuantity) {
        // ここに実際の外部システムの在庫数量を更新するロジックを実装する。
        // 例えば、Web APIを呼び出したり、メッセージングキューに送信したりする。
        // デモなので、未実装とする。

        throw new NotImplementedException("実際の外部システム連携ロジックは未実装です。");
    }
}

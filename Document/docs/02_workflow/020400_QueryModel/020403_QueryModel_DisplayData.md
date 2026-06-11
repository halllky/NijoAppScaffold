---
sidebar_label: 検索結果

---

# 検索結果（DisplayData）

QueryModel の検索処理は、内部的に **SearchResult** と **DisplayData** という2つのクラスを経由して結果を返します。

## SearchResult と DisplayData の役割分担

| クラス               | 役割                                                                    | 構造                                                                  |
| -------------------- | ----------------------------------------------------------------------- | --------------------------------------------------------------------- |
| `{Name}SearchResult` | EF Core の IQueryable に使うクラス。WHERE句・ORDER BY句の動的構築に使用 | **フラット構造**（Child は `_{フィールド名}` でアンダースコアつなぎ） |
| `{Name}DisplayData`  | フロントエンドに返す表示用クラス                                        | **階層構造**（Child / Children はネストしたオブジェクト）             |

SearchResult がフラット構造になっている理由は、EF Core の LINQ to Entity から直接 SQL にチューニングしやすくするためです。
開発者が直接扱うのは DisplayData であり、SearchResult を直接操作することは通常ありません。

## DisplayData クラスの構造

```csharp
public class OrderQueryDisplayData {
    // 各属性に対応するプロパティ
    public string?   OrderId   { get; set; }
    public DateTime? OrderDate { get; set; }

    // Children（1対多）はネストしたリスト
    public List<OrderQueryDisplayDataOrderDetails>? OrderDetails { get; set; }

    // --- 以下はフレームワーク用フィールド ---

    // DataModel に GenerateDefaultQueryModel="true" が指定された場合のみ存在
    public int? Version { get; set; }

    // 編集状態フラグ
    public bool ExistsInDatabase { get; set; }
    public bool WillBeDeleted    { get; set; }
    public bool WillBeChanged    { get; set; }
}
```

### Version フィールド

`Version` フィールドは、DataModel に `GenerateDefaultQueryModel="true"` を指定したときのみ生成されます。
楽観排他制御（更新時の競合検出）に使用します。

```xml
<!-- この場合は Version あり -->
<Order Type="data-model" GenerateDefaultQueryModel="true" DisplayName="受注">
  ...
</Order>

<!-- この場合は Version なし -->
<OrderSummary Type="query-model" DisplayName="受注サマリ">
  ...
</OrderSummary>
```

### 編集状態フラグ

`ExistsInDatabase`・`WillBeDeleted`・`WillBeChanged` はUIでの編集状態を追跡するフラグです。
フレームワークが自動的に設定・参照します。

## ページネーションレスポンス

`LoadAsync` メソッドは SearchResult ではなく、ページネーション情報を含むラッパーを返します。

```csharp
// 戻り値の型
Util.SearchProcessingResult<OrderQueryDisplayData>
```

| フィールド   | 説明                                         |
| ------------ | -------------------------------------------- |
| `Items`      | 現在ページの DisplayData の配列              |
| `TotalCount` | フィルタ後の全件数（ページネーション表示用） |

TypeScript 側では `LoadFeature.ReturnType` で型を参照できます。

## OnAfterLoaded — 検索後の加工処理

SQL で表現しきれない計算や、別テーブルからのデータ補完は `OnAfterLoaded` で実装します。

```csharp
protected override async Task OnAfterLoaded(
    IEnumerable<OrderQueryDisplayData> items,
    OrderQuerySearchCondition condition,
    CancellationToken cancellationToken) {

    // 例: 配送ステータスを外部サービスから取得して補完
    var orderIds = items.Select(x => x.OrderId).ToList();
    var statusMap = await _deliveryService.GetStatusBulkAsync(orderIds);

    foreach (var item in items) {
        if (statusMap.TryGetValue(item.OrderId!, out var status)) {
            item.DeliveryStatus = status;
        }
    }
}
```

:::note OnAfterLoaded の使いどころ
- 外部 API からのデータ補完
- C# でしか計算できないロジック（例: ロケール依存のフォーマット）
- ファイルシステムや Redis などの DB 以外のデータソースとの結合

パフォーマンスに注意し、N+1 問題が起きないよう一括取得するよう実装してください。
:::

## TypeScript での型情報

```typescript
// 検索結果の型
type OrderQueryResult = LoadFeature.ReturnType['OrderQuery']
// → { items: OrderQueryDisplayData[], totalCount: number }

// DisplayData の型
// LoadFeature.ReturnType から items の要素型として参照可能
```

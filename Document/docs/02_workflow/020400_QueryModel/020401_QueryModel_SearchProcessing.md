---

---

# 一覧検索処理

QueryModel の中核となる検索処理の実装方法を説明します。
`CreateQuerySource` でクエリの基盤を定義し、フレームワークが WHERE 句・ORDER BY 句・ページングを自動付加します。

## 処理の全体フロー

```
[フロントエンド] POST /api/{集約名}/load
        ↓
[Controller] Load() アクション
        ↓
ValidateSearchCondition()  ← 検索条件のバリデーション（オーバーライド可）
        ↓ エラーなし
LoadAsync()  ← 検索処理本体（フレームワーク固定）
        ├─ CreateQuerySource()      ← FROM句・SELECT句  [開発者実装]
        ├─ AppendWhereClause()      ← WHERE句 [自動生成・オーバーライド可]
        ├─ AppendOrderByClause()    ← ORDER BY句 [自動生成・オーバーライド可]
        ├─ CountAsync()             ← 全件数取得（ページネーション用）
        ├─ Skip() / Take()          ← ページング
        └─ OnAfterLoaded()          ← 読み込み後の C# メモリ上の加工 [オーバーライド可]
        ↓
{ items: [...], totalCount: N }  → フロントエンドへ返却
```

`LoadAsync` 自体はフレームワークが生成する固定実装です。
開発者が必ず実装するのは **`CreateQuerySource`** のみです。

---

## CreateQuerySource — クエリ定義（必須実装）

`CreateQuerySource` は、検索の基盤となる `IQueryable<{Name}SearchResult>` を返します。
SQL の **FROM 句と SELECT 句**に相当します。

```csharp
// 生成されるスタブ（通常の QueryModel）
protected abstract IQueryable<OrderQuerySearchResult> CreateQuerySource(
    OrderQuerySearchCondition searchCondition,
    IPresentationContext<OrderQuerySearchConditionMessages> context);
```

### 実装例：DataModel から射影する

```csharp
protected override IQueryable<OrderQuerySearchResult> CreateQuerySource(
    OrderQuerySearchCondition searchCondition,
    IPresentationContext<OrderQuerySearchConditionMessages> context) {

    return DbContext.Orders
        .Include(o => o.OrderDetails)
        .Select(o => new OrderQuerySearchResult {
            OrderId      = o.OrderId,
            OrderDate    = o.OrderDate,
            CustomerName = o.Customer!.Name,
            // Children は SelectMany や Select で展開
            OrderDetails = o.OrderDetails.Select(d => new OrderQuerySearchResultOrderDetails {
                LineNo    = d.LineNo,
                ProductId = d.ProductId,
                Quantity  = d.Quantity,
            }).ToList(),
        });
}
```

:::note GenerateDefaultQueryModel="true" のとき
`GenerateDefaultQueryModel="true"` を DataModel に指定した場合、DataModel と構造が完全一致する QueryModel が生成され、`CreateQuerySource` の実装も自動生成されます。この場合は開発者の実装は不要です。
:::

### DB View を使う場合（MapToView="true"）

`MapToView="true"` のときは `CreateQuerySource` の実装は不要です。
フレームワークが DbContext 上の View エンティティを直接参照します。
詳細は [DB View マッピング](./020404_QueryModel_DbView.md) を参照してください。

---

## AppendWhereClause — WHERE 句（自動生成）

`AppendWhereClause` は `CreateQuerySource` で得た IQueryable に WHERE 句を追加します。
各属性の型に応じた絞り込み条件が**自動生成**されます。

| 属性の型                     | 自動生成される条件                           |
| ---------------------------- | -------------------------------------------- |
| `string`（Word/Description） | LIKE による部分一致                          |
| 数値（int/long/decimal）     | `{Name}Min` ≤ value ≤ `{Name}Max` の範囲検索 |
| `DateTime`                   | `{Name}From` ≤ value ≤ `{Name}To` の範囲検索 |
| `StaticEnum`                 | チェックされた値の IN 条件                   |
| `OnlySearchCondition="true"` | 自動生成なし（開発者が実装）                 |

### オーバーライドして条件を追加する

```csharp
protected override IQueryable<OrderQuerySearchResult> AppendWhereClause(
    IQueryable<OrderQuerySearchResult> query,
    OrderQuerySearchCondition searchCondition) {

    // 基底クラスの自動生成条件を先に適用
    query = base.AppendWhereClause(query, searchCondition);

    // OnlySearchCondition フィールドの独自実装
    if (searchCondition.Filter?.IsUrgent == true) {
        var threshold = DateTime.Today.AddDays(-3);
        query = query.Where(o =>
            o.ShippedDate == null &&
            o.OrderDate <= threshold);
    }

    return query;
}
```

:::warning base.AppendWhereClause を忘れずに
オーバーライドする場合は必ず冒頭で `base.AppendWhereClause(query, searchCondition)` を呼んでください。
呼ばないと自動生成された絞り込み条件がすべてスキップされます。
:::

---

## AppendOrderByClause — ORDER BY 句（自動生成）

`Sort` プロパティに指定されたソートキーに基づいて ORDER BY 句を追加します。
こちらも自動生成されますが、デフォルトのソート順を変更したい場合はオーバーライドできます。

```csharp
protected override IQueryable<OrderQuerySearchResult> AppendOrderByClause(
    IQueryable<OrderQuerySearchResult> query,
    OrderQuerySearchCondition searchCondition) {

    // Sort が未指定のときのデフォルトソート順を設定する
    if (searchCondition.Sort == null || searchCondition.Sort.Count == 0) {
        return query.OrderByDescending(o => o.OrderDate)
                    .ThenBy(o => o.OrderId);
    }

    return base.AppendOrderByClause(query, searchCondition);
}
```

---

## OnAfterLoaded — 読み込み後処理（オプション）

EF Core のクエリ実行後、C# のメモリ上でデータを加工したい場合に使います。
SQL では表現しにくい加工や、外部 API / キャッシュからのデータ補完に使います。

```csharp
protected override IEnumerable<OrderQueryDisplayData> OnAfterLoaded(
    IEnumerable<OrderQueryDisplayData> currentPageItems,
    OrderQuerySearchCondition searchCondition,
    IPresentationContext<OrderQuerySearchConditionMessages> context) {

    // 例: 外部サービスから配送ステータスを一括取得して補完
    var orderIds = currentPageItems.Select(x => x.OrderId).ToList();
    var statusMap = _deliveryService.GetStatusBulk(orderIds);

    foreach (var item in currentPageItems) {
        if (statusMap.TryGetValue(item.OrderId!, out var status)) {
            item.DeliveryStatus = status;
        }
    }

    return currentPageItems;
}
```

:::note OnAfterLoaded は同期
`OnAfterLoaded` は同期メソッドです（`IEnumerable` を返す）。
非同期が必要な場合は `.GetAwaiter().GetResult()` か、`OnAfterLoaded` の外で処理するよう設計してください。
:::

---

## ValidateSearchCondition — 検索条件のバリデーション（オプション）

検索を実行する前に条件の整合性チェックを行いたい場合にオーバーライドします。
エラーをセットすると `LoadAsync` の実行がスキップされます。

```csharp
public override void ValidateSearchCondition(
    OrderQuerySearchCondition searchCondition,
    IPresentationContext<OrderQuerySearchConditionMessages> context) {

    var f = searchCondition.Filter;
    if (f?.OrderDateFrom > f?.OrderDateTo) {
        context.Messages.Filter?.OrderDateFrom?
            .AddError("開始日は終了日以前の日付を指定してください。");
    }
}
```

---

## TypeScript からの呼び出し

```typescript
// エンドポイントとパラメータ型は LoadFeature から取得
const endpoint = LoadFeature.Endpoint['OrderQuery']
// → POST "/api/order-query/load"

const condition: LoadFeature.ParamType['OrderQuery'] = {
  filter: {
    customerName: "田中",
    orderDateFrom: "2024-01-01",
    orderDateTo:   "2024-12-31",
  },
  sort: ["OrderDate DESC"],
  skip: 0,
  take: 20,
}

const response = await fetch(endpoint, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(condition),
})

// 戻り値: { items: OrderQueryDisplayData[], totalCount: number }
const result: LoadFeature.ReturnType['OrderQuery'] = await response.json()
console.log(`${result.totalCount} 件中 ${result.items.length} 件を表示`)
```

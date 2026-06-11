---

---

# 検索条件

QueryModel の定義から、検索条件を表す `{Name}SearchCondition` クラスが自動生成されます。
文字列の部分一致・数値の範囲検索・ソート・ページングが標準で対応しています。

## SearchCondition クラスの構造

```csharp
public class OrderQuerySearchCondition {
    public OrderQuerySearchConditionFilter? Filter { get; set; }
    public List<string>?                   Sort   { get; set; }
    public int?                            Skip   { get; set; }
    public int?                            Take   { get; set; }
}
```

| プロパティ | 型                            | 説明                                     |
| ---------- | ----------------------------- | ---------------------------------------- |
| `Filter`   | `{Name}SearchConditionFilter` | 絞り込み条件を格納するネストオブジェクト |
| `Sort`     | `List<string>?`               | ソートキーのリスト（後述）               |
| `Skip`     | `int?`                        | ページングのオフセット（件数）           |
| `Take`     | `int?`                        | 1ページの最大件数                        |

### Filter オブジェクト

Filter オブジェクトは、スキーマ定義の各属性に対応した検索条件フィールドを持ちます。

型別に自動生成される検索条件の例：

| 属性の型                       | 生成される検索フィールド            | 動作                            |
| ------------------------------ | ----------------------------------- | ------------------------------- |
| `string`（Word / Description） | 文字列フィールド                    | 部分一致（LIKE）                |
| `int` / `long` / `decimal`     | `{Name}Min`, `{Name}Max`            | 範囲検索（Between）             |
| `DateTime`                     | `{Name}From`, `{Name}To`            | 範囲検索                        |
| `StaticEnum`（静的区分値）     | `{EnumName}SearchCondition`         | チェックボックス形式（OR 条件） |
| `ref-to`                       | 参照先の SearchCondition フィールド | 参照先の属性での絞り込み        |

## ソートの指定

`Sort` プロパティには `SortableMemberOf{Name}` 型のソートキー文字列を指定します。
昇順は値そのまま、降順は末尾に ` DESC` を付けます（例: `"OrderDate DESC"`）。

TypeScript では、定数として自動生成された型を使用します。

```typescript
const condition: OrderQuerySearchCondition = {
  filter: { customerName: "田中" },
  sort: ["OrderDate DESC", "OrderId"],
  skip: 0,
  take: 20,
}
```

## OnlySearchCondition — 検索専用フィールド

`OnlySearchCondition="true"` を付与した属性は、検索条件にのみ現れ、検索結果（DisplayData）には表示されません。

```xml
<OrderQuery Type="query-model" DisplayName="受注一覧">
  <OrderId   IsKey="true"                  DisplayName="受注番号" />
  <OrderDate IsNotNull="true"              DisplayName="受注日" />
  <!-- 検索条件専用。一覧には表示されない -->
  <IsUrgent  OnlySearchCondition="true"    DisplayName="緊急対応のみ" />
</OrderQuery>
```

`OnlySearchCondition` フィールドで絞り込む実際のロジックは、`CreateQuerySource` メソッド内に手動実装します。

```csharp
protected override IQueryable<OrderQuerySearchResult> CreateQuerySource(
    OrderQuerySearchCondition condition) {

    var query = _dbContext.Orders.Select(o => new OrderQuerySearchResult {
        OrderId   = o.OrderId,
        OrderDate = o.OrderDate,
    });

    // OnlySearchCondition フィールドの手動実装
    if (condition.Filter?.IsUrgent == true) {
        var threshold = DateTime.Today.AddDays(-3);
        query = query.Where(o => o.ShippedDate == null && o.OrderDate <= threshold);
    }

    return query;
}
```

## カスタム WHERE / ORDER BY

自動生成される条件に加え、`AppendWhereClause` と `AppendOrderByClause` をオーバーライドして独自の絞り込みやソートを追加できます。

```csharp
// 追加の Where 条件
protected override IQueryable<OrderQuerySearchResult> AppendWhereClause(
    IQueryable<OrderQuerySearchResult> query,
    OrderQuerySearchCondition condition) {

    // 基底クラスの自動生成条件を適用してから追加
    query = base.AppendWhereClause(query, condition);

    // 独自条件を追加
    if (condition.Filter?.StatusList?.AnyChecked() == true) {
        // ...
    }
    return query;
}
```

## 検索条件のバリデーション

`ValidateSearchCondition` メソッドをオーバーライドすることで、検索条件に対するバリデーションを実装できます。

```csharp
protected override void ValidateSearchCondition(
    OrderQuerySearchCondition condition,
    IOrderQuerySearchConditionMessages messages) {

    if (condition.Filter?.DateFrom > condition.Filter?.DateTo) {
        messages.Filter?.DateFrom?.AddError("開始日は終了日以前の日付を指定してください。");
    }
}
```

## URL パラメータ変換（ブックマーク可能な検索 URL）

検索条件は URL クエリパラメータに変換できます。これにより、検索状態をブックマークや URL 共有できます。

| URL パラメータ | 内容                                        |
| -------------- | ------------------------------------------- |
| `f`            | Filter オブジェクトの JSON（URLエンコード） |
| `s`            | Sort 配列の JSON                            |
| `t`            | Take（ページサイズ）                        |
| `p`            | Skip（ページオフセット）                    |

TypeScript の変換関数は自動生成されます。

```typescript
// 検索条件 → URL パラメータ
const params = new URLSearchParams()
toQueryParameterOfOrderQuery(condition, params)
router.push(`/orders?${params.toString()}`)

// URL パラメータ → 検索条件（ページロード時）
const condition = parseQueryParameterAsOrderQuery(
  new URLSearchParams(window.location.search)
)
```

## TypeScript での型情報

検索処理のエンドポイントとパラメータ型は、自動生成された `LoadFeature` ネームスペースで確認できます。

```typescript
// エンドポイント URL
const endpoint = LoadFeature.Endpoint['OrderQuery']
// → "/api/order-query/load"

// 型安全な呼び出し
const params: LoadFeature.ParamType['OrderQuery'] = {
  filter: { orderDateFrom: "2024-01-01" },
  sort: ["OrderDate DESC"],
  take: 20,
  skip: 0,
}
```

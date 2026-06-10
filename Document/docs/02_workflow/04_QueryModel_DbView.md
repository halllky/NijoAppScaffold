---
draft: true
---

# DB View マッピング

QueryModel を既存の DB View にマッピングすることで、複雑な集計・JOIN・パフォーマンスチューニングが必要な検索を実現できます。

## 使いどころ

通常の QueryModel は `CreateQuerySource` メソッドで DataModel から IQueryable を組み立てます。
以下のケースでは DB View へのマッピングが有効です。

- 複数テーブルを JOIN した集計結果（売上合計、在庫数など）が必要な場合
- EF Core の LINQ では表現しにくい SQL を使いたい場合（PIVOT、ウィンドウ関数など）
- `CreateQuerySource` の実装が複雑になりすぎてパフォーマンスチューニングが難しい場合

## XML での設定方法

ルート集約に `MapToView="true"` を追加します。

```xml
<SalesSummary Type="query-model" MapToView="true" DisplayName="売上サマリ">
  <SalesDate IsKey="true"  DisplayName="売上日" />
  <StoreId   IsKey="true"  DisplayName="店舗コード" />
  <TotalSales             DisplayName="売上合計" />
  <OrderCount             DisplayName="件数" />
</SalesSummary>
```

## 生成されるコード

`MapToView="true"` の場合、`SearchResult` クラスが EF Core の **キーレスエンティティ** として登録されます。

```csharp
// DbContext の OnModelCreating に自動生成される設定
modelBuilder.Entity<SalesSummarySearchResult>(entity => {
    entity.HasNoKey();
    entity.ToView("SalesSummary"); // View 名はスキーマ定義の物理名
});
```

`CreateQuerySource` の実装は不要になります（`MapToView="true"` のときは呼ばれません）。

## View の作成

View 自体は EF Core のマイグレーションでは自動作成されません。開発者が手動で作成します。

```sql
-- マイグレーションファイルに追加するか、直接 DB に適用する
CREATE VIEW SalesSummary AS
SELECT
    o.OrderDate  AS SalesDate,
    o.StoreId    AS StoreId,
    SUM(d.Price * d.Quantity) AS TotalSales,
    COUNT(*)     AS OrderCount
FROM Orders o
JOIN OrderDetails d ON o.OrderId = d.OrderId
GROUP BY o.OrderDate, o.StoreId;
```

:::note EF Core マイグレーションと View の管理
EF Core の `Add-Migration` は `HasNoKey()` + `ToView()` のエンティティに対して migration コードを生成しません。
View の作成・更新は `migrationBuilder.Sql("CREATE VIEW ...")` を使って手動で migration に追加するか、別のスクリプトで管理してください。
:::

## `MapToView="true"` のときの動作の違い

| 項目 | 通常の QueryModel | MapToView="true" |
| --- | --- | --- |
| `CreateQuerySource` | 開発者が実装（必須） | 不要（呼ばれない） |
| EF Core エンティティ | `HasKey()` あり | `HasNoKey()` キーレス |
| DB 対応オブジェクト | テーブル | View |
| `AppendWhereClause` | 有効 | 有効（View に WHERE が追加される） |
| `AppendOrderByClause` | 有効 | 有効 |
| `OnAfterLoaded` | 有効 | 有効 |

WHERE / ORDER BY は自動的に View に対してかかるため、フィルタリング・ソートは通常の QueryModel と同じように動作します。

## `IsHardCodedPrimaryKey` との組み合わせ

View にキーが存在しない（またはキーなしで使いたい）場合は `MapToView="true"` のみで問題ありません。
View 上でキーに相当する項目を `IsKey="true"` で指定することもできます（EF Core の `HasNoKey()` ではなく通常のキー設定になります）。

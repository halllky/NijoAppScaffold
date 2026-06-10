---
sidebar_position: 8
---

# 静的区分値（StaticEnum）

値がソースコード上にハードコードされる区分値です。
C# は `enum`、TypeScript はリテラル型（Union 型）として生成されます。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `<enum name="...">` 要素

## 汎用参照テーブルとの使い分け

| | 静的区分値（StaticEnum） | 汎用参照テーブル |
| --- | --- | --- |
| 値の管理 | XML（コード管理） | DB テーブル |
| 変更 | コード変更・再デプロイが必要 | 画面から随時変更可能 |
| 型安全性 | C# enum / TS リテラル型で保証 | 文字列のみ |
| 用途 | 変わらない区分（性別・ステータス） | 頻繁に変わる選択肢（部門・カテゴリ） |

詳細は [汎用参照テーブル](./DataModel_GenericLookupTable) を参照してください。

## XML での定義

```xml
<enum name="OrderStatus" desc="受注ステータス">
  <Preparing  key="1" desc="準備中" />
  <Shipped    key="2" desc="発送済み" />
  <Delivered  key="3" desc="配達完了" />
  <Cancelled  key="4" desc="キャンセル" />
</enum>
```

| 属性 | 説明 |
| --- | --- |
| `name` | C# enum 名・TypeScript 型名 |
| `desc` | 表示名（省略可） |
| `key` | **整数必須・一意**。DB に保存される値 |

## 生成される C# コード

```csharp
/// <summary>
/// 受注ステータス
/// </summary>
public enum OrderStatus {
    [Display(Name = "準備中")]
    Preparing = 1,

    [Display(Name = "発送済み")]
    Shipped = 2,

    [Display(Name = "配達完了")]
    Delivered = 3,

    [Display(Name = "キャンセル")]
    Cancelled = 4,
}
```

`[Display(Name = "...")]` 属性が自動付与されるため、UI 表示時に日本語名を取得できます。

## 生成される TypeScript コード

```typescript
/** 受注ステータス */
export type OrderStatus = '準備中' | '発送済み' | '配達完了' | 'キャンセル'
```

TypeScript 側では **表示名（`desc` 属性）の文字列リテラル**が型として使用されます。
C# の enum の `key` 整数値はシリアライズ時に変換され、TypeScript 側では表示名文字列で扱います。

## DataModel / QueryModel での使用

静的区分値は他のモデルの属性型として使用できます。

```xml
<Order Type="data-model" DisplayName="受注">
  <OrderId   IsKey="true"                 DisplayName="受注番号" />
  <!-- Status 属性の型を静的区分値 OrderStatus にする -->
  <Status    Type="enum:OrderStatus"      DisplayName="ステータス" />
</Order>
```

DB には `key` の整数値が保存されます。C# の enum と自動でマッピングされます。

## StaticEnumSearchCondition — チェックボックス形式の検索条件

QueryModel で静的区分値型の属性を使うと、**チェックボックス形式の検索条件クラス**が自動生成されます。

### 生成される C# クラス

```csharp
/// <summary>受注ステータスの検索条件クラス</summary>
public class OrderStatusSearchCondition {
    public bool Preparing  { get; set; }
    public bool Shipped    { get; set; }
    public bool Delivered  { get; set; }
    public bool Cancelled  { get; set; }

    /// <summary>いずれかの値が選択されているかを返します。</summary>
    public bool AnyChecked() {
        if (Preparing)  return true;
        if (Shipped)    return true;
        if (Delivered)  return true;
        if (Cancelled)  return true;
        return false;
    }
}
```

### 生成される TypeScript 型

```typescript
/** 受注ステータスの検索条件オブジェクト */
export type OrderStatusSearchCondition = {
  Preparing?:  boolean
  Shipped?:    boolean
  Delivered?:  boolean
  Cancelled?:  boolean
}
```

### AnyChecked の使い方

検索条件で `AnyChecked()` を使うと、「何も選択されていないときは全件表示」のパターンを簡潔に実装できます。

```csharp
protected override IQueryable<OrderQuerySearchResult> AppendWhereClause(
    IQueryable<OrderQuerySearchResult> query,
    OrderQuerySearchCondition condition) {

    query = base.AppendWhereClause(query, condition);

    // 何もチェックされていなければ絞り込みなし（全ステータス表示）
    if (condition.Filter?.Status?.AnyChecked() == true) {
        var allowedStatuses = new List<OrderStatus>();
        if (condition.Filter.Status.Preparing)  allowedStatuses.Add(OrderStatus.Preparing);
        if (condition.Filter.Status.Shipped)    allowedStatuses.Add(OrderStatus.Shipped);
        if (condition.Filter.Status.Delivered)  allowedStatuses.Add(OrderStatus.Delivered);
        if (condition.Filter.Status.Cancelled)  allowedStatuses.Add(OrderStatus.Cancelled);
        query = query.Where(o => allowedStatuses.Contains(o.Status));
    }

    return query;
}
```

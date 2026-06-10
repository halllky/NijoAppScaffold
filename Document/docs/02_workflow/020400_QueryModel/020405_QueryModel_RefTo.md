---
draft: true
---

# 参照先絞り込み（ref-to）

QueryModel に `ref-to` を定義すると、参照先の属性を使った絞り込み検索が自動生成されます。

## 基本的な使い方

```xml
<OrderQuery Type="query-model" DisplayName="受注一覧">
  <OrderId   IsKey="true"  DisplayName="受注番号" />
  <OrderDate               DisplayName="受注日" />

  <!-- 得意先への参照 -->
  <CustomerRef Type="ref-to:Customer" DisplayName="得意先" />
</OrderQuery>
```

`ref-to:Customer` を定義すると、`OrderQuerySearchCondition` に Customer の検索条件フィールドが自動追加されます。

```csharp
public class OrderQuerySearchConditionFilter {
    // OrderQuery 自身のフィールド
    public string?   OrderId       { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo   { get; set; }

    // ref-to:Customer から自動追加されたフィールド（Customer の SearchCondition メンバー）
    public string?   CustomerRef_CustomerId   { get; set; }
    public string?   CustomerRef_CustomerName { get; set; }
    // ...
}
```

Customer に定義された検索条件フィールドが、`{参照名}_{フィールド名}` の形で OrderQuery の SearchCondition に現れます。

## 表示用データへの反映

DisplayData でも参照先の属性がネストして表示されます。

```csharp
public class OrderQueryDisplayData {
    public string? OrderId   { get; set; }
    public DateTime? OrderDate { get; set; }

    // 参照先の DisplayDataRef オブジェクト
    public CustomerDisplayDataRef? CustomerRef { get; set; }
}

public class CustomerDisplayDataRef {
    public string? CustomerId   { get; set; }
    public string? CustomerName { get; set; }
}
```

`{参照先名}DisplayDataRef` クラスが自動生成され、参照先のキー属性と表示用属性が含まれます。

## 参照先が子孫集約の場合

参照先が Root ではなく Children / Child の場合、親への参照が `Parent` という名前で表れます。

```xml
<OrderQuery Type="query-model" DisplayName="受注一覧">
  <!-- OrderDetail（受注明細）への参照 -->
  <DetailRef Type="ref-to:Order/OrderDetails" DisplayName="明細参照" />
</OrderQuery>
```

この場合、`DetailRef_Parent_{...}` という形で親集約（Order）の情報も SearchCondition に含まれます。

## パフォーマンスの考慮

ref-to の数が増えると、発行される SQL の JOIN 数も増えます。
パフォーマンスが問題になる場合は、参照先の属性の少ない軽量な QueryModel を別に定義して使い分けることを検討してください。

```xml
<!-- 受注一覧用（担当者の氏名だけが必要） -->
<OrderQuery Type="query-model" DisplayName="受注一覧">
  <AssigneeRef Type="ref-to:EmployeeLite" DisplayName="担当者" />
</OrderQuery>

<!-- 軽量な従業員 QueryModel（ID と氏名のみ） -->
<EmployeeLite Type="query-model" DisplayName="従業員（軽量）">
  <EmployeeId   IsKey="true" DisplayName="従業員番号" />
  <EmployeeName             DisplayName="氏名" />
</EmployeeLite>
```

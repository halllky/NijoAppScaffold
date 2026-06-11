---

---

# StructureDisplayData

StructureModel を **CommandModel の引数**として使用する場合、単純な C# クラスではなく `{Name}DisplayData` クラスが生成されます。

## PlainStructure と StructureDisplayData の違い

|                 | PlainStructure | StructureDisplayData                |
| --------------- | -------------- | ----------------------------------- |
| クラス名        | `{Name}`       | `{Name}DisplayData`                 |
| TypeScript 型名 | `{Name}`       | `{Name}DisplayData`                 |
| 生成タイミング  | 常に生成       | CommandModel の引数に指定されたとき |
| 削除フラグ      | なし           | Children に `WillBeDeleted` あり    |
| 編集管理        | なし           | あり                                |

`StructureDisplayData` は画面編集を前提とした設計になっており、削除フラグなどの仕組みが含まれています。

## 生成される C# クラスの構造

```csharp
// ルート
public partial class OrderFormDisplayData {
    public DateTime? OrderDate  { get; set; }
    public string?   CustomerId { get; set; }
    public List<OrderFormDisplayDataDetails>? Details { get; set; }
}

// Children の StructureDisplayData
public partial class OrderFormDisplayDataDetails {
    // 削除予定フラグ（UI での行削除操作を表現）
    public bool WillBeDeleted { get; set; }

    public int?    LineNo    { get; set; }
    public string? ProductId { get; set; }
    public int?    Quantity  { get; set; }
}
```

`Version` フィールドは `HasVersion = false` のため生成されません。
（StructureModel は DB 永続化がないため楽観排他制御は不要）

## TypeScript での使用

```typescript
// StructureDisplayData の型（自動生成）
export type OrderFormDisplayData = {
  orderDate?:  string
  customerId?: string
  details?:    OrderFormDisplayDataDetails[]
}

export type OrderFormDisplayDataDetails = {
  willBeDeleted?: boolean  // 行削除フラグ
  lineNo?:        number
  productId?:     string
  quantity?:      number
}
```

## Execute メソッドでの受け取り方

```csharp
public override async Task Execute受注登録Async(
    OrderFormDisplayData param,
    IPresentationContext<OrderFormDisplayDataMessages> context) {

    // Children の WillBeDeleted を見て処理を分岐
    foreach (var detail in param.Details ?? []) {
        if (detail.WillBeDeleted) {
            // 削除対象としてマーク
        } else {
            // 登録・更新対象として処理
        }
    }
}
```

## メッセージコンテナ

引数が `StructureDisplayData` の場合、ネストしたメッセージコンテナが自動生成されます。

```csharp
// メッセージコンテナ（バリデーションエラーのセット用）
public interface IOrderFormDisplayDataMessages {
    IValueMemberMessages? OrderDate  { get; }
    IValueMemberMessages? CustomerId { get; }
    IOrderFormDisplayDataDetailsMessagesCollection? Details { get; }
}
```

Children のメッセージは行番号付きでセットできます。

```csharp
// 明細行 0 の ProductId にエラーをセット
context.Messages.Details?[0]?.ProductId?.AddError("商品コードが存在しません。");
```

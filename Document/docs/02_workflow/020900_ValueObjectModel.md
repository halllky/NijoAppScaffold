---
sidebar_position: 9
---

# ValueObjectModel（値オブジェクト）

識別子や特殊な値を **専用の型**として定義します。
`string` の代わりに専用型を使うことで、異なる種類の文字列を誤って代入したときにコンパイルエラーで気づけます。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Type="value-object"`

## なぜ string ではなく ValueObject を使うか

```csharp
// string のまま使う場合 → コンパイル時にエラーにならない
string customerId = "C001";
string productId  = "P001";
customerId = productId;  // ← 意図しない代入でもエラーなし

// ValueObject を使う場合 → コンパイルエラーで気づける
CustomerId customerId = (CustomerId)"C001";
ProductId  productId  = (ProductId)"P001";
customerId = productId;  // ← コンパイルエラー！
```

集約のキーや業務上の意味を持つ値に使うことで、安全なコードが書けます。

## XML での定義

```xml
<!-- シンプルな値オブジェクト -->
<CustomerId Type="value-object" DisplayName="得意先コード" />
<ProductId  Type="value-object" DisplayName="商品コード" />
```

ValueObject はメンバーを持ちません（常に単一の文字列値を包む型です）。

## 生成される C# クラス

```csharp
/// <summary>
/// 得意先コード。
/// 誤って得意先コードではない文字列型の項目に代入してしまったときにエラーで気付けるよう、値オブジェクトで定義している。
/// stringと相互に変換するときは明示的に(string)や(CustomerId)でキャストする。
/// </summary>
public partial class CustomerId : IEquatable<CustomerId> {

    public CustomerId(string value) { _value = value; }

    // 値の比較（Equals / == / != / GetHashCode）
    public override bool Equals(object? obj) { ... }
    public bool Equals(CustomerId? other) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==(CustomerId? left, CustomerId? right) { ... }
    public static bool operator !=(CustomerId? left, CustomerId? right) { ... }

    // string との相互変換（明示的キャスト）
    public static explicit operator string?(CustomerId? value) => value?._value;
    public static explicit operator CustomerId?(string? value)
        => value == null ? null : new CustomerId(value);

    public override string ToString() => _value;

    // JSON シリアライズ用コンバーター（自動登録）
    public class JsonConverter : System.Text.Json.Serialization.JsonConverter<CustomerId?> { ... }
}
```

## 生成される TypeScript 型

```typescript
/**
 * 得意先コード。
 * 誤って得意先コードではない文字列型の項目に代入してしまったときにエラーで気付けるよう、公称型で定義している。
 */
export type CustomerId = string & { readonly __brand: unique symbol }
```

TypeScript の公称型（nominal typing）パターンです。
`string` との互換性はあるように見えますが、`CustomerId` 型として宣言された変数に `ProductId` 型を代入しようとすると型エラーになります。

```typescript
// string → CustomerId への変換
const id = "C001" as CustomerId

// 型安全な使い方
function loadCustomer(id: CustomerId) { ... }
loadCustomer("C001" as CustomerId)  // OK
loadCustomer("P001" as ProductId)   // TypeScript エラー
```

## JSON シリアライズ

ValueObject は JSON で文字列として自動的にシリアライズ・デシリアライズされます。
`JsonConverter` が自動生成・登録されるため、開発者の設定は不要です。

```csharp
// シリアライズ例
var order = new Order { CustomerId = (CustomerId)"C001" };
var json = JsonSerializer.Serialize(order);
// → { "customerId": "C001" }

// デシリアライズ例
var restored = JsonSerializer.Deserialize<Order>(json);
// → restored.CustomerId は CustomerId 型のまま
```

## DataModel での使用方法

ValueObject は他のモデルの属性型として使用できます。

```xml
<!-- ValueObject の定義 -->
<CustomerId Type="value-object" DisplayName="得意先コード" />

<!-- DataModel でキーとして使用 -->
<Customer Type="data-model" DisplayName="得意先">
  <Id   IsKey="true" Type="value-object:CustomerId" DisplayName="得意先コード" />
  <Name IsNotNull="true"                             DisplayName="得意先名" />
</Customer>
```

ValueObject をキーに使うことで、`string` キーと `CustomerId` キーを誤って混在させることを防げます。

## string との使い分け基準

| 使用する型 | 適しているケース |
| --- | --- |
| `string` | 名前・住所・備考など、他の文字列と混在しても問題ない値 |
| **ValueObject** | コード・ID など、**種類を混在させてはいけない**値 |
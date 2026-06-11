---
sidebar_label: 構造体モデル
sidebar_position: 6
---

# StructureModel（構造体モデル）

サーバーとクライアントで共有するデータ構造を定義します。
DataModel のような DB 永続化やキーは不要で、**単純なデータクラスとして C# と TypeScript の定義だけを生成**します。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Type="structure-model"`

## 自動生成されるモジュール

| モジュール                                  | 説明                                                           |
| ------------------------------------------- | -------------------------------------------------------------- |
| C# クラス                                   | `{PhysicalName}` クラス（サフィックスなし）                    |
| TypeScript 型                               | `{PhysicalName}` 型                                            |
| TypeScript ファクトリ関数                   | `createNew{PhysicalName}()`                                    |
| StructureDisplayData（CommandModel 引数時） | [StructureDisplayData](./020601_StructureModel_DisplayData.md) |

## DataModel / QueryModel との使い分け

| モデル             | DB 永続化            | キー | 用途                                               |
| ------------------ | -------------------- | ---- | -------------------------------------------------- |
| **DataModel**      | あり                 | 必須 | 登録・更新・削除するマスタ・トランザクションデータ |
| **QueryModel**     | なし（読み取り専用） | あり | 一覧検索・フィルタリング                           |
| **StructureModel** | なし                 | 不要 | CommandModel の入出力・画面間のデータ転送          |

StructureModel は **DB とは関係ない任意のデータ構造** を定義するときに使います。

## XML 定義例

```xml
<!-- 単純な構造 -->
<MailSendParam Type="structure-model" DisplayName="メール送信パラメータ">
  <ToAddress   IsNotNull="true" DisplayName="宛先" />
  <Subject     IsNotNull="true" DisplayName="件名" />
  <Body        IsNotNull="true" DisplayName="本文" />
</MailSendParam>

<!-- ネストした構造（Child / Children も使用可能） -->
<OrderForm Type="structure-model" DisplayName="受注フォーム">
  <OrderDate  IsNotNull="true" DisplayName="受注日" />
  <CustomerId IsNotNull="true" DisplayName="得意先コード" />
  <Details Type="children" DisplayName="明細">
    <LineNo    IsKey="true"    DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
    <Quantity  IsNotNull="true" DisplayName="数量" />
  </Details>
</OrderForm>
```

## 生成される C# クラス

```csharp
public partial class OrderForm {
    public DateTime? OrderDate  { get; set; }
    public string?   CustomerId { get; set; }
    public List<OrderFormDetails>? Details { get; set; }
}

public partial class OrderFormDetails {
    public int?    LineNo    { get; set; }
    public string? ProductId { get; set; }
    public int?    Quantity  { get; set; }
}
```

クラス名にサフィックスはつきません（`OrderForm` のまま）。
Children の子クラスは `{親名}{Children名}` の形で生成されます。

## 生成される TypeScript 型・ファクトリ関数

```typescript
export type OrderForm = {
  orderDate?:  string
  customerId?: string
  details?:    OrderFormDetails[]
}

export type OrderFormDetails = {
  lineNo?:    number
  productId?: string
  quantity?:  number
}

// ファクトリ関数：型安全に新規オブジェクトを作成
export const createNewOrderForm = (): OrderForm => ({
  orderDate:  undefined,
  customerId: undefined,
  details:    [],
})
```

`createNew{Name}()` 関数は、すべてのフィールドが初期化された空のオブジェクトを返します。
TypeScript で新規フォームデータを作成するときに使います。

## StructureModel を CommandModel の引数にする場合

StructureModel を CommandModel の引数として使用すると、単純なクラスではなく `{Name}DisplayData` が生成されます。
`DisplayData` には画面編集のための削除フラグや変更管理の仕組みが追加されています。

詳細は [StructureDisplayData](./020601_StructureModel_DisplayData.md) を参照してください。

## 制約

- キー（`IsKey`）は指定できますが、DB キー制約としては機能しません
- `ref-to` は使用可能ですが、外部キー制約は生成されません
- 再帰定義（自己参照）はサポートされていません

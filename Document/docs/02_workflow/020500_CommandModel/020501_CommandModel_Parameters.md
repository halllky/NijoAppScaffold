---

---

# 引数・戻り値の型定義

CommandModel の引数（Parameter）と戻り値（ReturnValue）には、他のモデルで定義した型を指定します。

## 指定できる型の組み合わせ

CommandModel の引数・戻り値にはそれぞれ以下のいずれかを指定します。

| 指定方法                         | C# 型                             | 用途                             |
| -------------------------------- | --------------------------------- | -------------------------------- |
| 指定なし                         | なし（引数なし・戻り値なし）      | 引数・戻り値が不要な処理         |
| `{StructureModel名}`             | `{Name}DisplayData`（画面編集用） | 任意の入力フォーム・出力データ   |
| `{QueryModel名}:SearchCondition` | `{Name}SearchCondition`           | 検索条件を引数にする処理         |
| `{QueryModel名}:DisplayData`     | `{Name}DisplayData`               | 検索結果を引数・戻り値にする処理 |

:::note StructureModel を引数にすると DisplayData が生成される
StructureModel 名を `Parameter` 属性に指定すると、単純な `Form` クラスではなく `FormDisplayData` クラスが生成されます。
`DisplayData` には削除フラグや変更管理のための仕組みが含まれており、画面での編集操作を前提としています。
詳細は [StructureDisplayData](../020600_StructureModel/020601_StructureModel_DisplayData.md) を参照してください。
:::

## XML での指定方法

### パターン1: StructureModel を引数・戻り値に使う（最も一般的）

```xml
<!-- StructureModel の定義 -->
<受注登録Form Type="structure-model" DisplayName="受注登録フォーム">
  <OrderDate  IsNotNull="true" DisplayName="受注日" />
  <CustomerId IsNotNull="true" DisplayName="得意先コード" />
  <Details Type="children" DisplayName="明細">
    <LineNo   IsKey="true"     DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
    <Quantity  IsNotNull="true" DisplayName="数量" />
  </Details>
</受注登録Form>

<!-- CommandModel で参照（Parameter はXML属性で指定する） -->
<受注登録 Type="command-model" DisplayName="受注登録" Parameter="受注登録Form" />
```

生成される Execute メソッドのシグネチャ:
```csharp
public abstract Task Execute受注登録Async(
    受注登録FormDisplayData param,
    IPresentationContext<受注登録FormDisplayDataMessages> context);
```

### パターン2: QueryModel の SearchCondition を引数にする

```xml
<受注一括処理 Type="command-model" DisplayName="受注一括処理"
             Parameter="OrderQuery:SearchCondition" />
```

生成される Execute メソッドのシグネチャ:
```csharp
public abstract Task Execute受注一括処理Async(
    OrderQuerySearchCondition param,
    IPresentationContext<OrderQuerySearchConditionMessages> context);
```

### パターン3: QueryModel の DisplayData を戻り値にする

```xml
<受注詳細取得 Type="command-model" DisplayName="受注詳細取得"
             Parameter="受注検索キー"
             ReturnValue="受注詳細Query:DisplayData" />
```

生成される Execute メソッドのシグネチャ:
```csharp
public abstract Task Execute受注詳細取得Async(
    受注検索キーDisplayData param,
    IPresentationContextWithReturnValue<受注詳細QueryDisplayData, 受注検索キーDisplayDataMessages> context);
// context.ReturnValue に 受注詳細QueryDisplayData をセットして返す
```

### パターン4: 引数なし・戻り値なし

```xml
<月次集計 Type="command-model" DisplayName="月次集計実行">
</月次集計>
```

生成される Execute メソッドのシグネチャ:
```csharp
public abstract Task Execute月次集計Async(
    IPresentationContext context);
```

## TypeScript での型情報

コマンド処理のエンドポイントとパラメータ型は、自動生成された `ExecuteFeature` ネームスペースで確認できます。

```typescript
// エンドポイント URL
const endpoint = ExecuteFeature.Endpoint['受注登録']
// → "/api/受注登録/execute"

// パラメータ型（型安全な呼び出し）
const param: ExecuteFeature.ParamType['受注登録'] = {
  orderDate: "2024-06-01",
  customerId: "C001",
  details: [
    { lineNo: 1, productId: "P001", quantity: 5 }
  ]
}

// 戻り値の型
type ReturnVal = ExecuteFeature.ReturnType['受注登録']
```

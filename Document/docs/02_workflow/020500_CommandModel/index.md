---
sidebar_position: 5
---

# CommandModel（コマンドモデル）

「処理の1サイクル」を定義します。関数定義のように **引数（Parameter）** と **戻り値（ReturnValue）** を持ちます。
データの登録・更新だけでなく、「複雑な画面の初期表示」「データ集計 Excel 出力」といった操作も CommandModel として定義します。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Type="command-model"`

## 自動生成されるモジュール

CommandModel は他のモデルと異なり、**処理の中身（ロジック）は自動生成されません**。生成されるのはフロントエンドとバックエンドをつなぐ枠組みです。

| モジュール                                           | 詳細ページ                                                  |
| ---------------------------------------------------- | ----------------------------------------------------------- |
| Execute メソッドのシグネチャ・Web API エンドポイント | このページ参照                                              |
| 引数・戻り値の型定義（Parameter / ReturnValue）      | [引数・戻り値の型定義](./020501_CommandModel_Parameters.md) |

## 設計指針

### コマンドモデルの粒度

**「1つのユースケース（ユーザーのアクション）」につき「1つの CommandModel」** を定義します。
「受注登録ボタンを押す」「月次集計を実行する」「CSV を取り込む」がそれぞれ CommandModel になります。

DataModel が「名詞（データ）」であるのに対し、CommandModel は「動詞（処理）」です。

### 引数と戻り値のデータ構造

CommandModel 自体はデータ構造を持ちません。
引数・戻り値が必要な場合は **StructureModel** または **QueryModel**（SearchCondition / DisplayData）を指定します。

詳細は [引数・戻り値の型定義](./020501_CommandModel_Parameters.md) を参照してください。

## Execute メソッド

CommandModel の処理本体は `Execute{Name}Async` メソッドに実装します。**必ず開発者が実装する必要があります。**

```csharp
// Application Service に生成されるスタブ
public abstract Task Execute受注登録Async(
    受注登録DisplayData param,
    IPresentationContext<受注登録DisplayDataMessages> context);

// 開発者が実装
public override async Task Execute受注登録Async(
    受注登録DisplayData param,
    IPresentationContext<受注登録DisplayDataMessages> context) {

    // トランザクションを開始
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

    // DataModel の CRUD メソッドを呼び出す
    var result = await CreateOrderAsync(param.ToCreateCommand(), context);
    if (!result.IsSuccess) {
        await transaction.RollbackAsync();
        return;
    }

    await transaction.CommitAsync();
}
```

## 基本的な XML 定義例

```xml
<!-- 引数と戻り値あり -->
<受注登録 Type="command-model" DisplayName="受注登録">
  <Parameter   Type="structure-model:受注登録Form"  />
  <ReturnValue Type="query-model:受注詳細Query" />
</受注登録>

<!-- 引数なし・戻り値なし -->
<月次集計 Type="command-model" DisplayName="月次集計実行">
</月次集計>
```

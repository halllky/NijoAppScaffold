---
sidebar_label: コマンド
sidebar_position: 5
---

# コマンド

「処理の1サイクル」を定義します。関数定義のように **引数（Parameter）** と **戻り値（ReturnValue）** を持ちます。
データの登録・更新だけでなく、「複雑な画面の初期表示」「データ集計 Excel 出力」といった操作もコマンドとして定義します。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Commands` セクションにルート集約を定義します。

## 自動生成されるモジュール

コマンドはデータ構造と異なり、**処理の中身（ロジック）は自動生成されません**。生成されるのはフロントエンドとバックエンドをつなぐ枠組みです。

| モジュール                                           | 詳細ページ                                                  |
| ---------------------------------------------------- | ----------------------------------------------------------- |
| Execute メソッドのシグネチャ・Web API エンドポイント | このページ参照                                              |
| 引数・戻り値の型定義（Parameter / ReturnValue）      | [引数・戻り値の型定義](./020501_Parameters.md) |
| IPresentationContext（エラー管理・戻り値・検証モード） | [IPresentationContext](./020502_PresentationContext.md) |

## 設計指針

### コマンドの粒度

**「1つのユースケース（ユーザーのアクション）」につき「1つのコマンド」** を定義します。
「受注登録ボタンを押す」「月次集計を実行する」「CSV を取り込む」がそれぞれコマンドになります。

データ構造が「名詞（データ）」であるのに対し、コマンドは「動詞（処理）」です。

### 引数と戻り値のデータ構造

コマンド自体はデータ構造を持ちません。
引数・戻り値が必要な場合は、IsStored も IsQueryable も指定しない **構造体**、または IsQueryable 属性が指定されたデータ構造（SearchCondition / DisplayData）を指定します。

詳細は [引数・戻り値の型定義](./020501_Parameters.md) を参照してください。

## Execute メソッド

コマンドの処理本体は `Execute{Name}Async` メソッドに実装します。**必ず開発者が実装する必要があります。**

```csharp
// Application Service に生成されるスタブ
public abstract Task Execute受注登録Async(
    受注登録FormDisplayData param,
    IPresentationContext<受注登録FormDisplayDataMessages> context);

// 開発者が実装
public override async Task Execute受注登録Async(
    受注登録FormDisplayData param,
    IPresentationContext<受注登録FormDisplayDataMessages> context) {

    // IsStored 属性が指定されたデータ構造のコマンドを作成して検証する
    var command = DbContext.StartCreatingOrder();
    command.OrderDate = param.OrderDate;
    command.GetValidationErrors().CopyTo(context.Messages);

    if (context.Messages.HasError() || context.ValidationOnly) return;

    // トランザクションの範囲を明示して保存する
    await using var transaction = await DbContext.BeginTransactionAsync();

    var result = await DbContext.TrySaveAsync(command);
    if (!result.Success) {
        result.Errors.CopyTo(context.Messages);
        return;
    }

    await transaction.CommitAsync();
}
```

## 基本的な XML 定義例

```xml
<!-- 引数と戻り値あり（Parameter・ReturnValue はXML属性で指定する） -->
<受注登録 DisplayName="受注登録"
          Parameter="受注登録Form"
          ReturnValue="受注詳細Query:DisplayData" />

<!-- 引数なし・戻り値なし -->
<月次集計 DisplayName="月次集計実行" />
```

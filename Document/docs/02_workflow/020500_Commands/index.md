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
| メッセージ・戻り値・OperationContext（検証モード） | [OperationContext](./020502_OperationContext.md) |

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

コマンドごとに、Application Service に次の2つのメソッドが生成されます。

- `Execute{Name}Async`: 呼び出し元（Web、バッチ、単体テスト、ほかの処理）が呼ぶ公開メソッド。関門（後述の「すべての処理に共通の処理（関門）」）を通して `OnExecute{Name}Async` を呼びます。
- `OnExecute{Name}Async`: 処理の本体。**必ず開発者が実装する必要があります。**

```csharp
// Application Service に生成されるメソッド。
// TOperationContext は、アプリケーションが型引数に指定したクラス（テンプレートでは OperationContext）
public Task Execute受注登録Async(
    受注登録FormDisplayData param,
    受注登録FormDisplayDataMessages messages,
    TOperationContext context) {
    return InterceptAsync(受注登録Operation, param, messages, context, async () => { await OnExecute受注登録Async(param, messages, context); return null; });
}
protected abstract Task OnExecute受注登録Async(
    受注登録FormDisplayData param,
    受注登録FormDisplayDataMessages messages,
    TOperationContext context);

// 開発者が実装
protected override async Task OnExecute受注登録Async(
    受注登録FormDisplayData param,
    受注登録FormDisplayDataMessages messages,
    OperationContext context) {

    // IsStored 属性が指定されたデータ構造のコマンドを作成して検証する
    var command = DbContext.StartCreatingOrder();
    command.OrderDate = param.OrderDate;
    if (!command.TryValidate(out var validated, out var errors)) {
        errors.CopyTo(messages);
        return;
    }
    if (context.ValidationOnly) return;

    // トランザクションの範囲を明示して保存する
    await using var transaction = await DbContext.Database.BeginTransactionAsync();

    var result = await DbContext.TrySaveAsync(validated);
    if (!result.Success) {
        result.Errors.CopyTo(messages);
        return;
    }

    await transaction.CommitAsync();
}
```

## すべての処理に共通の処理（関門）

コマンドの `Execute{Name}Async` と、クエリの一覧検索（`LoadAsync`）・参照検索（`LoadRefs{Name}Async`）は、どこから呼ばれても必ず Application Service の `InterceptAsync` を通して処理の本体を実行します。
権限の確認やログなど、処理の種類を問わない共通の処理は、このメソッドをオーバーライドして書きます。既定では何もせずに本体を実行します。

```csharp
protected override Task<TReturnValue?> InterceptAsync<TParameter, TReturnValue, TMessages>(
    Operation<OperationContext, TParameter, TReturnValue, TMessages> operation, // 呼び出された処理
    TParameter parameter,
    TMessages messages,
    OperationContext context,
    Func<Task<TReturnValue?>> invokeAsync) where TReturnValue : default {   // オーバーライドでは where TReturnValue : default が必要

    // operation.Metadata には、nijo.xml でその処理のルート要素に指定したカスタム属性が型付きで入っている
    if (!CanInvoke(operation.Metadata)) throw new ForbiddenException();

    return invokeAsync(); // 呼ばなければ本体は実行されない
}
```

`operation` は `Operation<OperationContext>.All` に並ぶ処理と同じインスタンスで、名前（`Name`）、種類（`Kind`: 一覧検索・参照検索・コマンド）、URL（`Path`）、メタデータ（`Metadata`）を持ちます。
メタデータについては [カスタム属性](../025000_CustomAttribute.md) の「クエリとコマンドのメタデータ」を参照してください。
処理の宣言をもとに権限を確かめる例は、デモ101（`demo/101_販売管理システム`）の `Core/ログイン・認証/処理の呼び出し可否.cs` です。

## 基本的な XML 定義例

```xml
<!-- 引数と戻り値あり（Parameter・ReturnValue はXML属性で指定する） -->
<受注登録 DisplayName="受注登録"
          Parameter="受注登録Form"
          ReturnValue="受注詳細Query:DisplayData" />

<!-- 引数なし・戻り値なし -->
<月次集計 DisplayName="月次集計実行" />
```

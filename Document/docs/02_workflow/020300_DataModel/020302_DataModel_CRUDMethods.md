---
draft: true
---

# CRUD メソッド

DataModel の定義から、新規登録・更新・物理削除の3種類のメソッドが自動生成されます。
これらのメソッドは Application Service クラスのメソッドとして生成されます。

## 生成されるメソッド一覧

| メソッド名 | 処理 | 引数 | 戻り値 |
| --- | --- | --- | --- |
| `Create{集約名}Async` | 新規登録 | `{集約名}CreateCommand` | `DataModelSaveResult<{集約名}DbEntity>` |
| `Update{集約名}Async` | 更新 | `{集約名}UpdateOrDeleteKey`, 更新関数 | `DataModelSaveResult<{集約名}DbEntity>` |
| `Delete{集約名}Async` | 物理削除 | `{集約名}UpdateOrDeleteKey` | `DataModelSaveResult<{集約名}DbEntity>` |

## SaveCommand クラス

CRUD メソッドの引数には EF Core エンティティクラス（`DbEntity`）ではなく、用途に特化した **SaveCommand クラス**を使います。

### 3種類の SaveCommand

| クラス名 | 用途 | 含まれる項目 |
| --- | --- | --- |
| `{集約名}CreateCommand` | 新規登録 | 全項目（ただしシーケンス自動採番項目は除く） |
| `{集約名}UpdateCommand` | 更新時の値 | 全項目 |
| `{集約名}UpdateOrDeleteKey` | 更新・削除対象の特定 | キー項目 + `Version`（楽観排他用） |

### なぜ DbEntity ではなく SaveCommand を使うのか

更新処理の引数に `DbEntity` を直接使うと、更新すべきでないカラムまで誤って上書きしてしまうリスクがあります。
SaveCommand は操作ごとに必要な項目のみを持つため、実装ミスを防ぎます。

例えば更新時に、参照先の詳細情報（ナビゲーションプロパティ）は不要です。
`UpdateCommand` では参照先のキーのみを保持する `{参照名}Key` クラスが使われます。

### DbEntity との相互変換

`CreateCommand` および `UpdateCommand` には以下のメソッドが生成されます：

```csharp
// SaveCommand → DbEntity への変換（Createコマンドに生成）
public OrderDbEntity ToDbEntity() { ... }

// DbEntity → UpdateCommand への変換（Updateコマンドに生成）
public static OrderUpdateCommand FromDbEntity(OrderDbEntity dbEntity) { ... }
```

## Create（新規登録）

### メソッドシグネチャ

```csharp
public virtual async Task<DataModelSaveResult<OrderDbEntity>> CreateOrderAsync(
    OrderCreateCommand command,
    IPresentationContext context,
    IMessageContainerSetter? messageOwner = null)
```

### 処理の流れ

1. `command.ToDbEntity()` でエンティティを生成
2. `CreatedAt`, `UpdatedAt`, `CreateUser`, `UpdateUser`, `Version = 0` を自動設定
3. スキーマ定義から自動生成されたバリデーション（必須・最大長・文字種・桁数）を実行
4. `OnBeforeCreateOrderAsync`（フック）を呼び出し
5. エラーがある場合は中断して戻る
6. `context.ValidationOnly == true` の場合は DB への保存を行わず戻る
7. SAVEPOINT を作成して `DbContext.SaveChangesAsync()` を実行
8. `OnAfterCreateOrderAsync`（フック）を呼び出し
9. SAVEPOINT を解放して完了

### 呼び出し例

```csharp
// CommandModel の Execute メソッドの中で呼び出す
await DbContext.Database.BeginTransactionAsync();

var command = new OrderCreateCommand {
    OrderId   = "ORD-001",
    OrderDate = DateTime.Today,
    CustomerRef = new CustomerKeyClass { CustomerId = "C001" },
    OrderDetails = [
        new OrderDetailCreateCommand { LineNo = 1, ProductId = "P001", Quantity = 5 },
    ],
};

var result = await CreateOrderAsync(command, context);
if (!result.IsSuccess) {
    await DbContext.Database.RollbackTransactionAsync();
    return result.ToFailureResponse();
}

await DbContext.Database.CommitTransactionAsync();
```

## Update（更新）

### メソッドシグネチャ

```csharp
// 非同期更新関数版
public virtual async Task<DataModelSaveResult<OrderDbEntity>> UpdateOrderAsync(
    OrderUpdateOrDeleteKey key,
    Func<OrderUpdateCommand, Task> updater,
    IPresentationContext context,
    IMessageContainerSetter? messageOwner = null)

// 同期更新関数版（内部で非同期版に委譲）
public virtual Task<DataModelSaveResult<OrderDbEntity>> UpdateOrderAsync(
    OrderUpdateOrDeleteKey key,
    Action<OrderUpdateCommand> updater,
    IPresentationContext context,
    IMessageContainerSetter? messageOwner = null)
```

### 処理の流れ

1. `key` のキー項目が null でないかチェック（空なら中断）
2. DB からキーで更新前データを取得（見つからなければエラー）
3. `UpdateCommand.FromDbEntity(beforeEntity)` で更新前の値を UpdateCommand に変換
4. `updater` 関数を呼び出し（開発者が更新したい項目を書き換える）
5. `command.ToDbEntity()` で更新後エンティティを生成
6. メタデータ（`Version + 1`, `UpdatedAt`, `UpdateUser`）を自動設定
7. バリデーション実行 → フック実行 → `ValidationOnly` チェック
8. SAVEPOINT 作成 → EF Core の `EntityState.Modified` でアタッチ → `SaveChangesAsync()`
9. 楽観排他エラー（`DbUpdateConcurrencyException`）の場合は専用メッセージを返す
10. 後処理フック実行 → SAVEPOINT 解放

### 呼び出し例

```csharp
var key = new OrderUpdateOrDeleteKey {
    OrderId = "ORD-001",
    Version = originalVersion,  // 画面表示時に取得したバージョンを渡す
};

var result = await UpdateOrderAsync(key, command => {
    command.OrderDate = DateTime.Today;
    // 変更する項目だけ書き換える
}, context);
```

:::warning バージョンの渡し方
`Version` には、**画面を初期表示したときに取得したバージョン値**を渡してください。
更新直前に再取得した最新版を渡してしまうと、楽観排他制御の意味がなくなります。
:::

### Children の更新ルール

`Children`（1対多の子集約）の更新は、更新前後のレコードをキーで照合して以下のように処理されます：

- 更新前に存在し、更新後も存在する → `UPDATE`
- 更新前に存在せず、更新後に存在する → `INSERT`
- 更新前に存在し、更新後に存在しない → `DELETE`

## Delete（物理削除）

### メソッドシグネチャ

```csharp
public virtual async Task<DataModelSaveResult<OrderDbEntity>> DeleteOrderAsync(
    OrderUpdateOrDeleteKey command,
    IPresentationContext context,
    IMessageContainerSetter? messageOwner = null)
```

物理削除では `UpdateCommand` は不要です。`UpdateOrDeleteKey`（キー + Version）だけを渡します。

### 処理の流れ

Update と同様のフロー（キーチェック → データ取得 → フック → SaveChanges → 後処理）ですが、`EntityState.Deleted` をセットして削除します。

## フック（拡張ポイント）

各処理には以下のフックメソッドが自動生成されます。Application Service を継承したクラスでオーバーライドして実装します。

### OnBefore（保存前フック）

```csharp
// 新規登録前
public virtual void OnBeforeCreateOrder(
    OrderCreateCommand command,
    IOrderMessages messages,
    IPresentationContext context) { }

// 更新前
public virtual void OnBeforeUpdateOrder(
    OrderUpdateCommand command,
    OrderDbEntity oldValue,  // 更新前のデータ
    IOrderMessages messages,
    IPresentationContext context) { }

// 物理削除前
public virtual void OnBeforeDeleteOrder(
    OrderUpdateOrDeleteKey command,
    OrderDbEntity oldValue,  // 削除前のデータ
    IOrderMessages messages,
    IPresentationContext context) { }
```

**用途：** どのユースケースから更新が来ても必ず守られなければならないデータの整合性を実装します。
ここに追加したエラーメッセージがあると保存処理が中断されます。

:::note 特定ユースケースのみのバリデーション
特定のユースケースのみに適用されるバリデーションは、このフックではなく、
呼び出し元の CommandModel の処理の中で実装してください。
:::

### OnAfter（保存後フック）

```csharp
// 新規登録後（SQL発行後、コミット前）
public virtual Task OnAfterCreateOrderAsync(
    OrderDbEntity newValue,
    IOrderMessages messages,
    IPresentationContext context) => Task.CompletedTask;

// 更新後（SQL発行後、コミット前）
public virtual Task OnAfterUpdateOrderAsync(
    OrderDbEntity newValue,
    OrderDbEntity oldValue,
    IOrderMessages messages,
    IPresentationContext context) => Task.CompletedTask;

// 削除後（SQL発行後、コミット前）
public virtual Task OnAfterDeleteOrderAsync(
    OrderDbEntity oldValue,
    IOrderMessages messages,
    IPresentationContext context) => Task.CompletedTask;
```

**用途：** リードレプリカへの反映・メッセージ基盤への通知など、データ変更と常に同期が必要な処理を実装します。
このメソッド内で例外が送出された場合、SAVEPOINT へのロールバックが行われます。

## DataModelSaveResult（戻り値）

```csharp
// 成功判定
if (result.IsSuccess) {
    var savedEntity = result.Value; // 保存後の DbEntity
}

// エラー理由
switch (result.ErrorReason) {
    case DataModelSaveErrorReason.ValidationError:    // バリデーションエラー
    case DataModelSaveErrorReason.ConcurrencyError:   // 楽観排他エラー
    case DataModelSaveErrorReason.AfterSaveError:     // 後処理フックのエラー
}
```

エラーの場合でも例外はスローされません。エラー内容は `messageOwner`（または `context`）に格納されます。

## トランザクション管理

:::warning トランザクションは呼び出し元で管理する
これらのメソッド自体はトランザクションを開始・コミット・ロールバックしません。
呼び出し元の CommandModel の処理内で `DbContext.Database.BeginTransactionAsync()` を実行してください。

トランザクション未開始の状態でこれらのメソッドを呼び出すと `InvalidOperationException` がスローされます。
:::

```csharp
// CommandModel の Execute メソッドでの典型的なトランザクション管理
await using var transaction = await DbContext.Database.BeginTransactionAsync();

var createResult = await CreateOrderAsync(createCmd, context);
if (!createResult.IsSuccess) {
    await transaction.RollbackAsync();
    return;
}

var updateResult = await UpdateCustomerAsync(updateKey, updater, context);
if (!updateResult.IsSuccess) {
    await transaction.RollbackAsync();
    return;
}

await transaction.CommitAsync();
```

## `ValidationOnly` フラグ

`context.ValidationOnly == true` の場合、バリデーションとフックのみ実行し、DB への保存は行いません。

Web アプリケーションで「更新しますか？」の確認ダイアログを挟む場合、次の2巡構成になります：

1. **1巡目**（`ValidationOnly = true`）: エラーチェックのみ実施。問題なければ確認ダイアログを表示。
2. **2巡目**（`ValidationOnly = false`）: エラーチェック + 実際の DB 保存。

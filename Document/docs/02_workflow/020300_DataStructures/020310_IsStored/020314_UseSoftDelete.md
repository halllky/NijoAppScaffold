---

---

# UseSoftDelete（論理削除）

通常の[削除コマンド](./020312_CRUDMethods.md)はデータをテーブルから物理的に削除します（`DELETE FROM ...`）。
`UseSoftDelete` オプションを有効にすると、削除処理が**論理削除**に切り替わります。

## 論理削除の仕組み

論理削除を行うと、元テーブルから行を削除し、`{テーブル名}_DELETED` という専用の別テーブルにコピーします。
このため「削除された記録」が残り、後から照会できます。

```
通常のテーブル: ORDER
  ┌─────────┬───────────┬─────────┐
  │ OrderId │ OrderDate │ Version │
  ├─────────┼───────────┼─────────┤
  │ ORD-001 │ 2024-01-01│    2    │  ← 論理削除するとここから消える
  └─────────┴───────────┴─────────┘

削除済みテーブル: ORDER_DELETED
  ┌─────────────────────┬─────────┬───────────┬────────────┬───────────┐
  │ DeletedUuid（新PK） │ OrderId │ OrderDate │ DeletedAt  │ DeletedUser│
  ├─────────────────────┼─────────┼───────────┼────────────┼───────────┤
  │ xxxxxxxx-xxxx-...   │ ORD-001 │ 2024-01-01│ 2024-06-10 │ user@ex   │  ← コピーされる
  └─────────────────────┴─────────┴───────────┴────────────┴───────────┘
```

:::warning 削除フラグ
テーブルに論理削除フラグを持たせる形式を採用したい場合、特別な仕組みは特に存在しないため、単に各データ構造に削除フラグを定義し、削除コマンドを使わず更新コマンドで更新してください。
:::

## XML での有効化方法

永続化されるデータ構造のルート集約に `UseSoftDelete="true"` を追加します。

```xml
<!-- UseSoftDelete を付与するだけで論理削除に切り替わる -->
<Order IsStored="True" UseSoftDelete="true" DisplayName="受注">
  <OrderId   IsKey="true"    DisplayName="受注番号" />
  <OrderDate IsNotNull="true" DisplayName="受注日" />
</Order>
```

:::note 適用範囲
`UseSoftDelete` はルート集約にのみ指定できます。
Child / Children には指定できません（ルートが論理削除されると子孫も一緒に移送されます）。
:::

## 生成されるもの

`UseSoftDelete="true"` を付与すると、削除コマンドの動作が論理削除に**切り替わります**。呼び出し側のコードは物理削除の場合と同じです。

| 生成されるもの                   | 説明                                    |
| -------------------------------- | --------------------------------------- |
| `{集約名}DeletedDbEntity` クラス | 削除済みデータ用の EF Core エンティティ |
| `{集約名}DeletedDbSet`           | 削除済みテーブルの DbSet                |

## 削除済みテーブルの構造

元テーブルの全カラムに加えて、以下の列が**追加**されます。

| 追加カラム    | 型          | 説明                                                           |
| ------------- | ----------- | -------------------------------------------------------------- |
| `DeletedUuid` | `Guid`      | 同一キーのデータを複数回削除できるようにするための新しい主キー |
| `DeletedAt`   | `DateTime?` | 論理削除した日時                                               |
| `DeletedUser` | `string?`   | 論理削除を行ったユーザー                                       |

元テーブルのキー（例: `OrderId`）は主キーではなくなり、NULL 許容カラムとして保持されます。
`DeletedUuid` が新しい主キーになります。これにより、同じ `OrderId` のデータを削除→復元→再削除しても重複なく記録できます。

## 論理削除の実行

いったん読み込んでから削除コマンドに変換して保存します。集約全体を退避テーブルへ移送するので、読み込みが必要です。

```csharp
var loaded = await DbContext.LoadForEditOrderAsync(new OrderKeyWithVersion(originalVersion) {
    OrderId = "ORD-001",
});
if (loaded == null) { ... }

var deleteCommand = loaded.ToDeleteCommand();
deleteCommand.GetValidationErrors().CopyTo(context.Messages);
if (context.Messages.HasError() || context.ValidationOnly) return;

await using var tran = await DbContext.Database.BeginTransactionAsync();

var result = await DbContext.TrySaveAsync(deleteCommand);
if (!result.Success) {
    result.Errors.CopyTo(context.Messages);
    return;
}

await tran.CommitAsync();
```

## 処理の流れ

`TrySaveAsync` の中で次の順に処理されます。

1. 削除済みエンティティ（`{集約名}DeletedDbEntity`）を子孫ごと生成して `INSERT`
2. 元テーブルから行を子孫ごと `DELETE`（楽観排他チェックあり）

## 復元処理について

:::warning 復元処理は自動生成されません
`UseSoftDelete` は削除処理のみを生成します。復元処理は手動で実装してください。

復元方法はケースバイケースで異なるためです。
- 新しいキーを採番して新規登録として復元する
- 削除前と同じキーで `INSERT` する
- 既存の新規登録処理を流用する

いずれの方法が適切かはアプリケーション仕様によって決まります。
:::

## 採用判断の基準

| 物理削除                     | 論理削除                                 |
| ---------------------------- | ---------------------------------------- |
| 削除後にデータが参照されない | 削除後も参照・監査が必要                 |
| ストレージ効率を優先         | 削除の証跡・履歴管理が必要               |
| 参照整合性を DB に委ねる     | 参照整合性を失っても削除できる必要がある |

:::note 外部キー制約について
論理削除では元テーブルからレコードが消えるため、他テーブルからの外部キー制約が効きません。
参照元テーブルにデータが残ったまま論理削除しようとすると、DB レベルの外部キー制約違反で失敗します。
参照元を先に削除するか、物理削除・論理削除を組み合わせる設計を検討してください。
:::

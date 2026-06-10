---
draft: true
---

# 論理削除（ソフトデリート）

通常の `Delete` メソッドはデータをテーブルから物理的に削除します（`DELETE FROM ...`）。
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

## XML での有効化方法

DataModel のルート集約に `UseSoftDelete="true"` を追加します。

```xml
<!-- UseSoftDelete を付与するだけで論理削除に切り替わる -->
<Order Type="data-model" UseSoftDelete="true" DisplayName="受注">
  <OrderId   IsKey="true"    DisplayName="受注番号" />
  <OrderDate IsNotNull="true" DisplayName="受注日" />
</Order>
```

:::note 適用範囲
`UseSoftDelete` はルート集約にのみ指定できます。
Child / Children には指定できません（ルートが論理削除されると子孫も一緒に移送されます）。
:::

## 生成されるもの

`UseSoftDelete="true"` を付与すると、`Delete{集約名}Async` が `SoftDelete{集約名}Async` に**置き換わります**。
物理削除メソッドは生成されません。

| 生成されるもの | 説明 |
| --- | --- |
| `{集約名}DeletedDbEntity` クラス | 削除済みデータ用の EF Core エンティティ |
| `{集約名}DeletedDbSet` | 削除済みテーブルの DbSet |
| `SoftDelete{集約名}Async` メソッド | 論理削除を実行するメソッド |
| `OnBeforeSoftDelete{集約名}` フック | 論理削除前の処理（オーバーライド可） |
| `OnAfterSoftDelete{集約名}Async` フック | 論理削除後の処理（オーバーライド可） |

## 削除済みテーブルの構造

元テーブルの全カラムに加えて、以下の列が**追加**されます。

| 追加カラム | 型 | 説明 |
| --- | --- | --- |
| `DeletedUuid` | `Guid` | 同一キーのデータを複数回削除できるようにするための新しい主キー |
| `DeletedAt` | `DateTime?` | 論理削除した日時 |
| `DeletedUser` | `string?` | 論理削除を行ったユーザー |

元テーブルのキー（例: `OrderId`）は主キーではなくなり、NULL 許容カラムとして保持されます。
`DeletedUuid` が新しい主キーになります。これにより、同じ `OrderId` のデータを削除→復元→再削除しても重複なく記録できます。

## SoftDelete メソッドの呼び出し

メソッドシグネチャと挙動は物理削除の `Delete{集約名}Async` とほぼ同じです。

```csharp
var key = new OrderUpdateOrDeleteKey {
    OrderId = "ORD-001",
    Version = originalVersion,
};

var result = await SoftDeleteOrderAsync(key, context);
if (!result.IsSuccess) {
    await transaction.RollbackAsync();
    return;
}
```

## 処理の流れ

1. キーが空でないかチェック
2. 元テーブルから対象データを取得（なければエラー）
3. `OnBeforeSoftDelete{集約名}` フックを呼び出し
4. エラーがある、または `ValidationOnly` の場合は中断
5. SAVEPOINT を作成
6. 削除済みエンティティ（`{集約名}DeletedDbEntity`）を生成して `INSERT`
7. 元テーブルから行を `DELETE`（楽観排他チェックあり）
8. `OnAfterSoftDelete{集約名}Async` フックを呼び出し
9. SAVEPOINT を解放

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

| 物理削除 | 論理削除 |
| --- | --- |
| 削除後にデータが参照されない | 削除後も参照・監査が必要 |
| ストレージ効率を優先 | 削除の証跡・履歴管理が必要 |
| 参照整合性を DB に委ねる | 参照整合性を失っても削除できる必要がある |

:::note 外部キー制約について
論理削除では元テーブルからレコードが消えるため、他テーブルからの外部キー制約が効きません。
参照元テーブルにデータが残ったまま論理削除しようとすると、DB レベルの外部キー制約違反で失敗します。
参照元を先に削除するか、物理削除・論理削除を組み合わせる設計を検討してください。
:::

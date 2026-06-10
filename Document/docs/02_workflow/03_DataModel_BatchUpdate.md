---
draft: true
---

# 一括更新

`GenerateBatchUpdateCommand="true"` を指定すると、一括更新用の Web API エンドポイントと Application Service メソッドが自動生成されます。

## 概要

一括更新は、**QueryModel の DisplayData の配列を受け取り、追加・更新・削除を一括で処理する**機能です。

画面の一覧表から複数件を編集して「一括保存」するような用途に使います。

## 前提条件

一括更新を使用するには、以下の2つの属性を **同時に** 指定する必要があります。

| XML 属性 | 説明 |
| --- | --- |
| `GenerateDefaultQueryModel="true"` | DataModel と全く同じ構造の QueryModel を生成する |
| `GenerateBatchUpdateCommand="true"` | 一括更新 API を生成する |

```xml
<MasterItem
  Type="data-model"
  GenerateDefaultQueryModel="true"
  GenerateBatchUpdateCommand="true"
  DisplayName="マスタ品目">
  <ItemCode IsKey="true" DisplayName="品目コード" />
  <Name     IsNotNull="true" DisplayName="名称" />
  <Unit     DisplayName="単位" />
</MasterItem>
```

:::note なぜ GenerateDefaultQueryModel が必要か
一括更新の引数型は QueryModel の `DisplayData` クラスです。
`GenerateDefaultQueryModel` を指定することで DataModel と構造が一致する DisplayData が生成され、
そのまま一括更新の引数として使用できます。
:::

## 生成されるもの

| 生成されるもの | 詳細 |
| --- | --- |
| `BatchUpdateAsync` メソッド | Application Service にメソッドとして生成 |
| Web API エンドポイント（`POST /batch-update`） | Controller に生成 |
| TypeScript 型マッピング（`BatchUpdateFeature`） | フロントエンド向けのエンドポイント情報 |

## BatchUpdateAsync メソッド

```csharp
public virtual async Task<bool> BatchUpdateAsync(
    MasterItemDisplayData[] displayDataItems,
    IPresentationContext<MessageContainerList<MasterItemDisplayDataMessages>> context)
```

### 処理の流れ

引数の `displayDataItems` 配列を先頭から順に処理します。
各要素の `ExistsInDatabase`・`WillBeDeleted`・`WillBeChanged` フラグを見て、追加・更新・削除のいずれかを実行します。

```
displayDataItems[0]
  ├─ ExistsInDatabase = false → Create{集約名}Async を呼び出し（新規登録）
  ├─ ExistsInDatabase = true, WillBeDeleted = true → Delete{集約名}Async を呼び出し（削除）
  ├─ ExistsInDatabase = true, WillBeChanged = true → Update{集約名}Async を呼び出し（更新）
  └─ それ以外 → スキップ（変更なし）

displayDataItems[1]
  └─ ...
```

- 1件でもエラーがあった場合、全件がロールバックされます。
- ただし、エラーが発生しても残りの要素のエラーチェックは続行されます（全件エラーを一度に返すため）。

### トランザクション管理

通常の CRUD メソッドと異なり、一括更新メソッド自身がトランザクションを開始・コミット・ロールバックします。
呼び出し元でトランザクションを管理する必要はありません。

（ただし `ValidationOnly = true` の場合はトランザクションを開始しません）

## DisplayData のフラグ

一括更新で使用される DisplayData の各要素は、以下のフラグで操作種別を示します。

| フラグ | 意味 |
| --- | --- |
| `ExistsInDatabase = false` | DB に存在しない新規レコード → 新規登録（Create） |
| `ExistsInDatabase = true, WillBeDeleted = true` | DB に存在し削除対象 → 削除（Delete） |
| `ExistsInDatabase = true, WillBeChanged = true` | DB に存在し変更あり → 更新（Update） |
| `ExistsInDatabase = true, WillBeChanged = false` | DB に存在し変更なし → スキップ |

フロントエンドでは画面上で編集した結果に基づいてこれらのフラグをセットして API に渡します。

## TypeScript での使用方法

一括更新が有効な集約は、自動生成された TypeScript 側の `BatchUpdateFeature` ネームスペースでアクセスできます。

```typescript
// 自動生成された型情報
const endpoint = BatchUpdateFeature.Endpoint['MasterItem']
// → "/api/master-item/batch-update"

// 型安全な呼び出し例
const items: BatchUpdateFeature.ParamType['MasterItem'] = [ /* DisplayData[] */ ]
await fetch(endpoint, {
  method: 'POST',
  body: JSON.stringify(items),
})
```

## 採用判断の基準

一括更新が適しているケース：

- 画面の一覧表から複数件を編集して「一括保存」するシンプルなマスタメンテナンス画面
- DataModel と QueryModel の構造が完全一致する（= `GenerateDefaultQueryModel` が使える）シンプルなデータ
- CRUD の処理ロジックが標準のもので十分なケース

以下のケースでは一括更新を使わず、個別の CommandModel を実装することを検討してください：

- 複雑なビジネスロジックが必要な場合（一括更新の引数は標準 CRUD メソッドをそのまま呼ぶだけです）
- DataModel と画面の表示データ構造が大きく異なる場合
- 一括操作の途中でエラーが出た場合の挙動を細かく制御したい場合

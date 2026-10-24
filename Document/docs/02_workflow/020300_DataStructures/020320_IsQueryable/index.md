---
sidebar_label: IsQueryable（検索可能）
sidebar_position: 2
---

# IsQueryable（検索可能）

データ構造が画面や帳票から検索・照会されることを表す属性です。
フィルタリング・ソート・ページングの処理が自動生成され、アプリケーションは検索条件オブジェクトを渡すだけで検索結果を取得できます。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `DataStructures` セクションのルート集約に `IsQueryable="True"` を指定します。
[IsStored](../020310_IsStored/index.md) 属性と同時に指定した場合は、テーブルと全く同じ形の検索処理になります。

## 自動生成されるモジュール

IsQueryable 属性が指定されたデータ構造1個から、以下のモジュールが自動生成されます。

| モジュール                                              | 詳細ページ                                                            |
| ------------------------------------------------------- | --------------------------------------------------------------------- |
| **一覧検索処理（LoadAsync・CreateQuerySource の実装）** | [一覧検索処理](./020321_SearchProcessing.md)               |
| SearchCondition クラス・ソート                          | [検索条件（SearchCondition）](./020322_SearchCondition.md) |
| DisplayData クラス・ページネーション・OnAfterLoaded     | [検索結果（DisplayData）](./020323_DisplayData.md)         |
| DB View へのマッピング（MapToView 属性）                | [DB View マッピング](./020324_MapToView.md)                |
| ref-to による参照先絞り込み                             | [参照先絞り込み（ref-to）](./020325_RefTo.md)              |

## 設計指針

### 検索可能なデータ構造の粒度

IsQueryable 属性が指定されたデータ構造は **ユーザーが一覧検索する粒度** で定義します。ほぼ「画面項目定義」の粒度とイコールです。

受注一覧であれば「受注ヘッダ＋合計金額」、従業員一覧であれば「従業員情報」がそれぞれ1つのデータ構造になります。

### 実装パターン

| パターン                     | 使いどころ                                               |
| ---------------------------- | -------------------------------------------------------- |
| **IsStored 属性が指定されたデータ構造からの直接射影** | テーブルの構造をほぼそのまま表示する単純な一覧 |
| **DB View へのマッピング**   | 複雑な集計・JOIN・パフォーマンスチューニングが必要な場合 |

詳細は [DB View マッピング](./020324_MapToView.md) を参照してください。

### 参照と検索

IsQueryable 属性が指定されたデータ構造に `ref-to` を定義すると、参照先の属性で絞り込める検索条件が自動生成されます。
詳細は [参照先絞り込み（ref-to）](./020325_RefTo.md) を参照してください。

## 基本的な XML 定義例

```xml
<OrderQuery IsQueryable="True" DisplayName="受注一覧">
  <OrderId     IsKey="true"  DisplayName="受注番号" />
  <OrderDate   IsNotNull="true" DisplayName="受注日" />
  <CustomerRef Type="ref-to:Customer" DisplayName="得意先" />

  <!-- 1対多の子集約（children に IsKey は指定できない） -->
  <OrderDetails Type="children" DisplayName="受注明細">
    <LineNo    DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
  </OrderDetails>
</OrderQuery>
```

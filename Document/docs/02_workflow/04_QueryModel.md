---
sidebar_position: 4
---

# QueryModel（クエリモデル）

一覧検索処理に特化したデータモデルです。
フィルタリング・ソート・ページングの処理が自動生成され、アプリケーションは検索条件オブジェクトを渡すだけで検索結果を取得できます。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Type="query-model"`

## 自動生成されるモジュール

QueryModel の定義1個から、以下のモジュールが自動生成されます。

| モジュール | 詳細ページ |
| --- | --- |
| SearchCondition クラス・URL変換・ソート | [検索条件（SearchCondition）](./QueryModel_SearchCondition) |
| DisplayData クラス・ページネーション・OnAfterLoaded | [検索結果（DisplayData）](./QueryModel_DisplayData) |
| DB View へのマッピング（オプション） | [DB View マッピング](./QueryModel_DbView) |
| ref-to による参照先絞り込み | [参照先絞り込み（ref-to）](./QueryModel_RefTo) |

## 設計指針

### クエリモデルの粒度

クエリモデルは **ユーザーが一覧検索する粒度** で定義します。ほぼ「画面項目定義」の粒度とイコールです。

受注一覧であれば「受注ヘッダ＋合計金額」、従業員一覧であれば「従業員情報」がそれぞれ1つの QueryModel になります。

### 実装パターン

| パターン | 使いどころ |
| --- | --- |
| **DataModel からの直接射影** | DataModel の構造をほぼそのまま表示する単純な一覧 |
| **DB View へのマッピング** | 複雑な集計・JOIN・パフォーマンスチューニングが必要な場合 |

詳細は [DB View マッピング](./QueryModel_DbView) を参照してください。

### 参照と検索

QueryModel に `ref-to` を定義すると、参照先の属性で絞り込める検索条件が自動生成されます。
詳細は [参照先絞り込み（ref-to）](./QueryModel_RefTo) を参照してください。

## 基本的な XML 定義例

```xml
<OrderQuery Type="query-model" DisplayName="受注一覧">
  <OrderId     IsKey="true"  DisplayName="受注番号" />
  <OrderDate   IsNotNull="true" DisplayName="受注日" />
  <CustomerRef Type="ref-to:Customer" DisplayName="得意先" />

  <!-- 1対多の子集約 -->
  <OrderDetails Type="children" DisplayName="受注明細">
    <LineNo    IsKey="true"    DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
  </OrderDetails>
</OrderQuery>
```

---
sidebar_position: 3
---

# DataModel（データモデル）

アプリケーションに永続化されるデータの形を表します。
RDBMSのテーブル定義を、**トランザクションの境界ごとにまとめたもの**です。
DDDにおける「集約ルート」の概念とほぼ同じです。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `Type="data-model"`

## 自動生成されるモジュール

DataModel の定義1個から、以下のモジュールが自動生成されます。

| モジュール                                       | 詳細ページ                                                         |
| ------------------------------------------------ | ------------------------------------------------------------------ |
| EF Core エンティティクラス・DbContext 設定       | [EF Core エンティティ定義](./020301_DataModel_EFCoreEntity.md)     |
| CRUD メソッド（新規登録・更新・物理削除）        | [CRUD メソッド](./020302_DataModel_CRUDMethods.md)                 |
| 入力バリデーション（必須・最大長・文字種・桁数） | [バリデーション属性リファレンス](./020303_DataModel_Validators.md) |
| 論理削除メソッド（オプション）                   | [論理削除](./020304_DataModel_SoftDelete.md)                       |
| 一括更新 API（オプション）                       | [一括更新](./020305_DataModel_BatchUpdate.md)                      |
| 汎用参照テーブル（オプション）                   | [汎用参照テーブル](./020306_DataModel_GenericLookupTable.md)       |

## 設計指針

### データのまとまりとライフサイクル

DataModel は **楽観排他制御がかかるべき単位** で1つ定義します。

例えば「受注」と「受注明細」は通常セットで扱われ、明細だけが単独で存在することはありません。
この場合、「受注」をルートとする1つの DataModel として定義し、「受注明細」はその子要素（Children）として定義します。

### 集約の種類

| 種類                        | 説明                             | キー                             |
| --------------------------- | -------------------------------- | -------------------------------- |
| **Root**（ルート集約）      | 集約の起点。1つのテーブルに対応  | 必須（1個以上）                  |
| **Child**（1対1子集約）     | 親に対して0または1件の子テーブル | 指定不可（親キーを自動継承）     |
| **Children**（1対多子集約） | 親に対して0件以上の子テーブル    | 必須（親キーに加えて自身のキー） |

### キーの設計

主キー（`IsKey`）には **サロゲートキー**（自動採番IDなど）と **ナチュラルキー**（社員コード等）のどちらも使用可能です。

### 参照関係

他の DataModel への参照（`ref-to`）は RDB の **外部キー制約** として実装されます。
参照項目に `IsKey` を付与することで「識別関係（Identifying Relationship）」を表現できます。

## 基本的な XML 定義例

```xml
<!-- 受注データモデル -->
<Order Type="data-model" DisplayName="受注">
  <OrderId     IsKey="true"  DisplayName="受注番号" />
  <OrderDate   IsNotNull="true" DisplayName="受注日" />
  <CustomerRef Type="ref-to:Customer" IsNotNull="true" DisplayName="得意先" />

  <!-- 1対多の子集約（受注明細） -->
  <OrderDetails Type="children" DisplayName="受注明細">
    <LineNo    IsKey="true"    DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
    <Quantity  IsNotNull="true" DisplayName="数量" />
  </OrderDetails>
</Order>
```

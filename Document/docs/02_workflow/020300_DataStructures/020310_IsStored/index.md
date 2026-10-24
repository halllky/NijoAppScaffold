---
sidebar_label: IsStored（永続化）
sidebar_position: 1
---

# IsStored（永続化）

データ構造がデータベースに永続化されることを表す属性です。
IsStored 属性が指定されたデータ構造は、RDBMSのテーブル定義を **トランザクションの境界ごとにまとめたもの** になります。

スキーマ定義ファイル (`nijo.xml`) での指定方法: `DataStructures` セクションのルート集約に `IsStored="True"` を指定します。
`IsQueryable="True"` も同時に指定すると、テーブルと全く同じ形の[検索処理](../020320_IsQueryable/index.md)も生成されます。

## 自動生成されるモジュール

IsStored 属性が指定されたデータ構造1個から、以下のモジュールが自動生成されます。

| モジュール                                       | 詳細ページ                                                         |
| ------------------------------------------------ | ------------------------------------------------------------------ |
| EF Core エンティティクラス・DbContext 設定       | [EF Core エンティティ定義](./020311_EFCoreEntity.md)     |
| CRUD メソッド（新規登録・更新・物理削除）        | [CRUD メソッド](./020312_CRUDMethods.md)                 |
| 入力バリデーション（必須・最大長・文字種・桁数） | [バリデーション属性リファレンス](./020313_Validators.md) |
| 論理削除メソッド（UseSoftDelete 属性）           | [論理削除](./020314_UseSoftDelete.md)                    |
| 汎用参照テーブル（IsGenericLookupTable 属性）    | [汎用参照テーブル](../../020700_Constants/020703_GenericLookupTable.md) |

## 設計指針

### データのまとまりとライフサイクル

IsStored 属性が指定されたデータ構造は **楽観排他制御がかかるべき単位** で1つ定義します。

例えば「受注」と「受注明細」は通常セットで扱われ、明細だけが単独で存在することはありません。
この場合、「受注」をルートとする1つのデータ構造として定義し、「受注明細」はその子要素（Children）として定義します。

### 集約の種類

| 種類                        | 説明                             | キー                             |
| --------------------------- | -------------------------------- | -------------------------------- |
| **Root**（ルート集約）      | 集約の起点。1つのテーブルに対応  | 必須（1個以上）                  |
| **Child**（1対1子集約）     | 親に対して0または1件の子テーブル | 指定不可（親キーを自動継承）     |
| **Children**（1対多子集約） | 親に対して0件以上の子テーブル    | 必須（親キーに加えて自身のキー） |

### キーの設計

主キー（`IsKey`）には **サロゲートキー**（自動採番IDなど）と **ナチュラルキー**（社員コード等）のどちらも使用可能です。

### 参照関係

IsStored 属性が指定された他のデータ構造への参照（`ref-to`）は RDB の **外部キー制約** として実装されます。
参照項目に `IsKey` を付与することで「識別関係（Identifying Relationship）」を表現できます。

## 基本的な XML 定義例

```xml
<!-- 受注 -->
<Order IsStored="True" DisplayName="受注">
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

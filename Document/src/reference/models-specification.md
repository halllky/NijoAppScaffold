# モデル技術仕様

*情報指向 - 技術的詳細仕様*

Nijoの3つのモデル（DataModel、QueryModel、CommandModel）の技術仕様と制約事項を説明します。

[[toc]]

## モデル共通仕様

### 基本構文
すべてのモデルはXMLスキーマで定義され、以下の共通属性を持ちます：

```xml
<集約名 node-type="モデルタイプ" physical-name="物理名">
  <!-- メンバー定義 -->
</集約名>
```

### 共通属性

| 属性名          | 必須 | 説明               | 例                                           |
| --------------- | ---- | ------------------ | -------------------------------------------- |
| `node-type`     | ✅    | モデルタイプを指定 | `data-model`, `query-model`, `command-model` |
| `physical-name` | ✅    | 生成されるクラス名 | `Customer`, `CustomerSearch`                 |
| `display-name`  | ❌    | 画面表示用の名称   | `顧客`, `顧客検索`                           |

## DataModel 技術仕様

### スキーマ定義
```xml
<顧客 node-type="data-model" physical-name="Customer">
  <顧客ID key="true" type="uuid" />
  <顧客名 type="string" required="true" />
  <登録日時 type="datetime" />
</顧客>
```

### 自動生成される成果物

| カテゴリ             | 生成されるファイル/内容 | 説明                       |
| -------------------- | ----------------------- | -------------------------- |
| **データベース**     | テーブル作成SQL         | PostgreSQL/SQLite対応      |
|                      | インデックス定義        | 主キー、外部キーの自動作成 |
| **Entity Framework** | Entityクラス            | C#のエンティティクラス     |
|                      | DbContext設定           | テーブルマッピング設定     |
| **API**              | 登録API                 | `POST /api/{集約名}`       |
|                      | 更新API                 | `PUT /api/{集約名}`        |
|                      | 削除API                 | `DELETE /api/{集約名}`     |
| **TypeScript**       | 型定義                  | フロントエンド用型定義     |

### 利用可能な属性

| 属性名       | 型      | 説明       | 例                                  |
| ------------ | ------- | ---------- | ----------------------------------- |
| `key`        | boolean | 主キー指定 | `key="true"`                        |
| `type`       | string  | データ型   | `string`, `int`, `datetime`, `uuid` |
| `required`   | boolean | 必須項目   | `required="true"`                   |
| `max-length` | int     | 最大文字数 | `max-length="100"`                  |
| `ref-to`     | string  | 外部参照   | `ref-to="data-model:Customer"`      |

### 制約事項

- ✅ **主キーは必須**: 各DataModelには必ず主キーが必要
- ❌ **子集約に主キー不可**: 子集約には主キー属性を指定できない
- ⚠️ **トランザクション境界**: 1つのDataModelが整合性保証の範囲
- 🔄 **楽観排他制御**: 自動的にバージョン管理フィールドが追加

## QueryModel 技術仕様

### スキーマ定義
```xml
<顧客検索 node-type="query-model" physical-name="CustomerSearch">
  <顧客ID type="uuid" />
  <顧客名 type="string" />
  <登録日 type="date" />
</顧客検索>
```

### 自動生成される成果物

| カテゴリ     | 生成されるファイル/内容   | 説明                       |
| ------------ | ------------------------- | -------------------------- |
| **検索API**  | `GET /api/{集約名}`       | 検索エンドポイント         |
|              | `GET /api/{集約名}/count` | 件数取得エンドポイント     |
| **検索条件** | SearchConditionクラス     | C#の検索条件クラス         |
|              | TypeScript型定義          | フロントエンド用検索条件型 |
| **自動機能** | ページング                | `skip`, `take`パラメータ   |
|              | ソート                    | `orderby`パラメータ        |
|              | フィルタリング            | 各フィールドでの絞り込み   |

### 検索条件の構文

```typescript
// 自動生成されるTypeScript型
type CustomerSearchCondition = {
  顧客名?: {
    contains?: string;      // 部分一致
    startsWith?: string;    // 前方一致
    equals?: string;        // 完全一致
  };
  登録日?: {
    from?: Date;           // 以降
    to?: Date;             // 以前
    equals?: Date;         // 完全一致
  };
  // ページング
  skip?: number;
  take?: number;
  // ソート
  orderby?: string;
}
```

### 制約事項

- ❌ **子集約に主キー不可**: 子集約には主キー属性を指定できない
- ❌ **子配列に主キー不可**: 子配列には主キー属性を指定できない
- 📚 **参照専用**: データの更新は行わない（CQRSのQuery側）
- 🔍 **検索最適化**: 表示用の構造に最適化

## CommandModel 技術仕様

### スキーマ定義
```xml
<顧客登録 node-type="command-model" physical-name="CustomerRegistration">
  <顧客登録Parameter node-type="child">
    <顧客名 type="string" required="true" />
    <メールアドレス type="string" />
  </顧客登録Parameter>
  <顧客登録ReturnValue node-type="child">
    <顧客ID type="uuid" />
    <処理結果 type="string" />
  </顧客登録ReturnValue>
</顧客登録>
```

### 自動生成される成果物

| カテゴリ       | 生成されるファイル/内容 | 説明                         |
| -------------- | ----------------------- | ---------------------------- |
| **API**        | `POST /api/{集約名}`    | コマンド実行エンドポイント   |
| **パラメータ** | Parameterクラス         | C#のパラメータクラス         |
|                | TypeScript型定義        | フロントエンド用パラメータ型 |
| **戻り値**     | ReturnValueクラス       | C#の戻り値クラス             |
|                | TypeScript型定義        | フロントエンド用戻り値型     |
| **処理基盤**   | Executeメソッド骨組み   | 実装は開発者が行う           |

### 必須の子集約

CommandModelには以下の子集約が必須です：

| 子集約名    | 物理名パターン            | 説明           |
| ----------- | ------------------------- | -------------- |
| Parameter   | `{コマンド名}Parameter`   | 入力パラメータ |
| ReturnValue | `{コマンド名}ReturnValue` | 戻り値         |

### 外部参照制約

CommandModelから他のモデルを参照する場合の制約：

| 参照先           | 制約       | 必須属性        |
| ---------------- | ---------- | --------------- |
| QueryModel       | ✅ 参照可能 | `ref-to-object` |
| DataModel (GDQM) | ✅ 参照可能 | `ref-to-object` |
| 他のDataModel    | ❌ 参照不可 | -               |

#### ref-to-object 属性値

| 値                | 説明                   | 用途               |
| ----------------- | ---------------------- | ------------------ |
| `DisplayData`     | 画面表示用オブジェクト | UI表示データの取得 |
| `SearchCondition` | 検索条件用オブジェクト | 検索処理の実行     |

### 制約事項

- ❌ **主キー属性不可**: CommandModelの集約には主キー属性を定義できない
- 🔧 **処理ロジックは手動実装**: Executeメソッド内の実装はすべて開発者の責任
- 🎯 **単一責任**: 1つのCommandModelは1つの操作のみを表現

## データ型仕様

### 基本データ型

| 型名       | C#         | TypeScript | データベース | 説明       |
| ---------- | ---------- | ---------- | ------------ | ---------- |
| `string`   | `string`   | `string`   | `TEXT`       | 文字列     |
| `int`      | `int`      | `number`   | `INTEGER`    | 32bit整数  |
| `long`     | `long`     | `number`   | `BIGINT`     | 64bit整数  |
| `decimal`  | `decimal`  | `number`   | `DECIMAL`    | 十進数     |
| `datetime` | `DateTime` | `Date`     | `TIMESTAMP`  | 日時       |
| `date`     | `DateOnly` | `string`   | `DATE`       | 日付のみ   |
| `bool`     | `bool`     | `boolean`  | `BOOLEAN`    | 真偽値     |
| `uuid`     | `Guid`     | `string`   | `UUID`       | 一意識別子 |

### 制約指定

| 制約         | 適用型   | 説明       | 例                 |
| ------------ | -------- | ---------- | ------------------ |
| `max-length` | `string` | 最大文字数 | `max-length="100"` |
| `min-value`  | 数値型   | 最小値     | `min-value="0"`    |
| `max-value`  | 数値型   | 最大値     | `max-value="999"`  |
| `required`   | すべて   | 必須項目   | `required="true"`  |

## パフォーマンス考慮事項

### 自動最適化される項目

- **主キーインデックス**: DataModelの主キーに自動作成
- **外部キーインデックス**: 参照関係に自動作成
- **ページング処理**: QueryModelで効率的なLIMIT/OFFSET

### 手動最適化が必要な項目

- **複合インデックス**: 複数列にまたがる検索用インデックス
- **複雑なJOIN**: 複数テーブルをまたがる複雑な検索
- **集計処理**: SUM、COUNTなどの集計クエリ

---

技術的な詳細や実装例については、[🛠️ How-to Guides](../how-to-guides/)セクションを参照してください。

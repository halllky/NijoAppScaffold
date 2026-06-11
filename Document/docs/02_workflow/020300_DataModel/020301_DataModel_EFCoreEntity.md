---

---

# EF Core エンティティ定義

DataModel の定義から、Entity Framework Core（EF Core）を通したデータベース定義が自動生成されます。
このページでは、生成される C# クラスの構造と、DB スキーマへの対応を説明します。

## 生成されるもの

1. **エンティティクラス（POCO）** — `{集約名}DbEntity` という名前のクラス
2. **DbContext 設定** — `partial class DbContext` の `OnModelCreating{集約名}` メソッド
3. **`KeyEquals` メソッド** — 2つのエンティティの主キーが一致するかを比較するメソッド

## エンティティクラスの構造

### 自動付与されるメタデータカラム

すべての Root 集約のエンティティに以下のカラムが**自動的に**追加されます。開発者が XML に定義する必要はありません。

| C# プロパティ名 | 型          | 説明                                        |
| --------------- | ----------- | ------------------------------------------- |
| `CreatedAt`     | `DateTime?` | データが新規作成された日時                  |
| `UpdatedAt`     | `DateTime?` | データが最後に更新された日時                |
| `CreateUser`    | `string?`   | データを新規作成したユーザー                |
| `UpdateUser`    | `string?`   | データを最後に更新したユーザー              |
| `Version`       | `int?`      | 楽観排他制御用のバージョン番号（Root のみ） |

`Version` は Root 集約にのみ付与されます。Child・Children のエンティティには付与されません。
また、QueryModel のビュー（`MapToView="true"`）にはバージョン列は生成されません。

### カラムのマッピングルール

XML で定義した各項目は以下のルールでテーブルカラムにマッピングされます。

| XML の定義                 | テーブル上の列               | 説明                                     |
| -------------------------- | ---------------------------- | ---------------------------------------- |
| `ValueMember`              | 自集約に属するカラム         | そのまま1列になる                        |
| `ref-to:SomeModel`         | 参照先のキー列（外部キー）   | 参照先のキー列の数だけ列が生成される     |
| `Type="children"` の親キー | 親テーブルのキーを継承した列 | 子テーブルに親のキーが自動的に追加される |

### 外部キー参照がある場合のカラム名

`ref-to` で他の集約を参照している場合、参照先のキーをそのまま外部キー列として持ちます。
カラム名は `{参照項目の物理名}_{参照先キーの物理名}` の形式になります。

**XML 定義例:**

```xml
<Product Type="data-model" DisplayName="商品">
  <ProductId   IsKey="true"    DisplayName="商品コード" />
  <Name        IsNotNull="true" DisplayName="商品名" />
  <CategoryRef Type="ref-to:Category" IsNotNull="true" DisplayName="カテゴリ" />
</Product>
```

**生成されるエンティティ（概略）:**

```csharp
public partial class ProductDbEntity {
    // 自身のカラム
    public string? ProductId { get; set; }
    public string? Name { get; set; }

    // CategoryRefの外部キー（参照先Categoryのキー列が展開される）
    public string? CategoryRef_CategoryId { get; set; }

    // ナビゲーションプロパティ
    public virtual CategoryDbEntity? CategoryRef { get; set; }

    // メタデータ（自動付与）
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreateUser { get; set; }
    public string? UpdateUser { get; set; }
    public int? Version { get; set; }

    public bool KeyEquals(ProductDbEntity entity) {
        if (entity.ProductId != this.ProductId) return false;
        return true;
    }
}
```

## Root / Child / Children のスキーマ上の違い

### Root

独立したテーブル。`Version`（楽観排他）と時刻・ユーザーのメタデータ列を持つ。

### Child（1対1子集約）

親テーブルのキーを継承した複合主キーを持つ別テーブルとして生成されます。
子が存在しない場合（`null` の場合）はレコード自体が存在しません。

```xml
<Order Type="data-model" DisplayName="受注">
  <OrderId IsKey="true" DisplayName="受注番号" />

  <!-- Child: 受注ヘッダと1対1で対応する配送情報 -->
  <Delivery Type="child" DisplayName="配送情報">
    <Address IsNotNull="true" DisplayName="配送先住所" />
  </Delivery>
</Order>
```

生成される配送情報テーブルの主キーは `Parent_OrderId`（親の受注番号を継承）になります。

### Children（1対多子集約）

親テーブルのキーに加えて、自身のキーを持つ別テーブルとして生成されます。
親テーブルのキーを含む複合主キーになります。

```xml
<Order Type="data-model" DisplayName="受注">
  <OrderId IsKey="true" DisplayName="受注番号" />

  <OrderDetails Type="children" DisplayName="受注明細">
    <LineNo    IsKey="true"    DisplayName="行番号" />
    <ProductId IsNotNull="true" DisplayName="商品コード" />
  </OrderDetails>
</Order>
```

生成される受注明細テーブルの主キーは `Parent_OrderId`（継承）と `LineNo`（自身）の複合キーになります。

## DbContext 設定（OnModelCreating）

各集約に対して、`partial class DbContext` の中に `OnModelCreating{集約名}` という名前のメソッドが自動生成されます。
このメソッドの中で、EF Core の Fluent API によるテーブル定義・リレーション設定・制約設定が行われます。

生成されるコードの一部例：
```csharp
// 自動生成されたコード（編集不要）
private void OnModelCreatingOrder(ModelBuilder modelBuilder) {
    modelBuilder.Entity<OrderDbEntity>(entity => {
        entity.HasKey(e => new { e.OrderId });
        entity.Property(e => e.Version).IsConcurrencyToken();
        entity.HasOne(e => e.OrderDetails)
              .WithMany()
              .HasForeignKey(e => e.Parent_OrderId);
        // ... その他の制約
    });
}
```

:::note 自動生成コードのカスタマイズ
上記メソッドは `partial class` の仕組みを使っています。
そのため、別ファイルで同名の `partial class DbContext` を定義して独自の EF Core 設定を追加できます。
ただし、自動生成ファイルへの手動変更は次回の生成で上書きされます。
:::

## ナビゲーションプロパティ

EF Core のナビゲーションプロパティが自動生成されます。
クエリ時は `Include` / `ThenInclude` で読み込みます（自動生成の CRUD メソッド内では自動的に Include されます）。

1対多（Children）が2件以上存在する場合、デカルト積によるパフォーマンス劣化を防ぐため、`.AsSplitQuery()` が自動的に挿入されます。

## テーブル名・カラム名のカスタマイズ

デフォルトでは XML の物理名がそのままテーブル名・カラム名になります。
`DbName` 属性を指定することで変更できます。

```xml
<Order Type="data-model" DbName="ORDER_MASTER" DisplayName="受注">
  <OrderId IsKey="true" DbName="ORDER_NO" DisplayName="受注番号" />
</Order>
```

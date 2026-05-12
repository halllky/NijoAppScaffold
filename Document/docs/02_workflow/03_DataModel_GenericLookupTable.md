---
# 執筆中
draft: true

---

# 汎用参照テーブル (Generic Lookup Table)

俗に汎用マスタ、区分マスタ、コードマスタなどと呼ばれるテーブル。
一般的には、国コード、通貨単位、分析集計表の分類など、コードと名称のペアのみが格納されることが多い。

通常の列挙体はこれではなく [Static Enum](./08_StaticEnumModel.md) で定義すること。
どちらを使うかの主な判断基準は、区分値がシステムの運用中に追加削除されうるかどうか。

* 運用中に増えない（リリース時点で区分値が確定している）場合、区分値によるプログラムの条件分岐や制御が可能になるが、値を増やすにはプログラム改修が必要になる。
* 運用中に増える区分値は、その値をプログラム中での制御に使用できないが、簡単に値を増やすことができる。

:::note 汎用参照テーブルの採用判断

基本的に、汎用参照テーブルはアンチパターンとされることが多い。

* トランザクションテーブルから汎用参照テーブルへの外部キー制約が効かないため、データの整合性が崩れる危険性がある。
* ER図を見ても、どのテーブルがどのデータに依存しているのかが視覚的に分からなくなる。
* すべてのクエリが一つのテーブルに集中するため、インデックスの効率が悪くなったり、ロック競合が発生しやすくなったりしやすい。

そのため、利用は慎重に検討すること。
基本的にこのテーブルにもたせるのはコード値と名称程度に留めるのを推奨。（許せて画面表示用の並び順程度か）
特に、 **オプション項目を増やしてオプションの値でプログラム上の制御をかけたくなった場合** は、
その区分だけ独立したテーブルに切り離すことを強く推奨する。

:::

## テーブル構造、スキーマ定義

典型的なデータは以下。ポイントは、

* 必ず複合キーであること。
* 主キーのうち一部はソースコード上にハードコードされる。以下の例だと、業務分類とコード種別。
* 主キーのうち一部はソースコード上にハードコードされない。この部分の値はそのデータの登録時に決まる。以下の例だと、コード値。
* あくまで DataModel の亜種であるため、 DataModel が使える機能はそのまま使える。
* ハードコードされる区分値は nijo.xml 上に出現する。

**区分マスタ(KBN_MST) ... データベースの汎用参照テーブル**

| 業務分類(PK) | コード種別(PK) | コード値(PK) | 表示用名称   | 表示順 |
| :----------- | :------------- | :----------- | :----------- | :----- |
| MG           | 001            | JP           | 日本         | 1      |
| MG           | 001            | US           | アメリカ     | 2      |
| MG           | 001            | CN           | 中国         | 3      |
| MG           | 002            | A            | 重要戦略分野 | 1      |
| MG           | 002            | B            | 成長見込み   | 2      |
| GB           | 001            | 01           | 社内業務     | 1      |
| GB           | 001            | 02           | 顧客対応     | 2      |
| GB           | 001            | 03           | 組織改正対応 | 3      |
| GB           | 002            | 01           | 優先度低     | 1      |
| GB           | 002            | 02           | 急ぎ         | 2      |
| GB           | 002            | 03           | 特急         | 3      |
| GB           | 002            | 04           | 最優先       | 4      |
| GB           | 002            | 05           | 超特急       | 5      |

---

上記と対応する nijo.xml のスキーマ定義はこのようになる。

```xml
<NijoAppScaffold>
  <DataStructures>
    <!-- 汎用参照テーブル -->
    <MyLookupTable UniqueId="xxxx-xxxx-xxxx-xxxx"
                   IsGenericLookupTable="True"
                   DisplayName="区分マスタ"
                   DbName="KBN_MST">
      <!-- ハードコードされる主キーその1（MG: マネジメント層業務用の区分、GB: 現場部門業務用の区分） -->
      <BusinessSection UniqueId="ssss-ssss-ssss-ssss"
                       DisplayName="業務分類"
                       Type="Word"
                       IsKey="True"
                       IsHardCodedPrimaryKey="True" />
      <!-- ハードコードされる主キーその2 -->
      <CodeType UniqueId="yyyy-yyyy-yyyy-yyyy"
                DisplayName="コード種別"
                Type="Word"
                IsKey="True"
                IsHardCodedPrimaryKey="True" />
      <!-- ハードコードされない主キー -->
      <CodeValue UniqueId="zzzz-zzzz-zzzz-zzzz"
                 DisplayName="コード値"
                 Type="Word"
                 IsKey="True" />
      <!-- 非主キー項目 -->
      <CodeName UniqueId="wwww-wwww-wwww-wwww"
                DisplayName="表示用名称" />
      <DisplayOrder UniqueId="vvvv-vvvv-vvvv-vvvv"
                    DisplayName="表示順" />
    </MyLookupTable>

    <!-- 上記汎用参照テーブルを参照するトランザクションデータの例 -->
    <Customers UniqueId="aaaa-aaaa-aaaa-aaaa"
               DisplayName="顧客マスタ"
               DbName="CUSTOMERS">
      <CustomerID UniqueId="1111-1111-1111-1111"
                  Type="Int"
                  IsKey="True" />
      <CustomerName UniqueId="2222-2222-2222-2222"
                    DisplayName="顧客名"
                    Type="Word" />
      <!-- 汎用参照テーブルのハードコードされない主キー部分のみを外部キーとして持つ -->
      <Country UniqueId="3333-3333-3333-3333"
               DisplayName="国地域"
               Type="ref-to:MyLookupTable"
               Category="Countries" />
    </Customers>
  </DataStructures>

  <GenericLookupTableCategories>
    <!-- 上記汎用参照テーブルのハードコードされる主キー部分の定義 -->
    <Categories For="xxxx-xxxx-xxxx-xxxx">
      <Countries DisplayName="国・地域区分">
        <Key For="ssss-ssss-ssss-ssss" Value="MG" />
        <Key For="yyyy-yyyy-yyyy-yyyy" Value="001" />
      </Countries>
      <Strategies DisplayName="経営戦略分類">
        <Key For="ssss-ssss-ssss-ssss" Value="MG" />
        <Key For="yyyy-yyyy-yyyy-yyyy" Value="002" />
      </Strategies>
      <Reporting DisplayName="日報実働時間計上区分">
        <Key For="ssss-ssss-ssss-ssss" Value="GB" />
        <Key For="yyyy-yyyy-yyyy-yyyy" Value="001" />
      </Reporting>
      <Urgency DisplayName="緊急度区分">
        <Key For="ssss-ssss-ssss-ssss" Value="GB" />
        <Key For="yyyy-yyyy-yyyy-yyyy" Value="002" />
      </Urgency>
    </Categories>
  </GenericLookupTableCategories>
</NijoAppScaffold>
```

## プログラム上での取り扱い方

### 登録・更新・削除時（汎用参照テーブル）

通常の DataModel と同様、Entity Framework Core を通じて登録・更新・削除が可能。
更新時はハードコードされる種別も指定する必要があるが、前述の自動生成される補助用のプロパティから参照可能。

```cs
partial class OverridedApplicationService {
    public override async Task Execute何らかの画面更新Async(何らかの画面更新ParameterDisplayData param, IPresentationContext<何らかの画面更新ParameterMessages> context) {

        // 新規追加の例。
        // ハードコードされる種別である「業務分類」「コード種別」は
        // 補助用のプロパティの「Countries（国・地域区分）」から参照可能
        await Create区分マスタAsync(new() {
            BusinessSection = this.区分マスタUtil.Countries.BusinessSection, // 業務分類
            CodeType = this.区分マスタUtil.Countries.CodeType, // コード種別
            CodeValue = "新しいコード値", // コード値
            DisplayName = "新しい表示用名称", // 表示用名称
            DisplayOrder = 99, // 表示順
        }, context, message);
    }
}
```

### 登録・更新・削除時（他のトランザクションテーブル）

トランザクションテーブル側では、汎用参照テーブルのハードコードされない主キー部分のみを外部キーとして持てばよい。

なお、外部キー制約が効かないため、自動生成されるチェック処理中でDBにその区分が存在するかのチェックが入る。
SQL発行のタイミングと汎用参照テーブル側の更新のタイミングによってはここでチェックしても整合性を100%保証できないことには注意が必要。

```cs
partial class OverridedApplicationService {
    public override async Task Execute顧客マスタ更新Async(顧客マスタ更新ParameterDisplayData param, IPresentationContext<顧客マスタ更新ParameterMessages> context) {

        // 新規追加の例
        await Create顧客マスタAsync(new() {
            CustomerID = 123,
            CustomerName = "新しい顧客",

            // トランザクションテーブル側では、汎用参照テーブルの
            // ハードコードされない主キー部分のみを外部キーとして持てばよい。
            // この例では「業務分類」「コード種別」「コード値」のうち
            // 前者2つはハードコードなので「コード値」のみを持てばよい。
            Country = new() { CodeValue = "JP" }, // 国地域区分.コード値

        }, context, message);
    }
}

```

### 参照時（汎用参照テーブル単独）

例えば登録画面でその区分を選択するときのドロップダウンやラジオボタンなどでは、その区分の値の塊が必要になる。
この用途で使える補助用のプロパティが自動生成されるため、コード上では以下のように簡単に参照できる。

```cs
// ------- 自動生成されるクラス -------
class AutoGeneratedApplicationService {

    // 補助用のプロパティが自動生成される。
    // 汎用参照テーブル単位で生成され、内部で「国・地域区分」「経営戦略分類」
    // など毎に補助用の機能が提供される。
    protected 区分マスタUtil 区分マスタUtil { get; }
}

// ------- 自動生成されない手動実装クラス -------
class OverridedApplicationService : AutoGeneratedApplicationService {
    public override async Task Execute何らかの画面の初期表示Async(IPresentationContextWithReturnValue<画面のDisplayData, MessageSetter> context) {

        // 国・地域区分の値をすべて取得する例。
        // このページの最初の方で示した nijo.xml の定義の場合、
        // 業務分類が MG、コード種別が 001 の区分の値の塊が取得できる。
        // 画面側のデータ構造で区分マスタへの ref-to を行なっておけば、
        // データ構造も自動的に同期することができる。
        var dataSource = this.区分マスタUtil.Countries.ToList();
        context.ReturnValue.国地域区分ドロップダウンデータソース = dataSource;
    }
}
```

### 参照時（他のトランザクションテーブルとの結合）

ハードコードされる区分種類単位でビューが自動生成される。
各トランザクションテーブルは、ハードコードされない部分の値のみを外部キーとして持てばよい。
（厳密には Foreign Key 制約が効いていないので外部キーではないのだが）

例えば、以下のようなトランザクションテーブルがあったとする。

| 顧客ID(PK) | 顧客名 | 国地域_コード値 |
| :--------- | :----- | :-------------- |
| 1          | 顧客A  | JP              |
| 2          | 顧客B  | US              |

このとき、このトランザクションデータを表示する画面でのクエリでは、
ビューに対する JOIN を行い、その時点の最新のコード値や名称がクエリ結果に含まれる形になる。
通常の DataModel の定義と同様、このビュー単位の Entity Framework Core 設定も自動生成される。

```cs
partial class OverridedApplicationService {
    protected override IQueryable<顧客マスタSearchResult> CreateQuerySource(顧客マスタSearchCondition searchCondition, IPresentationContext<顧客マスタSearchConditionMessages> context) {

        return DbContext.顧客マスタDbSet.Select(e => new 顧客マスタSearchResult {
            CustomerID = e.CustomerID,
            CustomerName = e.CustomerName,

            // このページの最初の方で示した nijo.xml の定義の場合、
            // 国地域_コード値 はコード値の部分にあたるため、トランザクションテーブルはこの列を持つ。
            // このように単にナビゲーションプロパティをたどることでビューへのJOINが行われ、コード値や名称が取得できる。
            // 表示順など任意の列も同様に取得できる。
            // ここにはハードコードされる種別である「業務分類」「コード種別」が含まれない。
            Country = new() {
                CodeValue = e.Country_CodeValue,        // 国地域.コード値
                CodeName = e.Country!.CodeName,         // 国地域.表示用名称
                DisplayOrder = e.Country!.DisplayOrder, // 国地域.表示順
            },
        });
    }
}
```

---

自動生成される DbContext は以下のようになる。

```cs
class AutoGeneratedDbContext : DbContext {
    // テーブルと対応する DbSet 。登録更新の場合はこちらが使われる
    public virtual DbSet<区分マスタDbEntity> 区分マスタDbSet { get; set; }

    // ビューと対応する DbSet
    public virtual DbSet<区分マスタ_CountriesDbEntity> 区分マスタ_CountriesDbSet { get; set; }
    public virtual DbSet<区分マスタ_StrategiesDbEntity> 区分マスタ_StrategiesDbSet { get; set; }
    public virtual DbSet<区分マスタ_ReportingDbEntity> 区分マスタ_ReportingDbSet { get; set; }
    public virtual DbSet<区分マスタ_UrgencyDbEntity> 区分マスタ_UrgencyDbSet { get; set; }
}
```

:::warning ビュー定義の自動生成について

Nijo は特定の RDBMS 非依存のため、ビュー作成SQLとその適用は自動生成されない。
整理すると以下のようになる。

* 自動生成されるもの
  * EFCore 上のビューのデータ構造定義
  * そのビューにどういう名前のカラムがあるか
  * そのビューとトランザクションテーブル側との関係性
  * ビューと対応するC#クラス定義、ナビゲーションプロパティ
* 自動生成されないもの
  * ビュー作成SQL
  * ビューの適用

ただし、スキーマ定義変更のたびに都度それに応じたSQLを書くのは大変なので、これを補助する仕組みがある。
一度各プロジェクトのRDBMSに合わせた仕組みを構築しておけば、スキーマ定義変更時に自動的にビュー再作成SQLが出来るようになるはず。

```cs
// ------- 自動生成 -------

// ビューの物理名などの情報をもつ構造体
interface IGeneralLookupViewInfo {
    public string ViewName { get; }
    public string SourceTableName { get; } // ビューの元になるテーブル名
    public IGeneralLookupTableHardcodedColumn[] HardcodedColumns { get; }
    public IGeneralLookupTableColumn[] NonHardcodedColumns { get; }
}
interface IGeneralLookupTableHardcodedColumn {
    public string ColumnName { get; }
    public object Value { get; } // 型が string か int かはテーブル定義に従う
}
interface IGeneralLookupTableColumn {
    public string ColumnName { get; }
}

class AutoGeneratedDbContext {

    // ビュー定義の自動生成に伴い、ビューの物理名などの情報をもつ構造体と、
    // その構造体のカラム定義を返すメソッドが自動生成される。
    public static IEnumerable<IGeneralLookupViewInfo> GetGeneralLookupViewsInfo();
}

// ------- 手動実装 -------
class XXXXXXXXXXXXXX {

    public void Sample() {

        // 上記の自動生成されたメソッドを呼び出すことで、ビュー定義の情報が取得できる。
        var viewsInfo = AutoGeneratedDbContext.GetGeneralLookupViewsInfo();

        // これをもとに、各RDBMSに合わせたビュー作成SQLを生成し、適用する。
        foreach (var view in viewsInfo) {

            // SQLiteの例
            var sql = $$"""
                DROP VIEW IF EXISTS "{{view.ViewName}}";

                CREATE VIEW "{{view.ViewName}}" AS
                SELECT {{string.Join(" , ", view.NonHardcodedColumns.Select(c => $"t1.\"{c.ColumnName}\""))}}
                FROM   "{{view.SourceTableName}}" AS t1
                WHERE  {{string.Join(" AND ", view.HardcodedColumns.Select(c => $"t1.\"{c.ColumnName}\" = '{c.Value}'"))}};
                """;
        }
    }
}
```

このページの最初の例で示すと、上記のSQL作成処理で以下の文字列ができる。

```sql
DROP VIEW IF EXISTS "KBN_MST_Countries";

CREATE VIEW "KBN_MST_Countries" AS
SELECT t1."CodeValue" , t1."CodeName" , t1."DisplayOrder"
FROM   "KBN_MST" AS t1
WHERE  t1."BusinessSection" = 'MG' AND t1."CodeType" = '001';
```

このビューの責務は以下。

* 大元の汎用参照テーブルから、特定のハードコードされる区分種類による絞り込みを行うこと。
* ハードコードされる主キー以外のカラムをすべて持つこと。

:::

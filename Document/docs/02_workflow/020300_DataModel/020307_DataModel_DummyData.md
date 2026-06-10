---
draft: true
---

# ダミーデータ生成

デバッグ・開発用の初期データを自動生成する `DummyDataGenerator` クラスが自動生成されます。

:::warning DEBUG ビルド限定
`DummyDataGenerator` は `#if DEBUG` ディレクティブで囲まれており、**DEBUG ビルドでのみ有効**です。
本番環境では使用できません。
:::

## 概要

`DummyDataGenerator` を使うと、すべての DataModel のダミーデータを依存関係の順番に沿って自動生成できます。
開発開始時や開発環境のリセット時などに便利です。

**依存関係の自動解決:** `ref-to` で参照しているデータモデルが先に生成されます。
開発者はどの順番でダミーデータを作成するかを意識する必要がありません。

## 使い方

`DummyDataGenerator` は抽象クラスです。継承したクラスを作成して使います。

```csharp
// 継承クラスの作成（最低限のケース：カスタマイズなし）
public class MyDummyDataGenerator : DummyDataGenerator { }

// 呼び出し
var generator = new MyDummyDataGenerator();

// GenerateAsync は EF Core エンティティの配列を生成するところまで行う
// 実際の保存は開発者が実装する
var messages = await generator.GenerateAsync(applicationService);
```

`GenerateAsync` は EF Core エンティティの配列を生成するところまでを行います。
データベースへの保存や、Excel への書き出しなど、どのように使うかは開発者が実装します。

## カスタマイズポイント

### 集約ごとの件数・パターン指定

デフォルトでは各集約に一定数のダミーデータが生成されます。
`CreatePatternsOf{集約名}` メソッドをオーバーライドして、件数やパターンを制御できます。

```csharp
public class MyDummyDataGenerator : DummyDataGenerator {

    // 受注は100件生成する
    protected override IEnumerable<DummyDataGenerateOptions.PatternOf<OrderCreateCommand>>
        CreatePatternsOfOrder() {
        for (int i = 0; i < 100; i++) {
            yield return new() {
                Pattern = $"受注{i + 1:000}",
            };
        }
    }

    // 商品は「在庫あり」「在庫なし」「廃番」の3パターンを組み合わせて生成する
    protected override IEnumerable<DummyDataGenerateOptions.PatternOf<ProductCreateCommand>>
        CreatePatternsOfProduct() {
        foreach (var status in new[] { "在庫あり", "在庫なし", "廃番" }) {
            yield return new() { Pattern = status };
        }
    }
}
```

### インスタンス1件の生成ロジック

`CreateRandom{集約名}` メソッドをオーバーライドすることで、
1件のダミーデータを生成する具体的な値を制御できます。

```csharp
protected override OrderCreateCommand CreateRandomOrder(
    DummyDataGenerateContext ctx,
    DummyDataGenerateOptions.PatternOf<OrderCreateCommand> pattern) {

    var cmd = base.CreateRandomOrder(ctx, pattern);

    // 受注日を過去1年以内にする
    cmd.OrderDate = DateTime.Today.AddDays(-Random.Shared.Next(0, 365));

    // パターン名を名称に反映する
    cmd.Note = pattern.Pattern;

    return cmd;
}
```

### 型ごとの標準ダミー値

各型（string, DateTime, decimal など）のデフォルトの値生成ロジックは
`GetRandom{型名}` メソッドで定義されています。オーバーライドで変更できます。

```csharp
// 日付型のダミー値を現在時刻 ±1年のランダムな値にする
protected override DateTime? GetRandomDateTime(string columnName) {
    var days = Random.Shared.Next(-365, 365);
    return DateTime.Today.AddDays(days);
}

// 文字列型は英数字ランダム10文字にする
protected override string? GetRandomWord(string columnName) {
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    return new string(Enumerable.Repeat(chars, 10)
        .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
}
```

## 典型的な使用例

開発環境のセットアップスクリプトや、デバッグ用コントローラなどから呼び出します。

```csharp
// Program.cs や開発用エンドポイントからの呼び出し例
#if DEBUG
if (args.Contains("--seed")) {
    using var scope = app.Services.CreateScope();
    var appService = scope.ServiceProvider.GetRequiredService<MyApplicationService>();
    var dbContext  = scope.ServiceProvider.GetRequiredService<MyDbContext>();

    var generator = new MyDummyDataGenerator();
    var messages = await generator.GenerateAsync(appService);

    if (messages.HasErrors()) {
        Console.WriteLine("ダミーデータ生成中にエラーが発生しました");
        return;
    }

    await dbContext.SaveChangesAsync();
    Console.WriteLine("ダミーデータの生成が完了しました");
}
#endif
```

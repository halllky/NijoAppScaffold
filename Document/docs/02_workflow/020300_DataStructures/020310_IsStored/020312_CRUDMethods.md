---

---

# 登録・更新・削除

永続化されるデータ構造の定義から、データを保存するための部品が自動生成されます。
処理の順序（読み込み → 値の設定 → 検証 → 確認メッセージ → トランザクション → 保存）は、コマンドの `Execute{コマンド名}Async` の中に開発者が明示的に書きます。

| 部品                            | 提供元                                 | 役割                                                 |
| ------------------------------- | -------------------------------------- | ---------------------------------------------------- |
| `StartCreating{集約名}()`       | 自動生成（DbContext）                  | 新規登録コマンドの作成                               |
| `LoadForEdit{集約名}Async(...)` | 自動生成（DbContext）                  | 更新コマンドの読み込み                               |
| `TryValidate(...)`              | 自動生成（各コマンド）                 | スキーマ定義から導かれる検証と楽観排他のチェック。エラーが無ければ検証済みコマンドを返す |
| `ToDeleteCommand()`             | 自動生成（更新コマンド）               | 削除コマンドへの変換                                 |
| `TrySaveAsync(...)`             | `Core/OverridedDbContext.TrySaveAsync.cs` | 保存の実行。**システムごとに書き換える前提の手書き** |

## 全体の流れ

複数行を一括で更新するコマンドの例です。

```csharp
public override async Task Execute売上一括更新Async(売上一括更新DisplayData param, IPresentationContext<売上一括更新Messages> context) {

    // 集約単位の読み込み。AsNoTracking で読み込まれる。
    // Version には更新時ではなく画面表示時の値を渡すこと。
    var keys = param.Rows
        .Where(x => x.ExistsInDatabase && (x.WillBeChanged || x.WillBeDeleted))
        .Select(x => new 売上KeyWithVersion(x.Version) { Id = x.Id });
    var loaded = await DbContext.LoadForEdit売上Async(keys);

    // ループ後にまとめて保存するのでメモリ上に蓄積する
    var saveCommands = new List<ValidatedCommand<売上UpdateCommand>>();

    for (var i = 0; i < param.Rows.Count; i++) {
        var row = param.Rows[i];
        var message = context.Messages.Rows[i];

        // 新規
        if (!row.ExistsInDatabase) {
            var createCommand = DbContext.StartCreating売上();
            createCommand.Id = row.Id;
            createCommand.Amount = row.Amount;

            if (createCommand.TryValidate(out var validatedCreate, out var createErrors)) {
                saveCommands.Add(validatedCreate);
            }
            createErrors.CopyTo(message);
            if (createCommand.Amount < 0m) message.AddError("売上は0以上");
            continue;
        }

        // 変更なし
        if (!row.WillBeChanged && !row.WillBeDeleted) continue;

        // 既存データの引当
        if (!loaded.TryGetValue(new() { Id = row.Id }, out var updateCommand)) {
            message.AddError(MSG.DataNotFoundError());
            continue;
        }

        // 削除
        if (row.WillBeDeleted) {
            if (updateCommand.ToDeleteCommand().TryValidate(out var validatedDelete, out var deleteErrors)) {
                saveCommands.Add(validatedDelete);
            }
            deleteErrors.CopyTo(message);
            continue;
        }

        // 更新
        updateCommand.Amount = row.Amount;
        if (updateCommand.TryValidate(out var validatedUpdate, out var updateErrors)) {
            saveCommands.Add(validatedUpdate);
        }
        updateErrors.CopyTo(message);
    }

    // いずれか1件でもエラーなら中断
    if (context.Messages.HasError()) return;

    // 1巡目はここまで
    if (context.ValidationOnly) {
        context.AddConfirm("更新しますか？");
        return;
    }

    // トランザクションの範囲はシステムによって異なるので明示的に書く
    await using var tran = await DbContext.Database.BeginTransactionAsync();

    var result = await DbContext.TrySaveAsync(saveCommands);
    if (!result.Success) {
        result.Errors.CopyTo(context.Messages);
        return; // コミットせずにスコープを抜けるとロールバックされる
    }

    await tran.CommitAsync();
    context.Messages.AddInfo("保存しました。");
}
```

## コマンド

集約ごとに次のクラスが生成されます。

```
売上CreateCommand … 新規登録
売上UpdateCommand … 更新
売上DeleteCommand … 削除
```

ルート集約のコマンドは `new` できず、次の方法でのみ入手できます。子孫集約のコマンド（`売上明細CreateCommand` など）は単なる値の入れ物であり、`new() { ... }` で作成します。

### 新規登録コマンド

```csharp
var command = DbContext.StartCreating売上();
command.売上日時 = DateTime.Now;
command.担当者 = new() { 従業員番号 = "E001" };
command.売上明細 = [
    new() { 明細ID = "1", 売上数量 = 3 },
];
```

シーケンス（自動採番）の項目は新規登録コマンドにはありません。保存後のデータから参照します。

### 更新コマンド

読み込み方は3通りあります。いずれも子孫集約ごと `AsNoTracking` で読み込まれます。

```csharp
// キーで1件。見つからなければ null
売上UpdateCommand? command = await DbContext.LoadForEdit売上Async(new 売上KeyWithVersion(param.Version) { 売上SEQ = param.売上SEQ });

// キーで複数件。見つからなかったものは結果の辞書に含まれない
IReadOnlyDictionary<売上Key, 売上UpdateCommand> commands = await DbContext.LoadForEdit売上Async(keys);

// 任意の条件。更新開始時点で対象データのキーが分からない更新で使う
IReadOnlyList<売上UpdateCommand> commands = await DbContext.LoadForEdit売上Async(query => query.Where(x => x.売上日時 < threshold));
```

`売上Key` は値で比較されるので、別のインスタンスでも同じ値であれば辞書を引けます。

:::warning バージョンの渡し方
`売上KeyWithVersion` のバージョンには、**画面を初期表示したときに取得した値**を渡してください。更新直前に読み直した値を渡すと、利用者が画面を開いている間の他者の更新を黙って上書きしてしまいます。

バージョンに null を渡すと `ArgumentNullException` になります。集約の更新では楽観排他を省略できません。

任意の条件で読み込んだ場合は読み込んだ時点のバージョンが期待値になるので、読み込みから保存までの間に他者が更新すると楽観排他エラーになります。
:::

### 削除コマンド

削除するときもいったん読み込み、`ToDeleteCommand()` で変換します。
`UseSoftDelete` が指定された集約では論理削除になり、呼び出し側のコードは変わりません（[UseSoftDelete](./020314_UseSoftDelete.md)）。

### Child / Children の更新

`Child`（1対1の子集約）と `Children`（1対多の子集約）は、読み込み時のデータとキーで照合して保存方法が決まります。

- 読み込み時に存在し、保存時も存在する → `UPDATE`
- 読み込み時に存在せず、保存時に存在する → `INSERT`
- 読み込み時に存在し、保存時に存在しない → `DELETE`

## 検証（TryValidate）

`TryValidate` は [スキーマ定義から自動生成されるチェック](./020313_Validators.md) と、読み込み時点の楽観排他のチェックを行います。

```csharp
bool TryValidate(out ValidatedCommand<売上UpdateCommand>? validated, out 売上SaveCommandMessages errors);
```

- `errors`: 検証エラー。コマンドと同じ形のメッセージコンテナ。
- `validated`: エラーが無い場合のみ得られる検証済みコマンド。`TrySaveAsync` はこれだけを受け取るので、検証していないコマンドの保存はコンパイルエラーになります。

検証済みコマンドは検証時点の値を保持します。検証後にコマンドを書き換えても保存される内容は変わりません。コマンド自身の状態は検証で変更されません。

`CopyTo` で画面側のメッセージコンテナへコピーします。**同名のパスにそのままコピーする**ので、項目の階層や名前が違うときは対応する階層を渡してください。

```csharp
if (command.TryValidate(out var validated, out var errors)) {
    saveCommands.Add(validated);
}

// 画面側は message.従業員.氏名 のように1階層深い場合
errors.CopyTo(message.従業員);

// 画面側とデータモデルで項目名が違う場合
errors.入荷数量.CopyTo(message.数量);
```

1件だけを保存する場合は、エラーがあればその場で抜けると `validated` が null でないことをコンパイラが判定できます。

```csharp
if (!command.TryValidate(out var validated, out var errors)) {
    errors.CopyTo(context.Messages);
    return;
}
```

ヘッダと明細のように他の検証も続けてからまとめて抜ける場合は、抜ける箇所で `validated` の null を確認します。

```csharp
header.TryValidate(out var validatedHeader, out var headerErrors);
headerErrors.CopyTo(context.Messages);
// ...明細の検証...
if (validatedHeader == null || context.Messages.HasError()) return;
```

## 保存（TrySaveAsync）

`TrySaveAsync` は `Core/OverridedDbContext.TrySaveAsync.cs` に**手書きのコードとして**置かれています。
保存時に行うべきことはシステムの要件によって大きく異なるので自動生成せず、テンプレートが提供するコードを自由に書き換えられるようにしています。

既定の実装には次のオーバーロードがあります。

```csharp
Task<SaveResult<売上UpdateCommand>> TrySaveAsync(ValidatedCommand<売上UpdateCommand>);
Task<SaveResult> TrySaveAsync(ValidatedDeleteCommand<売上UpdateCommand>);
Task<SaveResult<IList<売上UpdateCommand>>> TrySaveAsync(ValidatedCommand<売上UpdateCommand> の配列やリスト);
```

既定の実装は次の順に処理します。

1. トランザクションが開始されていなければ例外を送出
2. `ChangeTracker` に他の変更があれば失敗
3. 同時に保存するコマンド同士でキーが重複していれば失敗
4. 検証済みコマンドの `StartTracking()` で監査項目を設定して変更追跡を開始
5. `SaveChangesAsync()` を実行
6. 変更追跡を終了
7. 保存後のデータを `result.Updated` として返す

### 保存結果

| 失敗の種類                   | 原因                                                   | ログレベル |
| ---------------------------- | ------------------------------------------------------ | ---------- |
| `ProgrammingError`           | 手順2 に該当。プログラムの誤り                         | Error      |
| `Concurrency`                | 楽観排他エラー                                         | Warning    |
| `UniqueConstraint`           | 一意制約違反。他ユーザーとの操作競合の可能性が高い     | Warning    |
| `DatabaseError`              | その他のデータベースのエラー                           | Error      |

DB 接続エラーなど上記以外は例外がそのまま送出されます。

運用担当者への通知は Error レベルのログとして出力されるので、通知先はログの出力先の設定で制御してください。
EF Core が既定で Error レベルで出力する DB エラーのログは `OverridedDbContext` で Warning に引き下げているので、一意制約違反などで通知が飛ぶことはありません。

エラーメッセージは項目ごとではなく保存処理全体の単位で `result.Errors` に入ります。キー重複を行単位で見せたいなどの場合は、検証の際に独自に確認してください。

### 保存後のデータ

`result.Updated` は検証前のコマンドとは別の新しい更新コマンドです。保存時に採番されたシーケンスの参照や、続けて別の更新を行なうときに使えます。
一度 `TrySaveAsync` に渡した検証済みコマンドと、同じコマンドから作成された別の検証済みコマンドは再度使用できません。

```csharp
var result = await DbContext.TrySaveAsync(validated);
if (!result.Success) { ... }
context.ReturnValue.売上SEQ = result.Updated.売上SEQ;
```

リストを渡した場合、`result.Updated` の順番は引数の順番と対応し、削除されたものは含まれません。

:::note 制約
- シーケンスが主キーの新規データは、保存前のキーでは `result.Updated` を引き当てられません。順番の対応で引き当ててください。
- 同じキーの削除と新規登録を同時に行なうことはできません。`TrySaveAsync` を2回に分けて呼んでください。
- 一部が失敗しても他はコミットしたい場合は、コマンドの数だけ `TrySaveAsync` とコミットを繰り返してください。
:::

## 全経路で守るべきルール

「このデータは常にこうでなければならない」というルールで、スキーマ定義では表現できないものは、名前付きのメソッドに切り出して各コマンドから呼びます。
検証メッセージの入れ物（`売上SaveCommandMessages`）に追記すれば `CopyTo` でまとめて画面に渡せます。

```csharp
command.TryValidate(out var validated, out var errors);
Validate売上新規(command, errors); // データモデルごとのファイルに定義しておく
errors.CopyTo(context.Messages);

if (validated == null || context.Messages.HasError()) return;
```

独自のチェックのエラーは `TryValidate` が返す検証済みコマンドの有無に影響しません。保存を止めるには `context.Messages.HasError()` を確認してください。

保存後に常に行なう処理（外部システムへの通知など）も同様に、`TrySaveAsync` の成功後に明示的に呼びます。

## 監査情報

`CreatedAt`, `UpdatedAt`, `CreateUser`, `UpdateUser` は `TrySaveAsync` の中で自動的に設定されます。
値は DI コンテナに登録された `IAuditInfo` から取得されます。既定の実装は `Core/AuditInfo.cs` なので、ログインユーザーを記録する場合はこれを書き換えてください。
テンプレートの `OverridedApplicationService` の `CurrentTime` も同じ値を返します。

## `ValidationOnly` フラグ

Web アプリケーションで「更新しますか？」の確認ダイアログを挟む場合、同じコマンドが2回呼ばれます。

1. **1巡目**（`ValidationOnly = true`）: エラーチェックのみ実施。問題なければ確認メッセージを返す。
2. **2巡目**（`ValidationOnly = false`）: エラーチェック + 実際の DB 保存。

自動生成された部品は `ValidationOnly` を判定しません。冒頭の例のように、`TrySaveAsync` の前で判定して処理を抜けてください。

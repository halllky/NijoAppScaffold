---
name: implement-command-model
description: nijo.xml で として定義した処理を C# で実装・修正するときに使う。
---

# コマンドモデルの実装

コマンドモデルは、利用者やスケジューラーによる「1 回の操作」を表す。
自動生成されるのは `Execute{コマンド名}Async` の抽象メソッドと、それを呼び出す窓口だけであり、処理の中身はすべて開発者が書く。

決まりと仕組みは [標準仕様書](../../../標準仕様書/README.md) を正とする。この手順書は、それを実装するときに使う具体的な名前と書き方を補うものである。
標準仕様書と食い違う場合は標準仕様書に従い、食い違いをユーザーに報告する。

## 進め方

1. 自動生成された抽象メソッドのシグネチャ（引数・戻り値・メッセージの型）を確認する。記憶や推測ではなく生成物を正とする。
2. アプリケーションサービス（`OverridedApplicationService`）の partial class にオーバーライドを書く。ファイルの分け方は標準仕様書「プロジェクトの構成と業務処理の書き方」の開発者向け情報に従う。
3. ビルドし、実際に呼び出して確かめる。正常系に加え、入力エラー・権限なし・検証のみモードの各場合を確かめる。

---

## 自動生成されたファイルは編集しない

振る舞いはオーバーライドで変える。生成物のままでは実現できない場合は、コードを歪めて回避せずユーザーに報告する。

## 処理の順序

順序とその理由は標準仕様書「更新」の「更新の処理の流れ」に従う。各段で使う名前は次のとおり。

| 段 | 使うもの |
|---|---|
| 権限の確認 | `LoginUser` |
| 読み込み・作成 | `DbContext.LoadForEdit{データモデル名}Async`、`DbContext.StartCreating{データモデル名}` |
| 入力チェック | 生成されたコマンドの `TryValidate`、データモデル用のファイルに書いた名前付きのメソッド |
| 入力チェックだけの判定 | `context.ValidationOnly`、確認メッセージは `context.Confirms` |
| トランザクション | `BeginTransactionAsync`（アプリケーションサービスのもの） |
| 保存 | `DbContext.TrySaveAsync` |

途中で抜けるときは、戻り値のあるコマンドなら null を返す。

## 検証のみモード

決まりは標準仕様書「更新」の開発者向け情報の「確認メッセージ」に従う。

- `TrySaveAsync` は `context.ValidationOnly` を判定しない。`TrySaveAsync` の前で判定して抜ける。
- 自前で書く副作用は、トランザクションへの予約を通すか、`ValidationOnly` が false のときだけ行う。

## エラーはメッセージで返すか例外で投げるか

決め方は標準仕様書「エラーとメッセージ」の開発者向け情報に従う。

- メッセージは `messages.項目名.AddError(...)`、明細の行は `messages.明細名[i].項目名.AddError(...)`、全体は `messages.AddError(...)` に付ける。
- エラーの有無は `messages.HasError()` で確かめる。

## 保存

決まりは標準仕様書「更新」の「保存の処理」に従う。

- `TryValidate` が返すエラーは `errors.CopyTo(messages)` で画面側のメッセージへ移す。`CopyTo` は同名のパスにそのままコピーするので、階層や名前が違うときは対応する階層（例: `messages.明細名[i]`）を渡す。
- 保存の結果は `result.Success` で確かめ、失敗していれば `result.Errors.CopyTo(messages)` を呼んで抜ける。
- 複数件はループの後でまとめて `TrySaveAsync` に渡す。
- 更新・削除では、画面が読み込んだ時点のバージョンを引数から受け取って `{データモデル名}KeyWithVersion` に渡す。更新の開始時点でキーが分からない場合は `LoadForEdit{データモデル名}Async(query => ...)` で任意の条件で読み込み、読み込んだ時点のバージョンで排他をかける。

## トランザクション

決まりは標準仕様書「更新」の「トランザクション」と、「外部システム連携」の開発者向け情報に従う。

```csharp
await using var tran = await BeginTransactionAsync();
var result = await DbContext.TrySaveAsync(command);
if (!result.Success) { // コミットせずに抜ければ、DB はロールバックされ、予約した外部更新も実行されない
    result.Errors.CopyTo(messages);
    return; // 戻り値のあるコマンドでは return null;
}
await tran.CommitAsync();
```

- `DbContext.Database.BeginTransactionAsync` を直接呼ばない。
- 外部システムの更新の予約は、外部システムのインターフェースの実装の中で `CurrentTransaction.Prepare(リソース名, 更新処理)` を呼んで行う。予約した更新の実行順と失敗時の扱いは `ExternalResourceTransaction` のコメントを読んで確認する。

## 現在時刻

`DateTime.Now` を直接呼ばず、アプリケーションサービスの `CurrentTime` を使う（標準仕様書「プロジェクトの構成と業務処理の書き方」）。

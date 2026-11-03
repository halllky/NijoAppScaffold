---
sidebar_position: 2
sidebar_label: IPresentationContext
---

# IPresentationContext

コマンドの `Execute{Name}Async` メソッドは、次の形をしています。

```csharp
public override async Task<戻り値の型?> Execute{Name}Async(
    引数の型 param,           // 処理の入力
    引数のメッセージの型 messages, // 処理の出力のうち、引数の形に沿ったエラー・情報メッセージ
    IPresentationContext context) // この処理を呼び出したプレゼンテーション層（Web、バッチ、単体テストなど）からの情報
```

1回の処理の情報は、性質ごとに次のように分かれています。

| 情報 | 受け渡し方 | 形が決まる場所 |
|---|---|---|
| 入力 | 引数 `param` | スキーマ定義の `Parameter` 属性 |
| 戻り値 | メソッドの戻り値（`return`） | スキーマ定義の `ReturnValue` 属性 |
| エラー・情報メッセージ | 引数 `messages` | 引数の形から決まる |
| 呼び出し元の情報（検証専用モードなど） | 引数 `context`（`IPresentationContext`） | プレゼンテーション層ごとの実装 |

引数のないコマンドは `param` が無く、`messages` は `MessageSetter` 型になります。戻り値のないコマンドは `Task` を返します。


## Messages（エラー・情報メッセージ）

### 操作レベルのメッセージ

フォーム全体に関わるエラーや、処理完了後の通知メッセージを設定します。

```csharp
// 権限がない場合のエラー（戻り値のあるコマンドなので null を返して中断）
if (LoginUser == null || !LoginUser.CanUse入荷登録) {
    messages.AddError("入荷担当のみ実行可能です。");
    return null;
}

// 処理成功後の通知
messages.AddInfo("入荷登録が完了しました。");

// エラーが1件でもあるか確認
if (messages.HasError()) return null;
```

### フィールドレベルのメッセージ

特定の入力項目に対してエラーを紐付けます。

```csharp
// ログイン処理の例
if (string.IsNullOrWhiteSpace(param.従業員番号)) {
    messages.従業員番号.AddError("従業員番号を入力してください。");
}
if (string.IsNullOrWhiteSpace(param.パスワード)) {
    messages.パスワード.AddError("パスワードを入力してください。");
}
if (messages.HasError()) return;
```

### コレクション要素のメッセージ

明細行など繰り返し要素の各行にエラーを紐付けます。
`messages.コレクション名[i]` でその行専用のメッセージコンテナを取得します。

```csharp
// 売上明細の一覧を検証する例
for (var i = 0; i < param.売上詳細の売上明細.Count; i++) {
    var detail = param.売上詳細の売上明細[i];
    var message = messages.売上詳細の売上明細[i]; // i 行目のメッセージコンテナ

    if (detail.商品.商品SEQ == null) {
        message.商品.AddError("商品を選択してください。");
        continue;
    }
    if (detail.売上数量 == null || detail.売上数量 <= 0) {
        message.売上数量.AddError("売上数量は1以上の整数を入力してください。");
        continue;
    }
}

// 全行のチェックが終わった後で、エラーがあれば中断
if (messages.HasError()) return;
```

---

## ValidationOnly（2フェーズ検証）

保存前にユーザーへ「本当に実行しますか？」という確認を求めたい場面があります。
このために、コマンドの呼び出しは内部的に2フェーズで実行されることがあります。

- **フェーズ1（`ValidationOnly = true`）**: バリデーションのみ実行し、問題がなければ確認メッセージを積む。実際の保存はしない。
- **フェーズ2（`ValidationOnly = false`）**: ユーザーが確認した後に実際の保存処理を実行する。

```csharp
public override async Task Execute売上修正Async(
    売上詳細DisplayData param,
    売上詳細Messages messages,
    IPresentationContext context) {

    // ─── バリデーション ───
    if (param.売上SEQ == null) {
        messages.AddError("売上SEQが指定されていません。");
        return;
    }
    // ... 明細のチェック ...

    if (messages.HasError()) return;

    // フェーズ1: 確認メッセージを積んで終了（実際の保存はしない）
    if (context.ValidationOnly) {
        // ※ AddConfirm はアプリ側で拡張するメソッド（後述）
        context.AddConfirm("修正を確定します。よろしいですか？");
        return;
    }

    // フェーズ2: ここから実際の保存処理
    await using var tran = await BeginTransactionAsync();
    // ...
    await tran.CommitAsync();
}
```

:::note `AddConfirm` は Nijo 標準 API ではありません
`context.AddConfirm()` と `context.HasConfirm()` は Nijo が自動生成するものではなく、
アプリ側（`PresentationContextExtensions.cs`）で定義する拡張メソッドです。
「Web ではダイアログを出す・バッチでは無視して続行する」などアプリの要件に応じて自由に設計できます。
:::

---

## 戻り値

処理結果はメソッドの戻り値として返します。
エラーで処理を中断する場合や、検証専用モードで処理を終える場合は `null` を返して構いません。

```csharp
// 入荷登録の例: 発番した入荷ID を戻り値として返す
await tran.CommitAsync();
messages.AddInfo("入荷登録が完了しました。");
return new() { 入荷ID = newId };
```

```csharp
// ログインの例: 権限がなければ null を返して中断する
if (employee == null) {
    messages.AddError("従業員番号またはパスワードが違います。");
    return null;
}
await tran.CommitAsync();
return LoginUser.From(employee).ToDisplayData();
```

---

## トランザクションとの組み合わせパターン

[登録・更新・削除](../020300_DataStructures/020310_IsStored/020312_CRUDMethods.md) の部品は `messages` を受け取りません。
検証結果や保存結果のメッセージは、コマンドの処理の中で `CopyTo` により `messages` へ移します。

```csharp
// 検証
var header = DbContext.StartCreating入荷();
header.入荷ID = newId;
header.TryValidate(out var validatedHeader, out var headerErrors);
headerErrors.CopyTo(messages);

var details = new List<ValidatedCommand<入荷明細UpdateCommand>>();
for (var i = 0; i < param.入荷商品一覧.Count; i++) {
    var detail = DbContext.StartCreating入荷明細();
    // ...
    if (detail.TryValidate(out var validated, out var errors)) {
        details.Add(validated);
    }
    errors.CopyTo(messages.入荷商品一覧[i]);
}

if (validatedHeader == null || messages.HasError() || context.ValidationOnly) return null;

// 保存。コミットせずに return するだけでロールバックされる
await using var tran = await BeginTransactionAsync();

var headerResult = await DbContext.TrySaveAsync(validatedHeader);
if (!headerResult.Success) {
    headerResult.Errors.CopyTo(messages);
    return null;
}

var detailResult = await DbContext.TrySaveAsync(details);
if (!detailResult.Success) {
    detailResult.Errors.CopyTo(messages);
    return null;
}

await tran.CommitAsync();
messages.AddInfo("入荷登録が完了しました。");
return new() { 入荷ID = newId };
```

---

## プレゼンテーション層ごとの実装

`IPresentationContext` はプレゼンテーション層ごとに1クラスずつ実装します。
Web、バッチ、単体テストは呼び出し元として別物なので、それぞれが持つ情報も別々に設計できます。

| プレゼンテーション層 | 実装クラスの例 | `ValidationOnly` |
|---|---|---|
| Web | `PresentationContextInWebApi` | HTTPリクエストの `ignore-confirm` が無い1回目は `true` |
| 単体テスト | `PresentationContextInUnitTest` | テストケースで指定 |
| バッチ | アプリ側で実装 | 通常は `false` |

ある処理の中から別のコマンドを呼ぶ場合は、同じ `context` をそのまま渡し、メッセージは呼び出し先の引数の形に変換して渡します。

```csharp
await applicationService.Execute入荷登録Async(param, messages.As<入荷詳細Messages>(), context);
```

### Web からの呼び出し

自動生成された `CommandQuery.All` には、スキーマ定義から生成されたすべてのクエリとコマンドが、引数・戻り値・メッセージの型と URL とともに並んでいます。
テンプレートの `WebApi/Base/CommandQueryEndpoints.cs` は、これを1件ずつ ASP.NET Core の Minimal API のエンドポイントとして登録し、HTTPリクエストと処理の引数・戻り値・メッセージとを変換します。
変換方法（JSONの形、ステータスコードなど）はアプリ側のコードなので、クライアント側の `callComplexPostEndpointAsync` と整合性がとれていれば自由に変更できます。

```csharp
// Program.cs
app.MapCommandQueryEndpoints((commandQuery, endpoint) => {
    // 処理ごとに異なる設定がある場合はここで行う
    if (commandQuery.Name is nameof(E_CommandQueryType.ログイン)) {
        endpoint.AllowAnonymous();
    }
});
```

---

## IPresentationContext の拡張

`IPresentationContext` には、アプリケーション固有の情報をサーバーからクライアントへ返す仕組みを追加できます。
**サーバーとクライアントで同じフィールド名・型を使う**ことが条件で、内容は自由に設計できます。

デモ101が採用している `AddConfirm`（確認ダイアログ）はその一例です。

### 仕組み

拡張は次の3ステップで機能します。

1. **C#: 追加インターフェイスと拡張メソッドを定義する**
2. **C#: サーバー側の具体的なコンテキスト（`PresentationContextInWebApi` など）に実装する**
3. **TypeScript: クライアント側のレスポンス型に同じフィールドを追加し、画面側で利用する**

### デモ101の実装例：確認ダイアログ（AddConfirm）

**サーバー側（C#）**

`PresentationContextExtensions.cs` に、追加インターフェイスと拡張メソッドを定義します。

```csharp
// 追加インターフェイス: 確認メッセージを持てるコンテキスト
public interface IConfirmablePresentationContext {
    List<string> Confirms { get; }
}

public static class PresentationContextExtensions {
    // IPresentationContext の拡張メソッドとして定義することで、
    // ビジネスロジック層のコードが具体的な実装クラスに依存しないようにする。
    public static void AddConfirm<T>(this T context, string text) where T : IPresentationContext {
        if (context is IConfirmablePresentationContext cpc) {
            cpc.Confirms.Add(text);
        } else {
            // バッチ処理など確認ダイアログを出せない実行環境の場合は無視して続行する。
            // アプリの要件によっては実行時例外を投げる設計も考えられる。
        }
    }
    public static bool HasConfirm<T>(this T context) where T : IPresentationContext {
        if (context is IConfirmablePresentationContext cpc) {
            return cpc.Confirms.Count > 0;
        }
        return false;
    }
}
```

**クライアント側（TypeScript）**

レスポンス型に `confirms` フィールドを追加し、`window.confirm()` でダイアログを表示します。
ここでは、ステータスコード `202 Accepted` が「確認待ち」を意味するものと決めており、ユーザーが承諾したら `ignore-confirm=true` を付けて2回目のリクエストを送ることとしています。

```typescript
// サーバー側の PresentationContext の応答構造（アプリ側で定義）
type PresentationContextResponse<T> = {
  detail: DetailMessagesContainer  // フィールドごとのエラー・情報メッセージ
  confirms: string[]               // 確認ダイアログのメッセージ一覧
  toastMessage?: string            // トースト通知メッセージ（別途拡張した例）
  returnValue: T                   // C# 側のメソッドの戻り値
}

// callComplexPostEndpointAsync.ts の抜粋
if (response.status === 202) {
  // confirms フィールドを読み取ってダイアログを表示
  const confirmMessage = json.confirms.join('\n') || '処理を確定しますか？'
  if (window.confirm(confirmMessage)) {
    searchParams.set('ignore-confirm', 'true')
    continue  // ignore-confirm=true を付けて2回目のリクエストへ
  } else {
    return { type: 'canceled' }
  }
}
```

### 他の拡張例

同じパターンで、アプリの要件に合わせて自由に属性を追加できます。

**例1: AddToast（成功時のトースト通知）**

上のレスポンス型にはすでに `toastMessage?: string` フィールドが含まれており、デモ101でも実装されています。

```csharp
// サーバー側
public interface IToastablePresentationContext {
    string? ToastMessage { get; set; }
}
public static class PresentationContextExtensions {
    public static void AddToast<T>(this T context, string message) where T : IPresentationContext {
        if (context is IToastablePresentationContext tc) {
            tc.ToastMessage = message;
        }
    }
}

// ビジネスロジック層での使い方
await tran.CommitAsync();
context.AddToast("登録が完了しました。");
```

```typescript
// クライアント側: レスポンスから toastMessage を取り出してトースト表示
const result = await callComplexPostEndpointAsync('受注登録', param)
if (result.type === 'ok' && result.toastMessage) {
  showToast(result.toastMessage)
}
```

**例2: SetRedirectUrl（処理完了後のページ遷移）**

```csharp
// サーバー側
public interface IRedirectablePresentationContext {
    string? RedirectUrl { get; set; }
}
public static class PresentationContextExtensions {
    public static void SetRedirectUrl<T>(this T context, string url) where T : IPresentationContext {
        if (context is IRedirectablePresentationContext rc) {
            rc.RedirectUrl = url;
        }
    }
}

// ビジネスロジック層での使い方
await tran.CommitAsync();
context.SetRedirectUrl($"/orders/{savedId}");
```

```typescript
// クライアント側: レスポンスから redirectUrl を取り出してページ遷移
if (result.type === 'ok' && result.redirectUrl) {
  navigate(result.redirectUrl)
}
```

### 設計のポイント

- **拡張メソッドを `IPresentationContext` に定義する**: ビジネスロジック層のコードが具体的な実装クラス（`PresentationContextInWebApi` 等）に依存しない。
- **サーバーとクライアントの型を同期する**: レスポンス型 (`PresentationContextResponse`) にフィールドを追加したら、クライアント側の型定義も必ず更新する。自動生成される部分ではないため、変更漏れに注意する。

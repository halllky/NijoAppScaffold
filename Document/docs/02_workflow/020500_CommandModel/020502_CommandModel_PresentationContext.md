---
sidebar_position: 2
sidebar_label: IPresentationContext
---

# IPresentationContext

`IPresentationContext` は、CommandModel の `Execute{Name}Async` メソッドに渡される「処理文脈」オブジェクトです。
**エラーメッセージの管理**、**戻り値のセット**、**検証専用モードの制御**という3つの役割を担います。


## Messages（エラー・情報メッセージ）

### 操作レベルのメッセージ

フォーム全体に関わるエラーや、処理完了後の通知メッセージを設定します。

```csharp
// 権限がない場合のエラー
if (LoginUser == null || !LoginUser.CanUse入荷登録) {
    context.Messages.AddError("入荷担当のみ実行可能です。");
    return;
}

// 処理成功後の通知
context.Messages.AddInfo("入荷登録が完了しました。");

// エラーが1件でもあるか確認
if (context.Messages.HasError()) return;
```

### フィールドレベルのメッセージ

特定の入力項目に対してエラーを紐付けます。

```csharp
// ログイン処理の例
if (string.IsNullOrWhiteSpace(param.従業員番号)) {
    context.Messages.従業員番号.AddError("従業員番号を入力してください。");
}
if (string.IsNullOrWhiteSpace(param.パスワード)) {
    context.Messages.パスワード.AddError("パスワードを入力してください。");
}
if (context.Messages.HasError()) return;
```

### コレクション要素のメッセージ

明細行など繰り返し要素の各行にエラーを紐付けます。
`context.Messages.コレクション名[i]` でその行専用のメッセージコンテナを取得します。

```csharp
// 売上明細の一覧を検証する例
for (var i = 0; i < param.売上詳細の売上明細.Count; i++) {
    var detail = param.売上詳細の売上明細[i];
    var message = context.Messages.売上詳細の売上明細[i]; // i 行目のメッセージコンテナ

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
if (context.Messages.HasError()) return;
```

---

## ValidationOnly（2フェーズ検証）

保存前にユーザーへ「本当に実行しますか？」という確認を求めたい場面があります。
このために、CommandModel の呼び出しは内部的に2フェーズで実行されることがあります。

- **フェーズ1（`ValidationOnly = true`）**: バリデーションのみ実行し、問題がなければ確認メッセージを積む。実際の保存はしない。
- **フェーズ2（`ValidationOnly = false`）**: ユーザーが確認した後に実際の保存処理を実行する。

```csharp
public override async Task Execute売上修正Async(
    売上詳細DisplayData param,
    IPresentationContext<売上詳細Messages> context) {

    // ─── バリデーション ───
    if (param.売上SEQ == null) {
        context.Messages.AddError("売上SEQが指定されていません。");
        return;
    }
    // ... 明細のチェック ...

    if (context.Messages.HasError()) return;

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

## ReturnValue（戻り値）

`IPresentationContextWithReturnValue` を使う場合、処理が成功した後に `context.ReturnValue` へ返却データをセットします。

```csharp
// 入荷登録の例: 発番した入荷ID を戻り値として返す
if (!context.ValidationOnly && headerResult.IsSaveCompleted() && !hasErrorInDetail) {
    await tran.CommitAsync();
    context.ReturnValue.入荷ID = newId;               // コミット後にセット
    context.Messages.AddInfo("入荷登録が完了しました。");
}
```

```csharp
// ログインの例: オブジェクトごと代入する
context.ReturnValue = new() {
    従業員番号 = employee.従業員番号,
    氏名 = employee.氏名,
    入荷機能を利用可能 = employee.入荷担当 == true,
    販売機能を利用可能 = employee.販売担当 == true,
};
await tran.CommitAsync();
```

---

## トランザクションとの組み合わせパターン

`context` は [DataModel のCRUD メソッド](../020300_DataModel/020302_DataModel_CRUDMethods.md) （`CreateXxxAsync` / `UpdateXxxAsync` など）にもそのまま渡します。
CRUD メソッド内でエラーが起きた場合、エラーメッセージは CRUD メソッド内で自動的に `context.Messages` へ設定されます。

```csharp
await using var tran = await BeginTransactionAsync();

var result = await Create入荷Async(new() { ... }, context);

// IsSaveCompleted() が false のとき、エラーメッセージはすでに設定済み
// コミットせずに return するだけでロールバックされる
if (!result.IsSaveCompleted()) return;

// 明細も同じトランザクション内で処理
for (var i = 0; i < param.入荷商品一覧.Count; i++) {
    var detailResult = await Create入荷明細Async(new() { ... }, context, context.Messages.入荷商品一覧[i]);
    if (!detailResult.IsSaveCompleted()) hasError = true;
}

if (!hasError) {
    await tran.CommitAsync();
    context.ReturnValue.入荷ID = newId;
    context.Messages.AddInfo("入荷登録が完了しました。");
}
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
  returnValue: T                   // C# 側の context.ReturnValue
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

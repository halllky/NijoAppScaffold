---
sidebar_position: 7
---

# ConstantModel（定数モデル）

定数モデルは、C#とJavaScript/TypeScriptで常に値が同期される定数のソースコードを生成するためのモデルです。

## 概要

定数モデルを使用することで、以下のメリットがあります：

- **型安全性**: 定数値をハードコードする代わりに、型安全な定数として定義できます
- **同期性**: C#とJavaScript/TypeScript間で定数値が自動的に同期されます
- **保守性**: 定数値の変更時に、一箇所の変更で全体に反映されます
- **階層構造**: ネストされた定数グループにより、論理的な構造を持った定数定義が可能です

## サポートされる定数の種類

### 基本型

| 型         | 説明               | C#での型  | TypeScriptでの型 |
| ---------- | ------------------ | --------- | ---------------- |
| `string`   | 文字列定数         | `string`  | `string`         |
| `int`      | 整数定数           | `int`     | `number`         |
| `decimal`  | 小数定数           | `decimal` | `number`         |
| `template` | テンプレート文字列 | 関数      | 関数             |

### テンプレート文字列

`template` 型を使用すると、引数を受け取って文字列を返す関数が生成されます。
使用できる変数の数は、ConstantValue中に含まれる `{0}` , `{1}` , ... から自動的に判定されます。
テンプレート文字列中に `{0}` が2回、 `{1}` が1回登場した場合、
関数の第1引数がそれら2箇所に、第2引数が後者1箇所に適用されます。


```xml
<!-- 定数です。 -->
<MyConstants Type="constant-model">
  <!-- エラーメッセージの基本的フォーマット。 -->
  <ErrorMessage ConstantType="template" ConstantValue="エラーが発生しました: {0}" DisplayName="エラーメッセージ" />
</MyConstants>
```

これにより以下のようなコードが生成されます：

**C#:**
```csharp
/// <summary>
/// 定数です。
/// </summary>
public static class MyConstants {
    /// <summary>
    /// エラーメッセージ
    /// エラーメッセージの基本的フォーマット。
    /// </summary>
    public static string ErrorMessage(string arg0) => $"エラーが発生しました: {arg0}";
}
```

**TypeScript:**
```typescript
/**
 * 定数です。
 */
export const MyConstants = {
  /**
   * エラーメッセージ
   * エラーメッセージの基本的フォーマット。
   */
  ErrorMessage: (arg0: string): string => `エラーが発生しました: ${arg0}`,
} as const
```

## XML定義例

### 基本的な定数定義

```xml
<MyConstants Type="constant-model" DisplayName="アプリケーション定数">
  <!-- クライアント側のトップページの表示に利用。 -->
  <AppName ConstantType="string" ConstantValue="MyApplication" DisplayName="アプリケーション名" />
  <!-- csprojファイルのバージョン番号と合わせること。 -->
  <Version ConstantType="string" ConstantValue="1.0.0" DisplayName="バージョン" />
  <MaxRetryCount ConstantType="int" ConstantValue="3" DisplayName="最大リトライ回数" />
  <TimeoutSeconds ConstantType="decimal" ConstantValue="30.5" DisplayName="タイムアウト秒数" />
</MyConstants>
```

### ネストされた定数定義

```xml
<!-- アプリケーション設定の定数定義 -->
<MyConstants Type="constant-model" DisplayName="アプリケーション設定">
  <Database ConstantType="child" DisplayName="データベース設定">
    <ConnectionTimeout ConstantType="int" ConstantValue="30" DisplayName="接続タイムアウト" />
    <CommandTimeout ConstantType="int" ConstantValue="60" DisplayName="コマンドタイムアウト" />
  </Database>

  <Messages ConstantType="child" DisplayName="メッセージ">
    <Success ConstantType="string" ConstantValue="正常に処理されました" DisplayName="成功メッセージ" />
    <!-- コロンの後ろには開発者向けの情報ではなくアプリケーションのユーザーに向けた文字にすること。 -->
    <Error ConstantType="template" ConstantValue="エラーが発生しました: {0}" DisplayName="エラーメッセージ" />
  </Messages>

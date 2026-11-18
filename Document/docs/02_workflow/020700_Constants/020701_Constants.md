---
sidebar_label: 定数（Constants）
draft: true
sidebar_position: 1
---

# 定数（Constants）

`Constants` セクションには、C#とJavaScript/TypeScriptで常に値が同期される定数を定義します。

## 概要

`Constants` セクションに定数を定義することで、以下のメリットがあります：

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

値を埋め込んで使う文字列（画面の URL やメッセージなど）は、定数ではなく[テンプレート文字列](./020704_Templates.md)に定義します。

## XML定義例

### 基本的な定数定義

```xml
<MyConstants DisplayName="アプリケーション定数">
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
<MyConstants DisplayName="アプリケーション設定">
  <Database ConstantType="child" DisplayName="データベース設定">
    <ConnectionTimeout ConstantType="int" ConstantValue="30" DisplayName="接続タイムアウト" />
    <CommandTimeout ConstantType="int" ConstantValue="60" DisplayName="コマンドタイムアウト" />
  </Database>

  <Messages ConstantType="child" DisplayName="メッセージ">
    <Success ConstantType="string" ConstantValue="正常に処理されました" DisplayName="成功メッセージ" />
  </Messages>

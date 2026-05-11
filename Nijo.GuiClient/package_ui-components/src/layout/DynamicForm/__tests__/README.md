# DynamicForm の `__tests__` フォルダ

このフォルダにあるテストデータはユニットテストとデバッグ用画面の両方で使われます。

ユニットテストではこのテストデータ構造をimportしてユニットテストを行ないます。
デバッグ用画面ではこのテストデータ構造を使って実際にDynamicFormを表示し人間によるデバッグを行ないます。

## 基本的な形

```ts
import { MemberOwner, ValueMember } from "../types";

/** ここにテストデータの意図や説明 */
export default function (): MemberOwner {

  // 使いまわすUIレンダラーがある場合は事前に定義
  const UI_TEXT = (physicalName: string): Partial<ValueMember> => ({
    // ここにデータ種類定義
  })
  const UI_NUMBER = ...

  return {
    members: [/* ここにデータ構造定義 */],
  }
}
```

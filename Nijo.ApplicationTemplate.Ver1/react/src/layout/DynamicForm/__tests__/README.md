# DynamicForm の `__tests__` フォルダ

このフォルダにあるテストデータはユニットテストとデバッグ用画面の両方で使われます。

ユニットテストではこのテストデータ構造をimportしてユニットテストを行ないます。
デバッグ用画面ではこのテストデータ構造を使って実際にDynamicFormを表示し人間によるデバッグを行ないます。

## 基本的な形

```ts
import { MemberOwner, ValueMemberDefinitionMap } from "../types";

/** ここにテストデータの意図や説明 */
export default function (): [MemberOwner, ValueMemberDefinitionMap] {
  return [{
    members: [/* ここにデータ構造定義 */],
  }, {
    aaa: {/* ここにデータ種類定義 */},
    bbb: {/* ここにデータ種類定義 */},
    ccc: {/* ここにデータ種類定義 */},
  }]
}
```

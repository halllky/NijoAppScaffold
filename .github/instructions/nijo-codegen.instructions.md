---
applyTo: "Nijo/**/*.cs"
description: "Nijo の全コード生成で raw string, SelectTextTemplate, If, WithIndent を強制する"
---

Nijo 配下のコード生成では、生成コードの組み立てに以下の規約を必ず守ること。

- 生成コードは raw string を起点に記述すること。
- コード生成規約は [Nijo/CodeGenerating/TemplateTextHelper.cs](../../Nijo/CodeGenerating/TemplateTextHelper.cs) の XML コメントを正とする。
- ループ
  - `string.Join` を使わないこと。
  - ループにより複数行にわたる垂直方向に反復するコード生成は `SelectTextTemplate` を使うこと。
  - `SelectTextTemplate` は raw string の補間式として使い、selector も raw string を返すこと。
  - selector が返す raw string の終端インデントは、挿入先の位置に揃えること。
- 条件分岐
  - 条件分岐によるコード生成は `If` を使うこと。
  - `If` / `ElseIf` / `Else` は raw string の補間式として同じ挿入位置で連鎖させ、各分岐も raw string を返すこと。
- インデント
  - インデント制御は `WithIndent` を使うこと。
  - 新規コードでは `WithIndent` の 1 引数版を優先し、挿入位置で使うこと。
  - 1 引数版 `WithIndent` の左側には半角スペースだけを置き、内容側は原則としてインデント 0 で組み立てること。
  - TypeScript を生成する raw string では、行頭の空白は単なる見た目ではなく生成後ソースの桁位置を決める意味を持つ。C# 側の見た目を整える目的で先頭空白を増減しないこと。
  - 特に `{{...}}` で始まる補間行、`SelectTextTemplate` の selector が返す raw string、`If` / `ElseIf` / `Else` の各分岐、`WithIndent(...)` の呼び出し行は、先頭空白の列位置を維持すること。
  - `WithIndent` を使っていない TypeScript 生成箇所でも、raw string 内の先頭空白、閉じ側の `"""` の位置、補間式の開始列を不用意に動かさないこと。これらは生成後の TypeScript の 2 スペースインデントに直接影響する。
  - TypeScript 生成コードを編集するときは、「C# のインデント」と「生成される TypeScript のインデント」を別物として扱うこと。C# として自然に見えても、生成後の桁位置が変わる編集はしないこと。
  - 既存行の内容を変えずに整形だけしたく見える場合でも、raw string 内の先頭空白は原則そのまま維持すること。変更が必要な場合は、生成後のソースで何桁増減するかを明示的に判断してから行うこと。
  - 空白や改行の差分を `Trim`、後置換、後連結でごまかさず、テンプレート構造自体を正すこと。

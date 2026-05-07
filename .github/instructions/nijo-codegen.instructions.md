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
  - `SelectTextTemplate` は raw string の左端から開始すること。
- 条件分岐
  - 条件分岐によるコード生成は `If` を使うこと。
  - `If` は raw string の左端から開始すること。
- インデント
  - インデント制御は `WithIndent` を使うこと。
  - `WithIndent` は挿入位置で使い、内容側は原則としてインデント 0 で組み立てること。
  - 空白や改行の差分を `Trim`、後置換、後連結でごまかさず、テンプレート構造自体を正すこと。

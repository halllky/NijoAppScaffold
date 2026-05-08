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
  - 空白や改行の差分を `Trim`、後置換、後連結でごまかさず、テンプレート構造自体を正すこと。

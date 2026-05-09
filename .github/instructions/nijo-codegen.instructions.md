---
applyTo: "Nijo/**/*.cs"
description: "Nijo の全コード生成で raw string, SelectTextTemplate, If, WithIndent を強制する"
---

Nijo 配下のコード生成では、生成コードの組み立てに以下の規約を必ず守ること。

- 生成コードは raw string を起点に記述すること。
- コード生成規約は [Nijo/CodeGenerating/TemplateTextHelper.cs](../../Nijo/CodeGenerating/TemplateTextHelper.cs) の XML コメントを正とする。
- ループと分岐
  - `string.Join` は使わないこと。
  - 垂直方向の反復は `SelectTextTemplate`、条件分岐は `If` / `ElseIf` / `Else` を raw string の補間式として使うこと。
  - `SelectTextTemplate` の selector と各分岐も raw string を返し、終端インデントは挿入先に揃えること。
- インデント
  - インデント制御は `WithIndent` を使い、新規コードでは 1 引数版を優先すること。
  - 1 引数版 `WithIndent` の左側には半角スペースだけを置き、内容側は原則インデント 0 で組み立てること。
  - TypeScript を生成する raw string では、先頭空白・補間開始列・閉じ側の `"""` の位置が生成後ソースの桁位置を決める。C# の見た目を整える目的で動かさないこと。
  - 特に `{{...}}` 行、`SelectTextTemplate` の selector、`If` / `ElseIf` / `Else` の各分岐、`WithIndent(...)` の呼び出し行は列位置を維持すること。
  - TypeScript 生成コードを含むメソッドでは、「C# のインデント」と「生成される TypeScript のインデント」を別物として扱うこと。メソッド全体を機械的に再インデントしないこと。
  - raw string を編集するときは「C# 側のメソッド/if/return」「raw string 本文の先頭空白」「閉じ側の `"""`」を別々に扱うこと。閉じ側の `"""` を動かすと本文全体の意味が変わる。
  - 既存の空白は原則維持すること。変更が必要なら、生成後ソースで何桁増減するかを明示的に判断すること。
  - TypeScript の doc comment は `/**`、`*`、`*/` で必要な空白数が異なる場合があるため、別々に確認すること。
  - インデントが一様に 2 倍または半分に崩れたら、C# レイヤーと TS レイヤーを同時に動かしたとみなし、両者の基準列を分けて復元すること。
  - raw string のインデント修正は目視だけで済ませず、代表行の先頭空白数を確認すること。
- 確認
  - TypeScript を生成する raw string を編集した直後は、少なくとも `CS8999` が出ていないことと、生成後 TypeScript の期待インデントや文字列一致テストが崩れていないことを確認すること。
  - 空白や改行の差分を `Trim`、後置換、後連結でごまかさず、テンプレート構造自体を正すこと。

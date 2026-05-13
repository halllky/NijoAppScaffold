---
applyTo: "Nijo/**/*.cs"
description: "Nijo の全コード生成で raw string, SelectTextTemplate, If, WithIndent を強制する"
---

この指示は Nijo 配下の全 C# に渡されるが、強制対象は Render 系メソッドの中で生成コードを組み立てる箇所だけ。ここでいう Render 系メソッドとは、生成コード文字列を返すメソッドと、その直下のテンプレート組み立て箇所を指す。非 Render 系の通常ロジックには適用しない。

- 正本
  - 詳細規約は [Nijo/CodeGenerating/TemplateTextHelper.cs](../../Nijo/CodeGenerating/TemplateTextHelper.cs) の XML コメントを正とする。
  - 迷ったら既存の Render 実装を踏襲し、新しい流儀を持ち込まないこと。
- 生成方法
  - 生成コードは raw string を起点に記述すること。崩れたからといって `string.Join`、後置換、後連結、`Trim` 逃げしてはならない。
  - 垂直方向の反復は `SelectTextTemplate`、条件分岐は `If` / `ElseIf` / `Else` を raw string の補間式として使うこと。
  - `SelectTextTemplate` の selector と各分岐も raw string を返すこと。
- インデント
  - インデント制御は `WithIndent` を使い、新規コードでは 1 引数版を優先すること。
  - 1 引数版 `WithIndent` の左側には半角スペースだけを置き、内容側は原則インデント 0 で組み立てること。
  - TypeScript を生成する raw string では、生成後ソースの桁位置は「raw string 本文の先頭空白」「補間開始列」「閉じ側の `"""` の列」で決まる。C# の見た目を整える目的で動かさないこと。
  - 特に `{{...}}` 行、`SelectTextTemplate` の selector、`If` / `ElseIf` / `Else` の各分岐、`WithIndent(...)` 呼び出し行は列位置を維持すること。
  - TypeScript 生成コードを含むメソッドでは、C# のインデントと生成される TypeScript のインデントを別物として扱うこと。メソッド全体を機械的に再インデントしないこと。
  - TypeScript を生成する raw string の編集は `apply_patch` で行わず、 `sed` などの行単位の置換コマンドで修正すること。 `apply_patch` に不具合があり、C#の4スペースインデントとTypeScriptの2スペースインデントが混在する箇所で、インデントを崩す誤動作が発生するため。
  - よくある誤りは「TS の 2 スペースを 4 スペースに直す」「raw string 開始以降を丸ごと右へずらして 2 倍にする」「raw string をやめる」の 3 つ。これらはすべて禁止。
  - raw string の空白を直すときは目視で済ませず、代表行の先頭空白数を確認すること。インデントが一様に 2 倍または半分に崩れたら、C# レイヤーと TS レイヤーを分けて復元すること。
- 確認
  - TypeScript を生成する raw string を編集した直後は、少なくとも `CS8999` が出ていないことを確認すること。
  - 可能なら、生成後 TypeScript の期待インデントや文字列一致テストも確認すること。

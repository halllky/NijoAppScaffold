# コード生成エンジン
このフォルダには、入力データの形式に依存しないソースコード自動生成の仕組みが含まれています。
何を入力とし、どのようなソースコードを生成するかは利用側が定義します。

## 主な責務
- ディレクトリ構造の入れ子に沿ったファイルの出力
- 生成処理のあちこちから内容が登録される共有ソースファイルの管理
- 生成されなかった既存ファイルの自動削除

## 主な型
- **`CodeRenderingContext<TSelf>`**: コード生成のコンテキストの基底クラス。利用側はこれを継承し、生成処理が参照する情報を定義する
- **`ISharedSourceFile<TContext>`**: 生成処理のあちこちから内容が登録され、最後に1回だけレンダリングされるソースファイル
- **`DirectorySetupper`**: 生成先のディレクトリ構造を直感的に記述するためのクラス
- **`SourceFile`**: 生成される個別ソースファイル
- **`TemplateTextHelper`**: 条件分岐や反復処理を含むテンプレート文字列の記述を支援

## 使い方

```cs
class MyContext : CodeRenderingContext<MyContext> {
    public MyContext(string outDir, MyInput input) : base([outDir]) {
        OutDir = outDir;
        Input = input;
    }
    public string OutDir { get; }
    public MyInput Input { get; }
}

var ctx = new MyContext(outDir, input);
ctx.Run(define: ctx => {
    ctx.Directory(ctx.OutDir, dir => {
        foreach (var item in ctx.Input.Items) {
            dir.Generate(new SourceFile { FileName = $"{item.Name}.cs", Contents = item.Render() });
            ctx.Use<MyIndexFile>().Add(item);
        }
    });
});
```

## C#の生テキストリテラル（Raw String Literal）文法の支援
- **`TemplateTextHelper`**: 条件分岐や反復処理を含むテンプレート生成を支援

### 分岐処理の記法
何らかの条件に基づいて特定のソースをレンダリングするか否かが分かれる場合は [TemplateTextHelper](./TemplateTextHelper.cs) で定義されている `If()` , `.ElseIf()` , `.Else()` を使用してください。
また、それらのソースは、C#のテンプレートリテラルの構文の末尾の `"""` の位置とインデントを合わせて下さい。

OK

```cs
string RenderXXX() {
    return $$"""
        // ここに何らかのソースをレンダリングする
        {{If(/* 条件A */, () => $$"""
            // 条件Aを満たした場合のみレンダリングされるソース
        """).ElseIf(/* 条件B */, () => $$"""
            // 条件Bを満たした場合のみレンダリングされるソース
        """).Else(() => $$"""
            // 条件A, B いずれも満たさない場合のみレンダリングされるソース
        """)}}
        """;
}
```

NG

```cs
string RenderXXX() {
    return $$"""
        // ここに何らかのソースをレンダリングする
            {{If(/* 条件A */, () => $$"""
            // 条件Aを満たした場合のみレンダリングされるソース
            """).ElseIf(/* 条件B */, () => $$"""
            // 条件Bを満たした場合のみレンダリングされるソース
            """).Else(() => $$"""
            // 条件A, B いずれも満たさない場合のみレンダリングされるソース
            """)}}
        """;
}
```

### 反復処理の記法
何らかの条件に基づいて特定のソースを反復処理する場合は `.SelectTextTemplate` メソッドを使ってください。
また、それらのソースは、C#のテンプレートリテラルの構文の末尾の `"""` の位置とインデントを合わせて下さい。

OK

```cs
string RenderXXX() {
    var array = Enumerable.Range(0, 4).Select(_ => "あ");
    return $$"""
        // ここに何らかのソースをレンダリングする
        {{array.SelectTextTemplate((_, i) => $$"""
            // {{i + 1}}番目の要素です。
        """)}}
        """;
}
```

NG

```cs
string RenderXXX() {
    var array = Enumerable.Range(0, 4).Select(_ => "あ");
    return $$"""
        // ここに何らかのソースをレンダリングする
            {{array.SelectTextTemplate((_, i) => $$"""
                // {{i + 1}}番目の要素です。
            """)}}
        """;
}
```

### ソースコードレンダリング処理の入れ子の記法
ソースコードレンダリング処理の中で他のソースコードレンダリング処理を呼び出す際は、呼び出す側で `WithIndent` を用いてインデントを合わせます。
`WithIndent` の第2引数には、その行のインデントのサイズと等しい数の半角スペースを渡してください。

OK

```cs
string RenderXXX(object root) {
    return $$"""
        {{GetSomeChildren(root).SelectTextTemplate((x, i) => $$"""
          {{WithIndent(RenderTypeScriptObjectTypeRecursively(x), "  ")}}
        """)}}
        """;

    string RenderTypeScriptObjectTypeRecursively(object obj) {
        return $$"""
            {
            {{GetSomeChildren(obj).SelectTextTemplate(x => $$"""
              {{WithIndent(RenderTypeScriptObjectTypeRecursively(x), "  ")}}
            """)}}
            }
            """;
    }
}
```

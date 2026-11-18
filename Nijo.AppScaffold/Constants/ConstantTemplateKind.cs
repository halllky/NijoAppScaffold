using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.Constants;

/// <summary>
/// テンプレート文字列の種類ごとの、引数を埋め込む前の変換。
/// 変換の中身はアプリごとに決めるので自動生成せず、自動生成されないコードに書いてもらう。
/// C# は自動生成されるプロジェクトの直下（自動生成されるフォルダの外）、TypeScript は React プロジェクトの src フォルダに書く。
/// ここで生成するのは、nijo.xml に指定された種類の一覧から作る、その変換の宣言だけ。
/// 宣言に対応する変換が書かれていなければ、C# も TypeScript もコンパイルエラーになる。
/// </summary>
internal static class ConstantTemplateKind {
    /// <summary>C# のクラス名と、TypeScript のオブジェクト名</summary>
    internal const string CLASS_NAME = "ConstantTemplateKind";
    /// <summary>自動生成される TypeScript のファイル名。テンプレート文字列の定数はここから変換を import する</summary>
    internal const string TS_FILENAME = "constant-template-kind.ts";
    /// <summary>変換の中身を書く TypeScript のモジュール。React プロジェクトの src フォルダからの相対パス</summary>
    private const string TS_IMPLEMENTATION_MODULE = "ConstantTemplateKind";

    /// <summary>
    /// 種類ごとの変換の宣言をレンダリングします。種類が1個も無ければ何もしません。
    /// </summary>
    internal static void Render(CodeRenderingContext ctx, IEnumerable<string> kinds) {
        var sortedKinds = kinds.Distinct().Order().ToArray();
        if (sortedKinds.Length == 0) return;

        ctx.CoreLibrary(dir => {
            dir.Generate(new SourceFile {
                FileName = $"{CLASS_NAME}.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// テンプレート文字列の定数の種類ごとの、引数を埋め込む前の変換。
                    /// 種類は nijo.xml の {{TemplateKindOption.ATTRIBUTE_NAME}} 属性で指定したもの。
                    /// 変換の中身は、このプロジェクトの直下（自動生成されるフォルダの外）に、このクラスの partial として書くこと。
                    /// 自動生成されるコードから呼ぶため、同じプロジェクトに置く必要がある。
                    /// </summary>
                    public static partial class {{CLASS_NAME}} {
                    {{sortedKinds.SelectTextTemplate(kind => $$"""
                        /// <summary>種類「{{kind}}」の引数を埋め込む前の変換</summary>
                        public static partial string {{kind}}(string value);
                    """)}}
                    }
                    """,
            });
        });

        ctx.ReactProject(dir => {
            dir.Generate(new SourceFile {
                FileName = TS_FILENAME,
                Contents = $$"""
                    import { {{CLASS_NAME}} as implementation } from "../{{TS_IMPLEMENTATION_MODULE}}"

                    /**
                     * テンプレート文字列の定数の種類ごとの、引数を埋め込む前の変換。
                     * 種類は nijo.xml の {{TemplateKindOption.ATTRIBUTE_NAME}} 属性で指定したもの。
                     * 変換の中身は、src/{{TS_IMPLEMENTATION_MODULE}}.ts に、同じ名前のオブジェクトとして書くこと。
                     */
                    export const {{CLASS_NAME}}: {
                    {{sortedKinds.SelectTextTemplate(kind => $$"""
                      /** 種類「{{kind}}」の引数を埋め込む前の変換 */
                      {{kind}}: (value: string) => string
                    """)}}
                    } = implementation
                    """,
            });
        });
    }
}

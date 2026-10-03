using Nijo.CodeGenerating;
using static Nijo.CodeGenerating.TemplateTextHelper;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class WithIndentのテスト {

    [Test]
    public void 文字列版WithIndentのネストしたインデントをRender時に解決できる() {

        var consoleWriteLine = $$"""
            Console.WriteLine("A");
            Console.WriteLine("B");
            """;

        var ifTrue = $$"""
            if (true) {
                {{WithIndent(consoleWriteLine)}}
            }
            """;

        var lines = RenderLines($$"""
            class Example {
                {{WithIndent(ifTrue)}}
            }
            """);

        Assert.That(lines[^6..], Is.EqualTo(new[] {
            "class Example {",
            "    if (true) {",
            "        Console.WriteLine(\"A\");",
            "        Console.WriteLine(\"B\");",
            "    }",
            "}",
        }));
    }

    [Test]
    public void IEnumerable版WithIndentで複数要素を同じ幅でインデントできる() {
        var doBeta = $$"""
            Beta();
            """;

        var doSomethingMany = new List<string> {
            $$"""
                Alpha();
                """,
            $$"""
                if (true) {
                    {{WithIndent(doBeta)}}
                }
                """,
            $$"""
                Gamma();
                """,
        };

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(doSomethingMany)}}
            }
            """);

        Assert.That(lines[^7..], Is.EqualTo(new[] {
            "void Example() {",
            "    Alpha();",
            "    if (true) {",
            "        Beta();",
            "    }",
            "    Gamma();",
            "}",
        }));
    }

    [Test]
    public void WithIndent内の先頭行がスキップ対象でも後続行のインデントが壊れない() {
        var optionalHeader = Enumerable.Empty<int>().SelectTextTemplate(_ => $$"""
            Header();
            """);

        var block = $$"""
            {{optionalHeader}}
            Body();
            """;

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(block)}}
            }
            """);

        Assert.That(lines[^3..], Is.EqualTo(new[] {
            "void Example() {",
            "    Body();",
            "}",
        }));
    }

    [Test]
    public void SKIP_MARKERを含まない通常の空行は保持される() {
        var lines = RenderLines($$"""
            class Example {

                void Method() {
                }
            }
            """);

        Assert.That(lines[^5..], Is.EqualTo(new[] {
            "class Example {",
            string.Empty,
            "    void Method() {",
            "    }",
            "}",
        }));
    }

    [Test]
    public void SelectTextTemplateでスペースなしWithIndentを使っても出力に影響を与えない() {

        var jsdoc = $$"""
            /**
            {{If(true, () => $$"""
             * This is a JSDoc comment.
            """)}}
             * It should be indented correctly.
             */
            """;

        // 左側スペースなしで WithIndent を使っているので出力には影響を与えないはず
        var jsdocWithIndent = WithIndent(jsdoc);

        var actual = $$"""
            {{Enumerable.Range(0, 1).SelectTextTemplate(_ => $$"""
                {{WithIndent(jsdocWithIndent)}}
            """)}}
            """;

        Assert.That(RenderLines(actual)[^4..], Is.EqualTo(new[] {
            "    /**",
            "     * This is a JSDoc comment.",
            "     * It should be indented correctly.",
            "     */",
        }));
    }

    [Test]
    public void インデント文字列指定版WithIndentの内側で文字列版WithIndentを使ってもインデントが重複しない() {
        var grandChild = $$"""
            X: '',
            Y: '',
            """;

        var child = $$"""
            A: '',
            B: {
              {{WithIndent(grandChild)}}
            },
            """;

        var body = $$"""
            {
              {{WithIndent(child)}}
            }
            """;

        var lines = RenderLines($$"""
            function f() {
              return {{WithIndent(body, "  ")}}
            }
            """);

        Assert.That(lines[^9..], Is.EqualTo(new[] {
            "function f() {",
            "  return {",
            "    A: '',",
            "    B: {",
            "      X: '',",
            "      Y: '',",
            "    },",
            "  }",
            "}",
        }));
    }

    [Test]
    public void IEnumerable版インデント文字列指定版WithIndentで複数要素を同じ幅でインデントできる() {
        var items = new[] {
            $$"""
                [
                  1,
                ]
                """,
            $$"""
                [
                  2,
                ]
                """,
        };

        var lines = RenderLines($$"""
            const x = {{WithIndent(items, "          ")}}
            """);

        Assert.That(lines[^6..], Is.EqualTo(new[] {
            "const x = [",
            "            1,",
            "          ]",
            "          [",
            "            2,",
            "          ]",
        }));
    }

    [Test]
    public void 同じ行で開始されるWithIndentが重なってもインデントが重複しない() {
        var items = new[] {
            WithIndent($$"""
                A1();
                A2();
                """),
            WithIndent($$"""
                B1();
                B2();
                """),
        };

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(items)}}
            }
            """);

        Assert.That(lines[^6..], Is.EqualTo(new[] {
            "void Example() {",
            "    A1();",
            "    A2();",
            "    B1();",
            "    B2();",
            "}",
        }));
    }

    private static string[] RenderLines(string contents) {
        var filePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.cs");
        try {
            new SourceFile {
                FileName = Path.GetFileName(filePath),
                Contents = contents,
            }.Render(filePath);

            return File.ReadAllLines(filePath);
        } finally {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }
    }
}

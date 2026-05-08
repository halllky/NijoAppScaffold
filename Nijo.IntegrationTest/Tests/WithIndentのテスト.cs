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

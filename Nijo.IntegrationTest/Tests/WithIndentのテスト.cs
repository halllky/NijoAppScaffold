using Nijo.CodeGenerating;
using Nijo.SchemaParsing;
using static Nijo.CodeGenerating.TemplateTextHelper;
using NUnit.Framework;
using System.Xml.Linq;

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
    public void IEnumerable版WithIndentの重ね掛けで内側ブロックの深さを維持できる() {
        var childBlocks = new List<string> {
            $$"""
                Beta();
                """,
            $$"""
                Gamma();
                """,
        };

        var parentBlocks = new List<string> {
            $$"""
                if (true) {
                    {{WithIndent(childBlocks)}}
                }
                """,
        };

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(parentBlocks)}}
            }
            """);

        Assert.That(lines[^6..], Is.EqualTo(new[] {
            "void Example() {",
            "    if (true) {",
            "        Beta();",
            "        Gamma();",
            "    }",
            "}",
        }));
    }

    [Test]
    public void IEnumerable版WithIndentと文字列版WithIndentを多重併用しても余計なインデントが増えない() {
        var leaf = $$"""
            cmd.CommandText = "SELECT";
            entity.Id = 1;
            """;

        var nestedStringBlock = $$"""
            if (entity.Id == null) {
                {{WithIndent(leaf)}}
            }
            """;

        var blocks = new List<string> {
            $$"""
                // ID
                """,
            nestedStringBlock,
        };

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(blocks)}}
            }
            """);

        Assert.That(lines[^7..], Is.EqualTo(new[] {
            "void Example() {",
            "    // ID",
            "    if (entity.Id == null) {",
            "        cmd.CommandText = \"SELECT\";",
            "        entity.Id = 1;",
            "    }",
            "}",
        }));
    }

    [Test]
    public void 生成器と同型のraw_string要素をIEnumerable版WithIndentに渡しても余計なインデントが増えない() {
        var blocks = new List<string> {
            $$"""
            // 内部キー
            """,
            $$"""
            if (entity.内部キー == null) {
                cmd.CommandText = $"SELECT \"APP_KIND_SEQ\".nextval FROM DUAL";
                entity.内部キー = Convert.ToInt32(cmd.ExecuteScalar())!;
            }
            """,
        };

        var lines = RenderLines($$"""
            void Example() {
                {{WithIndent(blocks)}}
            }
            """);

        Assert.That(lines[^7..], Is.EqualTo(new[] {
            "void Example() {",
            "    // 内部キー",
            "    if (entity.内部キー == null) {",
            "        cmd.CommandText = $\"SELECT \\\"APP_KIND_SEQ\\\".nextval FROM DUAL\";",
            "        entity.内部キー = Convert.ToInt32(cmd.ExecuteScalar())!;",
            "    }",
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

    private static string[] RenderLines(string contents) {
        var projectRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var filePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.cs");
        try {
            Directory.CreateDirectory(projectRoot);
            File.WriteAllText(Path.Combine(projectRoot, "nijo.xml"), $$"""
                <NijoAppScaffold SuppressAutoGeneratedComment="true" />
                """);

            if (!GeneratedProject.TryOpen(projectRoot, out var project, out var error) || project == null) {
                throw new AssertionException(error ?? "GeneratedProject.TryOpen に失敗しました。");
            }

            var document = XDocument.Load(project.SchemaXmlPath);
            var parseContext = new SchemaParseContext(document, SchemaParseRule.Default());
            Assert.That(parseContext.TryBuildSchema(document, out var immutableSchema, out var errors), Is.True, () => string.Join(Environment.NewLine, errors.Select(e => e.ToString())));

            using var ctx = new CodeRenderingContext(
                project,
                project.GetConfig(),
                new CodeRenderingOptions { AllowNotImplemented = false },
                parseContext,
                immutableSchema!);

            new SourceFile {
                FileName = Path.GetFileName(filePath),
                Contents = contents,
            }.Render(filePath);

            return File.ReadAllLines(filePath);
        } finally {
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
            if (Directory.Exists(projectRoot)) {
                Directory.Delete(projectRoot, recursive: true);
            }
        }
    }
}

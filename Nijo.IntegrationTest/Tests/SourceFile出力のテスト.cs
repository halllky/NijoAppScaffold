using Nijo.CodeGenerator;
using NUnit.Framework;
using System.Text;

namespace Nijo.IntegrationTest.Tests;

public class SourceFile出力のテスト {

    [Test]
    public void ファイルが存在しない場合は新規作成される() {
        using var dir = new TempDirectory();
        var filePath = dir.Combine("a.txt");

        Render(filePath, "あいう");

        Assert.That(File.ReadAllText(filePath), Is.EqualTo("あいう\n"));
    }

    [Test]
    public void 内容に変更がない場合はファイルが更新されない() {
        using var dir = new TempDirectory();
        var filePath = dir.Combine("a.txt");
        Render(filePath, "あいう");
        var lastWriteTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(filePath, lastWriteTime);

        Render(filePath, "あいう");

        Assert.That(File.GetLastWriteTimeUtc(filePath), Is.EqualTo(lastWriteTime));
    }

    [TestCase("あいう", "あいえ", Description = "長さ同じ")]
    [TestCase("あいう", "あいうえお", Description = "長くなる")]
    [TestCase("あいうえお", "あい", Description = "短くなる")]
    [TestCase("あいう", "", Description = "空になる")]
    public void 内容に変更がある場合はファイルが更新される(string before, string after) {
        using var dir = new TempDirectory();
        var filePath = dir.Combine("a.txt");
        Render(filePath, before);

        Render(filePath, after);

        Assert.That(File.ReadAllText(filePath), Is.EqualTo(after + "\n"));
    }

    [Test]
    public void 比較用バッファより大きなファイルでも差異を検出できる() {
        using var dir = new TempDirectory();
        var filePath = dir.Combine("a.txt");
        var before = string.Join("\n", Enumerable.Range(0, 100_000).Select(i => $"line{i}"));
        var after = before.Replace("line99999", "LINE99999");
        Render(filePath, before);

        Render(filePath, after);

        Assert.That(File.ReadAllText(filePath), Is.EqualTo(after + "\n"));
    }

    private static void Render(string filePath, string contents) {
        new SourceFile {
            FileName = Path.GetFileName(filePath),
            Contents = contents,
        }.Render(filePath, new SourceFileFormat { Encoding = new UTF8Encoding(false), NewLine = "\n" });
    }

    private sealed class TempDirectory : IDisposable {
        private readonly string _path = Directory.CreateTempSubdirectory().FullName;
        public string Combine(string fileName) => Path.Combine(_path, fileName);
        public void Dispose() => Directory.Delete(_path, recursive: true);
    }
}

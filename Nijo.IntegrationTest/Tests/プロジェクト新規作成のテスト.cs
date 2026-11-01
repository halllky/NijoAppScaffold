using System.IO.Compression;
using System.Text;
using Nijo.WebService;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

/// <summary>
/// プロジェクトの新規作成で、既存のファイルを壊さないことと、指定のフォルダの外に作らないことを確認する。
/// </summary>
public class プロジェクト新規作成のテスト {

    private const string TEMPLATE_NAME = "empty-template";

    [Test]
    public void テンプレートのzipを存在しないフォルダに展開する() {
        var projectRoot = GetUniquePath();

        var (success, error) = GeneratedProject.ExtractTemplateArchive(CreateZip("nijo.xml", "Core/App.cs"), projectRoot);

        Assert.That(success, Is.True, error);
        Assert.That(File.Exists(Path.Combine(projectRoot, "nijo.xml")), Is.True);
        Assert.That(File.Exists(Path.Combine(projectRoot, "Core", "App.cs")), Is.True);
    }

    [Test]
    public void nijo_xmlを含まないテンプレートは何も書き込まずに失敗する() {
        var projectRoot = GetUniquePath();

        var (success, error) = GeneratedProject.ExtractTemplateArchive(CreateZip("DEBUG.sh"), projectRoot);

        Assert.That(success, Is.False);
        Assert.That(error, Does.Contain("nijo.xml"));
        Assert.That(Path.Exists(projectRoot), Is.False);
    }

    [Test]
    public void 既に存在するフォルダには作成しない() {
        var projectRoot = GetUniquePath();
        Directory.CreateDirectory(projectRoot);
        File.WriteAllText(Path.Combine(projectRoot, "memo.txt"), "mine");

        var (success, _) = GeneratedProject.CreateProjectFromTemplate(projectRoot, TEMPLATE_NAME);

        Assert.That(success, Is.False);
        Assert.That(Directory.GetFileSystemEntries(projectRoot), Has.Length.EqualTo(1));
        Assert.That(File.ReadAllText(Path.Combine(projectRoot, "memo.txt")), Is.EqualTo("mine"));
    }

    [Test]
    public void 既に存在するファイルと同じパスには作成しない() {
        var projectRoot = GetUniquePath();
        File.WriteAllText(projectRoot, "mine");

        var (success, _) = GeneratedProject.CreateProjectFromTemplate(projectRoot, TEMPLATE_NAME);

        Assert.That(success, Is.False);
        Assert.That(File.ReadAllText(projectRoot), Is.EqualTo("mine"));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(".")]
    [TestCase("..")]
    [TestCase("../outside")]
    [TestCase("a/b")]
    [TestCase("a\\b")]
    [TestCase(" name")]
    [TestCase("name ")]
    [TestCase("name.")]
    public void フォルダ名として不正なプロジェクト名では何も作成しない(string projectName) {
        var parent = GetUniquePath();
        var serveRoot = Path.Combine(parent, "serve");
        Directory.CreateDirectory(serveRoot);
        using var webService = new NijoWebService(serveRoot);

        var (success, error) = webService.CreateProject(projectName, TEMPLATE_NAME);

        Assert.That(success, Is.False);
        Assert.That(error, Does.Contain("プロジェクト名"));
        Assert.That(webService.ProjectRoot, Is.EqualTo(serveRoot));
        Assert.That(Directory.GetFileSystemEntries(serveRoot), Is.Empty);
        Assert.That(Directory.GetFileSystemEntries(parent), Has.Length.EqualTo(1));
    }

    [Test]
    public void 編集対象フォルダが既にプロジェクトなら直下に作成しない() {
        var serveRoot = GetUniquePath();
        Directory.CreateDirectory(serveRoot);
        File.WriteAllText(Path.Combine(serveRoot, "nijo.xml"), "<root />");
        using var webService = new NijoWebService(serveRoot);

        var (success, _) = webService.CreateProject("child", TEMPLATE_NAME);

        Assert.That(success, Is.False);
        Assert.That(webService.ProjectRoot, Is.EqualTo(serveRoot));
        Assert.That(Path.Exists(Path.Combine(serveRoot, "child")), Is.False);
    }

    [Test]
    public void 作成に失敗したら編集対象は変わらない() {
        var serveRoot = GetUniquePath();
        Directory.CreateDirectory(Path.Combine(serveRoot, "child"));
        using var webService = new NijoWebService(serveRoot);

        var (success, _) = webService.CreateProject("child", TEMPLATE_NAME);

        Assert.That(success, Is.False);
        Assert.That(webService.ProjectRoot, Is.EqualTo(serveRoot));
        Assert.That(webService.Preview.ProjectRoot, Is.EqualTo(serveRoot));
    }

    [Test]
    public void 作成に成功したら直下のプロジェクトが編集対象になる() {
        // テンプレートのzipはリリース手順で作られるので、埋め込まれていないビルドでは確認できない
        Assume.That(typeof(GeneratedProject).Assembly.GetManifestResourceInfo($"{TEMPLATE_NAME}.zip"), Is.Not.Null);

        var serveRoot = GetUniquePath();
        using var webService = new NijoWebService(serveRoot);

        var (success, error) = webService.CreateProject("child", TEMPLATE_NAME);

        var newProjectRoot = Path.Combine(serveRoot, "child");
        Assert.That(success, Is.True, error);
        Assert.That(File.Exists(Path.Combine(newProjectRoot, "nijo.xml")), Is.True);
        Assert.That(File.Exists(Path.Combine(serveRoot, "nijo.xml")), Is.False);
        Assert.That(webService.ProjectRoot, Is.EqualTo(newProjectRoot));
        Assert.That(webService.Preview.ProjectRoot, Is.EqualTo(newProjectRoot));
    }

    /// <summary>テストは並列に実行されるので、テストごとに別のパスを使う</summary>
    private static string GetUniquePath() {
        return Path.Combine(NijoTestUtil.BaseDirectory, $"{nameof(プロジェクト新規作成のテスト)}_{Guid.NewGuid():N}");
    }

    private static MemoryStream CreateZip(params string[] entryNames) {
        var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true)) {
            foreach (var name in entryNames) {
                using var writer = new StreamWriter(archive.CreateEntry(name).Open(), new UTF8Encoding(false));
                writer.Write("template");
            }
        }
        stream.Position = 0;
        return stream;
    }
}

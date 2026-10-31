using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

/// <summary>
/// <para>
/// `nijo new` で展開される新規プロジェクトテンプレートが、実際に使い始められる状態であることを確認する。
/// 自動生成をかけなおしたうえで、ビルド・単体テスト・型チェックを実行する。
/// </para>
/// <para>
/// demo101-template は /demo/101_販売管理システム から /Task/デモ101テンプレート作成.sh で作られるので、
/// デモ101への機能追加によって不具合が出やすい。デモ101側の構造が変わり、スクリプトの更新が必要になった場合は
/// このテストが失敗するので、/Task/Demo101TemplateBuilder/Demo101TemplatePruner.cs を追随させること。
/// </para>
/// </summary>
public class 新規プロジェクトテンプレートのテスト {

    [Test]
    [Category("時間がかかる")]
    public async Task 空テンプレートがビルドとテストを通過するか() {
        var repoRoot = FindRepoRoot();
        var logger = new ConsoleAndContextLogger();
        var zipPath = Path.Combine(NijoTestUtil.BaseDirectory, $"empty-template_{Guid.NewGuid():N}.zip");

        // リリース時と同じ方法で zip を作る（/Task/Nijoリリース.sh）
        var archived = await RunCommandAsync("git", $"archive HEAD:Nijo.NewProjectTemplate --format=zip -o \"{zipPath}\"", repoRoot, logger);
        Assert.That(archived, Is.True, "空テンプレートの git archive に失敗しました。");

        try {
            await AssertTemplateIsUsableAsync(zipPath, logger);
        } finally {
            File.Delete(zipPath);
        }
    }

    [Test]
    [Category("時間がかかる")]
    public async Task デモ101テンプレートがビルドとテストを通過するか() {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "Task", "デモ101テンプレート作成.sh");
        Assert.That(File.Exists(scriptPath), Is.True, $"シェルスクリプトが見つかりません: {scriptPath}");

        var logger = new ConsoleAndContextLogger();

        var scriptResult = await RunCommandAsync("bash", $"\"{scriptPath}\"", repoRoot, logger);
        Assert.That(scriptResult, Is.True, "デモ101テンプレート作成.sh の実行に失敗しました。");

        var zipPath = Path.Combine(repoRoot, "temp_release", "demo101-template.zip");
        Assert.That(File.Exists(zipPath), Is.True, $"テンプレートzipが生成されていません: {zipPath}");

        try {
            AssertNoDemoSpecificTerms(zipPath);
            await AssertTemplateIsUsableAsync(zipPath, logger);
        } finally {
            try { Directory.Delete(Path.Combine(repoRoot, "temp_release", "demo101-template-work"), recursive: true); } catch { /* 存在しなければ無視 */ }
            File.Delete(zipPath);
        }
    }

    /// <summary>
    /// デモ101テンプレートに販売管理業務やデモ101固有の語が残っていないことを確認する。
    /// Pruner がコンパイルエラーなしに消し忘れたものを検知するためのもの。
    /// 文書（*.md）はデモ101との関係の説明や例示で販売管理に触れるので対象外とする。
    /// </summary>
    private static void AssertNoDemoSpecificTerms(string zipPath) {
        var forbidden = new Regex("売上|入荷|在庫|商品|販売管理|Demo101|デモ101");
        var found = new List<string>();

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries) {
            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith(".md")) continue;
            if (forbidden.IsMatch(entry.FullName.Normalize())) found.Add(entry.FullName);

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            var lineNumber = 0;
            while (reader.ReadLine() is { } line) {
                lineNumber++;
                if (forbidden.IsMatch(line)) found.Add($"{entry.FullName}:{lineNumber}: {line.Trim()}");
            }
        }

        Assert.That(found, Is.Empty, "デモ101テンプレートに固有の語が残っています。");
    }

    /// <summary>
    /// テンプレートのzipを展開し、自動生成・ビルド・単体テスト・型チェックが通ることを確認する。
    /// テストが失敗した場合は調査できるよう展開先を残す。
    /// </summary>
    private static async Task AssertTemplateIsUsableAsync(string zipPath, ConsoleAndContextLogger logger) {
        var extractedRoot = Path.Combine(NijoTestUtil.BaseDirectory, $"{nameof(新規プロジェクトテンプレートのテスト)}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(extractedRoot);
        using (var archive = ZipFile.OpenRead(zipPath)) {
            archive.ExtractToDirectory(extractedRoot);
        }

        try {
            // 自動生成
            Assert.That(GeneratedProject.TryOpen(extractedRoot, out var project, out var openError), Is.True, openError);

            var xDocument = System.Xml.Linq.XDocument.Load(project!.SchemaXmlPath);
            var rule = SchemaParsing.SchemaParseRule.Default();
            var parseContext = new SchemaParsing.SchemaParseContext(xDocument, rule, SchemaParsing.GeneratedProjectOptions.Parse(xDocument, true));
            var renderingOptions = new CodeGenerating.CodeRenderingOptions { AllowNotImplemented = false };

            Assert.That(project.ValidateSchema(parseContext, logger), Is.True, "テンプレートのnijo.xmlにスキーマ定義エラーがあります。");
            Assert.That(project.GenerateCode(parseContext, renderingOptions, logger, out _), Is.True, "テンプレートに対するソースコード自動生成に失敗しました。");

            // ビルド
            var slnPath = Directory.GetFiles(extractedRoot, "*.sln").SingleOrDefault();
            Assert.That(slnPath, Is.Not.Null, "テンプレート直下に .sln ファイルが見つかりません。");

            var dotnetBuildOk = await RunCommandAsync("dotnet", $"build \"{slnPath}\"", extractedRoot, logger);
            Assert.That(dotnetBuildOk, Is.True, "テンプレートに対する dotnet build に失敗しました。");

            // リリースされるプロジェクトは Release でもビルドできること
            var releaseBuildOk = await RunCommandAsync("dotnet", "build WebApi -c Release", extractedRoot, logger);
            Assert.That(releaseBuildOk, Is.True, "テンプレートの WebApi に対する Release ビルドに失敗しました。");

            // 単体テスト
            var dotnetTestOk = await RunCommandAsync("dotnet", "test UnitTest --no-build", extractedRoot, logger);
            Assert.That(dotnetTestOk, Is.True, "テンプレートの単体テストに失敗しました。");

            // 型チェック
            var clientDir = Path.Combine(extractedRoot, "client");
            Assert.That(Directory.Exists(clientDir), Is.True, $"client フォルダが見つかりません: {clientDir}");

            var npmInstallOk = await RunCommandAsync("npm", "install", clientDir, logger);
            Assert.That(npmInstallOk, Is.True, "テンプレートの client に対する npm install に失敗しました。");

            var npmTscOk = await RunCommandAsync("npm", "run tsc", clientDir, logger);
            Assert.That(npmTscOk, Is.True, "テンプレートの client に対する npm run tsc に失敗しました。");

        } finally {
            if (TestContext.CurrentContext.Result.Outcome.Status != NUnit.Framework.Interfaces.TestStatus.Failed) {
                try { Directory.Delete(extractedRoot, recursive: true); } catch { /* ベストエフォート */ }
            }
        }
    }

    private static string FindRepoRoot() {
        var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "nijo.sln"))) {
            dir = dir.Parent;
        }
        if (dir == null) throw new InvalidOperationException("リポジトリルート（nijo.sln）が見つかりませんでした。");
        return dir.FullName;
    }

    private static async Task<bool> RunCommandAsync(string fileName, string arguments, string workingDirectory, ConsoleAndContextLogger logger) {
        var psi = new ProcessStartInfo {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        // ビルド後も常駐するプロセスが標準出力を掴んだままだと、コマンドが終わっても出力の読み取りが終わらなくなる
        psi.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        psi.Environment["UseSharedCompilation"] = "false";

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"プロセスを開始できませんでした: {fileName} {arguments}");
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        logger.LogInfo($"COMMAND: {fileName} {arguments} (cwd={workingDirectory})\nEXIT_CODE: {process.ExitCode}\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");

        return process.ExitCode == 0;
    }

    /// <summary>
    /// <see cref="GeneratedProject.ValidateSchema"/> / <see cref="GeneratedProject.GenerateCode"/> が要求する
    /// <see cref="Microsoft.Extensions.Logging.ILogger"/> の簡易実装。
    /// </summary>
    private class ConsoleAndContextLogger : Microsoft.Extensions.Logging.ILogger {
        public void LogInfo(string message) => TestContext.Progress.WriteLine(message);

        IDisposable? Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) => null;
        bool Microsoft.Extensions.Logging.ILogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        void Microsoft.Extensions.Logging.ILogger.Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) {
            TestContext.Progress.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception != null) TestContext.Progress.WriteLine(exception.ToString());
        }
    }
}

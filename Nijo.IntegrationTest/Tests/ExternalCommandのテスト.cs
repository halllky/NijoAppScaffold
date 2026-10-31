using System.Diagnostics;
using NUnit.Framework;

namespace Nijo.IntegrationTest.Tests;

public class ExternalCommandのテスト {

    [Test]
    public async Task 終了コードと標準出力と標準エラーを取得できる() {
        var result = await RunBashAsync("echo out; echo err >&2; exit 3");

        Assert.Multiple(() => {
            Assert.That(result.ExitCode, Is.EqualTo(3));
            Assert.That(result.Stdout, Is.EqualTo("out"));
            Assert.That(result.Stderr, Is.EqualTo("err"));
        });
    }

    /// <summary>
    /// MSBuild ワーカーのような常駐プロセスがパイプを保持していても、
    /// そのプロセスが終了するまで待たされないことを確認する。
    /// </summary>
    [Test]
    public async Task パイプを保持する子プロセスが残っていても待ち続けない() {
        var stopwatch = Stopwatch.StartNew();

        var result = await RunBashAsync("echo out; sleep 30 &");

        Assert.Multiple(() => {
            Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(20)));
            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Stdout, Is.EqualTo("out"));
        });
    }

    /// <summary>
    /// dotnet test から実行したときに MSBuild ワーカーがパイプを保持する原因となる設定を、
    /// 子プロセスに引き継がないことを確認する。
    /// </summary>
    [Test]
    public async Task MSBuildワーカーが常駐しない環境変数で実行される() {
        var result = await RunBashAsync("""echo "[$MSBUILDENSURESTDOUTFORTASKPROCESSES][$MSBUILDDISABLENODEREUSE]" """);

        Assert.That(result.Stdout, Is.EqualTo("[][1]"));
    }

    [Test]
    public void 時間内に終了しなければ例外になる() {
        var stopwatch = Stopwatch.StartNew();

        Assert.ThrowsAsync<TimeoutException>(() => RunBashAsync("sleep 300", TimeSpan.FromSeconds(1)));

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(20)));
    }

    private static Task<ExternalCommand.Result> RunBashAsync(string script, TimeSpan? timeout = null) {
        return ExternalCommand.RunAsync("bash", $"-c \"{script.Replace("\"", "\\\"")}\"", NijoTestUtil.BaseDirectory, timeout);
    }
}

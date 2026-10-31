using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Nijo.IntegrationTest;

/// <summary>
/// テストから実行する外部コマンド（dotnet, npm, bash など）。
/// 実行したプロセス自身の終了までを待機する。
/// 正常終了したプロセスが残した孫プロセスの終了は待たず、その後の出力も結果に含めない。
/// </summary>
internal static class ExternalCommand {

    /// <summary>
    /// コマンドを実行し、そのプロセスの終了まで待機する。
    /// </summary>
    /// <param name="timeout">この時間内に終了しなければ、プロセスツリーごと強制終了する。省略時は既定の上限時間を用いる。</param>
    /// <exception cref="TimeoutException">時間内にプロセスが終了しなかった場合。</exception>
    public static async Task<Result> RunAsync(string fileName, string arguments, string workingDirectory, TimeSpan? timeout = null) {
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

        // dotnet test はこの環境変数を testhost に渡す。
        // 継承すると dotnet build の MSBuild ワーカーがこのプロセスの標準出力パイプを保持したまま常駐してしまう。
        psi.Environment.Remove("MSBUILDENSURESTDOUTFORTASKPROCESSES");
        // テスト終了後に MSBuild ワーカーを常駐させない。
        psi.Environment["MSBUILDDISABLENODEREUSE"] = "1";

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException($"プロセスを開始できませんでした: {fileName} {arguments}");

        // 標準出力と標準エラーは同時に読む。片方ずつ読むと、もう片方のパイプが満杯になった時点でデッドロックする。
        var stdout = new ConcurrentQueue<string>();
        var stderr = new ConcurrentQueue<string>();
        var reading = Task.WhenAll(
            ReadLinesAsync(process.StandardOutput, stdout),
            ReadLinesAsync(process.StandardError, stderr));

        var limit = timeout ?? DEFAULT_TIMEOUT;
        using var cancellation = new CancellationTokenSource(limit);
        try {
            await process.WaitForExitAsync(cancellation.Token);
        } catch (OperationCanceledException) {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"{limit} 以内に終了しなかったため強制終了しました: {fileName} {arguments}");
        }

        // 完了の判定はパイプのEOFではなくプロセスの終了で行う。
        // 孫プロセスがパイプを保持していると、プロセスが終了してもEOFが来ないことがあるため。
        try {
            await reading.WaitAsync(DRAIN_TIMEOUT);
        } catch (TimeoutException) {
            // それまでに読めた分だけを結果とする
        }

        return new Result(
            $"{fileName} {arguments}",
            workingDirectory,
            process.ExitCode,
            string.Join(Environment.NewLine, stdout),
            string.Join(Environment.NewLine, stderr));
    }

    /// <summary>
    /// 外部コマンドの実行結果。
    /// </summary>
    public sealed record Result(string CommandLine, string WorkingDirectory, int ExitCode, string Stdout, string Stderr) {
        /// <summary>
        /// ログ出力用の文字列に変換する。
        /// </summary>
        public string ToLogText() {
            return $"""
                COMMAND: {CommandLine} (cwd={WorkingDirectory})
                EXIT_CODE: {ExitCode}
                --------------------------------------------------
                STDOUT:
                {Stdout}
                --------------------------------------------------
                STDERR:
                {Stderr}

                """;
        }
    }

    private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan DRAIN_TIMEOUT = TimeSpan.FromSeconds(5);

    private static async Task ReadLinesAsync(StreamReader reader, ConcurrentQueue<string> lines) {
        while (await reader.ReadLineAsync() is { } line) {
            lines.Enqueue(line);
        }
    }
}

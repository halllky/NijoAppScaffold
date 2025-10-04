using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nijo.Util.DotnetEx;

namespace Nijo.WebService.Debugging;

/// <summary>
/// デバッグプロセスの設定
/// </summary>
internal record ProcessConfig {
    public required string ProcessName { get; init; }
    public required string WorkingDirectory { get; init; }
    public required string Port { get; init; }
    public required string LaunchCommand { get; init; }
    public required string ProcessExecutableName { get; init; } // 例: "node.exe", "WebApi.exe"
}

/// <summary>
/// デバッグプロセスの起動・停止・監視を行う共通マネージャー
/// </summary>
internal class DebugProcessManager {

    /// <summary>
    /// デバッグプロセスを起動する
    /// </summary>
    internal static (StringBuilder consoleOut, StringBuilder errorSummary, Process? process) StartProcess(ProcessConfig config) {

        var errorSummary = new StringBuilder();
        var consoleOut = new StringBuilder();

        // 一時ファイル
        var workDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "temp"));
        var cmdFile = Path.Combine(workDir, $"{config.ProcessName}-run.cmd");
        var logFile = Path.Combine(workDir, $"{config.ProcessName}-run.log");

        Directory.CreateDirectory(workDir);

        consoleOut.AppendLine($"{config.ProcessName}の作業ディレクトリ: {config.WorkingDirectory}");

        // cmdを介して実行するスクリプトを作成
        var cmdScript = $$"""
            chcp 65001
            @echo off
            setlocal

            set "NO_COLOR=true"
            set "LOG_FILE={{logFile}}"

            @echo. > "%LOG_FILE%"
            echo [%date% %time%] {{config.ProcessName}} 開始 > "%LOG_FILE%"
            cd /d "{{config.WorkingDirectory}}"
            {{config.LaunchCommand}} >> "%LOG_FILE%" 2>&1
            echo [%date% %time%] {{config.ProcessName}} 終了（終了コード: %errorlevel%） >> "%LOG_FILE%"
            """;

        ProcessExtension.RenderCmdFile(cmdFile, cmdScript);
        consoleOut.AppendLine($"{config.ProcessName}プロセス開始用のcmdファイルを作成しました: {cmdFile}");

        // cmdファイルをUseShellExecute=trueで実行
        Process? process;
        try {
            var startInfo = new ProcessStartInfo {
                FileName = Path.GetFullPath(cmdFile),
                UseShellExecute = true, // viteなどは UseShellExecute で実行しないとまともに動かない
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = config.WorkingDirectory,
            };

            consoleOut.AppendLine($"{config.ProcessName}プロセスを起動します: {cmdFile}");
            process = Process.Start(startInfo);

            if (process == null) {
                throw new InvalidOperationException($"[ERROR] {config.ProcessName}プロセスの起動に失敗しました。Process.Startがnullを返しました。");
            }

            consoleOut.AppendLine($"{config.ProcessName}プロセスを起動しました (PID: {process.Id})");

        } catch (Exception ex) {
            errorSummary.AppendLine($"{config.ProcessName}プロセスの起動中に例外が発生しました: {ex.Message}");
            return (consoleOut, errorSummary, null);
        }

        return (consoleOut, errorSummary, process);
    }

    /// <summary>
    /// プロセスが起動するまで監視する
    /// </summary>
    internal static async Task<(StringBuilder consoleOut, StringBuilder errorSummary)> MonitorProcessStartupAsync(
        Process process,
        ProcessConfig config,
        Func<Task<int?>> checkIfProcessRunning,
        string logFilePath) {

        var consoleOut = new StringBuilder();
        var errorSummary = new StringBuilder();

        var timeout = DateTime.Now.AddSeconds(120);
        while (true) {
            // プロセスが終了していないかチェック
            if (process.HasExited) {
                consoleOut.AppendLine($"{config.ProcessName}プロセスが終了しました。終了コード: {process.ExitCode}");
                if (process.ExitCode != 0) {
                    errorSummary.AppendLine($"{config.ProcessName}プロセスがエラーで終了しました。終了コード: {process.ExitCode}");
                }
                break;
            }

            // ログファイルをチェックしてエラーを早期検出
            var currentLogContent = await ReadLogFileAsync(logFilePath, new StringBuilder());
            if (!string.IsNullOrEmpty(currentLogContent)) {
                if (ContainsErrorKeywords(currentLogContent)) {
                    consoleOut.AppendLine("ログファイルにエラーが検出されました。早期終了します。");
                    errorSummary.AppendLine($"{config.ProcessName}の実行中にエラーが発生しました。");
                    break;
                }
                // "終了" がログに含まれている場合も終了
                if (currentLogContent.Contains($"{config.ProcessName} 終了") || currentLogContent.Contains("終了")) {
                    consoleOut.AppendLine($"{config.ProcessName}が終了しました。");
                    break;
                }
            }

            var pid = await checkIfProcessRunning();
            if (pid != null) {
                consoleOut.AppendLine($"{config.ProcessName}の起動を確認しました (PID: {pid})");
                break;
            }
            if (DateTime.Now > timeout) {
                errorSummary.AppendLine($"一定時間経過しましたが{config.ProcessName}プロセスが起動しませんでした。");
                break;
            }
            await Task.Delay(500);
        }

        // ログファイルの内容を読み込んでエラー情報として追加
        var logContent = await ReadLogFileAsync(logFilePath, consoleOut);
        if (!string.IsNullOrEmpty(logContent)) {
            consoleOut.AppendLine($"=== {config.ProcessName} ログファイルの内容 ===");
            consoleOut.AppendLine(logContent);

            if (ContainsErrorKeywords(logContent)) {
                errorSummary.AppendLine($"{config.ProcessName}の実行中にエラーが発生した可能性があります。詳細はログを確認してください。");
            }
        }

        return (consoleOut, errorSummary);
    }

    /// <summary>
    /// プロセスを停止する
    /// </summary>
    internal static async Task<(StringBuilder consoleOut, StringBuilder errorSummary)> StopProcessAsync(
        int pid,
        string processName,
        string expectedExecutableName) {

        var consoleOut = new StringBuilder();
        var errorSummary = new StringBuilder();

        // 安全のためkill対象が期待されたexeであることを確認
        if (!processName.Equals(expectedExecutableName, StringComparison.OrdinalIgnoreCase)) {
            errorSummary.AppendLine($"[ERROR] デバッグ対象のプロセスが {expectedExecutableName} ではありません。kill対象: {processName}");
            return (consoleOut, errorSummary);
        }

        var exitCode = await ProcessExtension.ExecuteProcessAsync(startInfo => {
            startInfo.FileName = "taskkill";
            startInfo.ArgumentList.Add("/PID");
            startInfo.ArgumentList.Add(pid.ToString());
            startInfo.ArgumentList.Add("/T");
            startInfo.ArgumentList.Add("/F");
        }, (std, line) => {
            consoleOut.AppendLine($"[{std}] {line}");
        });

        consoleOut.AppendLine($"{processName}のtaskkillの終了コード: {exitCode}");
        if (exitCode != 0) {
            errorSummary.AppendLine($"{processName}のtaskkillの終了コードが0ではありません。終了コード: {exitCode}");
        }

        return (consoleOut, errorSummary);
    }

    /// <summary>
    /// ログファイルを安全に読み込む。他のプロセスが使用中の場合はリトライする。
    /// </summary>
    internal static async Task<string?> ReadLogFileAsync(string filePath, StringBuilder consoleOut) {
        const int maxRetries = 5;
        const int retryDelayMs = 200;

        if (!File.Exists(filePath)) {
            consoleOut.AppendLine($"ログファイルが存在しません: {filePath}");
            return null;
        }

        for (int attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                // FileShare.ReadWriteを使用してより柔軟なアクセスを試行
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var content = await reader.ReadToEndAsync();
                if (attempt > 1) {
                    consoleOut.AppendLine($"ログファイルの読み込みに成功しました（{attempt}回目の試行）");
                }
                return content;
            } catch (IOException ex) {
                if (attempt < maxRetries) {
                    consoleOut.AppendLine($"ログファイルの読み込みに失敗しました。{retryDelayMs}ms後に再試行します。（{attempt}/{maxRetries}回目）");
                    await Task.Delay(retryDelayMs);
                } else {
                    consoleOut.AppendLine($"ログファイルの読み込みに{maxRetries}回失敗しました。他のプロセスがファイルを占有している可能性があります: {ex.Message}");
                }
            } catch (Exception ex) {
                consoleOut.AppendLine($"ログファイルの読み込み中に予期しないエラーが発生しました: {ex.Message}");
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// ログ内容にエラーキーワードが含まれているかチェック
    /// </summary>
    private static bool ContainsErrorKeywords(string logContent) {
        return logContent.Contains("ERROR") || logContent.Contains("error") ||
               logContent.Contains("ENOENT") || logContent.Contains("Command failed") ||
               logContent.Contains("npm ERR!") || logContent.Contains("Failed to") ||
               logContent.Contains("fail") || logContent.Contains("Exception") ||
               logContent.Contains("Unable to");
    }
}


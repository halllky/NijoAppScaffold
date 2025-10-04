using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Nijo.Util.DotnetEx;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Reflection;
using Nijo.WebService.Common;

namespace Nijo.WebService.Debugging;

/// <summary>
/// Windows Form のUI上から、自動生成されたあとのアプリケーションのデバッグを開始したり終了したりする操作を提供する。
/// </summary>
internal class DebugTools {

    internal DebugTools() {
    }

    // とりあえずポート決め打ち
    private const string NPM_PORT = "5173";
    private const string DOTNET_PORT = "7098";

    /// <summary>
    /// UI上でデバッグ関連の操作をおこなうためのエンドポイントを構成します。
    /// </summary>
    /// <param name="app"></param>
    internal void ConfigureWebApplication(WebApplication app) {

        app.MapGet("/debug-state", DebugState);

        app.MapPost($"/api/{{{ProjectHelper.PROJECT_DIR_PARAMETER}}}/start-npm-debugging", StartNpmDebugging);
        app.MapPost($"/api/{{{ProjectHelper.PROJECT_DIR_PARAMETER}}}/stop-npm-debugging", StopNpmDebugging);

        app.MapPost($"/api/{{{ProjectHelper.PROJECT_DIR_PARAMETER}}}/start-dotnet-debugging", StartDotnetDebugging);
        app.MapPost($"/api/{{{ProjectHelper.PROJECT_DIR_PARAMETER}}}/stop-dotnet-debugging", StopDotnetDebugging);
    }

    /// <summary>
    /// デバッグ状態を調べるだけ
    /// </summary>
    private static async Task DebugState(HttpContext context) {
        var state = await CheckDebugState();
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(state, context.RequestAborted);
    }

    /// <summary>
    /// npm run devを別プロセスで開始する。既に開始されている場合は何もしない。
    /// </summary>
    private async Task StartNpmDebugging(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var state = await CheckDebugState();

        // 既に起動している場合は何もしない
        if (state.EstimatedPidOfNodeJs != null) {
            state.ConsoleOut += "npm run devは既に起動済みです。\n";
            await HttpResponseHelper.WriteJsonResponseAsync(context, state, cancellationToken: context.RequestAborted);
            return;
        }

        var config = new ProcessConfig {
            ProcessName = "npm run dev",
            WorkingDirectory = project.ReactProjectRoot,
            Port = NPM_PORT,
            LaunchCommand = "call npm run dev",
            ProcessExecutableName = "node.exe",
        };

        var (consoleOut, errorSummary, process) = DebugProcessManager.StartProcess(config);

        if (process == null) {
            var errorState = await CheckDebugState();
            errorState.ErrorSummary = errorSummary.ToString();
            errorState.ConsoleOut += consoleOut.ToString();
            await HttpResponseHelper.WriteJsonResponseAsync(context, errorState, cancellationToken: context.RequestAborted);
            return;
        }

        // 起動監視
        var workDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "temp"));
        var logFile = Path.Combine(workDir, $"{config.ProcessName}-run.log");

        var (monitorConsoleOut, monitorErrorSummary) = await DebugProcessManager.MonitorProcessStartupAsync(
            process,
            config,
            async () => (await CheckDebugState()).EstimatedPidOfNodeJs,
            logFile);

        consoleOut.Append(monitorConsoleOut);
        errorSummary.Append(monitorErrorSummary);

        // 起動し終わったのでpidを調べてクライアントに結果を返す
        var stateAfterLaunch = await CheckDebugState();
        stateAfterLaunch.ErrorSummary = errorSummary.ToString();
        stateAfterLaunch.ConsoleOut += consoleOut.ToString();
        await HttpResponseHelper.WriteJsonResponseAsync(context, stateAfterLaunch, cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// dotnet runを別プロセスで開始する。既に開始されている場合は何もしない。
    /// </summary>
    private async Task StartDotnetDebugging(HttpContext context) {
        var project = await ProjectHelper.GetProjectAndSetResponseIfErrorAsync(context);
        if (project == null) {
            return;
        }

        var state = await CheckDebugState();

        // 既に起動している場合は何もしない
        if (state.EstimatedPidOfAspNetCore != null) {
            state.ConsoleOut += "dotnet runは既に起動済みです。\n";
            await HttpResponseHelper.WriteJsonResponseAsync(context, state, cancellationToken: context.RequestAborted);
            return;
        }

        var config = new ProcessConfig {
            ProcessName = "dotnet run",
            WorkingDirectory = project.WebapiProjectRoot,
            Port = DOTNET_PORT,
            LaunchCommand = "call dotnet run --launch-profile https",
            ProcessExecutableName = "WebApi.exe",
        };

        var (consoleOut, errorSummary, process) = DebugProcessManager.StartProcess(config);

        if (process == null) {
            var errorState = await CheckDebugState();
            errorState.ErrorSummary = errorSummary.ToString();
            errorState.ConsoleOut += consoleOut.ToString();
            await HttpResponseHelper.WriteJsonResponseAsync(context, errorState, cancellationToken: context.RequestAborted);
            return;
        }

        // 起動監視
        var workDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "temp"));
        var logFile = Path.Combine(workDir, $"{config.ProcessName}-run.log");

        var (monitorConsoleOut, monitorErrorSummary) = await DebugProcessManager.MonitorProcessStartupAsync(
            process,
            config,
            async () => (await CheckDebugState()).EstimatedPidOfAspNetCore,
            logFile);

        consoleOut.Append(monitorConsoleOut);
        errorSummary.Append(monitorErrorSummary);

        // 起動し終わったのでpidを調べてクライアントに結果を返す
        var stateAfterLaunch = await CheckDebugState();
        stateAfterLaunch.ErrorSummary = errorSummary.ToString();
        stateAfterLaunch.ConsoleOut += consoleOut.ToString();
        await HttpResponseHelper.WriteJsonResponseAsync(context, stateAfterLaunch, cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// taskkillでnpmデバッグを止める
    /// </summary>
    private async Task StopNpmDebugging(HttpContext context) {
        var stateBeforeKill = await CheckDebugState();
        var consoleOut = new StringBuilder();
        var errorSummary = new StringBuilder();

        // Node.js のプロセスを止める
        if (stateBeforeKill.EstimatedPidOfNodeJs != null && stateBeforeKill.NodeJsProcessName != null) {
            var (stopConsoleOut, stopErrorSummary) = await DebugProcessManager.StopProcessAsync(
                stateBeforeKill.EstimatedPidOfNodeJs.Value,
                stateBeforeKill.NodeJsProcessName,
                "node.exe");
            consoleOut.Append(stopConsoleOut);
            errorSummary.Append(stopErrorSummary);
        } else {
            consoleOut.AppendLine("npm デバッグプロセスが見つかりませんでした。");
        }

        var stateAfterKill = await CheckDebugState();
        stateAfterKill.ErrorSummary = errorSummary.ToString();
        stateAfterKill.ConsoleOut += consoleOut.ToString();
        await HttpResponseHelper.WriteJsonResponseAsync(context, stateAfterKill, cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// taskkillでdotnetデバッグを止める
    /// </summary>
    private async Task StopDotnetDebugging(HttpContext context) {
        var stateBeforeKill = await CheckDebugState();
        var consoleOut = new StringBuilder();
        var errorSummary = new StringBuilder();

        // ASP.NET Core のプロセスを止める
        if (stateBeforeKill.EstimatedPidOfAspNetCore != null && stateBeforeKill.AspNetCoreProcessName != null) {
            // 念のため "～～.WebApi.exe" という名前のプロセスであることを確認
            if (!stateBeforeKill.AspNetCoreProcessName.EndsWith(".WebApi.exe")) {
                errorSummary.AppendLine($"[ERROR] デバッグ対象のプロセスが WebApi.exe ではありません。kill対象: {stateBeforeKill.AspNetCoreProcessName}");
            } else {
                var (stopConsoleOut, stopErrorSummary) = await DebugProcessManager.StopProcessAsync(
                    stateBeforeKill.EstimatedPidOfAspNetCore.Value,
                    stateBeforeKill.AspNetCoreProcessName,
                    stateBeforeKill.AspNetCoreProcessName);
                consoleOut.Append(stopConsoleOut);
                errorSummary.Append(stopErrorSummary);
            }
        } else {
            consoleOut.AppendLine("dotnet デバッグプロセスが見つかりませんでした。");
        }

        var stateAfterKill = await CheckDebugState();
        stateAfterKill.ErrorSummary = errorSummary.ToString();
        stateAfterKill.ConsoleOut += consoleOut.ToString();
        await HttpResponseHelper.WriteJsonResponseAsync(context, stateAfterKill, cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// 現在実行中の NijoApplicationBuilder のデバッグ状態を、httpポート番号を基準に調べる。
    /// </summary>
    private static async Task<DebugProcessState> CheckDebugState() {
        var consoleOutputBuilder = new StringBuilder();

        // Helper action to log to consoleOutputBuilder
        Action<string> logToConsole = (message) => {
            consoleOutputBuilder.AppendLine(message);
        };

        // Helper action for ProcessExtension.ExecuteProcessAsync
        // This will append process output to consoleOutputBuilder and also to a separate list if needed.
        Func<string, List<string>, Action<ProcessExtension.E_STD, string>> createLogHandler =
            (processName, outputList) => {
                return (std, line) => {
                    var formattedLine = $"[{processName}-{std}] {line}";
                    consoleOutputBuilder.AppendLine(formattedLine);
                    if (std == ProcessExtension.E_STD.StdOut && line != null) { // line can be null if stream closed
                        outputList.Add(line);
                    }
                };
            };

        int? nodePid = null;
        int? aspPid = null;
        string? nodeProcessName = null;
        string? aspProcessName = null;

        var netstatOutputLines = new List<string>();
        try {
            // ポート番号をベースにして実行中プロセスのpidを探す
            logToConsole("Executing: netstat -ano");
            await ProcessExtension.ExecuteProcessAsync(
                psi => {
                    psi.FileName = "netstat";
                    psi.ArgumentList.Add("-ano");
                },
                createLogHandler("netstat", netstatOutputLines),
                TimeSpan.FromSeconds(15) // Timeout for netstat
            );

            var listeningRegex = new Regex(@"\sLISTENING\s+(\d+)$");
            foreach (var line in netstatOutputLines.Where(l => l != null)) {
                if (!line.Contains($":{NPM_PORT}") && !line.Contains($":{DOTNET_PORT}")) continue;

                var match = listeningRegex.Match(line);
                if (match.Success) {
                    if (int.TryParse(match.Groups[1].Value, out var pid)) {
                        if (line.Contains($":{NPM_PORT}")) {
                            nodePid = pid;
                            logToConsole($"Found Node.js (port {NPM_PORT}) PID: {pid} from line: {line.Trim()}");
                        } else if (line.Contains($":{DOTNET_PORT}")) {
                            aspPid = pid;
                            logToConsole($"Found ASP.NET Core (port {DOTNET_PORT}) PID: {pid} from line: {line.Trim()}");
                        }
                    }
                }
            }

            // tasklist を使って Node.js のpidの詳細情報を得る
            if (nodePid.HasValue) {
                logToConsole($"Executing: tasklist /fi \"pid eq {nodePid.Value}\" /nh /fo csv");
                var tasklistNodeOutput = new List<string>();
                await ProcessExtension.ExecuteProcessAsync(
                    psi => {
                        psi.FileName = "tasklist";
                        psi.ArgumentList.Add("/fi");
                        psi.ArgumentList.Add($"pid eq {nodePid.Value}");
                        psi.ArgumentList.Add("/nh");
                        psi.ArgumentList.Add("/fo");
                        psi.ArgumentList.Add("csv");
                    },
                    createLogHandler($"tasklist-node({nodePid.Value})", tasklistNodeOutput),
                    TimeSpan.FromSeconds(10)
                );
                if (tasklistNodeOutput.Count > 0 && tasklistNodeOutput[0] != null) {
                    var parts = tasklistNodeOutput[0].Split(',');
                    if (parts.Length > 0) {
                        nodeProcessName = parts[0].Trim('"');
                        logToConsole($"Node.js process name: {nodeProcessName}");
                    }
                }
            } else {
                logToConsole($"Node.js PID (port {NPM_PORT}) not found or process not listening. Skipping tasklist.");
            }

            // tasklist を使って ASP.NET Core のpidの詳細情報を得る
            if (aspPid.HasValue) {
                logToConsole($"Executing: tasklist /fi \"pid eq {aspPid.Value}\" /nh /fo csv");
                var tasklistAspOutput = new List<string>();
                await ProcessExtension.ExecuteProcessAsync(
                    psi => {
                        psi.FileName = "tasklist";
                        psi.ArgumentList.Add("/fi");
                        psi.ArgumentList.Add($"pid eq {aspPid.Value}");
                        psi.ArgumentList.Add("/nh");
                        psi.ArgumentList.Add("/fo");
                        psi.ArgumentList.Add("csv");
                    },
                    createLogHandler($"tasklist-asp({aspPid.Value})", tasklistAspOutput),
                    TimeSpan.FromSeconds(10)
                );
                if (tasklistAspOutput.Count > 0 && tasklistAspOutput[0] != null) {
                    var parts = tasklistAspOutput[0].Split(',');
                    if (parts.Length > 0) {
                        aspProcessName = parts[0].Trim('"');
                        logToConsole($"ASP.NET Core process name: {aspProcessName}");
                    }
                }
            } else {
                logToConsole($"ASP.NET Core PID (port {DOTNET_PORT}) not found or process not listening. Skipping tasklist.");
            }

        } catch (TimeoutException tex) {
            logToConsole($"A process execution timed out: {tex.Message}");
        } catch (Exception ex) {
            logToConsole($"An error occurred while gathering debug process info: {ex.ToString()}");
        }

        return new DebugProcessState {
            EstimatedPidOfNodeJs = nodePid,
            EstimatedPidOfAspNetCore = aspPid,
            NodeJsProcessName = nodeProcessName,
            AspNetCoreProcessName = aspProcessName,
            NodeJsDebugUrl = $"http://localhost:{NPM_PORT}",
            AspNetCoreDebugUrl = $"https://localhost:{DOTNET_PORT}/swagger",
            ConsoleOut = consoleOutputBuilder.ToString(),
        };
    }

}

/// <summary>
/// 現在実行中のNijoApplicationBuilderのデバッグプロセスの状態。
/// このクラスのデータ構造はTypeScript側と合わせる必要あり
/// </summary>
public class DebugProcessState {
    /// <summary>
    /// サーバー側で発生した何らかのエラー
    /// </summary>
    [JsonPropertyName("errorSummary")]
    public string? ErrorSummary { get; set; }
    /// <summary>
    /// 現在実行中のNijoApplicationBuilderのNode.jsのデバッグプロセスと推測されるPID
    /// </summary>
    [JsonPropertyName("estimatedPidOfNodeJs")]
    public int? EstimatedPidOfNodeJs { get; set; }
    /// <summary>
    /// 現在実行中のNijoApplicationBuilderのASP.NET Coreのデバッグプロセスと推測されるPID
    /// </summary>
    [JsonPropertyName("estimatedPidOfAspNetCore")]
    public int? EstimatedPidOfAspNetCore { get; set; }
    /// <summary>
    /// Node.jsのプロセス名
    /// </summary>
    [JsonPropertyName("nodeJsProcessName")]
    public string? NodeJsProcessName { get; set; }
    /// <summary>
    /// ASP.NET Coreのプロセス名
    /// </summary>
    [JsonPropertyName("aspNetCoreProcessName")]
    public string? AspNetCoreProcessName { get; set; }
    /// <summary>
    /// Node.jsのデバッグURL
    /// </summary>
    [JsonPropertyName("nodeJsDebugUrl")]
    public string? NodeJsDebugUrl { get; set; }
    /// <summary>
    /// ASP.NET CoreのデバッグURL（swagger-ui）
    /// </summary>
    [JsonPropertyName("aspNetCoreDebugUrl")]
    public string? AspNetCoreDebugUrl { get; set; }
    /// <summary>
    /// PID推測時のコンソール出力
    /// </summary>
    [JsonPropertyName("consoleOut")]
    public string ConsoleOut { get; set; } = string.Empty;
}


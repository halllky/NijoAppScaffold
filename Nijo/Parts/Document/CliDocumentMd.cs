using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;

/// <summary>
/// コマンドラインインターフェースの仕様を説明するマークダウンドキュメントを生成する。
/// </summary>
internal class CliDocumentMd {

    internal static string FILE_NAME = "CLI.md";

    internal static string Render(RootCommand rootCommand) {
        var sb = new StringBuilder();

        sb.AppendLine("""
            ---
            title: コマンドライン API
            outline: [2,2]  # `##` の見出しをページ内ナビゲーションに表示
            ---

            # コマンドライン API

            このページでは nijo コマンドラインツールの使用方法について説明します。

            ## 概要

            `nijo` は、データベース設計からWebアプリケーションのコードを自動生成するためのツールです。
            プロジェクトの新規作成から開発、デバッグまでの各種操作をコマンドラインから実行できます。

            ### 基本的な使用方法

            ```bash
            nijo <command> [options] [arguments]
            ```

            """);

        // グローバル引数がある場合の記述
        if (rootCommand.Arguments.Any()) {
            sb.AppendLine("### グローバル引数");
            sb.AppendLine();
            foreach (var argument in rootCommand.Arguments) {
                sb.AppendLine($"#### `{argument.Name}`");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(argument.Description)) {
                    sb.AppendLine($"{argument.Description}");
                    sb.AppendLine();
                }
            }
        }

        // コマンド一覧
        sb.AppendLine("## コマンド一覧");
        sb.AppendLine();

        foreach (var command in rootCommand.Subcommands.OrderBy(c => c.Name)) {
            sb.AppendLine($"- [`{command.Name}`](#{command.Name.Replace("-", "")}) - {command.Description}");
        }
        sb.AppendLine();

        // 各コマンドの詳細
        foreach (var command in rootCommand.Subcommands.OrderBy(c => c.Name)) {
            RenderCommand(sb, command);
        }

        return sb.ToString();
    }

    private static void RenderCommand(StringBuilder sb, Command command) {
        sb.AppendLine($"## `{command.Name}`");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(command.Description)) {
            sb.AppendLine(command.Description);
            sb.AppendLine();
        }

        // 使用法
        sb.AppendLine("### 使用法");
        sb.AppendLine();

        var usage = new StringBuilder($"nijo {command.Name}");

        // 引数を追加
        foreach (var argument in command.Arguments) {
            if (argument.Arity.MinimumNumberOfValues == 0) {
                usage.Append($" [{argument.Name}]");
            } else {
                usage.Append($" <{argument.Name}>");
            }
        }

        // オプションを追加
        if (command.Options.Any()) {
            usage.Append(" [options]");
        }

        sb.AppendLine("```bash");
        sb.AppendLine(usage.ToString());
        sb.AppendLine("```");
        sb.AppendLine();

        // 引数の詳細
        if (command.Arguments.Any()) {
            sb.AppendLine("### 引数");
            sb.AppendLine();
            foreach (var argument in command.Arguments) {
                sb.AppendLine($"#### `{argument.Name}`");
                sb.AppendLine();
                if (!string.IsNullOrEmpty(argument.Description)) {
                    sb.AppendLine($"{argument.Description}");
                    sb.AppendLine();
                }

                // デフォルト値（オプショナル引数の場合のみ表示）
                if (argument.Arity.MinimumNumberOfValues == 0) {
                    sb.AppendLine($"**デフォルト値**: 空文字列またはnull");
                    sb.AppendLine();
                }

                // 必須かどうか
                if (argument.Arity.MinimumNumberOfValues > 0) {
                    sb.AppendLine("**必須**: はい");
                } else {
                    sb.AppendLine("**必須**: いいえ");
                }
                sb.AppendLine();
            }
        }

        // オプションの詳細
        if (command.Options.Any()) {
            sb.AppendLine("### オプション");
            sb.AppendLine();
            foreach (var option in command.Options.OrderBy(o => o.Name)) {
                RenderOption(sb, option);
            }
        }

        // 使用例
        sb.AppendLine("### 使用例");
        sb.AppendLine();
        RenderExamples(sb, command);
        sb.AppendLine();
    }

    private static void RenderOption(StringBuilder sb, Option option) {
        // エイリアスの表示
        var aliases = option.Aliases.ToArray();
        var aliasText = string.Join(", ", aliases.Select(a => $"`{a}`"));

        sb.AppendLine($"#### {aliasText}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(option.Description)) {
            sb.AppendLine(option.Description);
            sb.AppendLine();
        }

        // 型情報
        var argumentType = option.ValueType;
        if (argumentType != typeof(bool)) {
            sb.AppendLine($"**型**: `{GetTypeName(argumentType)}`");
            sb.AppendLine();
        }

        // デフォルト値（フラグオプションの場合のみ表示）
        if (argumentType == typeof(bool)) {
            sb.AppendLine($"**デフォルト値**: `false` (フラグオプション)");
            sb.AppendLine();
        }

        // 必須かどうか
        if (option.IsRequired) {
            sb.AppendLine("**必須**: はい");
            sb.AppendLine();
        }
    }

    private static string GetTypeName(Type type) {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(int?)) return "integer";
        if (type == typeof(bool)) return "boolean";
        if (type == typeof(double) || type == typeof(double?)) return "number";
        return type.Name;
    }

    private static void RenderExamples(StringBuilder sb, Command command) {
        sb.AppendLine("```bash");

        // 基本的な使用例
        var basicUsage = $"nijo {command.Name}";
        if (command.Arguments.Any()) {
            var firstArg = command.Arguments.First();
            if (firstArg.Arity.MinimumNumberOfValues == 0) {
                sb.AppendLine($"# 基本的な使用方法");
                sb.AppendLine(basicUsage);
                sb.AppendLine();

                // 引数ありの例
                var argName = GetExampleArgumentValue(firstArg);
                sb.AppendLine($"# 引数を指定した使用方法");
                sb.AppendLine($"{basicUsage} {argName}");
            } else {
                sb.AppendLine($"# 使用方法（引数必須）");
                var argName = GetExampleArgumentValue(firstArg);
                sb.AppendLine($"{basicUsage} {argName}");
            }
        } else {
            sb.AppendLine($"# 基本的な使用方法");
            sb.AppendLine(basicUsage);
        }

        // オプションありの例
        if (command.Options.Any()) {
            sb.AppendLine();
            sb.AppendLine("# オプションを使用した例");
            var optionExamples = GenerateOptionExamples(command);
            foreach (var example in optionExamples) {
                sb.AppendLine(example);
            }
        }

        sb.AppendLine("```");
    }

    private static string GetExampleArgumentValue(Argument argument) {
        var name = argument.Name.ToLowerInvariant();
        if (name.Contains("path") || name.Contains("directory")) {
            return "my-project";
        }
        if (name.Contains("file")) {
            return "example.txt";
        }
        if (name.Contains("port")) {
            return "8080";
        }
        if (name.Contains("url")) {
            return "https://example.com";
        }
        return $"<{argument.Name}>";
    }

    private static IEnumerable<string> GenerateOptionExamples(Command command) {
        var examples = new List<string>();
        var baseCommand = $"nijo {command.Name}";

        // 引数がある場合は例の値を含める
        if (command.Arguments.Any()) {
            var argValue = GetExampleArgumentValue(command.Arguments.First());
            baseCommand += $" {argValue}";
        }

        // 各オプションの例を生成
        foreach (var option in command.Options.Take(3)) { // 最大3つまで例を生成
            var exampleOption = GenerateOptionExample(option);
            if (!string.IsNullOrEmpty(exampleOption)) {
                examples.Add($"{baseCommand} {exampleOption}");
            }
        }

        return examples;
    }

    private static string GenerateOptionExample(Option option) {
        var alias = option.Aliases.FirstOrDefault();
        if (alias == null) return string.Empty;

        if (option.ValueType == typeof(bool)) {
            return alias;
        }

        var exampleValue = GetExampleOptionValue(option);
        return $"{alias} {exampleValue}";
    }

    private static string GetExampleOptionValue(Option option) {
        var name = option.Name.ToLowerInvariant();
        var firstAlias = option.Aliases.FirstOrDefault()?.ToLowerInvariant() ?? "";

        if (name.Contains("port") || firstAlias.Contains("port")) {
            return "8080";
        }
        if (name.Contains("file") || firstAlias.Contains("file")) {
            return "example.txt";
        }
        if (name.Contains("out") || name.Contains("output") || firstAlias.Contains("out")) {
            return "./docs";
        }
        if (name.Contains("path") || name.Contains("directory")) {
            return "./my-directory";
        }
        if (option.ValueType == typeof(string)) {
            return "value";
        }
        if (option.ValueType == typeof(int) || option.ValueType == typeof(int?)) {
            return "123";
        }

        return "value";
    }
}

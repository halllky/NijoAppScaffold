using Nijo.Core;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Models.CommandModelFeatures {
    /// <summary>
    /// <see cref="CommandModel"/> における本処理実行時引数。
    /// 主な役割は処理結果のハンドリングに関する処理。
    /// </summary>
    internal class CommandResult : ISummarizedFile {

        internal const string TS_TYPE_NAME = "CommandResult";

        // C#側ではWebAPIから実行された場合とコマンドラインで実行された場合で処理結果のハンドリング方法が異なる。
        // 例えばファイル生成処理の場合、Webならブラウザにバイナリを返してダウンロードさせるが、
        // コマンドラインの場合は実行環境のどこかのフォルダにファイルを出力する。
        internal const string GENERATOR_INTERFACE_NAME = "ICommandResultGenerator";

        internal const string GENERATOR_WEB_CLASS_NAME = "CommandResultGeneratorInWeb";
        internal const string GENERATOR_CLI_CLASS_NAME = "CommandResultGeneratorInCli";

        internal const string RESULT_INTERFACE_NAME = "ICommandResult";
        internal const string RESULT_WEB_CLASS_NAME = "CommandResultInWeb";
        internal const string RESULT_CLI_CLASS_NAME = "CommandResultInCli";

        // HTTPレスポンス
        internal const string TYPE_MESSAGE = "message";
        internal const string TYPE_REDIRECT = "redirect";
        internal const string HTTP_CONFIRM = "confirm";
        internal const string HTTP_MESSAGE_DETAIL = "detail";

        /// <summary>
        /// コマンド処理でこの詳細画面へ遷移する処理を書けるように登録する
        /// </summary>
        internal void Register(GraphNode<Aggregate> aggregate) {
            _redirectableList.Add(new() {
                Aggregate = aggregate,
                DisplayData = new ReadModel2Features.DataClassForDisplay(aggregate),
            });
        }
        private readonly List<Redirectable> _redirectableList = new();
        private class Redirectable {
            internal required GraphNode<Aggregate> Aggregate { get; init; }
            internal required ReadModel2Features.DataClassForDisplay DisplayData { get; init; }
        }


        void ISummarizedFile.OnEndGenerating(CodeRenderingContext context) {
            context.CoreLibrary.UtilDir(dir => {
                dir.Generate(RenderInterface(context));
            });
        }

        private SourceFile RenderInterface(CodeRenderingContext context) => new SourceFile {
            FileName = "ICommandResult.cs",
            RenderContent = ctx => {
                return $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// 未使用
                    /// </summary>
                    public interface {{RESULT_INTERFACE_NAME}} {
                    }
                    """;
            },
        };
    }
}

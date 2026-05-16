using Nijo.Models.CommandModelFeatures;
using Nijo.Models.WriteModel2Features;
using Nijo.Parts.WebServer;
using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.BothOfClientAndServer {

    /// <summary>
    /// サーバー側から見たプレゼンテーション側（画面やコンソール）の抽象。
    /// ここではC#とTypeScriptで共有しなければいけないインターフェース部分の定義のみを自動生成し、
    /// 処理の実装は修正速度を重視して生成後プロジェクトで行なう
    /// </summary>
    internal class PresentationContext {

        internal const string INTERFACE_NAME = "IPresentationContext";

        private const string C_OK = "ok";
        private const string C_SUMMARY = "summary";
        private const string C_DETAIL = "detail";
        private const string C_CONFIRMS = "confirms";
        private const string C_RETURN_VALUE = "returnValue";

        internal static SourceFile RenderCSharp() {
            return new SourceFile {
                FileName = "IPresentationContext.cs",
                RenderContent = ctx => {
                    return $$"""
                        using System.Text.Json.Nodes;

                        namespace {{ctx.Config.RootNamespace}};

                        /// <summary>
                        /// 更新系処理結果後のプレゼンテーション側の状態操作を提供します。
                        /// </summary>
                        public interface {{INTERFACE_NAME}} : {{DisplayMessageContainer.INTERFACE}} {

                            /// <summary>
                            /// 処理実行時オプション
                            /// </summary>
                            {{SaveContext.SAVE_OPTIONS}} Options { get; }

                            /// <summary>
                            /// エラーメッセージのコンテナを返します。
                            /// </summary>
                            T GetMessageContainerAs<T>() where T : {{DisplayMessageContainer.INTERFACE}};
                            /// <summary>
                            /// 処理内部で明示的にメッセージコンテナの型を変更したい場合に使用。
                            /// 通常は <see cref="GetMessageContainerAs" /> で足りるはずなので使用機会は少ないはず。
                            /// </summary>
                            IDisplayMessageContainer MessageContainerRoot { get; set; }

                            /// <summary>
                            /// プレゼンテーション側に返す任意の戻り値。
                            /// </summary>
                            object? ReturnValue { get; set; }

                            /// <summary>
                            /// 処理が成功した旨のみをユーザーに伝えます。
                            /// </summary>
                            /// <param name="text">メッセージ</param>
                            ICommandResult Ok(string? text = null);

                            /// <summary>
                            /// 処理の途中でエラーが発生した旨をユーザーに伝えます。
                            /// </summary>
                            /// <param name="error">エラー内容。未指定の場合は既にこのインスタンスが保持しているエラーが表示されます。</param>
                            ICommandResult Error(string? error = null);

                            /// <summary>
                            /// 処理を続行してもよいかどうかユーザー側が確認し了承する必要がある旨を返します。
                            /// </summary>
                            /// <param name="message">確認メッセージ</param>
                            void AddConfirm(string message);

                            /// <summary>
                            /// 処理を続行してもよいかどうかユーザー側が確認し了承する必要がある旨を返します。
                            /// </summary>
                            /// <param name="confirm">確認メッセージ。未指定の場合は標準の確定確認メッセージが表示されます。</param>
                            ICommandResult Confirm(string? confirm = null);

                            /// <inheritdoc cref="AddConfirm">
                            ICommandResult Confirm(IEnumerable<string> confirms);

                            /// <summary>
                            /// この処理の中で処理続行の是非をユーザー側に確認するメッセージがあるかどうかを返します。
                            /// </summary>
                            bool HasConfirm();
                        }

                        /// <summary>
                        /// 更新系処理結果後のプレゼンテーション側の状態。
                        /// </summary>
                        public class PresentationContextResult {
                            /// <summary>処理全体の成否</summary>
                            public required bool Ok { get; set; }
                            /// <summary>処理結果の概要。「処理成功しました」など</summary>
                            public string? Summary { get; set; }
                            /// <summary>処理結果の詳細。項目ごとにメッセージが格納される。</summary>
                            public DisplayMessageContainerBase? Detail { get; set; }
                            /// <summary>確認メッセージ</summary>
                            public List<string> Confirms { get; set; } = [];
                            /// <summary>アプリケーション側からプレゼンテーション側に返す任意の値</summary>
                            public object? ReturnValue { get; set; }

                            /// <summary>
                            /// Web用。
                            /// 処理結果を表すJSONにして返します。
                            /// </summary>
                            public JsonObject ToJsonObject() {
                                var confirms = new JsonArray();
                                foreach (var conf in Confirms) confirms.Add(conf);

                                var detail = Detail == null ? null : new JsonArray(Detail.ToReactHookFormErrors().ToArray());

                                var returnValue = ReturnValue == null ? null : JsonNode.Parse(ReturnValue.ToJson());

                                // ここの戻り値のオブジェクトのプロパティ名や型は React hook 側と合わせる必要がある
                                return new JsonObject {
                                    ["{{C_OK}}"] = Ok,
                                    ["{{C_SUMMARY}}"] = Summary,
                                    ["{{C_CONFIRMS}}"] = confirms,
                                    ["{{C_DETAIL}}"] = detail,
                                    ["{{C_RETURN_VALUE}}"] = returnValue,
                                };
                            }
                        }
                        """;
                },
            };
        }

        internal static SourceFile RenderTypeScript() {
            return new SourceFile {
                FileName = "presentation-context.ts",
                RenderContent = ctx => {

                    return $$"""
                        /** 更新系処理結果後のプレゼンテーション側の状態。このオブジェクトの型はサーバー側と合わせる必要がある */
                        export type PresentationContextState<T = object> = {
                          /** 処理全体の成否 */
                          {{C_OK}}: boolean
                          /** 処理結果の概要。「処理成功しました」など */
                          {{C_SUMMARY}}?: string
                          /** 確認メッセージ */
                          {{C_CONFIRMS}}?: string[]
                          /** 処理結果の詳細。項目ごとにメッセージが格納される。 */
                          {{C_DETAIL}}?: [string, ReactHookFormMultipleErrorObject][]
                          /** アプリケーション側からプレゼンテーション側に返す任意の値 */
                          {{C_RETURN_VALUE}}?: T
                        }

                        /**
                         * react-hook-form で1つのフィールドに複数のエラーを表示させる場合のエラーメッセージのオブジェクトの型。
                         * ただしキー名を "ERROR-" 等で始めるのは react-hook-forms ではなくこのプロジェクトの都合（警告やインフォメーションで色を分けるため）
                         */
                        export type ReactHookFormMultipleErrorObject = {
                          types: {
                            [key in `ERROR-${number | string}` | `WARN-${number | string}` | `INFO-${number | string}`]: string
                          }
                        }
                        """;
                },
            };
        }
    }
}

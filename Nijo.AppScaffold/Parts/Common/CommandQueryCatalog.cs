using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.Parts.CSharp;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Nijo.Parts.Common {
    /// <summary>
    /// スキーマ定義から生成されるクエリ（一覧検索、参照検索）とコマンドの一覧。
    /// C# 側にはプレゼンテーション層が名前で処理を特定して呼び出すためのカタログを、
    /// TypeScript 側にはそれと同じ名前をキーにした引数・戻り値の型とURLの対応表を生成する。
    /// 同じ定義から両方を生成することで、クライアントとサーバーの型定義やURLのずれを防ぐ。
    /// </summary>
    internal sealed class CommandQueryCatalog : ISharedSourceFile<CodeRenderingContext> {

        internal const string CS_CLASS = "CommandQuery";
        internal const string CS_VISITOR = "ICommandQueryVisitor";
        internal const string CS_NO_PARAMETER = "NoParameter";
        internal const string CS_NO_RETURN_VALUE = "NoReturnValue";

        private const string TS_FILE_NAME = "command-query";
        private const string TS_NAMESPACE = "CommandQuery";

        /// <summary>
        /// クエリまたはコマンド1件の定義。
        /// </summary>
        internal sealed class Definition {
            /// <summary>名前。C# と TypeScript で共通のキー</summary>
            internal required string Name { get; init; }
            internal required string DisplayName { get; init; }
            /// <summary>WebAPI として公開する場合のURLのパス</summary>
            internal required string Path { get; init; }
            /// <summary>クエリなら true、コマンドなら false</summary>
            internal required bool IsQuery { get; init; }
            /// <summary>この処理の本体であるアプリケーションサービスのメソッド名</summary>
            internal required string AppSrvMethodName { get; init; }
            /// <summary>引数のC#の型名。引数なしの場合は null</summary>
            internal required string? ParameterCsType { get; init; }
            /// <summary>戻り値のC#の型名。戻り値なしの場合は null</summary>
            internal required string? ReturnValueCsType { get; init; }
            /// <summary>引数に対するメッセージのC#の型名</summary>
            internal required string MessagesCsType { get; init; }
            /// <summary>引数のTypeScriptの型名。引数なしの場合は null</summary>
            internal required string? ParameterTsType { get; init; }
            /// <summary>戻り値のTypeScriptの型名。戻り値なしの場合は null</summary>
            internal required string? ReturnValueTsType { get; init; }
            /// <summary>TypeScript の型名の中で参照している集約</summary>
            internal required IEnumerable<AggregateBase> TsReferencedAggregates { get; init; }
        }

        /// <summary>
        /// WebAPI として公開する場合のURLのパスを返します。
        /// </summary>
        /// <param name="sectionRoot">処理の持ち主のルート要素</param>
        /// <param name="action">ルート要素内で処理を識別する名前</param>
        internal static string GetPath(ISectionRoot sectionRoot, string action) {
            return $"api/{sectionRoot.LatinName.ToKebabCase()}/{action}";
        }

        private readonly List<(ISectionRoot Root, Definition[] Definitions)> _definitions = [];
        private readonly Lock _lock = new();

        /// <summary>
        /// ルート要素が持つクエリやコマンドを登録します。
        /// </summary>
        internal CommandQueryCatalog Add(ISectionRoot root, params Definition[] definitions) {
            lock (_lock) {
                _definitions.Add((root, definitions));
                return this;
            }
        }

        void ISharedSourceFile<CodeRenderingContext>.RegisterDependencies(CodeRenderingContext ctx) {
            ctx.Use<IndexTs>().AddNamespaceModule(TS_FILE_NAME, TS_NAMESPACE);
        }

        void ISharedSourceFile<CodeRenderingContext>.Render(CodeRenderingContext ctx) {
            var definitions = _definitions
                .OrderBy(x => x.Root.GetIndexOfDataFlow(ctx.Schema))
                .SelectMany(x => x.Definitions)
                .ToArray();

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCSharp(ctx, definitions));
                });
            });
            ctx.ReactProject(dir => {
                dir.Generate(RenderTypeScript(definitions));
            });
        }

        private static SourceFile RenderCSharp(CodeRenderingContext ctx, Definition[] definitions) {
            return new SourceFile {
                FileName = $"{CS_CLASS}.cs",
                Contents = $$"""
                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// スキーマ定義から生成されたクエリまたはコマンド1件。
                    /// Web、バッチ、単体テストなどのプレゼンテーション層は、 <see cref="All"/> から処理を特定して呼び出す。
                    /// 引数・戻り値・メッセージの型を使った処理は <see cref="Accept"/> で <see cref="{{CS_VISITOR}}{TResult}"/> に委ねる。
                    /// </summary>
                    public abstract class {{CS_CLASS}} {
                        private protected {{CS_CLASS}}() { }

                        /// <summary>名前。TypeScript 側の型定義のキーと一致する。</summary>
                        public required string Name { get; init; }
                        /// <summary>表示用名称</summary>
                        public required string DisplayName { get; init; }
                        /// <summary>WebAPI として公開する場合のURLのパス。TypeScript 側のURLの定義と一致する。</summary>
                        public required string Path { get; init; }
                        /// <summary>
                        /// クエリなら true、コマンドなら false。
                        /// クエリはデータを変更しないため、更新前の確認（<see cref="{{PresentationContext.INTERFACE}}.ValidationOnly"/>）を必要としない。
                        /// </summary>
                        public required bool IsQuery { get; init; }

                        /// <summary>
                        /// このインスタンスの引数・戻り値・メッセージの型を型引数にして <paramref name="visitor"/> を呼び出します。
                        /// </summary>
                        public abstract TResult Accept<TResult>({{CS_VISITOR}}<TResult> visitor);

                        /// <summary>引数のないコマンドの引数の型</summary>
                        public sealed class {{CS_NO_PARAMETER}} { }
                        /// <summary>戻り値のないコマンドの戻り値の型。インスタンスは作られず、戻り値は常に null になる。</summary>
                        public sealed class {{CS_NO_RETURN_VALUE}} { }

                        /// <summary>
                        /// すべてのクエリとコマンド
                        /// </summary>
                        public static IReadOnlyList<{{CS_CLASS}}> All { get; } = [
                    {{definitions.SelectTextTemplate(def => $$"""
                            {{WithIndent(RenderCSharpItem(def))}}
                    """)}}
                        ];
                    }

                    /// <inheritdoc cref="{{CS_CLASS}}"/>
                    /// <typeparam name="TParameter">引数の型</typeparam>
                    /// <typeparam name="TReturnValue">戻り値の型</typeparam>
                    /// <typeparam name="TMessages">引数に対するメッセージの型</typeparam>
                    public sealed class {{CS_CLASS}}<TParameter, TReturnValue, TMessages> : {{CS_CLASS}} where TMessages : {{MessageContainer.SETTER_INTERFACE}} {
                        /// <summary>
                        /// 処理を実行します。
                        /// 処理の結果は、戻り値と、引数に対するメッセージ（エラーなど）に格納される。
                        /// </summary>
                        public required Func<{{ApplicationService.ABSTRACT_CLASS}}, TParameter, TMessages, {{PresentationContext.INTERFACE}}, Task<TReturnValue?>> InvokeAsync { get; init; }

                        /// <inheritdoc/>
                        public override TResult Accept<TResult>({{CS_VISITOR}}<TResult> visitor) => visitor.Visit(this);
                    }

                    /// <summary>
                    /// <see cref="{{CS_CLASS}}"/> の引数・戻り値・メッセージの型を使った処理。
                    /// </summary>
                    public interface {{CS_VISITOR}}<out TResult> {
                        TResult Visit<TParameter, TReturnValue, TMessages>({{CS_CLASS}}<TParameter, TReturnValue, TMessages> commandQuery)
                            where TMessages : {{MessageContainer.SETTER_INTERFACE}};
                    }
                    """,
            };
        }

        private static string RenderCSharpItem(Definition def) {
            var parameterType = def.ParameterCsType ?? $"{CS_CLASS}.{CS_NO_PARAMETER}";
            var returnValueType = def.ReturnValueCsType ?? $"{CS_CLASS}.{CS_NO_RETURN_VALUE}";

            // 引数や戻り値がない処理も、型引数をそろえて同じ形で呼び出せるようにする
            var parameterName = def.ParameterCsType == null ? "_" : "parameter";
            var arguments = def.ParameterCsType == null
                ? "messages, context"
                : "parameter, messages, context";
            var invoke = def.ReturnValueCsType == null
                ? $"async (app, {parameterName}, messages, context) => {{ await app.{def.AppSrvMethodName}({arguments}); return null; }}"
                : $"(app, {parameterName}, messages, context) => app.{def.AppSrvMethodName}({arguments})";

            return $$"""
                new {{CS_CLASS}}<{{parameterType}}, {{returnValueType}}, {{def.MessagesCsType}}> {
                    Name = "{{def.Name.Replace("\"", "\\\"")}}",
                    DisplayName = "{{def.DisplayName.Replace("\"", "\\\"")}}",
                    Path = "{{def.Path}}",
                    IsQuery = {{(def.IsQuery ? "true" : "false")}},
                    InvokeAsync = {{invoke}},
                },
                """;
        }

        private static SourceFile RenderTypeScript(Definition[] definitions) {
            var modules = definitions
                .SelectMany(def => def.TsReferencedAggregates)
                .Select(agg => new TypeScriptSectionRootModule(agg.GetRoot()))
                .DistinctBy(module => module.ImportAlias)
                .OrderBy(module => module.ImportAlias);

            return new SourceFile {
                FileName = $"{TS_FILE_NAME}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{modules.SelectTextTemplate(module => $$"""
                    {{module.RenderImportFromRoot(true)}}
                    """)}}

                    /** クエリとコマンドの名前 */
                    export type Name = keyof ParamType

                    /** クエリとコマンドのURLの一覧 */
                    export const Endpoint: { [K in Name]: string } = {
                    {{definitions.SelectTextTemplate(def => $$"""
                      '{{EscapeTs(def.Name)}}': '{{def.Path}}',
                    """)}}
                    }

                    /** クエリとコマンドの引数の型の一覧 */
                    export interface ParamType {
                    {{definitions.SelectTextTemplate(def => $$"""
                      '{{EscapeTs(def.Name)}}': {{def.ParameterTsType ?? "Record<string, never> // 引数なし"}}
                    """)}}
                    }

                    /** クエリとコマンドの戻り値の型の一覧 */
                    export interface ReturnType {
                    {{definitions.SelectTextTemplate(def => $$"""
                      '{{EscapeTs(def.Name)}}': {{def.ReturnValueTsType ?? "Record<string, never> // 戻り値なし"}}
                    """)}}
                    }
                    """,
            };

            static string EscapeTs(string value) => value.Replace("'", "\\'");
        }
    }
}

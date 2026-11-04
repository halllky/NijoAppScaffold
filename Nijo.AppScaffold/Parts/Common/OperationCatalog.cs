using Nijo.CodeGenerating;
using Nijo.DataStructures.Aggregates;
using Nijo.Parts.CSharp;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nijo.Parts.Common {
    /// <summary>
    /// スキーマ定義から生成されるクエリ（一覧検索、参照検索）とコマンドの一覧。
    /// C# 側にはプレゼンテーション層が名前で処理を特定して呼び出すためのカタログを、
    /// TypeScript 側にはそれと同じ名前をキーにした引数・戻り値の型とURLの対応表を生成する。
    /// 同じ定義から両方を生成することで、クライアントとサーバーの型定義やURLのずれを防ぐ。
    /// </summary>
    internal sealed class OperationCatalog : ISharedSourceFile<CodeRenderingContext> {

        internal const string CS_CLASS = "Operation";
        internal const string CS_CATALOG = "Operations";
        internal const string CS_VISITOR = "IOperationVisitor";
        internal const string CS_NO_PARAMETER = "NoParameter";
        internal const string CS_NO_RETURN_VALUE = "NoReturnValue";

        private const string TS_FILE_NAME = "operation";
        private const string TS_NAMESPACE = "Operation";

        /// <summary>
        /// クエリまたはコマンド1件の定義。
        /// </summary>
        internal sealed class Definition {
            /// <summary>名前。C# と TypeScript で共通のキー</summary>
            internal required string Name { get; init; }
            /// <summary>C# でこの処理を参照するときの static プロパティ名</summary>
            internal required string CsPropertyName { get; init; }
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
        internal OperationCatalog Add(ISectionRoot root, params Definition[] definitions) {
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
            ThrowIfCsPropertyNameConflicts(definitions);

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCSharp(ctx, definitions));
                });
            });
            ctx.ReactProject(dir => {
                dir.Generate(RenderTypeScript(definitions));
            });
        }

        /// <summary>
        /// 処理ごとの static プロパティ名がほかの処理のプロパティ名と重複していれば例外を投げる。
        /// 重複したまま生成すると、生成されたソースがコンパイルできないため。
        /// </summary>
        private static void ThrowIfCsPropertyNameConflicts(Definition[] definitions) {
            var duplicates = definitions
                .GroupBy(def => def.CsPropertyName)
                .Where(group => group.Count() >= 2)
                .ToArray();
            if (duplicates.Length > 0) {
                throw new InvalidOperationException(
                    $"次の処理は、C# のプロパティ名が重複するため生成できません。物理名を変更してください: " +
                    duplicates.Select(group => $"{group.Key}（{group.Select(def => def.DisplayName).Join(", ")}）").Join("; "));
            }
        }

        private static SourceFile RenderCSharp(CodeRenderingContext ctx, Definition[] definitions) {
            const string C = ApplicationService.CONTEXT_TYPE_PARAMETER;
            var ns = ctx.Config.RootNamespace;

            // 処理のプロパティを宣言するクラスとその型引数の名前は利用者が書くことがないので、処理のプロパティ名と重複しないものを選ぶ。
            // C# ではメンバーに、宣言しているクラスやその型引数と同じ名前を付けられないため
            var propertyNames = definitions.Select(def => def.CsPropertyName).ToHashSet();
            string Unique(string name) {
                while (propertyNames.Contains(name)) name += "_";
                return name;
            }
            var propertiesClass = Unique($"{CS_CATALOG}Base");
            var propertiesTypeParameter = Unique(C);

            return new SourceFile {
                FileName = $"{CS_CLASS}.cs",
                Contents = $$"""
                    namespace {{ns}};

                    /// <summary>
                    /// スキーマ定義から生成されたクエリまたはコマンド1件。
                    /// Web、バッチ、単体テストなどの呼び出し元は、<see cref="{{CS_CATALOG}}{TOperationContext}"/> の static プロパティまたは <see cref="All"/> から処理を特定して呼び出す。
                    /// 引数・戻り値・メッセージの型を使った処理は <see cref="Accept"/> で <see cref="{{CS_VISITOR}}{TOperationContext, TResult}"/> に委ねる。
                    /// </summary>
                    /// <typeparam name="{{C}}">クエリ・コマンドの呼び出し1回について、全処理に共通の入出力の型</typeparam>
                    public abstract class {{CS_CLASS}}<{{C}}> {
                        private protected {{CS_CLASS}}() { }

                        /// <summary>名前。TypeScript 側の型定義のキーと一致する。</summary>
                        public required string Name { get; init; }
                        /// <summary>表示用名称</summary>
                        public required string DisplayName { get; init; }
                        /// <summary>WebAPI として公開する場合のURLのパス。TypeScript 側のURLの定義と一致する。</summary>
                        public required string Path { get; init; }
                        /// <summary>
                        /// クエリなら true、コマンドなら false。
                        /// クエリはデータを変更しないため、更新前に利用者の確認を求める必要がない。
                        /// </summary>
                        public required bool IsQuery { get; init; }

                        /// <summary>
                        /// このインスタンスの引数・戻り値・メッセージの型を型引数にして <paramref name="visitor"/> を呼び出します。
                        /// </summary>
                        public abstract TResult Accept<TResult>({{CS_VISITOR}}<{{C}}, TResult> visitor);

                        /// <summary>引数のないコマンドの引数の型</summary>
                        public sealed class {{CS_NO_PARAMETER}} { }
                        /// <summary>戻り値のないコマンドの戻り値の型。インスタンスは作られず、戻り値は常に null になる。</summary>
                        public sealed class {{CS_NO_RETURN_VALUE}} { }

                        /// <summary>
                        /// すべてのクエリとコマンド
                        /// </summary>
                        public static IReadOnlyList<{{CS_CLASS}}<{{C}}>> All { get; } = [
                    {{definitions.SelectTextTemplate(def => $$"""
                            global::{{ns}}.{{propertiesClass}}<{{C}}>.{{def.CsPropertyName}},
                    """)}}
                        ];
                    }

                    /// <summary>
                    /// スキーマ定義から生成されたクエリとコマンドの一覧。
                    /// 処理ごとに、処理の物理名をプロパティ名とする static プロパティを持つ。
                    /// 各プロパティは常に同じインスタンスを返すので、処理の特定は参照の比較で行える。
                    /// </summary>
                    /// <typeparam name="{{C}}">クエリ・コマンドの呼び出し1回について、全処理に共通の入出力の型</typeparam>
                    public abstract class {{CS_CATALOG}}<{{C}}> : {{propertiesClass}}<{{C}}> {
                        private {{CS_CATALOG}}() { }
                    }

                    /// <summary>
                    /// Operations の static プロパティの実体。直接参照せず、Operations を通して参照すること。
                    /// 処理の物理名がどんな名前でもプロパティにできるよう、ほかのメンバーを持たないこのクラスでプロパティを宣言し、Operations はそれを継承するだけにしている。
                    /// このクラスと型引数の名前は、処理のプロパティ名と重複しないように生成時に選ばれる。
                    /// 同じ理由で、プロパティの中で参照する型はすべて global:: から始まる名前で書いている。
                    /// </summary>
                    public abstract class {{propertiesClass}}<{{propertiesTypeParameter}}> {
                        private protected {{propertiesClass}}() { }

                    {{definitions.SelectTextTemplate(def => $$"""
                        {{WithIndent(RenderCSharpItem(def, ns, propertiesTypeParameter))}}
                    """)}}
                    }

                    /// <inheritdoc cref="{{CS_CLASS}}{TOperationContext}"/>
                    /// <typeparam name="TParameter">引数の型</typeparam>
                    /// <typeparam name="TReturnValue">戻り値の型</typeparam>
                    /// <typeparam name="TMessages">引数に対するメッセージの型</typeparam>
                    public sealed class {{CS_CLASS}}<{{C}}, TParameter, TReturnValue, TMessages> : {{CS_CLASS}}<{{C}}> where TMessages : {{MessageContainer.SETTER_INTERFACE}} {
                        /// <summary>
                        /// 処理を実行します。
                        /// 処理の結果は、戻り値と、引数に対するメッセージ（エラーなど）と、呼び出し1回に共通の入出力に格納される。
                        /// </summary>
                        public required Func<{{ApplicationService.ABSTRACT_CLASS_WITH_TYPE_PARAMETER}}, TParameter, TMessages, {{C}}, Task<TReturnValue?>> InvokeAsync { get; init; }

                        /// <inheritdoc/>
                        public override TResult Accept<TResult>({{CS_VISITOR}}<{{C}}, TResult> visitor) => visitor.Visit(this);
                    }

                    /// <summary>
                    /// <see cref="{{CS_CLASS}}{TOperationContext}"/> の引数・戻り値・メッセージの型を使った処理。
                    /// </summary>
                    public interface {{CS_VISITOR}}<{{C}}, out TResult> {
                        TResult Visit<TParameter, TReturnValue, TMessages>({{CS_CLASS}}<{{C}}, TParameter, TReturnValue, TMessages> operation)
                            where TMessages : {{MessageContainer.SETTER_INTERFACE}};
                    }
                    """,
            };
        }

        /// <summary>
        /// object から継承されるメンバーの名前。処理のプロパティがこれらと同じ名前の場合は new 修飾子を付けて隠す。
        /// </summary>
        private static readonly HashSet<string> OBJECT_MEMBER_NAMES = [
            nameof(Equals), nameof(GetHashCode), nameof(GetType), nameof(ToString), nameof(ReferenceEquals), nameof(MemberwiseClone),
        ];

        private static string RenderCSharpItem(Definition def, string ns, string contextTypeParameter) {
            // プロパティ名と同じ名前の型があっても、プロパティではなく型として解釈されるよう、型名は global:: から書く
            string Qualify(string csType) => Regex.Replace(csType, @"(?<![\w.:])(?=[\p{L}_])", $"global::{ns}.");

            var operationType = $"global::{ns}.{CS_CLASS}<{contextTypeParameter}>";
            var parameterType = def.ParameterCsType == null ? $"{operationType}.{CS_NO_PARAMETER}" : Qualify(def.ParameterCsType);
            var returnValueType = def.ReturnValueCsType == null ? $"{operationType}.{CS_NO_RETURN_VALUE}" : Qualify(def.ReturnValueCsType);

            // 引数や戻り値がない処理も、型引数をそろえて同じ形で呼び出せるようにする
            var parameterName = def.ParameterCsType == null ? "_" : "parameter";
            var arguments = def.ParameterCsType == null
                ? "messages, context"
                : "parameter, messages, context";
            var invoke = def.ReturnValueCsType == null
                ? $"async (app, {parameterName}, messages, context) => {{ await app.{def.AppSrvMethodName}({arguments}); return null; }}"
                : $"(app, {parameterName}, messages, context) => app.{def.AppSrvMethodName}({arguments})";

            var type = $"global::{ns}.{CS_CLASS}<{contextTypeParameter}, {parameterType}, {returnValueType}, {Qualify(def.MessagesCsType)}>";
            var modifiers = OBJECT_MEMBER_NAMES.Contains(def.CsPropertyName) ? "public static new" : "public static";

            return $$"""
                /// <summary>{{def.DisplayName}}</summary>
                {{modifiers}} {{type}} {{def.CsPropertyName}} { get; } = new() {
                    Name = "{{def.Name.Replace("\"", "\\\"")}}",
                    DisplayName = "{{def.DisplayName.Replace("\"", "\\\"")}}",
                    Path = "{{def.Path}}",
                    IsQuery = {{(def.IsQuery ? "true" : "false")}},
                    InvokeAsync = {{invoke}},
                };
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

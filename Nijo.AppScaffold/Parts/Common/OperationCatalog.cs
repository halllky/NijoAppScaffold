using Nijo.CodeGenerating;
using Nijo.DataStructures;
using Nijo.DataStructures.Aggregates;
using Nijo.Parts.CSharp;
using Nijo.Parts.JavaScript;
using Nijo.SchemaParsing;
using Nijo.StaticEnums;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace Nijo.Parts.Common {
    /// <summary>
    /// スキーマ定義から生成されるクエリ（一覧検索、参照検索）とコマンドの一覧。
    /// C# 側には、呼び出し元が処理を列挙して型安全に呼び出すためのカタログと、
    /// 処理ごとに nijo.xml に定義された情報（メタデータ）と、すべての処理が通る関門（InterceptAsync）を生成する。
    /// TypeScript 側には、それと同じ名前をキーにした引数・戻り値の型、URL、メタデータの対応表を生成する。
    /// 同じ定義から両方を生成することで、クライアントとサーバーの型定義・URL・メタデータのずれを防ぐ。
    /// </summary>
    internal sealed class OperationCatalog : ISharedSourceFile<CodeRenderingContext> {

        internal const string CS_CLASS = "Operation";
        internal const string CS_VISITOR = "IOperationVisitor";
        internal const string CS_NO_PARAMETER = "NoParameter";
        internal const string CS_NO_RETURN_VALUE = "NoReturnValue";
        private const string CS_KIND = "E_OperationKind";
        private const string CS_METADATA = "OperationMetadata";
        /// <summary>すべての処理が本体を実行する前に通る、アプリケーションサービスのメソッド</summary>
        internal const string CS_INTERCEPT = "InterceptAsync";

        private const string TS_FILE_NAME = "operation";
        private const string TS_NAMESPACE = "Operation";

        /// <summary>
        /// クエリまたはコマンドの種類
        /// </summary>
        internal enum E_Kind {
            /// <summary>一覧検索</summary>
            Load,
            /// <summary>参照検索</summary>
            LoadRefs,
            /// <summary>コマンド</summary>
            Command,
        }

        /// <summary>
        /// クエリまたはコマンド1件の定義。
        /// </summary>
        internal sealed class Definition {
            /// <summary>名前。C# と TypeScript で共通のキー</summary>
            internal required string Name { get; init; }
            /// <summary>
            /// この処理を表すインスタンスを保持する、アプリケーションサービスの static フィールドの名前。
            /// <see cref="GetCsInstanceName"/> で決める。
            /// </summary>
            internal required string CsInstanceName { get; init; }
            internal required string DisplayName { get; init; }
            /// <summary>WebAPI として公開する場合のURLのパス</summary>
            internal required string Path { get; init; }
            internal required E_Kind Kind { get; init; }
            /// <summary>
            /// メタデータのカスタム属性を読み取るXML要素。
            /// 参照検索の場合は参照先のクエリモデルのルート要素で、一覧検索の宣言を引き継ぐ。
            /// </summary>
            internal required XElement MetadataSource { get; init; }
            /// <summary>この処理を呼び出すアプリケーションサービスの公開メソッド名</summary>
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

        /// <summary>
        /// 処理を表すインスタンスを保持する、アプリケーションサービスの static フィールドの名前を返します。
        /// アプリケーションサービスの公開メソッドは、このフィールドを <see cref="CS_INTERCEPT"/> に渡す。
        /// </summary>
        /// <param name="csName">処理を識別するC#の識別子として使える名前</param>
        internal static string GetCsInstanceName(string csName) => $"{csName}Operation";

        /// <summary>
        /// アプリケーションサービスの公開メソッドから、<see cref="CS_INTERCEPT"/> を通して処理の本体を呼び出すC#の式を返します。
        /// 引数のない処理の場合、関門には引数の代わりに <see cref="CS_NO_PARAMETER"/> のインスタンスを渡す。
        /// </summary>
        /// <param name="csInstanceName"><see cref="GetCsInstanceName"/> で決めた名前</param>
        /// <param name="parameter">引数を表す式。引数のない処理の場合は null</param>
        /// <param name="body">処理の本体を実行する非同期のラムダ式</param>
        internal static string RenderIntercept(string csInstanceName, string? parameter, string body) {
            var parameterExpr = parameter ?? $"new {CS_CLASS}<{ApplicationService.CONTEXT_TYPE_PARAMETER}>.{CS_NO_PARAMETER}()";
            return $"{CS_INTERCEPT}({csInstanceName}, {parameterExpr}, messages, context, {body})";
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
            ThrowIfCsInstanceNameConflicts(definitions);

            var metadataAttributes = GetMetadataAttributes(ctx, definitions);

            ctx.CoreLibrary(dir => {
                dir.Directory("Util", utilDir => {
                    utilDir.Generate(RenderCSharp(ctx, definitions, metadataAttributes));
                });
            });
            ctx.ReactProject(dir => {
                dir.Generate(RenderTypeScript(definitions, metadataAttributes));
            });
        }

        /// <summary>
        /// 処理ごとのインスタンスのフィールド名がほかの処理と重複していれば例外を投げる。
        /// 重複したまま生成すると、生成されたソースがコンパイルできないため。
        /// </summary>
        private static void ThrowIfCsInstanceNameConflicts(Definition[] definitions) {
            var duplicates = definitions
                .GroupBy(def => def.CsInstanceName)
                .Where(group => group.Count() >= 2)
                .ToArray();
            if (duplicates.Length > 0) {
                throw new InvalidOperationException(
                    $"次の処理は、C# の名前が重複するため生成できません。物理名を変更してください: " +
                    duplicates.Select(group => $"{group.Key}（{group.Select(def => def.DisplayName).Join(", ")}）").Join("; "));
            }
        }

        /// <summary>
        /// メタデータに載せるカスタム属性と、それがすべての処理で指定されているか。
        /// クエリモデルとコマンドのルート要素に1つでも指定されているカスタム属性を載せる。
        /// メンバーにだけ指定されるカスタム属性（マスキングなど）まで載せると、処理のメタデータとして意味の無いプロパティが並ぶため。
        /// </summary>
        private static (NijoXmlCustomAttribute Attribute, bool IsAlwaysSpecified)[] GetMetadataAttributes(CodeRenderingContext ctx, Definition[] definitions) {
            return ctx.Schema.CustomAttributes
                .Where(attr => !string.IsNullOrWhiteSpace(attr.PhysicalName)
                            && definitions.Any(def => attr.GetValues(def.MetadataSource).Length > 0))
                .Select(attr => (attr, definitions.All(def => attr.GetValues(def.MetadataSource).Length > 0)))
                .ToArray();
        }

        private static SourceFile RenderCSharp(CodeRenderingContext ctx, Definition[] definitions, (NijoXmlCustomAttribute Attribute, bool IsAlwaysSpecified)[] metadataAttributes) {
            const string C = ApplicationService.CONTEXT_TYPE_PARAMETER;
            var ns = ctx.Config.RootNamespace;

            return new SourceFile {
                FileName = $"{CS_CLASS}.cs",
                Contents = $$"""
                    namespace {{ns}};

                    /// <summary>
                    /// スキーマ定義から生成されたクエリまたはコマンド1件。
                    /// Web、バッチ、単体テストなどの呼び出し元は、<see cref="All"/> から処理を列挙し、<see cref="Name"/> や <see cref="Metadata"/> で処理を特定する。
                    /// 引数・戻り値・メッセージの型を使った処理は <see cref="Accept"/> で <see cref="{{CS_VISITOR}}{TOperationContext, TResult}"/> に委ねる。
                    /// </summary>
                    /// <typeparam name="{{C}}">クエリ・コマンドの呼び出し1回について、全処理に共通の入出力の型</typeparam>
                    public abstract class {{CS_CLASS}}<{{C}}> {
                        private protected {{CS_CLASS}}() { }

                        /// <summary>名前。TypeScript 側の型定義のキーと一致する。</summary>
                        public required string Name { get; init; }
                        /// <summary>処理の種類</summary>
                        public required {{CS_KIND}} Kind { get; init; }
                        /// <summary>WebAPI として公開する場合のURLのパス。TypeScript 側のURLの定義と一致する。</summary>
                        public required string Path { get; init; }
                        /// <summary>nijo.xml に定義された、この処理の情報。TypeScript 側のメタデータと一致する。</summary>
                        public required {{CS_METADATA}} Metadata { get; init; }

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
                            global::{{ns}}.{{ApplicationService.ABSTRACT_CLASS}}<{{C}}>.{{def.CsInstanceName}},
                    """)}}
                        ];
                    }

                    /// <inheritdoc cref="{{CS_CLASS}}{TOperationContext}"/>
                    /// <typeparam name="TParameter">引数の型</typeparam>
                    /// <typeparam name="TReturnValue">戻り値の型</typeparam>
                    /// <typeparam name="TMessages">引数に対するメッセージの型</typeparam>
                    public sealed class {{CS_CLASS}}<{{C}}, TParameter, TReturnValue, TMessages> : {{CS_CLASS}}<{{C}}> where TMessages : {{MessageContainer.SETTER_INTERFACE}} {
                        /// <summary>
                        /// 処理を実行します。
                        /// アプリケーションサービスの公開メソッドを呼ぶので、<see cref="{{ApplicationService.ABSTRACT_CLASS}}{TOperationContext}.{{CS_INTERCEPT}}"/> を通る。
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

                    /// <summary>
                    /// クエリまたはコマンドの種類
                    /// </summary>
                    public enum {{CS_KIND}} {
                        /// <summary>一覧検索</summary>
                        {{E_Kind.Load}},
                        /// <summary>参照検索</summary>
                        {{E_Kind.LoadRefs}},
                        /// <summary>コマンド</summary>
                        {{E_Kind.Command}},
                    }

                    /// <summary>
                    /// クエリまたはコマンドについて、nijo.xml に定義された情報。
                    /// クエリモデルとコマンドのルート要素に指定されたカスタム属性がプロパティになる。
                    /// 参照検索は、参照先のクエリモデルのルート要素に指定された値を引き継ぐ。
                    /// </summary>
                    public sealed class {{CS_METADATA}} {
                        /// <summary>表示用名称</summary>
                        public required string DisplayName { get; init; }
                    {{metadataAttributes.SelectTextTemplate(x => $$"""
                        /// <summary>{{x.Attribute.DisplayName ?? x.Attribute.PhysicalName}}</summary>
                        public required {{NijoAttr.GetCsValueType(ctx, x.Attribute, !x.IsAlwaysSpecified)}} {{x.Attribute.PhysicalName}} { get; init; }
                    """)}}
                    }

                    partial class {{ApplicationService.ABSTRACT_CLASS_WITH_TYPE_PARAMETER}} {

                        /// <summary>
                        /// すべてのクエリとコマンドが、本体を実行する前に通る関門。
                        /// 処理の公開メソッドは、どこから呼ばれても必ずこのメソッドを通して本体を実行する。
                        /// 権限の確認やログなど、処理の種類を問わない共通の処理が必要な場合はオーバーライドする。
                        /// オーバーライドするメソッドには、戻り値の型の null 許容のために where TReturnValue : default が必要。
                        /// 既定では何もせずに本体を実行する。
                        /// </summary>
                        /// <param name="operation">呼び出された処理</param>
                        /// <param name="parameter">処理の引数。引数のない処理の場合は <see cref="{{CS_CLASS}}{TOperationContext}.{{CS_NO_PARAMETER}}"/> のインスタンス</param>
                        /// <param name="messages">引数に対するエラーや警告などのメッセージの格納先</param>
                        /// <param name="context">この呼び出しについての、全処理に共通の入出力</param>
                        /// <param name="invokeAsync">処理の本体を実行する。呼ばなければ本体は実行されない</param>
                        /// <returns>処理の戻り値。戻り値のない処理の場合は常に null</returns>
                        protected virtual Task<TReturnValue?> {{CS_INTERCEPT}}<TParameter, TReturnValue, TMessages>(
                            {{CS_CLASS}}<{{C}}, TParameter, TReturnValue, TMessages> operation,
                            TParameter parameter,
                            TMessages messages,
                            {{C}} context,
                            Func<Task<TReturnValue?>> invokeAsync) where TMessages : {{MessageContainer.SETTER_INTERFACE}} {
                            return invokeAsync();
                        }
                    {{definitions.SelectTextTemplate(def => $$"""

                        {{WithIndent(RenderCSharpInstance(ctx, def, metadataAttributes))}}
                    """)}}
                    }
                    """,
            };
        }

        private static string RenderCSharpInstance(CodeRenderingContext ctx, Definition def, (NijoXmlCustomAttribute Attribute, bool IsAlwaysSpecified)[] metadataAttributes) {
            const string C = ApplicationService.CONTEXT_TYPE_PARAMETER;
            var ns = ctx.Config.RootNamespace;

            // 処理の物理名と同じ名前の型があっても、フィールドではなく型として解釈されるよう、型名は global:: から書く
            string Qualify(string csType) => Regex.Replace(csType, @"(?<![\w.:])(?=[\p{L}_])", $"global::{ns}.");

            var operationType = $"global::{ns}.{CS_CLASS}<{C}>";
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

            var type = $"global::{ns}.{CS_CLASS}<{C}, {parameterType}, {returnValueType}, {Qualify(def.MessagesCsType)}>";

            return $$"""
                /// <summary>{{def.DisplayName}}</summary>
                internal static readonly {{type}} {{def.CsInstanceName}} = new() {
                    Name = "{{def.Name.Replace("\"", "\\\"")}}",
                    Kind = global::{{ns}}.{{CS_KIND}}.{{def.Kind}},
                    Path = "{{def.Path}}",
                    Metadata = new() {
                        DisplayName = "{{def.DisplayName.Replace("\"", "\\\"")}}",
                {{metadataAttributes.SelectTextTemplate(x => $$"""
                        {{x.Attribute.PhysicalName}} = {{NijoAttr.RenderCsValue(ctx, x.Attribute, def.MetadataSource)}},
                """)}}
                    },
                    InvokeAsync = {{invoke}},
                };
                """;
        }

        private static SourceFile RenderTypeScript(Definition[] definitions, (NijoXmlCustomAttribute Attribute, bool IsAlwaysSpecified)[] metadataAttributes) {
            var modules = definitions
                .SelectMany(def => def.TsReferencedAggregates)
                .Select(agg => new TypeScriptSectionRootModule(agg.GetRoot()))
                .DistinctBy(module => module.ImportAlias)
                .OrderBy(module => module.ImportAlias);
            var usesEnumDefs = metadataAttributes.Any(x => x.Attribute.Type == NijoXmlCustomAttribute.E_Type.StaticEnum);

            return new SourceFile {
                FileName = $"{TS_FILE_NAME}.ts",
                Contents = $$"""
                    import type * as Util from "./index"
                    {{If(usesEnumDefs, () => $$"""
                    import type * as EnumDefs from "./{{Path.GetFileNameWithoutExtension(EnumFile.TS_FILENAME)}}"
                    """)}}
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

                    /**
                     * クエリまたはコマンドについて、nijo.xml に定義された情報の型。
                     * クエリモデルとコマンドのルート要素に指定されたカスタム属性がプロパティになる。
                     * 参照検索は、参照先のクエリモデルのルート要素に指定された値を引き継ぐ。
                     */
                    export type Metadata = {
                      /** 表示用名称 */
                      displayName: string
                    {{metadataAttributes.SelectTextTemplate(x => $$"""
                      /** {{x.Attribute.DisplayName ?? x.Attribute.PhysicalName}} */
                      {{x.Attribute.PhysicalName!.ToCamelCase()}}: {{AggregateMetadata.RenderTsCustomAttributeType(x.Attribute)}}{{(IsTsNullable(x) ? " | undefined" : "")}}
                    """)}}
                    }

                    /** クエリとコマンドのメタデータの一覧。C# 側の各処理の Metadata と一致する。 */
                    export const Metadata: { [K in Name]: Metadata } = {
                    {{definitions.SelectTextTemplate(def => $$"""
                      '{{EscapeTs(def.Name)}}': {
                        displayName: '{{EscapeTs(def.DisplayName)}}',
                    {{metadataAttributes.SelectTextTemplate(x => $$"""
                        {{x.Attribute.PhysicalName!.ToCamelCase()}}: {{AggregateMetadata.RenderTsCustomAttributeValue(x.Attribute, def.MetadataSource)}},
                    """)}}
                      },
                    """)}}
                    }
                    """,
            };

            static string EscapeTs(string value) => value.Replace("\\", "\\\\").Replace("'", "\\'");

            // Boolean と、値を複数持てるものは、指定が無くても false や空の配列になるので undefined にならない
            static bool IsTsNullable((NijoXmlCustomAttribute Attribute, bool IsAlwaysSpecified) x) => !x.IsAlwaysSpecified
                && x.Attribute.Type != NijoXmlCustomAttribute.E_Type.Boolean
                && !x.Attribute.IsMultiple;
        }
    }
}

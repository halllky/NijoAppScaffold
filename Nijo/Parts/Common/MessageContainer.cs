using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.Parts.Common {
    /// <summary>
    /// エラーメッセージなどのメッセージの入れ物。
    /// 対応するモデルと同じ形の構造を持つ。
    ///
    /// <code>インスタンス.プロパティ名.AddError(メッセージ)</code> のように直感的に書ける、
    /// 無駄なペイロードを避けるためにメッセージが無いときはJSON化されない、といった性質を持つ。
    /// </summary>
    internal abstract class MessageContainer {

        protected MessageContainer(AggregateBase aggregate) {
            _aggregate = aggregate;
        }
        protected readonly AggregateBase _aggregate;

        internal virtual string CsClassName => $"{_aggregate.PhysicalName}Messages";
        internal virtual string TsTypeName => $"{_aggregate.PhysicalName}Messages";

        /// <summary>
        /// C#クラスが何らかの基底クラスやインターフェースを実装するなら使う
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetCsClassImplements() {
            yield break;
        }

        /// <summary>
        /// このクラスに定義されるメンバーを列挙する。
        /// </summary>
        protected abstract IEnumerable<IMessageContainerMember> GetMembers();

        /// <summary>
        /// C#のクラス定義に追加で何かレンダリングが必要なコードがあればこれをオーバーライドして記載
        /// </summary>
        protected virtual string RenderCSharpAdditionalSource() {
            return SKIP_MARKER;
        }

        internal string RenderCSharp() {
            var impl = new List<string>() { CONCRETE_CLASS };
            impl.AddRange(GetCsClassImplements());

            var members = GetMembers().ToArray();

            return $$"""
                /// <summary>
                /// {{_aggregate.DisplayName}} のデータ構造と対応したメッセージの入れ物
                /// </summary>
                public class {{CsClassName}} : {{impl.Join(", ")}} {
                    public {{CsClassName}}(IEnumerable<string> path, PresentationMessageContext context) : base(path, context) {
                {{members.SelectTextTemplate(m => $$"""
                {{If(m.NestedObject == null, () => $$"""
                        this.{{m.PhysicalName}} = new {{CONCRETE_CLASS}}([.. path, "{{m.PhysicalName}}"], context);
                """).ElseIf(!m.IsArray, () => $$"""
                        this.{{m.PhysicalName}} = new {{m.NestedObject?.CsClassName}}([.. path, "{{m.PhysicalName}}"], context);
                """).Else(() => $$"""
                        this.{{m.PhysicalName}} = new {{CONCRETE_CLASS_LIST}}<{{m.NestedObject?.CsClassName}}>([.. path, "{{m.PhysicalName}}"], rowIndex => {
                            return new {{m.NestedObject?.CsClassName}}([.. path, "{{m.PhysicalName}}", rowIndex.ToString()], context);
                        }, context);
                """)}}
                """)}}
                    }

                {{members.SelectTextTemplate(m => $$"""
                    /// <summary>{{m.DisplayName}}に対して発生したメッセージの入れ物</summary>
                {{If(m.NestedObject == null, () => $$"""
                    public {{m.CsType ?? INTERFACE}} {{m.PhysicalName}} { get; }
                """).ElseIf(!m.IsArray, () => $$"""
                    public {{m.CsType ?? m.NestedObject?.CsClassName}} {{m.PhysicalName}} { get; }
                """).Else(() => $$"""
                    public {{m.CsType ?? $"{INTERFACE_LIST}<{m.NestedObject?.CsClassName}>"}} {{m.PhysicalName}} { get; }
                """)}}
                """)}}
                    {{WithIndent(RenderCSharpAdditionalSource(), "    ")}}
                }
                """;
        }
        internal string RenderTypeScript() {
            var members = GetMembers().ToArray();

            return $$"""
                /** {{_aggregate.DisplayName}} のデータ構造と対応したメッセージの入れ物 */
                export type {{TsTypeName}} = {
                  {{WithIndent(RenderBody(this), "  ")}}
                }
                """;

            static IEnumerable<string> RenderBody(MessageContainer message) {
                foreach (var member in message.GetMembers()) {
                    if (member.NestedObject == null) {
                        yield return $$"""
                            {{member.PhysicalName}}?: Util.{{TS_CONTAINER}}
                            """;

                    } else if (!member.IsArray) {
                        yield return $$"""
                            {{member.PhysicalName}}?: {
                              {{WithIndent(RenderBody(member.NestedObject), "  ")}}
                            }
                            """;

                    } else {
                        yield return $$"""
                            {{member.PhysicalName}}?: {
                              [key: `${number}`]: {
                                {{WithIndent(RenderBody(member.NestedObject), "    ")}}
                              }
                            }
                            """;
                    }
                }
            }
        }


        #region 基底クラス
        internal const string INTERFACE = "IMessageContainer";
        internal const string INTERFACE_LIST = "IMessageContainerList";
        internal const string CONCRETE_CLASS = "MessageContainer";
        internal const string CONCRETE_CLASS_LIST = "MessageContainerList";

        /// <summary>既定のクラスを探して返すstaticメソッド</summary>
        internal const string GET_DEFAULT_CLASS = "GetDefaultClass";

        internal const string TS_CONTAINER = "MessageContainer";
        private const string TS_ERROR = "error";
        private const string TS_WARN = "warn";
        private const string TS_INFO = "info";
        private const string TS_CHILDREN = "children";

        internal class BaseClass : IMultiAggregateSourceFile {

            private readonly Dictionary<string, string> _registered = new();
            private readonly Lock _lock = new();
            /// <summary>
            /// <see cref="GET_DEFAULT_CLASS"/> の内容を登録する。
            /// </summary>
            /// <param name="interfaceName"></param>
            /// <param name="concreteClassName"></param>
            internal BaseClass Register(string interfaceName, string concreteClassName) {
                lock (_lock) {
                    _registered.Add(interfaceName, concreteClassName);
                    return this;
                }
            }

            void IMultiAggregateSourceFile.RegisterDependencies(IMultiAggregateSourceFileManager ctx) {
                // 特になし
            }

            void IMultiAggregateSourceFile.Render(CodeRenderingContext ctx) {
                ctx.CoreLibrary(autoGenerated => {
                    autoGenerated.Directory("Util", dir => {
                        dir.Generate(RenderCSharpBaseClass(ctx));
                    });
                });
                ctx.ReactProject(autoGenerated => {
                    autoGenerated.Directory("util", dir => {
                        dir.Generate(RenderTypeScriptBaseFile());
                    });
                });
            }

            /// <summary>
            /// 基底クラスのレンダリング
            /// </summary>
            private SourceFile RenderCSharpBaseClass(CodeRenderingContext ctx) {
                var registered = new Dictionary<string, string>(_registered) {
                    { INTERFACE, CONCRETE_CLASS },
                    { CONCRETE_CLASS, CONCRETE_CLASS },
                };

                return new SourceFile {
                    FileName = "MessageContainer.cs",
                    Contents = $$"""
                        using System.Collections;
                        using System.Text.Json;
                        using System.Text.Json.Nodes;

                        namespace {{ctx.Config.RootNamespace}};

                        /// <summary>
                        /// ユーザーの画面入力や外部システムからのデータ連携でエラーが起きた際、
                        /// どの項目でエラー等が発生したかの明示が要求されることがしばしばある。
                        /// このクラスは、エラー・警告・インフォメーションのメッセージを、
                        /// それがどの項目で発生したかの情報と紐づけて保持する。
                        /// </summary>
                        public sealed partial class PresentationMessageContext {

                            /// <summary>メッセージの種類</summary>
                            public enum E_MessageType {
                                Error,
                                Warn,
                                Info,
                            }
                            /// <summary>内部用データ保持クラス</summary>
                            private class MessageContainerStructure {
                                public List<string> Errors { get; } = [];
                                public List<string> Warns { get; } = [];
                                public List<string> Infos { get; } = [];
                                public Dictionary<string, MessageContainerStructure> Children { get; } = [];
                            }

                            private readonly MessageContainerStructure _root = new();

                            public void AddError(IEnumerable<string> path, string message) {
                                AddMessagePrivate(_root, E_MessageType.Error, path.ToArray(), message);
                            }
                            public void AddWarn(IEnumerable<string> path, string message) {
                                AddMessagePrivate(_root, E_MessageType.Warn, path.ToArray(), message);
                            }
                            public void AddInfo(IEnumerable<string> path, string message) {
                                AddMessagePrivate(_root, E_MessageType.Info, path.ToArray(), message);
                            }
                            private static void AddMessagePrivate(MessageContainerStructure current, E_MessageType messageType, string[] path, string message) {
                                if (path.Length == 0) {
                                    var list = messageType switch {
                                        E_MessageType.Error => current.Errors,
                                        E_MessageType.Warn => current.Warns,
                                        E_MessageType.Info => current.Infos,
                                        _ => throw new InvalidOperationException($"不明なメッセージ種類: {messageType}"),
                                    };
                                    list.Add(message);
                                } else {
                                    if (!current.Children.TryGetValue(path[0], out var child)) {
                                        child = new MessageContainerStructure();
                                        current.Children[path[0]] = child;
                                    }
                                    AddMessagePrivate(child, messageType, path.Skip(1).ToArray(), message);
                                }
                            }

                            /// <summary>
                            /// エラーメッセージが1件以上あるかどうかを返します。
                            /// </summary>
                            public bool HasError() {
                                return HasError([]);
                            }
                            /// <summary>
                            /// 指定したパスまたはそれ以下のエラーメッセージが1件以上あるかどうかを返します。
                            /// </summary>
                            /// <param name="path">オブジェクトルートから該当のインスタンスまでのパス</param>
                            public bool HasError(IEnumerable<string> path) {
                                var target = _root;
                                foreach (var p in path) {
                                    if (!target.Children.TryGetValue(p, out target)) return false;
                                }
                                return HasErrorPrivate(target);

                                static bool HasErrorPrivate(MessageContainerStructure current) {
                                    if (current.Errors.Count > 0) {
                                        return true;
                                    }
                                    foreach (var child in current.Children.Values) {
                                        if (HasErrorPrivate(child)) return true;
                                    }
                                    return false;
                                }
                            }

                            /// <summary>
                            /// このインスタンスをJsonNode型に変換します。
                            /// <list type="bullet">
                            /// <item>このメソッドは、このオブジェクトおよび子孫オブジェクトが持っているメッセージを再帰的に集め、以下のようなJSONオブジェクトに変換します。</item>
                            /// <item>メッセージコンテナは、エラー、警告、インフォメーションの3種類のメッセージを、それぞれ配列として持ちます。</item>
                            /// <item>エラーだけ持っているなど、一部の種類のメッセージのみ持っている場合、他の種類の配列は配列自体が存在しなくなります。</item>
                            /// <item>子要素は children という名前のオブジェクトにまとめて格納されます。</item>
                            /// <item>
                            /// 3種類のメッセージのいずれも持っていない項目のプロパティは存在しません。
                            /// 例えば以下のオブジェクトで「項目A」「項目B」以外に「項目X」が存在するが、Xにはメッセージが発生していない場合、Xのプロパティは存在しません。
                            /// </item>
                            /// <item>ネストされたオブジェクトのメッセージも生成されます。（下記「子オブジェクトのメッセージ」）</item>
                            /// <item>
                            /// 配列は、配列インデックスをキーとしたオブジェクトになります。（下記「子配列のメッセージ」）
                            /// 配列インデックスか否かは、 children 直下のオブジェクトのキーが半角整数のみから成るか否かで判定できます。
                            /// </item>
                            /// </list>
                            /// <code>
                            /// {
                            ///   "{{TS_ERROR}}": ["xxxがエラーです"],
                            ///   "{{TS_CHILDREN}}": {
                            ///     "項目A": { "{{TS_ERROR}}": ["xxxがエラーです"], "{{TS_WARN}}": ["xxxという警告があります"], "{{TS_INFO}}": ["xxxという情報があります"] },
                            ///     "項目B": { "{{TS_ERROR}}": ["xxxがエラーです", "yyyがエラーです"], "{{TS_WARN}}": ["xxxという警告があります"], "{{TS_INFO}}": ["xxxという情報があります"] },
                            ///     "子オブジェクトのメッセージ": {
                            ///       "{{TS_CHILDREN}}": {
                            ///         "項目C": { "{{TS_ERROR}}": ["xxxがエラーです"] },
                            ///         "項目D": { "{{TS_ERROR}}": ["xxxがエラーです"] },
                            ///       },
                            ///     },
                            ///     "子配列のメッセージ": {
                            ///       "{{TS_ERROR}}": ["xxxがエラーです"],
                            ///       "{{TS_CHILDREN}}": {
                            ///         "1": {
                            ///           "{{TS_CHILDREN}}": {
                            ///             "項目E": { "{{TS_ERROR}}": ["xxxがエラーです"] },
                            ///           },
                            ///         },
                            ///         "5": {
                            ///           "{{TS_ERROR}}": ["xxxがエラーです"],
                            ///           "{{TS_CHILDREN}}": {
                            ///             "項目E": { "{{TS_ERROR}}": ["xxxがエラーです"] },
                            ///           },
                            ///         },
                            ///       },
                            ///     }
                            ///   }
                            /// }
                            /// </code>
                            /// </summary>
                            public JsonObject ToJsonObject() {
                                return ToJsonObjectPrivate(_root) ?? [];

                                static JsonObject? ToJsonObjectPrivate(MessageContainerStructure current) {
                                    var result = new JsonObject();

                                    if (current.Errors.Count > 0) {
                                        var strArray = new JsonArray();
                                        foreach (var str in current.Errors) strArray.Add(str);
                                        result["{{TS_ERROR}}"] = strArray;
                                    }
                                    if (current.Warns.Count > 0) {
                                        var strArray = new JsonArray();
                                        foreach (var str in current.Warns) strArray.Add(str);
                                        result["{{TS_WARN}}"] = strArray;
                                    }
                                    if (current.Infos.Count > 0) {
                                        var strArray = new JsonArray();
                                        foreach (var str in current.Infos) strArray.Add(str);
                                        result["{{TS_INFO}}"] = strArray;
                                    }

                                    var children = new JsonObject();
                                    foreach (var child in current.Children) {
                                        var childJson = ToJsonObjectPrivate(child.Value);
                                        if (childJson != null) children[child.Key] = childJson;
                                    }
                                    if (children.Count > 0) result["{{TS_CHILDREN}}"] = children;

                                    return result.Count == 0 ? null : result;
                                }
                            }
                        }

                        #region インターフェース
                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物
                        /// </summary>
                        public interface {{INTERFACE}} {
                            /// <summary>
                            /// メッセージ本体が格納されているオブジェクト。
                            /// 現在発生しているメッセージはこのオブジェクトを通じて取得してください。
                            /// </summary>
                            PresentationMessageContext Root { get; }

                            /// <summary>エラーメッセージを付加します。</summary>
                            void AddError(string message);
                            /// <summary>警告メッセージを付加します。</summary>
                            void AddWarn(string message);
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            void AddInfo(string message);

                            /// <summary>このインスタンスまたはこのインスタンスの子孫が1件以上エラーを持っているか否かを返します。</summary>
                            bool HasError();
                        }
                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物の配列
                        /// </summary>
                        public interface {{INTERFACE_LIST}}<out T> : {{INTERFACE}}, IReadOnlyList<T> where T : {{INTERFACE}} {
                        }
                        #endregion インターフェース


                        #region 具象クラス
                        /// <inheritdoc cref="{{INTERFACE}}">
                        public partial class {{CONCRETE_CLASS}} : {{INTERFACE}} {
                            /// <inheritdoc cref="{{INTERFACE}}">
                            /// <param name="path">オブジェクトルートからこのインスタンスまでのパス</param>
                            public {{CONCRETE_CLASS}}(IEnumerable<string> path, PresentationMessageContext root) {
                                _path = path;
                                Root = root;
                            }
                            private readonly IEnumerable<string> _path;
                            public PresentationMessageContext Root { get; }

                            /// <summary>エラーメッセージを付加します。</summary>
                            public virtual void AddError(string message) {
                                Root.AddError(_path, message);
                            }
                            /// <summary>警告メッセージを付加します。</summary>
                            public virtual void AddWarn(string message) {
                                Root.AddWarn(_path, message);
                            }
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            public virtual void AddInfo(string message) {
                                Root.AddInfo(_path, message);
                            }

                            /// <summary>このインスタンスまたはこのインスタンスの子孫が1件以上エラーを持っているか否かを返します。</summary>
                            public bool HasError() {
                                return Root.HasError(_path);
                            }

                            /// <summary>
                            /// このインスタンスを指定した型にキャストして返します。
                            /// </summary>
                            public T Cast<T>() where T : {{INTERFACE}} {
                                return GetDefaultClass<T>(_path, Root);
                            }

                            /// <summary>
                            /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
                            /// </summary>
                            public static T GetDefaultClass<T>(IEnumerable<string> path, PresentationMessageContext context) where T : {{INTERFACE}} {
                                return (T)GetDefaultClass(typeof(T), path, context);
                            }
                            /// <summary>
                            /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
                            /// </summary>
                            public static {{INTERFACE}} GetDefaultClass(Type type, IEnumerable<string> path, PresentationMessageContext context) {
                        {{registered.OrderBy(kv => kv.Key).SelectTextTemplate(kv => $$"""
                                if (type == typeof({{kv.Key}})) return new {{kv.Value}}(path, context);
                        """)}}

                                // メッセージのリストの場合
                                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof({{INTERFACE_LIST}}<>)
                                                        || type.GetGenericTypeDefinition() == typeof({{CONCRETE_CLASS_LIST}}<>))) {
                                    var itemType = type.GetGenericArguments()[0];

                        {{registered.OrderBy(kv => kv.Key).SelectTextTemplate(kv => $$"""
                                    if (itemType == typeof({{kv.Key}})) return new {{CONCRETE_CLASS_LIST}}<{{kv.Key}}>(path, index => new {{kv.Value}}([.. path, index.ToString()], context), context);
                        """)}}
                                }

                                throw new InvalidOperationException($"{type.Name} には既定のメッセージコンテナクラスが存在しません。");
                            }
                        }

                        /// <inheritdoc cref="{{INTERFACE_LIST}}"/>
                        public partial class {{CONCRETE_CLASS_LIST}}<T> : {{CONCRETE_CLASS}}, {{INTERFACE_LIST}}<T> where T : {{INTERFACE}} {
                            public {{CONCRETE_CLASS_LIST}}(IEnumerable<string> path, Func<int, T> createItem, PresentationMessageContext context) : base(path, context) {
                                _createItem = createItem;
                            }

                            private readonly Func<int, T> _createItem;
                            private readonly Dictionary<int, T> _items = new();

                            public T this[int index] {
                                get {
                                    ArgumentOutOfRangeException.ThrowIfNegative(index);

                                    if (_items.TryGetValue(index, out var item)) {
                                        return item;
                                    } else {
                                        var newItem = _createItem(index);
                                        _items[index] = newItem;
                                        return newItem;
                                    }
                                }
                            }

                            public int Count => _items.Keys.Count == 0
                                ? 0
                                : (_items.Keys.Max() + 1);

                            public IEnumerator<T> GetEnumerator() {
                                if (_items.Count == 0) yield break;

                                var max = _items.Keys.Max() + 1;
                                for (int i = 0; i < max; i++) {
                                    yield return this[i];
                                }
                            }
                            IEnumerator IEnumerable.GetEnumerator() {
                                return GetEnumerator();
                            }
                        }
                        #endregion 具象クラス
                        """,
                };
            }
            private static SourceFile RenderTypeScriptBaseFile() {
                return new SourceFile {
                    FileName = "message-container.ts",
                    Contents = $$"""
                        /** サーバー側で発生したエラーメッセージ等の入れ物 */
                        export type {{TS_CONTAINER}} = {
                          /** エラーメッセージ */
                          {{TS_ERROR}}?: string[]
                          /** 警告メッセージ */
                          {{TS_WARN}}?: string[]
                          /** インフォメーション */
                          {{TS_INFO}}?: string[]
                          /** 子要素 */
                          {{TS_CHILDREN}}?: {
                            [key: string]: {{TS_CONTAINER}}
                          }
                        }
                        """,
                };
            }
        }
        #endregion 基底クラス


        #region メンバー
        internal interface IMessageContainerMember {
            string PhysicalName { get; }
            string DisplayName { get; }
            /// <summary>ChildまたはChildren</summary>
            MessageContainer? NestedObject { get; }
            /// <summary>未指定の場合はデフォルトの型になる</summary>
            string? CsType { get; }
            /// <summary>このメンバーをリストとしてレンダリングするか否か</summary>
            bool IsArray { get; }
        }
        #endregion メンバー
    }
}

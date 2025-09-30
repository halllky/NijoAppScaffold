using Nijo.CodeGenerating;
using Nijo.ImmutableSchema;
using Nijo.Util.DotnetEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nijo.Parts.CSharp {
    /// <summary>
    /// エラーメッセージなどのメッセージの入れ物。
    /// 対応するモデルと同じ形の構造を持つ。
    ///
    /// <code>インスタンス.プロパティ名.AddError(メッセージ)</code> のように直感的に書ける、
    /// 無駄なペイロードを避けるためにメッセージが無いときはJSON化されない、といった性質を持つ。
    /// </summary>
    internal static class MessageContainer {

        /// <summary>
        /// <see cref="MessageContainer"/> へのメッセージの追加を
        /// 直感的に書けるようにするためのヘルパークラスのレンダリングをおこなう。
        /// </summary>
        internal abstract class Setter {

            protected Setter(AggregateBase aggregate) {
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
            protected abstract IEnumerable<IMember> GetMembers();

            /// <summary>
            /// C#のクラス定義に追加で何かレンダリングが必要なコードがあればこれをオーバーライドして記載
            /// </summary>
            protected virtual string RenderCSharpAdditionalSource() {
                return SKIP_MARKER;
            }

            internal string RenderCSharp() {
                var impl = new List<string>() { SETTER_CLASS };
                impl.AddRange(GetCsClassImplements());

                var members = GetMembers().ToArray();

                return $$"""
                    /// <summary>
                    /// {{_aggregate.DisplayName}} のデータ構造と対応したメッセージの入れ物
                    /// </summary>
                    public class {{CsClassName}} : {{impl.Join(", ")}} {
                        public {{CsClassName}}(IEnumerable<string> path, {{CONTEXT_CLASS}} context) : base(path, context) {
                    {{members.SelectTextTemplate(m => $$"""
                    {{If(m.NestedObject == null, () => $$"""
                            this.{{m.PhysicalName}} = new {{SETTER_CLASS}}([.. path, "{{m.PhysicalName}}"], context);
                    """).ElseIf(!m.IsArray, () => $$"""
                            this.{{m.PhysicalName}} = new {{m.NestedObject?.CsClassName}}([.. path, "{{m.PhysicalName}}"], context);
                    """).Else(() => $$"""
                            this.{{m.PhysicalName}} = new {{SETTER_CONCRETE_CLASS_LIST}}<{{m.NestedObject?.CsClassName}}>([.. path, "{{m.PhysicalName}}"], rowIndex => {
                                return new {{m.NestedObject?.CsClassName}}([.. path, "{{m.PhysicalName}}", rowIndex.ToString()], context);
                            }, context);
                    """)}}
                    """)}}
                        }

                    {{members.SelectTextTemplate(m => $$"""
                        /// <summary>{{m.DisplayName}}に対して発生したメッセージの入れ物</summary>
                    {{If(m.NestedObject == null, () => $$"""
                        public {{m.CsType ?? SETTER_INTERFACE}} {{m.PhysicalName}} { get; }
                    """).ElseIf(!m.IsArray, () => $$"""
                        public {{m.CsType ?? m.NestedObject?.CsClassName}} {{m.PhysicalName}} { get; }
                    """).Else(() => $$"""
                        public {{m.CsType ?? $"{SETTER_INTERFACE_LIST}<{m.NestedObject?.CsClassName}>"}} {{m.PhysicalName}} { get; }
                    """)}}
                    """)}}
                        {{WithIndent(RenderCSharpAdditionalSource(), "    ")}}
                    }
                    """;
            }
        }


        #region 基底クラス
        private const string CONTEXT_CLASS = "MessageContainer";

        internal const string SETTER_INTERFACE = "IMessageSetter";
        internal const string SETTER_INTERFACE_LIST = "IMessageSetterList";
        internal const string SETTER_CLASS = "MessageSetter";
        internal const string SETTER_CONCRETE_CLASS_LIST = "MessageSetterList";

        /// <summary>既定のクラスを探して返すstaticメソッド</summary>
        internal const string GET_IMPL = "GetImpl";

        internal class BaseClass : IMultiAggregateSourceFile {

            private readonly Dictionary<string, string> _registered = new();
            private readonly Lock _lock = new();
            /// <summary>
            /// <see cref="GET_IMPL"/> の内容を登録する。
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
            }

            /// <summary>
            /// 基底クラスのレンダリング
            /// </summary>
            private SourceFile RenderCSharpBaseClass(CodeRenderingContext ctx) {
                var registered = new Dictionary<string, string>(_registered) {
                    { SETTER_INTERFACE, SETTER_CLASS },
                    { SETTER_CLASS, SETTER_CLASS },
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
                        public sealed class {{CONTEXT_CLASS}} {

                            #region 外部公開API
                            /// <summary>
                            /// ルートメッセージコンテナを取得します。
                            /// 追加されたメッセージはすべてこのオブジェクトから取得できます。
                            /// </summary>
                            public IReadOnlyMessageContainer Root => _root;

                            /// <summary>エラーメッセージを追加します。</summary>
                            public void AddError(IEnumerable<string> path, string message) {
                                Get(path.ToArray()).Errors.Add(message);
                            }
                            /// <summary>警告メッセージを追加します。</summary>
                            public void AddWarn(IEnumerable<string> path, string message) {
                                Get(path.ToArray()).Warns.Add(message);
                            }
                            /// <summary>インフォメーションメッセージを追加します。</summary>
                            public void AddInfo(IEnumerable<string> path, string message) {
                                Get(path.ToArray()).Infos.Add(message);
                            }
                            #endregion 外部公開API

                            #region 内部状態
                            /// <summary>メッセージの実体はすべてこのオブジェクトが持つ</summary>
                            private readonly MessageContainerStructure _root = new() {
                                Path = [],
                            };

                            /// <summary>内部用データ保持クラス</summary>
                            private class MessageContainerStructure : IReadOnlyMessageContainer {
                                public required string[] Path { get; init; }
                                public List<string> Errors { get; } = [];
                                public List<string> Warns { get; } = [];
                                public List<string> Infos { get; } = [];
                                public Dictionary<string, MessageContainerStructure> Children { get; } = [];

                                public IEnumerable<MessageContainerStructure> DescendantsAndSelf() {
                                    yield return this;
                                    foreach (var child in Children.Values) {
                                        foreach (var descendant in child.DescendantsAndSelf()) {
                                            yield return descendant;
                                        }
                                    }
                                }
                                /// <summary>
                                /// 指定したパスのメッセージコンテナを取得します。
                                /// 対応するメッセージが存在しない場合はnullを返します。
                                /// </summary>
                                /// <param name="path">このオブジェクトから該当のインスタンスまでのパス</param>
                                /// <returns>メッセージコンテナ</returns>
                                IReadOnlyMessageContainer? IReadOnlyMessageContainer.Find(IEnumerable<string> path) {
                                    var current = this;
                                    foreach (var p in path) {
                                        if (!current.Children.TryGetValue(p, out var child)) {
                                            return null;
                                        }
                                        current = child;
                                    }
                                    return current;
                                }

                                IReadOnlyList<string> IReadOnlyMessageContainer.Errors => Errors;
                                IReadOnlyList<string> IReadOnlyMessageContainer.Warns => Warns;
                                IReadOnlyList<string> IReadOnlyMessageContainer.Infos => Infos;
                                IReadOnlyDictionary<string, IReadOnlyMessageContainer> IReadOnlyMessageContainer.Children => Children.ToDictionary(kv => kv.Key, kv => (IReadOnlyMessageContainer)kv.Value);
                                IEnumerable<IReadOnlyMessageContainer> IReadOnlyMessageContainer.DescendantsAndSelf() => DescendantsAndSelf();
                            }

                            /// <summary>
                            /// 指定したパスのメッセージコンテナを取得する。
                            /// パスに対応するノードが存在しない場合は新規作成する。
                            /// </summary>
                            /// <param name="path">オブジェクトルートから該当のインスタンスまでのパス</param>
                            /// <returns>メッセージコンテナ</returns>
                            private MessageContainerStructure Get(string[] path) {
                                return GetRecursive(_root, path);

                                MessageContainerStructure GetRecursive(MessageContainerStructure current, string[] path) {
                                    if (path.Length == 0) return current;

                                    if (!current.Children.TryGetValue(path[0], out var child)) {
                                        child = new MessageContainerStructure {
                                            Path = [.. current.Path, path[0]],
                                        };
                                        current.Children[path[0]] = child;
                                    }
                                    return GetRecursive(child, path.Skip(1).ToArray());
                                }
                            }
                            #endregion 内部状態
                        }

                        /// <summary>現時点で追加されているメッセージの読み取り専用インターフェース</summary>
                        public interface IReadOnlyMessageContainer {
                            /// <summary>ルートオブジェクトからこのオブジェクトまでのパス</summary>
                            string[] Path { get; }
                            /// <summary>エラーメッセージ</summary>
                            IReadOnlyList<string> Errors { get; }
                            /// <summary>警告メッセージ</summary>
                            IReadOnlyList<string> Warns { get; }
                            /// <summary>インフォメーションメッセージ</summary>
                            IReadOnlyList<string> Infos { get; }
                            /// <summary>このオブジェクトの直近の子メッセージコンテナ</summary>
                            IReadOnlyDictionary<string, IReadOnlyMessageContainer> Children { get; }
                            /// <summary>子メッセージコンテナの子孫と自身を列挙</summary>
                            IEnumerable<IReadOnlyMessageContainer> DescendantsAndSelf();
                            /// <summary>
                            /// 指定したパスのメッセージコンテナを取得します。
                            /// 対応するメッセージが存在しない場合はnullを返します。
                            /// </summary>
                            /// <param name="path">このオブジェクトから該当のインスタンスまでのパス</param>
                            /// <returns>メッセージコンテナ</returns>
                            IReadOnlyMessageContainer? Find(IEnumerable<string> path);
                        }

                        #region インターフェース
                        /// <summary>
                        /// <see cref="{{CONTEXT_CLASS}}"/> へのメッセージ設定を容易にするためのヘルパー
                        /// </summary>
                        public interface {{SETTER_INTERFACE}} {
                            /// <summary>
                            /// メッセージ本体が格納されているオブジェクト。
                            /// 現在発生しているメッセージはこのオブジェクトを通じて取得してください。
                            /// </summary>
                            {{CONTEXT_CLASS}} UnderlyingContext { get; }

                            /// <summary>エラーメッセージを付加します。</summary>
                            void AddError(string message);
                            /// <summary>警告メッセージを付加します。</summary>
                            void AddWarn(string message);
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            void AddInfo(string message);

                            /// <summary>このインスタンスまたはこのインスタンスの子孫が1件以上エラーを持っているか否かを返します。</summary>
                            bool HasError();
                        }
                        /// <inheritdoc cref="{{SETTER_INTERFACE}}">
                        public interface {{SETTER_INTERFACE_LIST}}<out T> : {{SETTER_INTERFACE}}, IReadOnlyList<T> where T : {{SETTER_INTERFACE}} {
                        }
                        #endregion インターフェース


                        #region 具象クラス
                        /// <inheritdoc cref="{{SETTER_INTERFACE}}">
                        public partial class {{SETTER_CLASS}} : {{SETTER_INTERFACE}} {
                            /// <inheritdoc cref="{{SETTER_INTERFACE}}">
                            /// <param name="path">オブジェクトルートからこのインスタンスまでのパス</param>
                            public {{SETTER_CLASS}}(IEnumerable<string> path, {{CONTEXT_CLASS}} underlyingContext) {
                                _path = path;
                                UnderlyingContext = underlyingContext;
                            }
                            private readonly IEnumerable<string> _path;
                            public {{CONTEXT_CLASS}} UnderlyingContext { get; }

                            /// <summary>エラーメッセージを付加します。</summary>
                            public virtual void AddError(string message) {
                                UnderlyingContext.AddError(_path, message);
                            }
                            /// <summary>警告メッセージを付加します。</summary>
                            public virtual void AddWarn(string message) {
                                UnderlyingContext.AddWarn(_path, message);
                            }
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            public virtual void AddInfo(string message) {
                                UnderlyingContext.AddInfo(_path, message);
                            }

                            /// <summary>このインスタンスまたはこのインスタンスの子孫が1件以上エラーを持っているか否かを返します。</summary>
                            public bool HasError() {
                                return UnderlyingContext.Root.Find(_path)?.Errors.Any() == true;
                            }

                            /// <summary>
                            /// このインスタンスを指定した型にキャストして返します。
                            /// </summary>
                            public T As<T>() where T : {{SETTER_INTERFACE}} {
                                return {{GET_IMPL}}<T>(_path, UnderlyingContext);
                            }

                            /// <summary>
                            /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
                            /// </summary>
                            public static T {{GET_IMPL}}<T>(IEnumerable<string> path, {{CONTEXT_CLASS}} context) where T : {{SETTER_INTERFACE}} {
                                return (T){{GET_IMPL}}(typeof(T), path, context);
                            }
                            /// <summary>
                            /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
                            /// </summary>
                            public static {{SETTER_INTERFACE}} {{GET_IMPL}}(Type type, IEnumerable<string> path, {{CONTEXT_CLASS}} context) {
                        {{registered.OrderBy(kv => kv.Key).SelectTextTemplate(kv => $$"""
                                if (type == typeof({{kv.Key}})) return new {{kv.Value}}(path, context);
                        """)}}

                                // メッセージのリストの場合
                                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof({{SETTER_INTERFACE_LIST}}<>)
                                                        || type.GetGenericTypeDefinition() == typeof({{SETTER_CONCRETE_CLASS_LIST}}<>))) {
                                    var itemType = type.GetGenericArguments()[0];

                        {{registered.OrderBy(kv => kv.Key).SelectTextTemplate(kv => $$"""
                                    if (itemType == typeof({{kv.Key}})) return new {{SETTER_CONCRETE_CLASS_LIST}}<{{kv.Key}}>(path, index => new {{kv.Value}}([.. path, index.ToString()], context), context);
                        """)}}
                                }

                                throw new InvalidOperationException($"{type.Name} には既定のメッセージコンテナクラスが存在しません。");
                            }
                        }

                        /// <inheritdoc cref="{{SETTER_INTERFACE_LIST}}"/>
                        public partial class {{SETTER_CONCRETE_CLASS_LIST}}<T> : {{SETTER_CLASS}}, {{SETTER_INTERFACE_LIST}}<T> where T : {{SETTER_INTERFACE}} {
                            public {{SETTER_CONCRETE_CLASS_LIST}}(IEnumerable<string> path, Func<int, T> createItem, {{CONTEXT_CLASS}} context) : base(path, context) {
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
        }
        #endregion 基底クラス


        #region メンバー
        /// <summary>
        /// <see cref="Setter"/> のメンバーを表すインターフェース。
        /// </summary>
        internal interface IMember {
            string PhysicalName { get; }
            string DisplayName { get; }
            /// <summary>ChildまたはChildren</summary>
            Setter? NestedObject { get; }
            /// <summary>未指定の場合はデフォルトの型になる</summary>
            string? CsType { get; }
            /// <summary>このメンバーをリストとしてレンダリングするか否か</summary>
            bool IsArray { get; }
        }
        #endregion メンバー
    }
}

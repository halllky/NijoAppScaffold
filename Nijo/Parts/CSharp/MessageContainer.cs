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
                            this.{{m.PhysicalName}} = new {{SETTER_CLASS}}([.. path, {{m.GetPathSinceParent().Select(s => $"\"{s}\"").Join(", ")}}], context);
                    """).ElseIf(!m.IsArray, () => $$"""
                            this.{{m.PhysicalName}} = new {{m.NestedObject?.CsClassName}}([.. path, {{m.GetPathSinceParent().Select(s => $"\"{s}\"").Join(", ")}}], context);
                    """).Else(() => $$"""
                            this.{{m.PhysicalName}} = new {{SETTER_CONCRETE_CLASS_LIST}}<{{m.NestedObject?.CsClassName}}>([.. path, {{m.GetPathSinceParent().Select(s => $"\"{s}\"").Join(", ")}}], rowIndex => {
                                return new {{m.NestedObject?.CsClassName}}([.. path, {{m.GetPathSinceParent().Select(s => $"\"{s}\"").Join(", ")}}, rowIndex.ToString()], context);
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
                        {{WithIndent(RenderCSharpAdditionalSource())}}
                    }
                    """;
            }
        }


        #region 基底クラス
        internal const string CONTEXT_CLASS = "MessageContainer";

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
                if (ctx.IsLegacyCompatibilityMode()) {
                    return RenderLegacyCSharpBaseClass(ctx);
                }

                var registered = new Dictionary<string, string>(_registered) {
                    { SETTER_INTERFACE, SETTER_CLASS },
                    { SETTER_CLASS, SETTER_CLASS },
                };

                return new SourceFile {
                    FileName = ctx.IsLegacyCompatibilityMode() ? "MessageReceiver.cs" : "MessageContainer.cs",
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

                            /// <summary>
                            /// 指定したパスのメッセージコンテナを取得します。
                            /// 対応するメッセージが存在しない場合はnullを返します。
                            /// </summary>
                            public IReadOnlyMessageContainer? Find(IEnumerable<string> path) {
                                return _root.Find(path);
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
                                public IReadOnlyMessageContainer? Find(IEnumerable<string> path) {
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

                                static MessageContainerStructure GetRecursive(MessageContainerStructure current, string[] path) {
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
                        }

                        #region インターフェース
                        /// <summary>
                        /// <see cref="{{CONTEXT_CLASS}}"/> へのメッセージ設定を容易にするためのヘルパー
                        /// </summary>
                        public interface {{SETTER_INTERFACE}} {

                            /// <summary>エラーメッセージを付加します。</summary>
                            void AddError(string message);
                            /// <summary>警告メッセージを付加します。</summary>
                            void AddWarn(string message);
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            void AddInfo(string message);

                            /// <summary>
                            /// このインスタンスを、メッセージコンテナを持つ型にキャストします。
                            /// このインスタンスが持っているメッセージはすべて引き継がれます。
                            /// </summary>
                            TMessage As<TMessage>() where TMessage : {{SETTER_INTERFACE}};

                            /// <summary>
                            /// この項目に対して現在追加されているメッセージを取得します。
                            /// </summary>
                            IReadOnlyMessageContainer? GetState();
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
                                _underlyingContext = underlyingContext;
                            }
                            private readonly IEnumerable<string> _path;
                            private readonly {{CONTEXT_CLASS}} _underlyingContext;

                            /// <summary>エラーメッセージを付加します。</summary>
                            public virtual void AddError(string message) {
                                _underlyingContext.AddError(_path, message);
                            }
                            /// <summary>警告メッセージを付加します。</summary>
                            public virtual void AddWarn(string message) {
                                _underlyingContext.AddWarn(_path, message);
                            }
                            /// <summary>インフォメーションメッセージを付加します。</summary>
                            public virtual void AddInfo(string message) {
                                _underlyingContext.AddInfo(_path, message);
                            }

                            /// <summary>
                            /// この項目に対して現在追加されているメッセージを取得します。
                            /// </summary>
                            public IReadOnlyMessageContainer? GetState() {
                                return _underlyingContext.Find(_path);
                            }

                            /// <summary>
                            /// このインスタンスを指定した型にキャストして返します。
                            /// </summary>
                            public T As<T>() where T : {{SETTER_INTERFACE}} {
                                return {{GET_IMPL}}<T>(_path, _underlyingContext);
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

            private SourceFile RenderLegacyCSharpBaseClass(CodeRenderingContext ctx) {
                var readModel2Roots = ctx.Schema.GetRootAggregates()
                    .Where(root => root.Model is Models.ReadModel2)
                    .OrderByDataFlow()
                    .ToArray();
                var defaultClassMappings = ctx.Schema.GetRootAggregates()
                    .Where(root => root.Model is Models.WriteModel2 || root.Model is Models.ReadModel2 || root.Model is Models.CommandModel2)
                    .OrderByDataFlow()
                    .SelectMany(root => {
                        if (root.Model is Models.WriteModel2) {
                            return root
                                .EnumerateThisAndDescendants()
                                .SelectMany((agg, index) => {
                                    var saveContainer = new Models.DataModelModules.SaveCommandMessageContainer(agg);
                                    var saveMappings = new[] {
                                        new KeyValuePair<string, string>(saveContainer.InterfaceName, saveContainer.CsClassName),
                                        new KeyValuePair<string, string>(saveContainer.CsClassName, saveContainer.CsClassName),
                                    };

                                    if (!root.GenerateDefaultQueryModel) {
                                        return saveMappings;
                                    }

                                    var displayName = new Models.QueryModelModules.DisplayDataMessageContainer(agg).CsClassName;
                                    var displayMappings = index == 0
                                        ? new[] {
                                            new KeyValuePair<string, string>(displayName, displayName),
                                            new KeyValuePair<string, string>($"{displayName}List", $"{displayName}List"),
                                        }
                                        : new[] {
                                            new KeyValuePair<string, string>(displayName, displayName),
                                        };

                                    return saveMappings.Concat(displayMappings);
                                });
                        }

                        if (root.Model is Models.ReadModel2) {
                            var rootClassName = new Models.QueryModelModules.DisplayDataMessageContainer(root).CsClassName;
                            var rootClass = new[] { new KeyValuePair<string, string>(rootClassName, rootClassName) };
                            var rootList = new[] { new KeyValuePair<string, string>($"{rootClassName}List", $"{rootClassName}List") };
                            var descendants = root
                                .EnumerateDescendants()
                                .Select(agg => new Models.QueryModelModules.DisplayDataMessageContainer(agg).CsClassName)
                                .Select(name => new KeyValuePair<string, string>(name, name));
                            return rootClass.Concat(rootList).Concat(descendants);
                        }

                        if (root.Model is Models.CommandModel2) {
                            var name = new Models.CommandModel2Modules.CommandParameter(root).MessageDataCsClassName;
                            return new[] { new KeyValuePair<string, string>(name, name) };
                        }

                        return [];
                    })
                    .DistinctBy(kv => kv.Key)
                    .ToArray();

                return new SourceFile {
                    FileName = "MessageReceiver.cs",
                    Contents = $$"""
                        using System.Collections;
                        using System.Text.Json;
                        using System.Text.Json.Nodes;

                        namespace {{ctx.Config.RootNamespace}};

                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物のインターフェース
                        /// </summary>
                        public interface IDisplayMessageContainer {
                            void AddError(string message);
                            void AddConcurrencyError(string message);
                            void AddInfo(string message);
                            void AddWarn(string message);
                            IEnumerable<IDisplayMessageContainer> EnumerateChildren();

                            bool HasError();
                            bool HasWarning();

                            /// <summary>
                            /// エラーの中でも特に排他エラーが発生しているか否かを返します。
                            /// </summary>
                            bool HasConcurrencyError();
                        }

                        public static class DisplayMessageContainerExtensions {
                            /// <summary>
                            /// エラー等のメッセージを送出する処理が求める型と、そのメッセージを受け取って画面等に表示する処理が知っている型が相違している時に
                            /// 画面ルートにすべてのメッセージを転送するために用いられるマッピング。
                            /// 引数はメッセージ送出側が求める型。
                            /// 戻り値はその型の具象型。
                            /// </summary>
                            /// <param name="pageRoot">メッセージ転送先</param>
                            public static IDisplayMessageContainer GetDefaultClass(Type type, IDisplayMessageContainer pageRoot) {
                        {{defaultClassMappings.SelectTextTemplate(kv => $$"""
                                if (type == typeof({{kv.Key}})) return new {{kv.Value}}(pageRoot);
                        """)}}
                                throw new ArgumentException($"{type.Name} 型には既定のメッセージコンテナクラスがありません。");
                            }
                        }

                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物の抽象クラス
                        /// </summary>
                        public abstract partial class DisplayMessageContainerBase : IDisplayMessageContainer {
                            public DisplayMessageContainerBase(IEnumerable<string> path) {
                                _path = path;
                            }
                            /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                            public DisplayMessageContainerBase(IDisplayMessageContainer origin) {
                                _origin = origin;
                            }
                            private readonly IEnumerable<string>? _path;
                            /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられる転送先</summary>
                            private readonly IDisplayMessageContainer? _origin;

                            private readonly List<string> _errors = new();
                            private readonly List<string> _warnings = new();
                            private readonly List<string> _informations = new();
                            private bool _hasConcurrencyError = false;

                            public virtual void AddError(string message) {
                                if (_origin == null) {
                                    _errors.Add(message);
                                } else {
                                    _origin.AddError(message); // メッセージを画面ルートへ転送する
                                }
                            }
                            public virtual void AddConcurrencyError(string message) {
                                AddError(message);
                                _hasConcurrencyError = true;
                            }
                            public virtual void AddWarn(string message) {
                                if (_origin == null) {
                                    _warnings.Add(message);
                                } else {
                                    _origin.AddWarn(message); // メッセージを画面ルートへ転送する
                                }
                            }
                            public virtual void AddInfo(string message) {
                                if (_origin == null) {
                                    _informations.Add(message);
                                } else {
                                    _origin.AddInfo(message); // メッセージを画面ルートへ転送する
                                }
                            }

                            public bool HasError() {
                                if (_errors.Count > 0) return true;
                                if (EnumerateDescendants().Any(container => container.HasError())) return true;
                                return false;
                            }
                            public bool HasWarning() {
                                if (_warnings.Count > 0) return true;
                                if (EnumerateDescendants().Any(container => container.HasWarning())) return true;
                                return false;
                            }
                            public bool HasConcurrencyError() {
                                if (_hasConcurrencyError) return true;
                                if (EnumerateDescendants().Any(container => container.HasConcurrencyError())) return true;
                                return false;
                            }

                            public virtual IEnumerable<JsonArray> ToReactHookFormErrors() {
                                // 全メッセージを画面ルート（origin）に転送している場合
                                if (_origin is DisplayMessageContainerBase origin) {
                                    foreach (var msg in origin.ToReactHookFormErrors()) {
                                        yield return msg;
                                    }
                                    yield break;
                                }

                                // このオブジェクト自身または子孫自身がそれぞれメッセージを保持している場合
                                if (_errors.Count > 0 || _warnings.Count > 0 || _informations.Count > 0) {
                                    var types = new JsonObject();
                                    for (var i = 0; i < _errors.Count; i++) {
                                        types[$"ERROR-{i}"] = _errors[i]; // キーを "ERROR-" で始めるというルールはTypeScript側と合わせる必要がある
                                    }
                                    for (var i = 0; i < _warnings.Count; i++) {
                                        types[$"WARN-{i}"] = _warnings[i]; // キーを "WARN-" で始めるというルールはTypeScript側と合わせる必要がある
                                    }
                                    for (var i = 0; i < _informations.Count; i++) {
                                        types[$"INFO-{i}"] = _informations[i]; // キーを "INFO-" で始めるというルールはTypeScript側と合わせる必要がある
                                    }
                                    yield return new JsonArray {
                                        _path != null && _path.Any()
                                            ? string.Join(".", _path)
                                            : "root", // "root" という名前は React hook form のエラーデータのルール
                                        new JsonObject { ["types"] = types }, // "types" という名前は React hook form のエラーデータのルール
                                    };
                                }
                                foreach (var child in EnumerateChildren().OfType<DisplayMessageContainerBase>()) {
                                    foreach (var msg in child.ToReactHookFormErrors()) {
                                        yield return msg;
                                    }
                                }
                            }

                            public abstract IEnumerable<IDisplayMessageContainer> EnumerateChildren();

                            public IEnumerable<DisplayMessageContainerBase> EnumerateDescendants() {
                                foreach (var child in EnumerateChildren().OfType<DisplayMessageContainerBase>()) {
                                    yield return child;

                                    foreach (var desc in child.EnumerateDescendants()) {
                                        yield return desc;
                                    }
                                }
                            }

                            /// <summary>
                            /// このオブジェクトまたは子孫が持っているメッセージのうち
                            /// エラーメッセージのみを列挙します。
                            /// </summary>
                            public IEnumerable<string> GetErrorMessages() {
                                // 全メッセージを画面ルート（origin）に転送している場合
                                if (_origin is DisplayMessageContainerBase origin) {
                                    foreach (var err in origin.GetErrorMessages()) {
                                        yield return err;
                                    }
                                    yield break;
                                }

                                // このオブジェクト自身または子孫自身がそれぞれメッセージを保持している場合
                                foreach (var err in _errors) {
                                    var fieldName = GetThisFieldName();
                                    yield return string.IsNullOrEmpty(fieldName) ? err : $"{fieldName}: {err}";
                                }
                                foreach (var desc in EnumerateChildren().OfType<DisplayMessageContainerBase>()) {
                                    foreach (var err in desc.GetErrorMessages()) {
                                        yield return err;
                                    }
                                }
                            }
                            /// <summary>
                            /// このオブジェクトまたは子孫が持っているメッセージのうち
                            /// 警告メッセージのみを列挙します。
                            /// </summary>
                            public IEnumerable<string> GetWarningMessages() {
                                // 全メッセージを画面ルート（origin）に転送している場合
                                if (_origin is DisplayMessageContainerBase origin) {
                                    foreach (var warn in origin.GetWarningMessages()) {
                                        yield return warn;
                                    }
                                    yield break;
                                }

                                // このオブジェクト自身または子孫自身がそれぞれメッセージを保持している場合
                                foreach (var warn in _warnings) {
                                    var fieldName = GetThisFieldName();
                                    yield return string.IsNullOrEmpty(fieldName) ? warn : $"{fieldName}: {warn}";
                                }
                                foreach (var desc in EnumerateChildren().OfType<DisplayMessageContainerBase>()) {
                                    foreach (var warn in desc.GetWarningMessages()) {
                                        yield return warn;
                                    }
                                }
                            }
                            /// <summary>
                            /// このオブジェクトまたは子孫が持っているメッセージのうち
                            /// インフォメーションメッセージのみを列挙します。
                            /// </summary>
                            public IEnumerable<string> GetInformationMessages() {
                                // 全メッセージを画面ルート（origin）に転送している場合
                                if (_origin is DisplayMessageContainerBase origin) {
                                    foreach (var info in origin.GetInformationMessages()) {
                                        yield return info;
                                    }
                                    yield break;
                                }

                                // このオブジェクト自身または子孫自身がそれぞれメッセージを保持している場合
                                foreach (var info in _informations) {
                                    var fieldName = GetThisFieldName();
                                    yield return string.IsNullOrEmpty(fieldName) ? info : $"{fieldName}: {info}";
                                }
                                foreach (var desc in EnumerateChildren().OfType<DisplayMessageContainerBase>()) {
                                    foreach (var info in desc.GetInformationMessages()) {
                                        yield return info;
                                    }
                                }
                            }
                            /// <summary>
                            /// このオブジェクトの表示用の名称を返します。
                            /// 祖先からのパスは含まれません。
                            /// </summary>
                            public string GetThisFieldName() {
                                if (_path == null) return string.Empty;
                                if (!_path.Any()) return string.Empty;

                                // パスの最後がintの場合、この要素は配列の要素
                                var last = _path.Last();
                                if (int.TryParse(last, out var index)) {
                                    var last2 = _path.SkipLast(1).LastOrDefault();
                                    if (last2 == null) {
                                        return $"{index + 1}行目";
                                    } else {
                                        return $"{last2}({index + 1}行目)";
                                    }
                                } else {
                                    return last;
                                }
                            }
                        }

                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物
                        /// </summary>
                        public partial class DisplayMessageContainer : DisplayMessageContainerBase {
                            public DisplayMessageContainer(IEnumerable<string> path) : base(path) { }
                            /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                            public DisplayMessageContainer(IDisplayMessageContainer origin) : base(origin) { }

                            public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                                yield break;
                            }
                        }

                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物のうち、グリッドの内部の項目。
                        /// グリッドのヘッダと自身のセルの部分の2か所にエラー等のメッセージを表示する必要があるため、
                        /// エラーメッセージが1個追加されるごとにHTTPレスポンスのエラーメッセージのオブジェクトが2個ずつ増えていく。
                        /// </summary>
                        public partial class DisplayMessageContainerInGrid : DisplayMessageContainerBase {
                            /// <param name="path">このメンバー自身のパス</param>
                            /// <param name="gridRoot">グリッドに表示されるメッセージの入れ物。全メッセージを画面ルートに転送する場合はnullになる。</param>
                            /// <param name="rowIndex">このオブジェクトがグリッドの何行目か</param>
                            public DisplayMessageContainerInGrid(IEnumerable<string> path, IDisplayMessageContainer gridRoot, int rowIndex) : base(path) {
                                _gridRoot = gridRoot;
                                _rowIndex = rowIndex;
                            }
                            /// <summary>全メッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                            public DisplayMessageContainerInGrid(IDisplayMessageContainer origin) : base(origin) {
                                _gridRoot = null;
                                _rowIndex = null;
                            }
                            private readonly IDisplayMessageContainer? _gridRoot;
                            private readonly int? _rowIndex;

                            public override void AddError(string message) {
                                _gridRoot?.AddError($"{_rowIndex + 1}行目: {message}");
                                base.AddError(message);
                            }
                            public override void AddWarn(string message) {
                                _gridRoot?.AddWarn($"{_rowIndex + 1}行目: {message}");
                                base.AddWarn(message);
                            }
                            public override void AddInfo(string message) {
                                _gridRoot?.AddInfo($"{_rowIndex + 1}行目: {message}");
                                base.AddInfo(message);
                            }

                            public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                                yield break;
                            }
                        }

                        /// <summary>
                        /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物の配列
                        /// </summary>
                        public interface IDisplayMessageContainerList<out T> : IDisplayMessageContainer, IReadOnlyList<T> where T : IDisplayMessageContainer {
                        }

                        /// <inheritdoc cref="IDisplayMessageContainerList"/>
                        public partial class DisplayMessageContainerList<T> : DisplayMessageContainerBase, IDisplayMessageContainerList<T> where T : IDisplayMessageContainer {
                            public DisplayMessageContainerList(IEnumerable<string> path, Func<int, T> createItem) : base(path) {
                                _createItem = createItem;
                            }
                            /// <summary>全メッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                            public DisplayMessageContainerList(IDisplayMessageContainer origin, Func<int, T> createItem) : base(origin) {
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

                            public override IEnumerable<IDisplayMessageContainer> EnumerateChildren() {
                                return this.Cast<IDisplayMessageContainer>();
                            }
                        }
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

            /// <summary>
            /// 親オブジェクトからこのメンバーまでのパスを列挙する。
            /// 例えば DisplayData のValueMemberの場合は [<see cref="PhysicalName"/>]、
            /// SearchCondition のValueMemberの場合は ["filter", <see cref="PhysicalName"/>]、
            /// それ以外のプレーンな構造のオブジェクトの場合は単に [<see cref="PhysicalName"/>] となる。
            /// </summary>
            IEnumerable<string> GetPathSinceParent();
        }
        #endregion メンバー
    }
}

using Nijo.Core;
using Nijo.Models.CommandModelFeatures;
using Nijo.Models.ReadModel2Features;
using Nijo.Models.WriteModel2Features;
using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Parts.WebServer {
    /// <summary>
    /// <see cref="Models.ReadModel2"/> においてサーバーからクライアント側に返すエラーメッセージ等のコンテナ。
    /// ここで設定されたエラー等は React hook form のエラーメッセージのAPIを通して表示されるため、当該APIの仕様の影響を強く受ける。
    /// 
    /// <code>インスタンス.プロパティ名.AddError(メッセージ)</code> のように直感的に書ける、
    /// 無駄なペイロードを避けるためにメッセージが無いときはJSON化されない、といった性質を持つ。
    /// </summary>
    internal class DisplayMessageContainer {
        internal const string INTERFACE = "IDisplayMessageContainer";
        internal const string ABSTRACT_CLASS = "DisplayMessageContainerBase";
        internal const string CONCRETE_CLASS = "DisplayMessageContainer";
        internal const string CONCRETE_CLASS_IN_GRID = "DisplayMessageContainerInGrid";
        internal const string CONCRETE_CLASS_LIST = "DisplayMessageContainerList";
        internal const string LIST_INTERFACE = "IDisplayMessageContainerList";

        /// <summary>
        /// React hook form のsetErrorsの引数の形に準じている
        /// </summary>
        internal const string CLIENT_TYPE_TS = "[string, { types: { [key: `ERROR-${number}` | `WARN-${number}` | `INFO-${number}`]: string } }]";
        /// <summary>
        /// React hook form ではルート要素自体へのエラーはこの名前で設定される
        /// </summary>
        internal const string ROOT = "root";

        internal static SourceFile RenderCSharp() => new SourceFile {
            FileName = "MessageReceiver.cs",
            RenderContent = context => {

                // エラーメッセージの転送先が未指定の場合はすべてのメッセージを画面ルートに転送する。
                // その仕組みを実現するためのインターフェースと具象クラスの対応関係表
                var concreteClassNameMap = new List<(string InterfaceName, string ConcreteClassName)>();
                foreach (var agg in context.Schema.AllAggregates()) {
                    var model = agg.GetRoot().Item.Options.Handler;
                    if (model == NijoCodeGenerator.Models.WriteModel2.Key) {
                        var saveCommand = new DataClassForSave(agg, DataClassForSave.E_Type.UpdateOrDelete);
                        concreteClassNameMap.Add((saveCommand.MessageDataCsInterfaceName, saveCommand.MessageDataCsClassName));
                        concreteClassNameMap.Add((saveCommand.MessageDataCsClassName, saveCommand.MessageDataCsClassName));
                    }

                    if (model == NijoCodeGenerator.Models.ReadModel2.Key || agg.GetRoot().Item.Options.GenerateDefaultReadModel) {
                        var displayData = new DataClassForDisplay(agg);
                        concreteClassNameMap.Add((displayData.MessageDataCsClassName, displayData.MessageDataCsClassName));
                        if (agg.IsRoot()) concreteClassNameMap.Add((displayData.MessageDataListCsClassName, displayData.MessageDataListCsClassName));
                    }

                    if (model == NijoCodeGenerator.Models.CommandModel.Key) {
                        var commandParam = new CommandParameter(agg);
                        concreteClassNameMap.Add((commandParam.MessageDataCsClassName, commandParam.MessageDataCsClassName));
                    }
                }

                return $$"""
                    using System.Collections;
                    using System.Text.Json;
                    using System.Text.Json.Nodes;

                    namespace {{context.Config.RootNamespace}};

                    /// <summary>
                    /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物のインターフェース
                    /// </summary>
                    public interface {{INTERFACE}} {
                        void AddError(string message);
                        void AddConcurrencyError(string message);
                        void AddInfo(string message);
                        void AddWarn(string message);
                        IEnumerable<{{INTERFACE}}> EnumerateChildren();

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
                        public static {{INTERFACE}} GetDefaultClass(Type type, {{INTERFACE}} pageRoot) {
                    {{concreteClassNameMap.SelectTextTemplate(x => $$"""
                            if (type == typeof({{x.InterfaceName}})) return new {{x.ConcreteClassName}}(pageRoot);
                    """)}}
                            throw new ArgumentException($"{type.Name} 型には既定のメッセージコンテナクラスがありません。");
                        }
                    }

                    /// <summary>
                    /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物の抽象クラス
                    /// </summary>
                    public abstract partial class {{ABSTRACT_CLASS}} : {{INTERFACE}} {
                        public {{ABSTRACT_CLASS}}(IEnumerable<string> path) {
                            _path = path;
                        }
                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{ABSTRACT_CLASS}}({{INTERFACE}} origin) {
                            _origin = origin;
                        }
                        private readonly IEnumerable<string>? _path;
                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられる転送先</summary>
                        private readonly {{INTERFACE}}? _origin;

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
                            if (_origin is {{ABSTRACT_CLASS}} origin) {
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
                            foreach (var child in EnumerateChildren().OfType<{{ABSTRACT_CLASS}}>()) {
                                foreach (var msg in child.ToReactHookFormErrors()) {
                                    yield return msg;
                                }
                            }
                        }

                        public abstract IEnumerable<{{INTERFACE}}> EnumerateChildren();

                        public IEnumerable<{{ABSTRACT_CLASS}}> EnumerateDescendants() {
                            foreach (var child in EnumerateChildren().OfType<{{ABSTRACT_CLASS}}>()) {
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
                            if (_origin is {{ABSTRACT_CLASS}} origin) {
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
                            foreach (var desc in EnumerateChildren().OfType<{{ABSTRACT_CLASS}}>()) {
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
                            if (_origin is {{ABSTRACT_CLASS}} origin) {
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
                            foreach (var desc in EnumerateChildren().OfType<{{ABSTRACT_CLASS}}>()) {
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
                            if (_origin is {{ABSTRACT_CLASS}} origin) {
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
                            foreach (var desc in EnumerateChildren().OfType<{{ABSTRACT_CLASS}}>()) {
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
                    public partial class {{CONCRETE_CLASS}} : {{ABSTRACT_CLASS}} {
                        public {{CONCRETE_CLASS}}(IEnumerable<string> path) : base(path) { }
                        /// <summary>すべてのメッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{CONCRETE_CLASS}}({{INTERFACE}} origin) : base(origin) { }

                        public override IEnumerable<{{INTERFACE}}> EnumerateChildren() {
                            yield break;
                        }
                    }

                    /// <summary>
                    /// 登録処理などで生じたエラーメッセージなどをHTTPレスポンスとして返すまでの入れ物のうち、グリッドの内部の項目。
                    /// グリッドのヘッダと自身のセルの部分の2か所にエラー等のメッセージを表示する必要があるため、
                    /// エラーメッセージが1個追加されるごとにHTTPレスポンスのエラーメッセージのオブジェクトが2個ずつ増えていく。
                    /// </summary>
                    public partial class {{CONCRETE_CLASS_IN_GRID}} : {{ABSTRACT_CLASS}} {
                        /// <param name="path">このメンバー自身のパス</param>
                        /// <param name="gridRoot">グリッドに表示されるメッセージの入れ物。全メッセージを画面ルートに転送する場合はnullになる。</param>
                        /// <param name="rowIndex">このオブジェクトがグリッドの何行目か</param>
                        public {{CONCRETE_CLASS_IN_GRID}}(IEnumerable<string> path, {{INTERFACE}} gridRoot, int rowIndex) : base(path) {
                            _gridRoot = gridRoot;
                            _rowIndex = rowIndex;
                        }
                        /// <summary>全メッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{CONCRETE_CLASS_IN_GRID}}({{INTERFACE}} origin) : base(origin) {
                            _gridRoot = null;
                            _rowIndex = null;
                        }
                        private readonly {{INTERFACE}}? _gridRoot;
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
                    public interface {{LIST_INTERFACE}}<out T> : {{INTERFACE}}, IReadOnlyList<T> where T : {{INTERFACE}} {
                    }

                    /// <inheritdoc cref="{{LIST_INTERFACE}}"/>
                    public partial class {{CONCRETE_CLASS_LIST}}<T> : {{ABSTRACT_CLASS}}, {{LIST_INTERFACE}}<T> where T : {{INTERFACE}} {
                        public {{CONCRETE_CLASS_LIST}}(IEnumerable<string> path, Func<int, T> createItem) : base(path) {
                            _createItem = createItem;
                        }
                        /// <summary>全メッセージを画面ルートに転送する場合に用いられるコンストラクタ</summary>
                        public {{CONCRETE_CLASS_LIST}}({{INTERFACE}} origin, Func<int, T> createItem) : base(origin) {
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

                        public override IEnumerable<{{INTERFACE}}> EnumerateChildren() {
                            return this.Cast<{{INTERFACE}}>();
                        }
                    }
                    """;
            },
        };
    }
}

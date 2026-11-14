
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : MessageContainer.cs
// ※ このファイルを生成したメソッド: RenderCSharpBaseClass

using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MyApp;

/// <summary>
/// ユーザーの画面入力や外部システムからのデータ連携でエラーが起きた際、
/// どの項目でエラー等が発生したかの明示が要求されることがしばしばある。
/// このクラスは、エラー・警告・インフォメーションのメッセージを、
/// それがどの項目で発生したかの情報と紐づけて保持する。
/// </summary>
public sealed class MessageContainer {

    #region 外部公開API
    /// <summary>指定したパスの項目自身にエラーメッセージを追加します。祖先や子孫の項目には追加しません。</summary>
    public void AddError(IEnumerable<string> path, string message) {
        Get(path.ToArray()).Errors.Add(message);
    }
    /// <summary>指定したパスの項目自身に警告メッセージを追加します。祖先や子孫の項目には追加しません。</summary>
    public void AddWarn(IEnumerable<string> path, string message) {
        Get(path.ToArray()).Warns.Add(message);
    }
    /// <summary>指定したパスの項目自身にインフォメーションメッセージを追加します。祖先や子孫の項目には追加しません。</summary>
    public void AddInfo(IEnumerable<string> path, string message) {
        Get(path.ToArray()).Infos.Add(message);
    }

    /// <summary>
    /// 指定したパスの項目のメッセージコンテナを取得します。
    /// 戻り値からはその項目の子孫のメッセージも辿れますが、祖先のメッセージは辿れません。
    /// その項目と子孫のいずれにもメッセージが存在しない場合はnullを返します。
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
        /// このオブジェクトを起点に、指定したパスの子孫のメッセージコンテナを取得します。
        /// その項目と子孫のいずれにもメッセージが存在しない場合はnullを返します。
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
    /// <summary>この項目自身のエラーメッセージ。子孫の項目のものは含みません。</summary>
    IReadOnlyList<string> Errors { get; }
    /// <summary>この項目自身の警告メッセージ。子孫の項目のものは含みません。</summary>
    IReadOnlyList<string> Warns { get; }
    /// <summary>この項目自身のインフォメーションメッセージ。子孫の項目のものは含みません。</summary>
    IReadOnlyList<string> Infos { get; }
    /// <summary>このオブジェクトの直近の子メッセージコンテナ。孫以下はそれぞれの子から辿ります。</summary>
    IReadOnlyDictionary<string, IReadOnlyMessageContainer> Children { get; }
    /// <summary>このオブジェクト自身とそのすべての子孫を列挙します。祖先は含みません。</summary>
    IEnumerable<IReadOnlyMessageContainer> DescendantsAndSelf();
}

#region インターフェース
/// <summary>
/// <see cref="MessageContainer"/> へのメッセージ設定を容易にするためのヘルパー
/// </summary>
public interface IMessageSetter {

    /// <summary>この項目自身にエラーメッセージを付加します。祖先や子孫の項目には付加しません。</summary>
    void AddError(string message);
    /// <summary>この項目自身に警告メッセージを付加します。祖先や子孫の項目には付加しません。</summary>
    void AddWarn(string message);
    /// <summary>この項目自身にインフォメーションメッセージを付加します。祖先や子孫の項目には付加しません。</summary>
    void AddInfo(string message);

    /// <summary>
    /// このインスタンスと同じ項目を指す、指定した型のインスタンスを返します。
    /// メッセージの実体は共有されるので、祖先・自身・子孫のメッセージはすべて戻り値からも同じパスで参照・追加できます。
    /// </summary>
    TMessage As<TMessage>() where TMessage : IMessageSetter;

    /// <summary>
    /// この項目に対して現在追加されているメッセージを取得します。
    /// 戻り値からはこの項目の子孫のメッセージも辿れますが、祖先のメッセージは辿れません。
    /// この項目と子孫のいずれにもメッセージが無い場合はnullを返します。
    /// </summary>
    IReadOnlyMessageContainer? GetState();

    /// <summary>
    /// この項目自身とその子孫のメッセージを、引数のオブジェクトの同名のパスにコピーします。祖先のメッセージはコピーしません。
    /// この項目自身のメッセージは引数のオブジェクト自身に、子孫のメッセージは引数のオブジェクトの子孫に追加されます。
    /// 引数のオブジェクトとこのオブジェクトで項目の階層が異なる場合、対応する階層のオブジェクトを引数に渡してください。
    /// 階層が異なるものを渡すと、エラーは追加されるものの画面の項目には表示されなくなります。
    /// </summary>
    void CopyTo(IMessageSetter target);
}
/// <inheritdoc cref="IMessageSetter">
public interface IMessageSetterList<out T> : IMessageSetter, IReadOnlyList<T> where T : IMessageSetter {
}
#endregion インターフェース


#region 具象クラス
/// <inheritdoc cref="IMessageSetter">
public partial class MessageSetter : IMessageSetter {
    /// <inheritdoc cref="IMessageSetter">
    /// <param name="path">オブジェクトルートからこのインスタンスまでのパス</param>
    public MessageSetter(IEnumerable<string> path, MessageContainer underlyingContext) {
        _path = path;
        _underlyingContext = underlyingContext;
    }
    private readonly IEnumerable<string> _path;
    private readonly MessageContainer _underlyingContext;

    /// <inheritdoc />
    public virtual void AddError(string message) {
        _underlyingContext.AddError(_path, message);
    }
    /// <inheritdoc />
    public virtual void AddWarn(string message) {
        _underlyingContext.AddWarn(_path, message);
    }
    /// <inheritdoc />
    public virtual void AddInfo(string message) {
        _underlyingContext.AddInfo(_path, message);
    }

    /// <inheritdoc />
    public IReadOnlyMessageContainer? GetState() {
        return _underlyingContext.Find(_path);
    }

    /// <inheritdoc />
    public void CopyTo(IMessageSetter target) {
        var state = GetState();
        if (state == null) return;
        if (target is not MessageSetter targetSetter) throw new ArgumentException($"{target.GetType().Name} にはコピーできません。", nameof(target));

        var depth = _path.Count();
        foreach (var node in state.DescendantsAndSelf()) {
            string[] path = [.. targetSetter._path, .. node.Path.Skip(depth)];
            foreach (var message in node.Errors) targetSetter._underlyingContext.AddError(path, message);
            foreach (var message in node.Warns) targetSetter._underlyingContext.AddWarn(path, message);
            foreach (var message in node.Infos) targetSetter._underlyingContext.AddInfo(path, message);
        }
    }

    /// <inheritdoc />
    public T As<T>() where T : IMessageSetter {
        return GetImpl<T>(_path, _underlyingContext);
    }

    /// <summary>
    /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
    /// </summary>
    public static T GetImpl<T>(IEnumerable<string> path, MessageContainer context) where T : IMessageSetter {
        return (T)GetImpl(typeof(T), path, context);
    }
    /// <summary>
    /// 引数のメッセージのコンテナの形と対応する既定のインスタンスを作成して返します。
    /// </summary>
    public static IMessageSetter GetImpl(Type type, IEnumerable<string> path, MessageContainer context) {
        if (type == typeof(IMessageSetter)) return new MessageSetter(path, context);
        if (type == typeof(MessageSetter)) return new MessageSetter(path, context);
        if (type == typeof(セッションSaveCommandMessages)) return new セッションSaveCommandMessages(path, context);
        if (type == typeof(パスワード再発行ParameterMessages)) return new パスワード再発行ParameterMessages(path, context);
        if (type == typeof(パスワード変更ParameterMessages)) return new パスワード変更ParameterMessages(path, context);
        if (type == typeof(ログインParameterMessages)) return new ログインParameterMessages(path, context);
        if (type == typeof(入荷SaveCommandMessages)) return new 入荷SaveCommandMessages(path, context);
        if (type == typeof(入荷一覧DisplayDataMessages)) return new 入荷一覧DisplayDataMessages(path, context);
        if (type == typeof(入荷一覧SearchConditionMessages)) return new 入荷一覧SearchConditionMessages(path, context);
        if (type == typeof(入荷明細SaveCommandMessages)) return new 入荷明細SaveCommandMessages(path, context);
        if (type == typeof(入荷詳細Messages)) return new 入荷詳細Messages(path, context);
        if (type == typeof(入荷詳細画面初期表示ParameterMessages)) return new 入荷詳細画面初期表示ParameterMessages(path, context);
        if (type == typeof(商品DisplayDataMessages)) return new 商品DisplayDataMessages(path, context);
        if (type == typeof(商品SaveCommandMessages)) return new 商品SaveCommandMessages(path, context);
        if (type == typeof(商品SearchConditionMessages)) return new 商品SearchConditionMessages(path, context);
        if (type == typeof(商品一覧DisplayDataMessages)) return new 商品一覧DisplayDataMessages(path, context);
        if (type == typeof(商品一覧SearchConditionMessages)) return new 商品一覧SearchConditionMessages(path, context);
        if (type == typeof(商品在庫増減履歴DisplayDataMessages)) return new 商品在庫増減履歴DisplayDataMessages(path, context);
        if (type == typeof(商品在庫増減履歴SearchConditionMessages)) return new 商品在庫増減履歴SearchConditionMessages(path, context);
        if (type == typeof(在庫調整ParameterMessages)) return new 在庫調整ParameterMessages(path, context);
        if (type == typeof(在庫調整SaveCommandMessages)) return new 在庫調整SaveCommandMessages(path, context);
        if (type == typeof(売上SaveCommandMessages)) return new 売上SaveCommandMessages(path, context);
        if (type == typeof(売上一覧DisplayDataMessages)) return new 売上一覧DisplayDataMessages(path, context);
        if (type == typeof(売上一覧SearchConditionMessages)) return new 売上一覧SearchConditionMessages(path, context);
        if (type == typeof(売上詳細Messages)) return new 売上詳細Messages(path, context);
        if (type == typeof(売上詳細画面初期表示ParameterMessages)) return new 売上詳細画面初期表示ParameterMessages(path, context);
        if (type == typeof(従業員RefDisplayDataMessages)) return new 従業員RefDisplayDataMessages(path, context);
        if (type == typeof(従業員RefSearchConditionMessages)) return new 従業員RefSearchConditionMessages(path, context);
        if (type == typeof(従業員SaveCommandMessages)) return new 従業員SaveCommandMessages(path, context);
        if (type == typeof(従業員マスタDisplayDataMessages)) return new 従業員マスタDisplayDataMessages(path, context);
        if (type == typeof(従業員マスタSearchConditionMessages)) return new 従業員マスタSearchConditionMessages(path, context);
        if (type == typeof(従業員一括更新ParameterMessages)) return new 従業員一括更新ParameterMessages(path, context);

        // メッセージのリストの場合
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IMessageSetterList<>)
                                || type.GetGenericTypeDefinition() == typeof(MessageSetterList<>))) {
            var itemType = type.GetGenericArguments()[0];

            if (itemType == typeof(IMessageSetter)) return new MessageSetterList<IMessageSetter>(path, index => new MessageSetter([.. path, index.ToString()], context), context);
            if (itemType == typeof(MessageSetter)) return new MessageSetterList<MessageSetter>(path, index => new MessageSetter([.. path, index.ToString()], context), context);
            if (itemType == typeof(セッションSaveCommandMessages)) return new MessageSetterList<セッションSaveCommandMessages>(path, index => new セッションSaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(パスワード再発行ParameterMessages)) return new MessageSetterList<パスワード再発行ParameterMessages>(path, index => new パスワード再発行ParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(パスワード変更ParameterMessages)) return new MessageSetterList<パスワード変更ParameterMessages>(path, index => new パスワード変更ParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(ログインParameterMessages)) return new MessageSetterList<ログインParameterMessages>(path, index => new ログインParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷SaveCommandMessages)) return new MessageSetterList<入荷SaveCommandMessages>(path, index => new 入荷SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷一覧DisplayDataMessages)) return new MessageSetterList<入荷一覧DisplayDataMessages>(path, index => new 入荷一覧DisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷一覧SearchConditionMessages)) return new MessageSetterList<入荷一覧SearchConditionMessages>(path, index => new 入荷一覧SearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷明細SaveCommandMessages)) return new MessageSetterList<入荷明細SaveCommandMessages>(path, index => new 入荷明細SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷詳細Messages)) return new MessageSetterList<入荷詳細Messages>(path, index => new 入荷詳細Messages([.. path, index.ToString()], context), context);
            if (itemType == typeof(入荷詳細画面初期表示ParameterMessages)) return new MessageSetterList<入荷詳細画面初期表示ParameterMessages>(path, index => new 入荷詳細画面初期表示ParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品DisplayDataMessages)) return new MessageSetterList<商品DisplayDataMessages>(path, index => new 商品DisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品SaveCommandMessages)) return new MessageSetterList<商品SaveCommandMessages>(path, index => new 商品SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品SearchConditionMessages)) return new MessageSetterList<商品SearchConditionMessages>(path, index => new 商品SearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品一覧DisplayDataMessages)) return new MessageSetterList<商品一覧DisplayDataMessages>(path, index => new 商品一覧DisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品一覧SearchConditionMessages)) return new MessageSetterList<商品一覧SearchConditionMessages>(path, index => new 商品一覧SearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品在庫増減履歴DisplayDataMessages)) return new MessageSetterList<商品在庫増減履歴DisplayDataMessages>(path, index => new 商品在庫増減履歴DisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(商品在庫増減履歴SearchConditionMessages)) return new MessageSetterList<商品在庫増減履歴SearchConditionMessages>(path, index => new 商品在庫増減履歴SearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(在庫調整ParameterMessages)) return new MessageSetterList<在庫調整ParameterMessages>(path, index => new 在庫調整ParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(在庫調整SaveCommandMessages)) return new MessageSetterList<在庫調整SaveCommandMessages>(path, index => new 在庫調整SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(売上SaveCommandMessages)) return new MessageSetterList<売上SaveCommandMessages>(path, index => new 売上SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(売上一覧DisplayDataMessages)) return new MessageSetterList<売上一覧DisplayDataMessages>(path, index => new 売上一覧DisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(売上一覧SearchConditionMessages)) return new MessageSetterList<売上一覧SearchConditionMessages>(path, index => new 売上一覧SearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(売上詳細Messages)) return new MessageSetterList<売上詳細Messages>(path, index => new 売上詳細Messages([.. path, index.ToString()], context), context);
            if (itemType == typeof(売上詳細画面初期表示ParameterMessages)) return new MessageSetterList<売上詳細画面初期表示ParameterMessages>(path, index => new 売上詳細画面初期表示ParameterMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員RefDisplayDataMessages)) return new MessageSetterList<従業員RefDisplayDataMessages>(path, index => new 従業員RefDisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員RefSearchConditionMessages)) return new MessageSetterList<従業員RefSearchConditionMessages>(path, index => new 従業員RefSearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員SaveCommandMessages)) return new MessageSetterList<従業員SaveCommandMessages>(path, index => new 従業員SaveCommandMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員マスタDisplayDataMessages)) return new MessageSetterList<従業員マスタDisplayDataMessages>(path, index => new 従業員マスタDisplayDataMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員マスタSearchConditionMessages)) return new MessageSetterList<従業員マスタSearchConditionMessages>(path, index => new 従業員マスタSearchConditionMessages([.. path, index.ToString()], context), context);
            if (itemType == typeof(従業員一括更新ParameterMessages)) return new MessageSetterList<従業員一括更新ParameterMessages>(path, index => new 従業員一括更新ParameterMessages([.. path, index.ToString()], context), context);
        }

        throw new InvalidOperationException($"{type.Name} には既定のメッセージコンテナクラスが存在しません。");
    }
}

/// <inheritdoc cref="IMessageSetterList"/>
public partial class MessageSetterList<T> : MessageSetter, IMessageSetterList<T> where T : IMessageSetter {
    public MessageSetterList(IEnumerable<string> path, Func<int, T> createItem, MessageContainer context) : base(path, context) {
        _createItem = createItem;
    }

    private readonly Func<int, T> _createItem;
    private readonly Dictionary<int, T> _items = new();

    /// <summary>
    /// 指定した行のメッセージの入れ物を返します。
    /// 行が存在しなければ作成します。作成するだけではメッセージは追加されません。
    /// </summary>
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

    /// <summary>これまでにアクセスされた行のうち最大の添字に1を足した数。実際のデータの行数やメッセージの件数とは関係ありません。</summary>
    public int Count => _items.Keys.Count == 0
        ? 0
        : (_items.Keys.Max() + 1);

    /// <summary>先頭から、これまでにアクセスされた行のうち最大の添字までの各行を列挙します。</summary>
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

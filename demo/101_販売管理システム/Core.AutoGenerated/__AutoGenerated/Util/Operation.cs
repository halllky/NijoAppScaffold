
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : OperationCatalog.cs
// ※ このファイルを生成したメソッド: RenderCSharp

namespace MyApp;

/// <summary>
/// スキーマ定義から生成されたクエリまたはコマンド1件。
/// Web、バッチ、単体テストなどの呼び出し元は、<see cref="Operations{TOperationContext}"/> の static プロパティまたは <see cref="All"/> から処理を特定して呼び出す。
/// 引数・戻り値・メッセージの型を使った処理は <see cref="Accept"/> で <see cref="IOperationVisitor{TOperationContext, TResult}"/> に委ねる。
/// </summary>
/// <typeparam name="TOperationContext">クエリ・コマンドの呼び出し1回について、全処理に共通の入出力の型</typeparam>
public abstract class Operation<TOperationContext> {
    private protected Operation() { }

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
    public abstract TResult Accept<TResult>(IOperationVisitor<TOperationContext, TResult> visitor);

    /// <summary>引数のないコマンドの引数の型</summary>
    public sealed class NoParameter { }
    /// <summary>戻り値のないコマンドの戻り値の型。インスタンスは作られず、戻り値は常に null になる。</summary>
    public sealed class NoReturnValue { }

    /// <summary>
    /// すべてのクエリとコマンド
    /// </summary>
    public static IReadOnlyList<Operation<TOperationContext>> All { get; } = [
        global::MyApp.OperationsBase<TOperationContext>.商品,
        global::MyApp.OperationsBase<TOperationContext>.RefTo商品,
        global::MyApp.OperationsBase<TOperationContext>.従業員Ref,
        global::MyApp.OperationsBase<TOperationContext>.RefTo従業員Ref,
        global::MyApp.OperationsBase<TOperationContext>.従業員マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo従業員マスタ,
        global::MyApp.OperationsBase<TOperationContext>.売上一覧,
        global::MyApp.OperationsBase<TOperationContext>.入荷一覧,
        global::MyApp.OperationsBase<TOperationContext>.商品一覧,
        global::MyApp.OperationsBase<TOperationContext>.商品在庫増減履歴,
        global::MyApp.OperationsBase<TOperationContext>.入荷登録,
        global::MyApp.OperationsBase<TOperationContext>.商品データ取込,
        global::MyApp.OperationsBase<TOperationContext>.ログイン,
        global::MyApp.OperationsBase<TOperationContext>.ログアウト,
        global::MyApp.OperationsBase<TOperationContext>.パスワード再発行,
        global::MyApp.OperationsBase<TOperationContext>.従業員一括更新,
        global::MyApp.OperationsBase<TOperationContext>.売上新規登録,
        global::MyApp.OperationsBase<TOperationContext>.売上詳細画面初期表示,
        global::MyApp.OperationsBase<TOperationContext>.売上修正,
        global::MyApp.OperationsBase<TOperationContext>.入荷修正,
        global::MyApp.OperationsBase<TOperationContext>.在庫調整登録,
        global::MyApp.OperationsBase<TOperationContext>.パスワード変更,
        global::MyApp.OperationsBase<TOperationContext>.クリーンナップ,
        global::MyApp.OperationsBase<TOperationContext>.入荷詳細画面初期表示,
        global::MyApp.OperationsBase<TOperationContext>.売上金額シミュレート,
        global::MyApp.OperationsBase<TOperationContext>.ログイン状態確認,
    ];
}

/// <summary>
/// スキーマ定義から生成されたクエリとコマンドの一覧。
/// 処理ごとに、処理の物理名をプロパティ名とする static プロパティを持つ。
/// 各プロパティは常に同じインスタンスを返すので、処理の特定は参照の比較で行える。
/// </summary>
/// <typeparam name="TOperationContext">クエリ・コマンドの呼び出し1回について、全処理に共通の入出力の型</typeparam>
public abstract class Operations<TOperationContext> : OperationsBase<TOperationContext> {
    private Operations() { }
}

/// <summary>
/// Operations の static プロパティの実体。直接参照せず、Operations を通して参照すること。
/// 処理の物理名がどんな名前でもプロパティにできるよう、ほかのメンバーを持たないこのクラスでプロパティを宣言し、Operations はそれを継承するだけにしている。
/// このクラスと型引数の名前は、処理のプロパティ名と重複しないように生成時に選ばれる。
/// 同じ理由で、プロパティの中で参照する型はすべて global:: から始まる名前で書いている。
/// </summary>
public abstract class OperationsBase<TOperationContext> {
    private protected OperationsBase() { }

    /// <summary>商品</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.商品SearchCondition, global::MyApp.LoadReturnType<global::MyApp.商品DisplayData>, global::MyApp.商品SearchConditionMessages> 商品 { get; } = new() {
        Name = "商品",
        DisplayName = "商品",
        Path = "api/x84d89798b153073e568ca63588d75fa6/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>商品の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.商品SearchCondition, global::MyApp.LoadReturnType<global::MyApp.商品RefTarget>, global::MyApp.商品SearchConditionMessages> RefTo商品 { get; } = new() {
        Name = "ref-to:商品",
        DisplayName = "商品の参照検索",
        Path = "api/x84d89798b153073e568ca63588d75fa6/load-refs-x84d89798b153073e568ca63588d75fa6",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs商品Async(parameter, messages, context),
    };
    /// <summary>従業員Ref</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.従業員RefSearchCondition, global::MyApp.LoadReturnType<global::MyApp.従業員RefDisplayData>, global::MyApp.従業員RefSearchConditionMessages> 従業員Ref { get; } = new() {
        Name = "従業員Ref",
        DisplayName = "従業員Ref",
        Path = "api/x4fb556811cf0173660ac9554db27d63a/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>従業員Refの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.従業員RefSearchCondition, global::MyApp.LoadReturnType<global::MyApp.従業員RefRefTarget>, global::MyApp.従業員RefSearchConditionMessages> RefTo従業員Ref { get; } = new() {
        Name = "ref-to:従業員Ref",
        DisplayName = "従業員Refの参照検索",
        Path = "api/x4fb556811cf0173660ac9554db27d63a/load-refs-x4fb556811cf0173660ac9554db27d63a",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs従業員RefAsync(parameter, messages, context),
    };
    /// <summary>従業員マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.従業員マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.従業員マスタDisplayData>, global::MyApp.従業員マスタSearchConditionMessages> 従業員マスタ { get; } = new() {
        Name = "従業員マスタ",
        DisplayName = "従業員マスタ",
        Path = "api/xb1b46ee1f2e655af001c5d2452cdaefa/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>従業員マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.従業員マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.従業員マスタRefTarget>, global::MyApp.従業員マスタSearchConditionMessages> RefTo従業員マスタ { get; } = new() {
        Name = "ref-to:従業員マスタ",
        DisplayName = "従業員マスタの参照検索",
        Path = "api/xb1b46ee1f2e655af001c5d2452cdaefa/load-refs-xb1b46ee1f2e655af001c5d2452cdaefa",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs従業員マスタAsync(parameter, messages, context),
    };
    /// <summary>売上一覧</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.売上一覧SearchCondition, global::MyApp.LoadReturnType<global::MyApp.売上一覧DisplayData>, global::MyApp.売上一覧SearchConditionMessages> 売上一覧 { get; } = new() {
        Name = "売上一覧",
        DisplayName = "売上一覧",
        Path = "api/x27e684c7b886cbe3858b33c36a9c9865/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>入荷一覧</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.入荷一覧SearchCondition, global::MyApp.LoadReturnType<global::MyApp.入荷一覧DisplayData>, global::MyApp.入荷一覧SearchConditionMessages> 入荷一覧 { get; } = new() {
        Name = "入荷一覧",
        DisplayName = "入荷一覧",
        Path = "api/xc1d9f5ce6078744ae4e8fe270d2f2c5a/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>商品一覧</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.商品一覧SearchCondition, global::MyApp.LoadReturnType<global::MyApp.商品一覧DisplayData>, global::MyApp.商品一覧SearchConditionMessages> 商品一覧 { get; } = new() {
        Name = "商品一覧",
        DisplayName = "商品一覧",
        Path = "api/x9a4745f9e55cee71562e84f10ffad340/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>商品在庫増減履歴</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.商品在庫増減履歴SearchCondition, global::MyApp.LoadReturnType<global::MyApp.商品在庫増減履歴DisplayData>, global::MyApp.商品在庫増減履歴SearchConditionMessages> 商品在庫増減履歴 { get; } = new() {
        Name = "商品在庫増減履歴",
        DisplayName = "商品在庫増減履歴",
        Path = "api/x782009efdef066b92e0cd5c378ca947d/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>入荷登録</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.入荷詳細DisplayData, global::MyApp.入荷登録ReturnValueDisplayData, global::MyApp.入荷詳細Messages> 入荷登録 { get; } = new() {
        Name = "入荷登録",
        DisplayName = "入荷登録",
        Path = "api/xd14f4d89fb2adb0f35190352d52e032d/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute入荷登録Async(parameter, messages, context),
    };
    /// <summary>商品データ取込</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.Operation<TOperationContext>.NoParameter, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.MessageSetter> 商品データ取込 { get; } = new() {
        Name = "商品データ取込",
        DisplayName = "商品データ取込",
        Path = "api/x1da54d663d738b748b77758651bb4522/execute",
        IsQuery = false,
        InvokeAsync = async (app, _, messages, context) => { await app.Execute商品データ取込Async(messages, context); return null; },
    };
    /// <summary>ログイン</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.ログインParameterDisplayData, global::MyApp.ログインユーザー情報DisplayData, global::MyApp.ログインParameterMessages> ログイン { get; } = new() {
        Name = "ログイン",
        DisplayName = "ログイン",
        Path = "api/x7cda55dc0744dd2c61802153d4af8aed/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.ExecuteログインAsync(parameter, messages, context),
    };
    /// <summary>ログアウト</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.Operation<TOperationContext>.NoParameter, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.MessageSetter> ログアウト { get; } = new() {
        Name = "ログアウト",
        DisplayName = "ログアウト",
        Path = "api/xb4ebaa9c8d2306d0724d4f83301dc296/execute",
        IsQuery = false,
        InvokeAsync = async (app, _, messages, context) => { await app.ExecuteログアウトAsync(messages, context); return null; },
    };
    /// <summary>パスワード再発行</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.パスワード再発行ParameterDisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.パスワード再発行ParameterMessages> パスワード再発行 { get; } = new() {
        Name = "パスワード再発行",
        DisplayName = "パスワード再発行",
        Path = "api/xbba4773b1cc3dbccc3e5f243b46002cc/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Executeパスワード再発行Async(parameter, messages, context); return null; },
    };
    /// <summary>従業員一括更新</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.従業員一括更新ParameterDisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.従業員一括更新ParameterMessages> 従業員一括更新 { get; } = new() {
        Name = "従業員一括更新",
        DisplayName = "従業員一括更新",
        Path = "api/xafc1cb720c4ca7d3775a36938c0a763a/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Execute従業員一括更新Async(parameter, messages, context); return null; },
    };
    /// <summary>売上新規登録</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.売上詳細DisplayData, global::MyApp.売上新規登録ReturnValueDisplayData, global::MyApp.売上詳細Messages> 売上新規登録 { get; } = new() {
        Name = "売上新規登録",
        DisplayName = "売上新規登録",
        Path = "api/x19db87536c0905e6f7c739496867eb26/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute売上新規登録Async(parameter, messages, context),
    };
    /// <summary>売上詳細画面初期表示</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.売上詳細画面初期表示ParameterDisplayData, global::MyApp.売上詳細DisplayData, global::MyApp.売上詳細画面初期表示ParameterMessages> 売上詳細画面初期表示 { get; } = new() {
        Name = "売上詳細画面初期表示",
        DisplayName = "売上詳細画面初期表示",
        Path = "api/xd1a2d17337377bc77eedd669be22502f/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute売上詳細画面初期表示Async(parameter, messages, context),
    };
    /// <summary>売上修正</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.売上詳細DisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.売上詳細Messages> 売上修正 { get; } = new() {
        Name = "売上修正",
        DisplayName = "売上修正",
        Path = "api/x7d54a26bf13686c2c53d37b9495d3a4e/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Execute売上修正Async(parameter, messages, context); return null; },
    };
    /// <summary>入荷修正</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.入荷詳細DisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.入荷詳細Messages> 入荷修正 { get; } = new() {
        Name = "入荷修正",
        DisplayName = "入荷修正",
        Path = "api/x0033902a75bddee48538c66acecef2ae/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Execute入荷修正Async(parameter, messages, context); return null; },
    };
    /// <summary>在庫調整登録</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.在庫調整ParameterDisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.在庫調整ParameterMessages> 在庫調整登録 { get; } = new() {
        Name = "在庫調整登録",
        DisplayName = "在庫調整登録",
        Path = "api/xbf41aa9cdcac44f535e129c19936ea0a/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Execute在庫調整登録Async(parameter, messages, context); return null; },
    };
    /// <summary>パスワード変更</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.パスワード変更ParameterDisplayData, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.パスワード変更ParameterMessages> パスワード変更 { get; } = new() {
        Name = "パスワード変更",
        DisplayName = "パスワード変更",
        Path = "api/xdc1d3226cde441f8930dd84163df9a77/execute",
        IsQuery = false,
        InvokeAsync = async (app, parameter, messages, context) => { await app.Executeパスワード変更Async(parameter, messages, context); return null; },
    };
    /// <summary>クリーンナップ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.Operation<TOperationContext>.NoParameter, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.MessageSetter> クリーンナップ { get; } = new() {
        Name = "クリーンナップ",
        DisplayName = "クリーンナップ",
        Path = "api/x3adc3ad40a2b99ec7b77645f55580e22/execute",
        IsQuery = false,
        InvokeAsync = async (app, _, messages, context) => { await app.ExecuteクリーンナップAsync(messages, context); return null; },
    };
    /// <summary>入荷詳細画面初期表示</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.入荷詳細画面初期表示ParameterDisplayData, global::MyApp.入荷詳細DisplayData, global::MyApp.入荷詳細画面初期表示ParameterMessages> 入荷詳細画面初期表示 { get; } = new() {
        Name = "入荷詳細画面初期表示",
        DisplayName = "入荷詳細画面初期表示",
        Path = "api/x33aa4a55478ae5999326b0323cc2302c/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute入荷詳細画面初期表示Async(parameter, messages, context),
    };
    /// <summary>売上金額シミュレート</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.売上詳細DisplayData, global::MyApp.売上詳細DisplayData, global::MyApp.売上詳細Messages> 売上金額シミュレート { get; } = new() {
        Name = "売上金額シミュレート",
        DisplayName = "売上金額シミュレート",
        Path = "api/xb272886d5bb6356eac086491d361ba00/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute売上金額シミュレートAsync(parameter, messages, context),
    };
    /// <summary>ログイン状態確認</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.Operation<TOperationContext>.NoParameter, global::MyApp.ログインユーザー情報DisplayData, global::MyApp.MessageSetter> ログイン状態確認 { get; } = new() {
        Name = "ログイン状態確認",
        DisplayName = "ログイン状態確認",
        Path = "api/xcc53c1e81a21617ce18a684f4429939b/execute",
        IsQuery = false,
        InvokeAsync = (app, _, messages, context) => app.Executeログイン状態確認Async(messages, context),
    };
}

/// <inheritdoc cref="Operation{TOperationContext}"/>
/// <typeparam name="TParameter">引数の型</typeparam>
/// <typeparam name="TReturnValue">戻り値の型</typeparam>
/// <typeparam name="TMessages">引数に対するメッセージの型</typeparam>
public sealed class Operation<TOperationContext, TParameter, TReturnValue, TMessages> : Operation<TOperationContext> where TMessages : IMessageSetter {
    /// <summary>
    /// 処理を実行します。
    /// 処理の結果は、戻り値と、引数に対するメッセージ（エラーなど）と、呼び出し1回に共通の入出力に格納される。
    /// </summary>
    public required Func<AutoGeneratedApplicationService<TOperationContext>, TParameter, TMessages, TOperationContext, Task<TReturnValue?>> InvokeAsync { get; init; }

    /// <inheritdoc/>
    public override TResult Accept<TResult>(IOperationVisitor<TOperationContext, TResult> visitor) => visitor.Visit(this);
}

/// <summary>
/// <see cref="Operation{TOperationContext}"/> の引数・戻り値・メッセージの型を使った処理。
/// </summary>
public interface IOperationVisitor<TOperationContext, out TResult> {
    TResult Visit<TParameter, TReturnValue, TMessages>(Operation<TOperationContext, TParameter, TReturnValue, TMessages> operation)
        where TMessages : IMessageSetter;
}

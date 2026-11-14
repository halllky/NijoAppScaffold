
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
        global::MyApp.OperationsBase<TOperationContext>.旧システム部署情報,
        global::MyApp.OperationsBase<TOperationContext>.RefTo旧システム部署情報,
        global::MyApp.OperationsBase<TOperationContext>.汎用マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo汎用マスタ,
        global::MyApp.OperationsBase<TOperationContext>.部署,
        global::MyApp.OperationsBase<TOperationContext>.RefTo部署,
        global::MyApp.OperationsBase<TOperationContext>.社員,
        global::MyApp.OperationsBase<TOperationContext>.事業所,
        global::MyApp.OperationsBase<TOperationContext>.RefTo事業所,
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

    /// <summary>旧システム部署情報</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.旧システム部署情報SearchCondition, global::MyApp.LoadReturnType<global::MyApp.旧システム部署情報DisplayData>, global::MyApp.旧システム部署情報SearchConditionMessages> 旧システム部署情報 { get; } = new() {
        Name = "旧システム部署情報",
        DisplayName = "旧システム部署情報",
        Path = "api/x50e6d61250eea9fb6d19a8503d3088cf/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>旧システム部署情報の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.旧システム部署情報SearchCondition, global::MyApp.LoadReturnType<global::MyApp.旧システム部署情報RefTarget>, global::MyApp.旧システム部署情報SearchConditionMessages> RefTo旧システム部署情報 { get; } = new() {
        Name = "ref-to:旧システム部署情報",
        DisplayName = "旧システム部署情報の参照検索",
        Path = "api/x50e6d61250eea9fb6d19a8503d3088cf/load-refs-x50e6d61250eea9fb6d19a8503d3088cf",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs旧システム部署情報Async(parameter, messages, context),
    };
    /// <summary>汎用マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.汎用マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.汎用マスタDisplayData>, global::MyApp.汎用マスタSearchConditionMessages> 汎用マスタ { get; } = new() {
        Name = "汎用マスタ",
        DisplayName = "汎用マスタ",
        Path = "api/xd1b9d832c9260e2d45e315dd2398465d/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>汎用マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.汎用マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.汎用マスタRefTarget>, global::MyApp.汎用マスタSearchConditionMessages> RefTo汎用マスタ { get; } = new() {
        Name = "ref-to:汎用マスタ",
        DisplayName = "汎用マスタの参照検索",
        Path = "api/xd1b9d832c9260e2d45e315dd2398465d/load-refs-xd1b9d832c9260e2d45e315dd2398465d",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs汎用マスタAsync(parameter, messages, context),
    };
    /// <summary>部署</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.部署SearchCondition, global::MyApp.LoadReturnType<global::MyApp.部署DisplayData>, global::MyApp.部署SearchConditionMessages> 部署 { get; } = new() {
        Name = "部署",
        DisplayName = "部署",
        Path = "api/xcd4df9a9ee57181ac0d305d379f5b7d9/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>部署の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.部署SearchCondition, global::MyApp.LoadReturnType<global::MyApp.部署RefTarget>, global::MyApp.部署SearchConditionMessages> RefTo部署 { get; } = new() {
        Name = "ref-to:部署",
        DisplayName = "部署の参照検索",
        Path = "api/xcd4df9a9ee57181ac0d305d379f5b7d9/load-refs-xcd4df9a9ee57181ac0d305d379f5b7d9",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs部署Async(parameter, messages, context),
    };
    /// <summary>社員</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.社員SearchCondition, global::MyApp.LoadReturnType<global::MyApp.社員DisplayData>, global::MyApp.社員SearchConditionMessages> 社員 { get; } = new() {
        Name = "社員",
        DisplayName = "社員",
        Path = "api/x3b52cae5ff659e0173e92d917d9a3805/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>事業所</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.事業所SearchCondition, global::MyApp.LoadReturnType<global::MyApp.事業所DisplayData>, global::MyApp.事業所SearchConditionMessages> 事業所 { get; } = new() {
        Name = "事業所",
        DisplayName = "事業所",
        Path = "api/x35dd3d3ba70dcd87ce94bb3c350aaeb3/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>事業所の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.事業所SearchCondition, global::MyApp.LoadReturnType<global::MyApp.事業所RefTarget>, global::MyApp.事業所SearchConditionMessages> RefTo事業所 { get; } = new() {
        Name = "ref-to:事業所",
        DisplayName = "事業所の参照検索",
        Path = "api/x35dd3d3ba70dcd87ce94bb3c350aaeb3/load-refs-x35dd3d3ba70dcd87ce94bb3c350aaeb3",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs事業所Async(parameter, messages, context),
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

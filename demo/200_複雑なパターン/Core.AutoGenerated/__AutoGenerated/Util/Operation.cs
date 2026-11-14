
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
        global::MyApp.OperationsBase<TOperationContext>.患者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo患者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.診療科,
        global::MyApp.OperationsBase<TOperationContext>.RefTo診療科,
        global::MyApp.OperationsBase<TOperationContext>.医療従事者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo医療従事者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo権限,
        global::MyApp.OperationsBase<TOperationContext>.予約,
        global::MyApp.OperationsBase<TOperationContext>.RefTo予約,
        global::MyApp.OperationsBase<TOperationContext>.機器分類マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo機器分類マスタ,
        global::MyApp.OperationsBase<TOperationContext>.供給業者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo供給業者マスタ,
        global::MyApp.OperationsBase<TOperationContext>.保管庫マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo保管庫マスタ,
        global::MyApp.OperationsBase<TOperationContext>.医療機器マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo医療機器マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo在庫情報,
        global::MyApp.OperationsBase<TOperationContext>.RefTo在庫状況履歴,
        global::MyApp.OperationsBase<TOperationContext>.診療科マスタ,
        global::MyApp.OperationsBase<TOperationContext>.RefTo診療科マスタ,
        global::MyApp.OperationsBase<TOperationContext>.機器点検報告,
        global::MyApp.OperationsBase<TOperationContext>.RefTo対応措置,
        global::MyApp.OperationsBase<TOperationContext>.措置結果,
        global::MyApp.OperationsBase<TOperationContext>.診療収益分析,
        global::MyApp.OperationsBase<TOperationContext>.RefTo診療収益分析,
        global::MyApp.OperationsBase<TOperationContext>.機器分類別在庫集計ビュー,
        global::MyApp.OperationsBase<TOperationContext>.診療科別患者統計ビュー,
        global::MyApp.OperationsBase<TOperationContext>.簡易診療登録,
        global::MyApp.OperationsBase<TOperationContext>.医療機器管理,
        global::MyApp.OperationsBase<TOperationContext>.アプリケーション死活監視,
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

    /// <summary>患者マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.患者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.患者マスタDisplayData>, global::MyApp.患者マスタSearchConditionMessages> 患者マスタ { get; } = new() {
        Name = "患者マスタ",
        DisplayName = "患者マスタ",
        Path = "api/patient-master/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>患者マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.患者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.患者マスタRefTarget>, global::MyApp.患者マスタSearchConditionMessages> RefTo患者マスタ { get; } = new() {
        Name = "ref-to:患者マスタ",
        DisplayName = "患者マスタの参照検索",
        Path = "api/patient-master/load-refs-patient-master",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs患者マスタAsync(parameter, messages, context),
    };
    /// <summary>診療科</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療科SearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療科DisplayData>, global::MyApp.診療科SearchConditionMessages> 診療科 { get; } = new() {
        Name = "診療科",
        DisplayName = "診療科",
        Path = "api/medical-department/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>診療科の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療科SearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療科RefTarget>, global::MyApp.診療科SearchConditionMessages> RefTo診療科 { get; } = new() {
        Name = "ref-to:診療科",
        DisplayName = "診療科の参照検索",
        Path = "api/medical-department/load-refs-medical-department",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs診療科Async(parameter, messages, context),
    };
    /// <summary>医療従事者マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療従事者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.医療従事者マスタDisplayData>, global::MyApp.医療従事者マスタSearchConditionMessages> 医療従事者マスタ { get; } = new() {
        Name = "医療従事者マスタ",
        DisplayName = "医療従事者マスタ",
        Path = "api/x972cf363fe7c2d9ca3f05e934aed7f2e/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>医療従事者マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療従事者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.医療従事者マスタRefTarget>, global::MyApp.医療従事者マスタSearchConditionMessages> RefTo医療従事者マスタ { get; } = new() {
        Name = "ref-to:医療従事者マスタ",
        DisplayName = "医療従事者マスタの参照検索",
        Path = "api/x972cf363fe7c2d9ca3f05e934aed7f2e/load-refs-x972cf363fe7c2d9ca3f05e934aed7f2e",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs医療従事者マスタAsync(parameter, messages, context),
    };
    /// <summary>権限の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療従事者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.権限RefTarget>, global::MyApp.医療従事者マスタSearchConditionMessages> RefTo権限 { get; } = new() {
        Name = "ref-to:医療従事者マスタ/権限",
        DisplayName = "権限の参照検索",
        Path = "api/x972cf363fe7c2d9ca3f05e934aed7f2e/load-refs-x6b3502621ee7370ecd22c264fff53292",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs権限Async(parameter, messages, context),
    };
    /// <summary>予約</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.予約SearchCondition, global::MyApp.LoadReturnType<global::MyApp.予約DisplayData>, global::MyApp.予約SearchConditionMessages> 予約 { get; } = new() {
        Name = "予約",
        DisplayName = "予約",
        Path = "api/reservation/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>予約の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.予約SearchCondition, global::MyApp.LoadReturnType<global::MyApp.予約RefTarget>, global::MyApp.予約SearchConditionMessages> RefTo予約 { get; } = new() {
        Name = "ref-to:予約",
        DisplayName = "予約の参照検索",
        Path = "api/reservation/load-refs-reservation",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs予約Async(parameter, messages, context),
    };
    /// <summary>機器分類マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.機器分類マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.機器分類マスタDisplayData>, global::MyApp.機器分類マスタSearchConditionMessages> 機器分類マスタ { get; } = new() {
        Name = "機器分類マスタ",
        DisplayName = "機器分類マスタ",
        Path = "api/x06ed939be99b15cddc9dca2bfcb7d9bd/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>機器分類マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.機器分類マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.機器分類マスタRefTarget>, global::MyApp.機器分類マスタSearchConditionMessages> RefTo機器分類マスタ { get; } = new() {
        Name = "ref-to:機器分類マスタ",
        DisplayName = "機器分類マスタの参照検索",
        Path = "api/x06ed939be99b15cddc9dca2bfcb7d9bd/load-refs-x06ed939be99b15cddc9dca2bfcb7d9bd",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs機器分類マスタAsync(parameter, messages, context),
    };
    /// <summary>供給業者マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.供給業者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.供給業者マスタDisplayData>, global::MyApp.供給業者マスタSearchConditionMessages> 供給業者マスタ { get; } = new() {
        Name = "供給業者マスタ",
        DisplayName = "供給業者マスタ",
        Path = "api/x05cdfccfcce375f9f861d83d8bc79642/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>供給業者マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.供給業者マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.供給業者マスタRefTarget>, global::MyApp.供給業者マスタSearchConditionMessages> RefTo供給業者マスタ { get; } = new() {
        Name = "ref-to:供給業者マスタ",
        DisplayName = "供給業者マスタの参照検索",
        Path = "api/x05cdfccfcce375f9f861d83d8bc79642/load-refs-x05cdfccfcce375f9f861d83d8bc79642",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs供給業者マスタAsync(parameter, messages, context),
    };
    /// <summary>保管庫マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.保管庫マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.保管庫マスタDisplayData>, global::MyApp.保管庫マスタSearchConditionMessages> 保管庫マスタ { get; } = new() {
        Name = "保管庫マスタ",
        DisplayName = "保管庫マスタ",
        Path = "api/x804064fcf48cf0bd1bfd7f7db7b0c70a/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>保管庫マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.保管庫マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.保管庫マスタRefTarget>, global::MyApp.保管庫マスタSearchConditionMessages> RefTo保管庫マスタ { get; } = new() {
        Name = "ref-to:保管庫マスタ",
        DisplayName = "保管庫マスタの参照検索",
        Path = "api/x804064fcf48cf0bd1bfd7f7db7b0c70a/load-refs-x804064fcf48cf0bd1bfd7f7db7b0c70a",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs保管庫マスタAsync(parameter, messages, context),
    };
    /// <summary>医療機器マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療機器マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.医療機器マスタDisplayData>, global::MyApp.医療機器マスタSearchConditionMessages> 医療機器マスタ { get; } = new() {
        Name = "医療機器マスタ",
        DisplayName = "医療機器マスタ",
        Path = "api/medical-equipment-master/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>医療機器マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療機器マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.医療機器マスタRefTarget>, global::MyApp.医療機器マスタSearchConditionMessages> RefTo医療機器マスタ { get; } = new() {
        Name = "ref-to:医療機器マスタ",
        DisplayName = "医療機器マスタの参照検索",
        Path = "api/medical-equipment-master/load-refs-medical-equipment-master",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs医療機器マスタAsync(parameter, messages, context),
    };
    /// <summary>在庫情報の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療機器マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.在庫情報RefTarget>, global::MyApp.医療機器マスタSearchConditionMessages> RefTo在庫情報 { get; } = new() {
        Name = "ref-to:医療機器マスタ/在庫情報",
        DisplayName = "在庫情報の参照検索",
        Path = "api/medical-equipment-master/load-refs-xec4faeabf3099c693ea42605bd6b82ea",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs在庫情報Async(parameter, messages, context),
    };
    /// <summary>在庫状況履歴の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療機器マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.在庫状況履歴RefTarget>, global::MyApp.医療機器マスタSearchConditionMessages> RefTo在庫状況履歴 { get; } = new() {
        Name = "ref-to:医療機器マスタ/在庫情報/在庫状況履歴",
        DisplayName = "在庫状況履歴の参照検索",
        Path = "api/medical-equipment-master/load-refs-x2a225550d5999079b92b4cb410118fb3",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs在庫状況履歴Async(parameter, messages, context),
    };
    /// <summary>診療科マスタ</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療科マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療科マスタDisplayData>, global::MyApp.診療科マスタSearchConditionMessages> 診療科マスタ { get; } = new() {
        Name = "診療科マスタ",
        DisplayName = "診療科マスタ",
        Path = "api/xcd9246aaf1ce7856bc73a0dfd5aed1e4/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>診療科マスタの参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療科マスタSearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療科マスタRefTarget>, global::MyApp.診療科マスタSearchConditionMessages> RefTo診療科マスタ { get; } = new() {
        Name = "ref-to:診療科マスタ",
        DisplayName = "診療科マスタの参照検索",
        Path = "api/xcd9246aaf1ce7856bc73a0dfd5aed1e4/load-refs-xcd9246aaf1ce7856bc73a0dfd5aed1e4",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs診療科マスタAsync(parameter, messages, context),
    };
    /// <summary>機器点検報告</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.機器点検報告SearchCondition, global::MyApp.LoadReturnType<global::MyApp.機器点検報告DisplayData>, global::MyApp.機器点検報告SearchConditionMessages> 機器点検報告 { get; } = new() {
        Name = "機器点検報告",
        DisplayName = "機器点検報告",
        Path = "api/equipment-inspection-report/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>対応措置の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.機器点検報告SearchCondition, global::MyApp.LoadReturnType<global::MyApp.対応措置RefTarget>, global::MyApp.機器点検報告SearchConditionMessages> RefTo対応措置 { get; } = new() {
        Name = "ref-to:機器点検報告/対応措置",
        DisplayName = "対応措置の参照検索",
        Path = "api/equipment-inspection-report/load-refs-xcf72021731b9cfe72b09c5b0220cd6b8",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs対応措置Async(parameter, messages, context),
    };
    /// <summary>措置結果</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.措置結果SearchCondition, global::MyApp.LoadReturnType<global::MyApp.措置結果DisplayData>, global::MyApp.措置結果SearchConditionMessages> 措置結果 { get; } = new() {
        Name = "措置結果",
        DisplayName = "措置結果",
        Path = "api/action-result/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>診療収益分析</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療収益分析SearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療収益分析DisplayData>, global::MyApp.診療収益分析SearchConditionMessages> 診療収益分析 { get; } = new() {
        Name = "診療収益分析",
        DisplayName = "診療収益分析",
        Path = "api/medical-revenue-analysis/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>診療収益分析の参照検索</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療収益分析SearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療収益分析RefTarget>, global::MyApp.診療収益分析SearchConditionMessages> RefTo診療収益分析 { get; } = new() {
        Name = "ref-to:診療収益分析",
        DisplayName = "診療収益分析の参照検索",
        Path = "api/medical-revenue-analysis/load-refs-medical-revenue-analysis",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadRefs診療収益分析Async(parameter, messages, context),
    };
    /// <summary>機器分類別在庫集計ビュー</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.機器分類別在庫集計ビューSearchCondition, global::MyApp.LoadReturnType<global::MyApp.機器分類別在庫集計ビューDisplayData>, global::MyApp.機器分類別在庫集計ビューSearchConditionMessages> 機器分類別在庫集計ビュー { get; } = new() {
        Name = "機器分類別在庫集計ビュー",
        DisplayName = "機器分類別在庫集計ビュー",
        Path = "api/inventory-by-category-view/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>診療科別患者統計ビュー</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.診療科別患者統計ビューSearchCondition, global::MyApp.LoadReturnType<global::MyApp.診療科別患者統計ビューDisplayData>, global::MyApp.診療科別患者統計ビューSearchConditionMessages> 診療科別患者統計ビュー { get; } = new() {
        Name = "診療科別患者統計ビュー",
        DisplayName = "診療科別患者統計ビュー",
        Path = "api/patient-statistics-by-department-view/load",
        IsQuery = true,
        InvokeAsync = (app, parameter, messages, context) => app.LoadAsync(parameter, messages, context),
    };
    /// <summary>簡易診療登録</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.簡易診療登録ParameterDisplayData, global::MyApp.簡易診療登録ReturnValueDisplayData, global::MyApp.簡易診療登録ParameterMessages> 簡易診療登録 { get; } = new() {
        Name = "簡易診療登録",
        DisplayName = "簡易診療登録",
        Path = "api/simple-treatment-registration/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute簡易診療登録Async(parameter, messages, context),
    };
    /// <summary>医療機器管理</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.医療機器管理ParameterDisplayData, global::MyApp.医療機器管理ReturnValueDisplayData, global::MyApp.医療機器管理ParameterMessages> 医療機器管理 { get; } = new() {
        Name = "医療機器管理",
        DisplayName = "医療機器管理",
        Path = "api/medical-equipment-management/execute",
        IsQuery = false,
        InvokeAsync = (app, parameter, messages, context) => app.Execute医療機器管理Async(parameter, messages, context),
    };
    /// <summary>アプリケーション死活監視</summary>
    public static global::MyApp.Operation<TOperationContext, global::MyApp.Operation<TOperationContext>.NoParameter, global::MyApp.Operation<TOperationContext>.NoReturnValue, global::MyApp.MessageSetter> アプリケーション死活監視 { get; } = new() {
        Name = "アプリケーション死活監視",
        DisplayName = "アプリケーション死活監視",
        Path = "api/x44d71997b9f7d6a181bdc1a1d70715b3/execute",
        IsQuery = false,
        InvokeAsync = async (app, _, messages, context) => { await app.Executeアプリケーション死活監視Async(messages, context); return null; },
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

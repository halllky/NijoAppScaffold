namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 初期パスワード
    /// </summary>
    private const string INIT_PASSWORD = "pass";

    public override async Task Executeパスワード再発行(パスワード再発行ParameterDisplayData param, IPresentationContext<パスワード再発行ParameterMessages> context) {

        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            context.Messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.Values.対象者 == null) {
            context.Messages.対象者.AddError("対象者を選択してください。");
            return;
        }
        var targetEmployeeId = param.Values.対象者.Values.従業員番号;

        // パスワードのハッシュ化
        var salt = GenerateSalt();
        var hash = ComputeHash(INIT_PASSWORD, salt);

        // 更新
        using var tran = await DbContext.Database.BeginTransactionAsync();

        await Update従業員Async(targetEmployeeId, null, employee => {
            employee.SALT = salt;
            employee.パスワード = hash;
        }, context);

        if (!context.HasError()) {
            await tran.CommitAsync();
        }
    }
}

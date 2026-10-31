using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 初期パスワード
    /// </summary>
    private const string INIT_PASSWORD = "pass";

    public override async Task Executeパスワード再発行Async(パスワード再発行ParameterDisplayData param, IPresentationContext<パスワード再発行ParameterMessages> context) {

        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            context.Messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.対象者 == null) {
            context.Messages.対象者.AddError("対象者を選択してください。");
            return;
        }

        // パスワードのハッシュ化
        var salt = GenerateSalt();
        var hash = ComputeHash(INIT_PASSWORD, salt);

        // 更新。
        // 画面で編集された行ではないので読み込み時点のバージョンで楽観排他をかける。
        var employeeNumber = param.対象者.従業員番号;
        var employee = (await DbContext
            .LoadForEdit従業員Async(query => query.Where(x => x.従業員番号 == employeeNumber)))
            .SingleOrDefault();
        if (employee == null) {
            context.Messages.AddError(MSG.DataNotFoundError());
            return;
        }
        employee.SALT = salt;
        employee.パスワード = hash;

        if (!employee.TryValidate(out var validated, out var errors)) {
            errors.CopyTo(context.Messages);
            return;
        }
        if (context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            result.Errors.CopyTo(context.Messages);
            return;
        }

        await tran.CommitAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 初期パスワード
    /// </summary>
    private const string INIT_PASSWORD = "pass";

    public override async Task Executeパスワード再発行Async(パスワード再発行ParameterDisplayData param, パスワード再発行ParameterMessages messages, OperationContext context) {

        // 権限チェック
        if (LoginUser == null || !LoginUser.Isシステム管理者) {
            messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 入力チェック
        if (param.対象者 == null) {
            messages.対象者.AddError("対象者を選択してください。");
            return;
        }

        // パスワードのハッシュ化
        var passwordHash = PasswordHash.Create(INIT_PASSWORD);

        // 更新。
        // 画面で編集された行ではないので読み込み時点のバージョンで楽観排他をかける。
        var employeeNumber = param.対象者.従業員番号;
        var employee = (await DbContext
            .LoadForEdit従業員Async(query => query.Where(x => x.従業員番号 == employeeNumber)))
            .SingleOrDefault();
        if (employee == null) {
            messages.AddError(MSG.DataNotFoundError());
            return;
        }
        employee.パスワード = passwordHash.Hash;
        employee.SALT = passwordHash.Salt;

        if (!employee.TryValidate(out var validated, out var errors)) {
            errors.CopyTo(messages);
            return;
        }
        if (context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            result.Errors.CopyTo(messages);
            return;
        }

        // 再発行前のパスワードでログインしている状態を残さないよう、対象者のセッションをすべて消す
        await DbContext.セッションDbSet
            .Where(s => s.ユーザ_従業員番号 == employeeNumber)
            .ExecuteDeleteAsync();

        await tran.CommitAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeパスワード変更Async(パスワード変更ParameterDisplayData param, パスワード変更ParameterMessages messages, OperationContext context) {
        // ログインチェック
        if (LoginUser == null) {
            messages.AddError("ログインしていません。");
            return;
        }

        // 入力チェック
        var newPassword = param.新しいパスワード;
        var confirmPassword = param.新しいパスワード_確認用;

        if (string.IsNullOrWhiteSpace(newPassword)) {
            messages.新しいパスワード.AddError("新しいパスワードを入力してください。");
            return;
        }
        if (newPassword != confirmPassword) {
            messages.新しいパスワード_確認用.AddError("パスワードが一致しません。");
            return;
        }

        // パスワードのハッシュ化
        var passwordHash = PasswordHash.Create(newPassword);

        // 更新。
        // 画面で編集された行ではないので読み込み時点のバージョンで楽観排他をかける。
        var employeeNumber = LoginUser.従業員番号;
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

        await tran.CommitAsync();
    }
}

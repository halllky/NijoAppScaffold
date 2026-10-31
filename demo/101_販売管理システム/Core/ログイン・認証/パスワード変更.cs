using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeパスワード変更Async(パスワード変更ParameterDisplayData param, IPresentationContext<パスワード変更ParameterMessages> context) {
        // ログインチェック
        if (LoginUser == null) {
            context.Messages.AddError("ログインしていません。");
            return;
        }

        // 入力チェック
        var newPassword = param.新しいパスワード;
        var confirmPassword = param.新しいパスワード_確認用;

        if (string.IsNullOrWhiteSpace(newPassword)) {
            context.Messages.新しいパスワード.AddError("新しいパスワードを入力してください。");
            return;
        }
        if (newPassword != confirmPassword) {
            context.Messages.新しいパスワード_確認用.AddError("パスワードが一致しません。");
            return;
        }

        // パスワードのハッシュ化
        var salt = GenerateSalt();
        var hash = ComputeHash(newPassword, salt);

        // 更新。
        // 画面で編集された行ではないので読み込み時点のバージョンで楽観排他をかける。
        var employeeNumber = LoginUser.従業員番号;
        var employee = (await DbContext
            .LoadForEdit従業員Async(query => query.Where(x => x.従業員番号 == employeeNumber)))
            .SingleOrDefault();
        if (employee == null) {
            context.Messages.AddError(MSG.DataNotFoundError());
            return;
        }
        employee.SALT = salt;
        employee.パスワード = hash;

        employee.GetValidationErrors().CopyTo(context.Messages);
        if (context.Messages.HasError() || context.ValidationOnly) return;

        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(employee);
        if (!result.Success) {
            result.Errors.CopyTo(context.Messages);
            return;
        }

        await tran.CommitAsync();
    }
}

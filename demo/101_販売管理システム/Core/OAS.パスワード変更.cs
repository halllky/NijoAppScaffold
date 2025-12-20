using Microsoft.EntityFrameworkCore;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeパスワード変更(パスワード変更ParameterDisplayData param, IPresentationContext<パスワード変更ParameterMessages> context) {
        // ログインチェック
        if (LoginUser == null) {
            context.Messages.AddError("ログインしていません。");
            return;
        }

        // 入力チェック
        var newPassword = param.Values.新しいパスワード;
        var confirmPassword = param.Values.新しいパスワード_確認用;

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

        // 更新
        using var tran = await DbContext.Database.BeginTransactionAsync();

        var result = await Update従業員Async(LoginUser.従業員番号, null, employee => {
            employee.SALT = salt;
            employee.パスワード = hash;
        }, context);

        if (result.Success) {
            await tran.CommitAsync();
        }
    }
}

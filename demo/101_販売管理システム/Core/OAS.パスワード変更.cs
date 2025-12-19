using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeパスワード変更(パスワード変更ParameterDisplayData param, IPresentationContext<パスワード変更ParameterMessages> context) {
        // ログインチェック
        if (this.LoginUser == null) {
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
        var salt = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(salt);
        }

        var passwordBytes = Encoding.UTF8.GetBytes(newPassword);
        using var hmac = new HMACSHA256(salt);
        var hash = hmac.ComputeHash(passwordBytes);

        // 更新
        using var tran = await DbContext.Database.BeginTransactionAsync();

        await Update従業員Async(LoginUser.従業員番号, null, employee => {
            employee.SALT = salt;
            employee.パスワード = hash;
        }, context);

        if (!context.HasError()) {
            await tran.CommitAsync();
        }
    }
}

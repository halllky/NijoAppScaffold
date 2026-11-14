using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        if (string.IsNullOrWhiteSpace(param.現在のパスワード)) {
            messages.現在のパスワード.AddError("現在のパスワードを入力してください。");
            return;
        }
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

        // 現在のパスワードの照合
        var employeeNumber = LoginUser.従業員番号;
        var current = await DbContext.従業員DbSet
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.従業員番号 == employeeNumber);
        if (current?.パスワード == null
            || current.SALT == null
            || PasswordHash.Verify(param.現在のパスワード, current.パスワード, current.SALT) == PasswordHash.VerificationResult.Failed) {
            messages.現在のパスワード.AddError("現在のパスワードが正しくありません。");
            return;
        }

        // パスワードのハッシュ化
        var passwordHash = PasswordHash.Create(newPassword);

        // 更新。
        // 画面で編集された行ではないので読み込み時点のバージョンで楽観排他をかける。
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

        // 古いパスワードで別の端末にログインしている状態を残さないよう、この操作をしたセッション以外の本人のセッションを消す
        var currentSessionKey = ServiceProvider.GetRequiredService<Core.Authorization.ISessionKeyProvider>().GetSessionKey();
        var currentStoredKey = currentSessionKey == null ? null : Core.Authorization.SessionKey.ToStoredValue(currentSessionKey);
        await DbContext.セッションDbSet
            .Where(s => s.ユーザ_従業員番号 == employeeNumber && s.セッションキー != currentStoredKey)
            .ExecuteDeleteAsync();

        await tran.CommitAsync();
        Log.LogInformation("パスワードを変更しました。");
    }
}

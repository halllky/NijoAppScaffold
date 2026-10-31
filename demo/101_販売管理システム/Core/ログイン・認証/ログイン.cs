
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task ExecuteログインAsync(ログインParameterDisplayData param, IPresentationContextWithReturnValue<ログインユーザー情報DisplayData, ログインParameterMessages> context) {
        // 入力チェック
        if (string.IsNullOrWhiteSpace(param.従業員番号)) {
            context.Messages.従業員番号.AddError("従業員番号を入力してください。");
            return;
        }
        if (string.IsNullOrWhiteSpace(param.パスワード)) {
            context.Messages.パスワード.AddError("パスワードを入力してください。");
            return;
        }

        // 従業員検索
        var employee = await DbContext.従業員DbSet
            .Include(e => e.従業員の権限)
            .SingleOrDefaultAsync(e => e.従業員番号 == param.従業員番号);

        if (employee == null) {
            context.Messages.AddError("従業員番号またはパスワードが間違っています。");
            return;
        }

        // パスワード検証
        if (employee.パスワード == null || employee.SALT == null) {
            context.Messages.AddError("パスワードが設定されていません。");
            return;
        }

        var computedHash = ComputeHash(param.パスワード, employee.SALT);

        if (!computedHash.SequenceEqual(employee.パスワード)) {
            context.Messages.AddError("従業員番号またはパスワードが間違っています。");
            return;
        }

        // セッションキー発番
        var sessionKey = Guid.NewGuid().ToString();

        // セッション保存
        var session = DbContext.StartCreatingセッション();
        session.セッションキー = sessionKey;
        session.ユーザ = new() { 従業員番号 = employee.従業員番号 };
        session.最終ログイン日時 = CurrentTime;

        if (!session.TryValidate(out var validated, out var errors)) {
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

        // クライアントに返す
        var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
        sessionKeyProvider.ReturnSessionKeyToClient(sessionKey);

        // JavaScriptで使うログインユーザー情報
        context.ReturnValue = Core.Authorization.LoginUser.From(employee).ToDisplayData();

        await tran.CommitAsync();
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task<ログインユーザー情報DisplayData?> ExecuteログインAsync(ログインParameterDisplayData param, ログインParameterMessages messages, IPresentationContext context) {
        // 入力チェック
        if (string.IsNullOrWhiteSpace(param.従業員番号)) {
            messages.従業員番号.AddError("従業員番号を入力してください。");
            return null;
        }
        if (string.IsNullOrWhiteSpace(param.パスワード)) {
            messages.パスワード.AddError("パスワードを入力してください。");
            return null;
        }

        // 従業員検索
        var employee = await DbContext.従業員DbSet
            .Include(e => e.従業員の権限)
            .SingleOrDefaultAsync(e => e.従業員番号 == param.従業員番号);

        if (employee == null) {
            messages.AddError("従業員番号またはパスワードが間違っています。");
            return null;
        }

        // パスワード検証
        if (employee.パスワード == null || employee.SALT == null) {
            messages.AddError("パスワードが設定されていません。");
            return null;
        }

        var computedHash = ComputeHash(param.パスワード, employee.SALT);

        if (!computedHash.SequenceEqual(employee.パスワード)) {
            messages.AddError("従業員番号またはパスワードが間違っています。");
            return null;
        }

        // セッションキー発番
        var sessionKey = Guid.NewGuid().ToString();

        // セッション保存
        var session = DbContext.StartCreatingセッション();
        session.セッションキー = sessionKey;
        session.ユーザ = new() { 従業員番号 = employee.従業員番号 };
        session.最終ログイン日時 = CurrentTime;

        if (!session.TryValidate(out var validated, out var errors)) {
            errors.CopyTo(messages);
            return null;
        }

        // ValidationOnly を無視しているのは意図的。
        // 「ログインしますか？」は普通は確認しないため。
        // if (context.ValidationOnly) return null;

        await using var tran = await BeginTransactionAsync();

        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            result.Errors.CopyTo(messages);
            return null;
        }

        // クライアントに返す
        var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
        sessionKeyProvider.ReturnSessionKeyToClient(sessionKey);

        await tran.CommitAsync();

        // JavaScriptで使うログインユーザー情報
        return Core.Authorization.LoginUser.From(employee).ToDisplayData();
    }
}

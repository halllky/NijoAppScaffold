
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;
using System.Security.Cryptography;
using System.Text;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeログイン(ログインParameterDisplayData param, IPresentationContext<ログインParameterMessages> context) {
        // 入力チェック
        if (string.IsNullOrWhiteSpace(param.Values.従業員番号)) {
            context.Messages.従業員番号.AddError("従業員番号を入力してください。");
            return;
        }
        if (string.IsNullOrWhiteSpace(param.Values.パスワード)) {
            context.Messages.パスワード.AddError("パスワードを入力してください。");
            return;
        }

        // 従業員検索
        var employee = await DbContext.従業員DbSet
            .SingleOrDefaultAsync(e => e.従業員番号 == param.Values.従業員番号);

        if (employee == null) {
            context.Messages.AddError("従業員番号またはパスワードが間違っています。");
            return;
        }

        // パスワード検証
        if (employee.パスワード == null || employee.SALT == null) {
            context.Messages.AddError("パスワードが設定されていません。");
            return;
        }

        var inputPasswordBytes = Encoding.UTF8.GetBytes(param.Values.パスワード);
        using var hmac = new HMACSHA256(employee.SALT);
        var computedHash = hmac.ComputeHash(inputPasswordBytes);

        if (!computedHash.SequenceEqual(employee.パスワード)) {
            context.Messages.AddError("従業員番号またはパスワードが間違っています。");
            return;
        }

        // セッションキー発番
        var sessionKey = Guid.NewGuid().ToString();

        // セッション保存
        using var tran = await DbContext.Database.BeginTransactionAsync();
        await CreateセッションAsync(new() {
            セッションキー = sessionKey,
            ユーザ = new() { 従業員番号 = employee.従業員番号 },
            最終ログイン日時 = CurrentTime,
        }, context);

        if (context.HasError()) {
            return;
        }

        // クライアントに返す
        var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
        sessionKeyProvider.ReturnSessionKeyToClient(sessionKey);

        await tran.CommitAsync();
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Core.Authorization;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeログイン(ログインParameterDisplayData param, IPresentationContextWithReturnValue<ログインユーザー情報DisplayData, ログインParameterMessages> context) {
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

        var computedHash = ComputeHash(param.Values.パスワード, employee.SALT);

        if (!computedHash.SequenceEqual(employee.パスワード)) {
            context.Messages.AddError("従業員番号またはパスワードが間違っています。");
            return;
        }

        // セッションキー発番
        var sessionKey = Guid.NewGuid().ToString();

        // セッション保存
        using var tran = await DbContext.Database.BeginTransactionAsync();
        var result = await CreateセッションAsync(new() {
            セッションキー = sessionKey,
            ユーザ = new() { 従業員番号 = employee.従業員番号 },
            最終ログイン日時 = CurrentTime,
        }, context);

        if (result.Result != DataModelSaveResultType.Completed) {
            return;
        }

        // クライアントに返す
        var sessionKeyProvider = ServiceProvider.GetRequiredService<ISessionKeyProvider>();
        sessionKeyProvider.ReturnSessionKeyToClient(sessionKey);

        // JavaScriptで使うログインユーザー情報
        context.ReturnValue = new() {
            Values = new() {
                従業員番号 = employee.従業員番号,
                氏名 = employee.氏名,
                入荷機能を利用可能 = employee.入荷担当 == true || employee.システム管理者 == true,
                販売機能を利用可能 = employee.販売担当 == true || employee.システム管理者 == true,
                システム管理者 = employee.システム管理者 == true,
            },
        };

        await tran.CommitAsync();
    }
}

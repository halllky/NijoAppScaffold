
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {

    public override async Task ExecuteログアウトAsync(MessageSetter messages, OperationContext context) {
        // セッションキー取得。
        // ログインしていない場合は何もしない
        var sessionKeyProvider = ServiceProvider.GetRequiredService<Core.Authorization.ISessionKeyProvider>();
        var sessionKey = sessionKeyProvider.GetSessionKey();
        if (sessionKey == null) {
            return;
        }

        // DBセッション削除。
        // 楽観排他などきちっとした検証は不要なのでExecuteDeleteAsyncで直接消す。
        var storedKey = Core.Authorization.SessionKey.ToStoredValue(sessionKey);
        await DbContext.セッションDbSet
            .Where(s => s.セッションキー == storedKey)
            .ExecuteDeleteAsync();

        // クライアント側のセッションキーもクリア
        sessionKeyProvider.ClearSessionKey();

        Log.LogInformation("ログアウトしました。");
    }

}

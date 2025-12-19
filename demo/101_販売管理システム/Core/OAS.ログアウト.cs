
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

partial class OverridedApplicationService {

    public override async Task Executeログアウト(IPresentationContext<MessageSetter> context) {
        // セッションキー取得。
        // ログインしていない場合は何もしない
        var sessionKeyProvider = ServiceProvider.GetRequiredService<Core.Authorization.ISessionKeyProvider>();
        var sessionKey = sessionKeyProvider.GetSessionKey();
        if (sessionKey == null) {
            return;
        }

        // DBセッション削除
        await DbContext.セッションDbSet
            .Where(s => s.セッションキー == sessionKey)
            .ExecuteDeleteAsync();

        // クライアント側のセッションキーもクリア
        sessionKeyProvider.ClearSessionKey();
    }

}

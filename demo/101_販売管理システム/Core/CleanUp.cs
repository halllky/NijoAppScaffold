using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task ExecuteクリーンナップAsync(MessageSetter messages, OperationContext context) {

        // 権限チェック。
        // この処理は画面からも呼び出せるので、利用者が実行する場合はシステム管理者に限る。
        // ログインユーザーがいない呼び出し（バッチなど）は、利用者の操作ではないので実行を認める。
        if (LoginUser != null && !LoginUser.Isシステム管理者) {
            messages.AddError("システム管理者のみ実行可能です。");
            return;
        }

        // 利用者に確認を求める前の入力チェックの段階では、削除しない
        if (context.ValidationOnly) return;

        // 最後に操作してから有効期限が過ぎたセッションを削除。
        // 期限が過ぎたセッションはログインユーザーの特定の時点で無効として扱われるので、ここで消すのは古い行の掃除のため
        var threshold = CurrentTime - Core.Authorization.SessionKey.IdleTimeout;

        var deletedCount = await DbContext.セッションDbSet
            .Where(s => s.最終操作日時 < threshold)
            .ExecuteDeleteAsync();

        Log.LogInformation($"クリーンナップ処理を実行しました。削除されたセッション数: {deletedCount}");

        // 古いログの削除は NLog の設定で自動的に行われるため、ここでは特に処理を実装しない。
    }
}

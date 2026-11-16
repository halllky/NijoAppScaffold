using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {
    protected override async Task OnExecuteクリーンナップAsync(MessageSetter messages, OperationContext context) {
        // 利用者に確認を求める前の入力チェックの段階では、削除しない
        if (context.ValidationOnly) return;

        // 最後に操作してから有効期限が過ぎたセッションを削除。
        // 期限が過ぎたセッションはログインユーザーの特定の時点で無効として扱われるので、ここで消すのは古い行の掃除のため
        var threshold = CurrentTime - Core.Authorization.SessionKey.IdleTimeout;

        var deletedCount = await DbContext.セッションDbSet
            .Where(s => s.最終操作日時 < threshold)
            .ExecuteDeleteAsync();

        // 保存期間を過ぎた冪等キーを削除
        var idempotencyKeyThreshold = CurrentTime - 冪等キーの保存期間;
        var deletedIdempotencyKeyCount = await DbContext.冪等キーDbSet
            .Where(k => k.CreatedAt < idempotencyKeyThreshold)
            .ExecuteDeleteAsync();

        // 不要になったユーザー宛ての通知を削除
        var deletedNotificationCount = await Delete不要な通知Async();

        Log.LogInformation("クリーンナップ処理を実行しました。削除されたセッション数: {SessionCount}, 削除された冪等キー数: {IdempotencyKeyCount}, 削除された通知数: {NotificationCount}",
            deletedCount, deletedIdempotencyKeyCount, deletedNotificationCount);

        // 古いログの削除は NLog の設定で自動的に行われるため、ここでは特に処理を実装しない。
    }
}

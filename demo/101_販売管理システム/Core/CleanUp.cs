using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// クリーンナップを毎日実行するジョブ。
    /// ジョブはログインユーザーのいない内部の処理なので、システム管理者だけが呼び出せるコマンドを通さず、処理の本体を直接呼ぶ。
    /// </summary>
    public static readonly ジョブの種類 クリーンナップのジョブ = new("クリーンナップ", async (app, job) => {
        job.結果メッセージ = await app.RunクリーンナップAsync();
    }, ジョブの予定.毎日(new TimeOnly(3, 0)));

    protected override async Task OnExecuteクリーンナップAsync(MessageSetter messages, OperationContext context) {
        // 利用者に確認を求める前の入力チェックの段階では、削除しない
        if (context.ValidationOnly) return;

        await RunクリーンナップAsync();
    }

    /// <summary>
    /// 古くなったデータを削除する。
    /// </summary>
    /// <returns>削除した件数の説明</returns>
    private async Task<string> RunクリーンナップAsync() {
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

        // 保存期間を過ぎたジョブの記録と作業フォルダを削除
        var deletedJobCount = await Delete終わったジョブAsync();

        Log.LogInformation("クリーンナップ処理を実行しました。削除されたセッション数: {SessionCount}, 削除された冪等キー数: {IdempotencyKeyCount}, 削除された通知数: {NotificationCount}, 削除されたジョブ数: {JobCount}",
            deletedCount, deletedIdempotencyKeyCount, deletedNotificationCount, deletedJobCount);

        // 古いログの削除は NLog の設定で自動的に行われるため、ここでは特に処理を実装しない。

        return $"セッション {deletedCount} 件、冪等キー {deletedIdempotencyKeyCount} 件、通知 {deletedNotificationCount} 件、ジョブ {deletedJobCount} 件を削除しました。";
    }
}

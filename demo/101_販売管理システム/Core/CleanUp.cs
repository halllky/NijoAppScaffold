using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp;

partial class OverridedApplicationService {
    public override async Task Executeクリーンナップ(IPresentationContext<MessageSetter> context) {
        // 最終ログイン日時から24時間経過したセッションを削除
        var threshold = CurrentTime.AddHours(-24);

        var deletedCount = await DbContext.セッションDbSet
            .Where(s => s.最終ログイン日時 < threshold)
            .ExecuteDeleteAsync();

        Log.LogInformation($"クリーンナップ処理を実行しました。削除されたセッション数: {deletedCount}");

        // 古いログの削除は NLog の設定で自動的に行われるため、ここでは特に処理を実装しない。
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.Idempotency;
using MyApp.Core.外部システム;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 記録した冪等キーを残しておく期間。これを過ぎたものはクリーンナップで削除する。
    /// 応答がないまま打ち切られた操作を、利用者がやり直すまでの間は残しておく必要がある。
    /// </summary>
    public static readonly TimeSpan 冪等キーの保存期間 = TimeSpan.FromHours(24);

    /// <summary>
    /// 冪等キーを、開始したトランザクションの中で記録する。
    /// 同じキーが記録済みなら、その操作は処理済みなので、<see cref="AlreadyExecutedException"/> を投げて処理を止める。
    /// 記録は業務のデータと一緒に確定する。業務の処理が取り消されれば記録も残らないので、送り直せば処理される。
    /// 冪等キーが渡されていなければ何もしない。
    /// 同じ呼び出しの中でトランザクションを何度か開始する処理では、記録が確定するまで、開始のたびに記録を試みる。
    /// </summary>
    /// <param name="transaction">記録するトランザクション</param>
    private async Task Record冪等キーAsync(ExternalResourceTransaction transaction) {
        if (_冪等キーRecordedIn?.IsCommitted == true) return;
        var key = ServiceProvider.GetService<IIdempotencyKeyProvider>()?.Key;
        if (key == null) return;

        var command = DbContext.StartCreating冪等キー();
        command.キー = key;
        if (!command.TryValidate(out var validated, out _)) {
            throw new InvalidOperationException($"冪等キーの値が不正です: {key}");
        }

        // 同じキーで並行して呼ばれた場合は、先に記録した側のトランザクションが終わるまで、DB の書き込みのロックで待たされる。
        // 先に記録した側がコミットすれば一意制約違反になり、取り消せばこちらが記録して処理を続ける
        var result = await DbContext.TrySaveAsync(validated);
        if (result.FailureKind == SaveFailureKind.UniqueConstraint) {
            Log.LogInformation("同じ冪等キーの操作が処理済みなので、処理を中断します。冪等キー: {Key}", key);
            throw new AlreadyExecutedException();
        }
        if (!result.Success) {
            throw new InvalidOperationException($"冪等キーを記録できませんでした: {key}");
        }
        _冪等キーRecordedIn = transaction;
    }
    /// <summary>冪等キーを記録したトランザクション</summary>
    private ExternalResourceTransaction? _冪等キーRecordedIn;
}

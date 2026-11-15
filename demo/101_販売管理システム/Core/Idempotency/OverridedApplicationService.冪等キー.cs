using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.Idempotency;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 記録した冪等キーを残しておく期間。これを過ぎたものはクリーンナップで削除する。
    /// 応答がないまま打ち切られた操作を、利用者がやり直すまでの間は残しておく必要がある。
    /// </summary>
    public static readonly TimeSpan 冪等キーの保存期間 = TimeSpan.FromHours(24);

    /// <summary>
    /// 呼び出し元から渡された冪等キーを、開始したばかりのトランザクションの中で記録する。
    /// 同じキーがすでに記録されていれば（同じ操作が処理済みなら）、<see cref="AlreadyExecutedException"/> を投げる。
    /// 記録は業務のデータと同じトランザクションで確定するので、業務の処理が取り消されたときは記録も取り消され、送り直せば処理される。
    /// 冪等キーが渡されていない呼び出し元（単体テスト、バッチなど）では何もしない。
    /// 1回の呼び出しの中でトランザクションを複数回開始する処理では、最初のトランザクションでだけ記録する。
    /// </summary>
    private async Task Record冪等キーAsync() {
        if (_is冪等キーRecorded) return;
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
        _is冪等キーRecorded = true;
    }
    private bool _is冪等キーRecorded;
}

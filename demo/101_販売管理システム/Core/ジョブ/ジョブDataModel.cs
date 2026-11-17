using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.Authorization;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// ジョブを登録する。登録したジョブは、サーバーのプロセスの中で、登録された順に後から実行される。
    /// 業務のデータと一緒に確定させるため、開始したトランザクションの中で呼ぶこと。業務の処理が取り消されれば、ジョブも登録されない。
    /// ログインユーザーがいれば、画面から起動したジョブとして、そのユーザーを起動したユーザーに記録する。
    /// </summary>
    /// <param name="種類">引数をとらないジョブの種類</param>
    /// <returns>登録したジョブの識別子</returns>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合</exception>
    public Task<string> RegisterジョブAsync(ジョブの種類 種類) {
        if (種類.Has引数) throw new ArgumentException($"ジョブの種類「{種類.Name}」には引数を渡してください。", nameof(種類));
        return RegisterジョブAsync(種類.Name, 引数: null);
    }

    /// <inheritdoc cref="RegisterジョブAsync(ジョブの種類)"/>
    /// <param name="種類">引数をとるジョブの種類</param>
    /// <param name="引数">ジョブの本体に渡す引数</param>
    public Task<string> RegisterジョブAsync<TArgument>(ジョブの種類<TArgument> 種類, TArgument 引数) {
        return RegisterジョブAsync(種類.Name, ジョブの種類<TArgument>.SerializeArgument(引数));
    }

    private async Task<string> RegisterジョブAsync(string 種類, string? 引数) {
        var command = DbContext.StartCreatingジョブ();
        command.ジョブID = Guid.NewGuid().ToString();
        command.種類 = 種類;
        command.引数 = 引数;
        command.状態 = ジョブの状態.待機;
        command.起動したユーザー = LoginUser?.従業員番号;

        // 登録の内容は業務処理のプログラムが決めるので、保存できない内容は利用者の入力ではなくプログラムの誤り
        if (!command.TryValidate(out var validated, out var errors)) {
            throw new InvalidOperationException($"ジョブを登録できません: {string.Join(", ", errors.GetAllMessages())}");
        }
        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            throw new InvalidOperationException($"ジョブを登録できませんでした: {string.Join(", ", result.Errors.GetAllMessages())}");
        }
        return command.ジョブID;
    }

    /// <summary>
    /// このスコープの中の処理を、そのジョブの処理として実行するようにする。
    /// 画面から起動したジョブは起動したユーザーをログインユーザーとし、定期的なジョブはログインユーザーなしとする。
    /// 作成者・更新者には、ログインユーザーか、いなければジョブの種類を記録する。
    /// 起動したユーザーが削除されていれば、ログインユーザーなしとする。
    /// </summary>
    internal async Task ActAsジョブAsync(string 種類, string? 起動したユーザー) {
        LoginUser? loginUser = null;
        if (起動したユーザー != null) {
            var employee = await DbContext.従業員DbSet
                .Include(e => e.従業員の権限)
                .SingleOrDefaultAsync(e => e.従業員番号 == 起動したユーザー);
            if (employee != null) loginUser = LoginUser.From(employee);
        }
        ServiceProvider.GetRequiredService<LoginUserProvider>().SetWithoutSession(loginUser, 種類);
    }

    /// <summary>
    /// 待機中のジョブのうち、最も早く登録されたものを実行中にして返す。
    /// 他のスレッドや他のサーバーが先に取り出したジョブは、楽観排他で保存に失敗するので、次のジョブを探す。
    /// </summary>
    /// <returns>取り出したジョブ。待機中のジョブが無ければ null</returns>
    internal async Task<取り出したジョブ?> Take次のジョブAsync() {
        while (true) {
            var candidate = (await DbContext.LoadForEditジョブAsync(query => query
                .Where(j => j.状態 == ジョブの状態.待機)
                .OrderBy(j => j.CreatedAt)
                .ThenBy(j => j.ジョブID)
                .Take(1)))
                .SingleOrDefault();
            if (candidate == null) return null;

            await ActAsジョブAsync(candidate.種類!, candidate.起動したユーザー);
            candidate.状態 = ジョブの状態.実行中;
            candidate.開始日時 = CurrentTime;
            candidate.実行したサーバー = Environment.MachineName;
            if (!candidate.TryValidate(out var validated, out var errors)) {
                throw new InvalidOperationException($"ジョブを実行中にできません: {string.Join(", ", errors.GetAllMessages())}");
            }

            await using var tran = await BeginTransactionAsync();
            var result = await DbContext.TrySaveAsync(validated);
            if (result.FailureKind == SaveFailureKind.Concurrency) continue;
            if (!result.Success) {
                throw new InvalidOperationException($"ジョブを実行中にできませんでした: {string.Join(", ", result.Errors.GetAllMessages())}");
            }
            await tran.CommitAsync();

            return new(candidate.ジョブID!, candidate.種類!, candidate.引数, candidate.起動したユーザー);
        }
    }

    /// <summary>
    /// 取り出したジョブの本体を実行する。作業フォルダとスプールのフォルダは、実行の前に作る。
    /// 本体が例外を投げたら、Error としてログに出力し、異常終了とする。
    /// </summary>
    /// <returns>終わった状態と、利用者に見せる結果</returns>
    internal async Task<(ジョブの状態 状態, string? 結果メッセージ)> Executeジョブの本体Async(取り出したジョブ taken, ジョブの種類 種類, CancellationToken 停止の合図) {
        var job = new 実行中のジョブ(taken.ジョブID, taken.引数, Settings.Job.Get作業フォルダ(taken.ジョブID), 停止の合図);
        try {
            Directory.CreateDirectory(job.スプールフォルダ);
            await 種類.ExecuteAsync(this, job);
            return (ジョブの状態.正常終了, job.結果メッセージ);

        } catch (Exception ex) when (ex is OperationCanceledException && 停止の合図.IsCancellationRequested) {
            Log.LogError(ex, "サーバーの停止により、ジョブを中断しました。");
            return (ジョブの状態.異常終了, job.結果メッセージ ?? "サーバーの停止により、中断しました。");

        } catch (Exception ex) {
            Log.LogError(ex, "ジョブが異常終了しました。");
            return (ジョブの状態.異常終了, job.結果メッセージ ?? "エラーが発生したため、異常終了しました。");
        }
    }

    /// <summary>
    /// 種類が定義されていないジョブを異常終了とする。
    /// 登録した後に、その種類を削除したプログラムに入れ替えた場合などに起きる。
    /// </summary>
    internal (ジョブの状態 状態, string? 結果メッセージ) Fail定義されていない種類のジョブ(string 種類) {
        Log.LogError("ジョブの種類「{Kind}」が定義されていません。", 種類);
        return (ジョブの状態.異常終了, "ジョブの種類が定義されていないため、実行できませんでした。");
    }

    /// <summary>
    /// ジョブの結果を記録する。画面から起動したジョブなら、起動したユーザーに終了を通知する。
    /// </summary>
    internal async Task Recordジョブの結果Async(string ジョブID, ジョブの状態 状態, string? 結果メッセージ) {
        var job = (await DbContext.LoadForEditジョブAsync(query => query.Where(j => j.ジョブID == ジョブID))).Single();
        job.状態 = 状態;
        job.終了日時 = CurrentTime;
        job.結果メッセージ = 結果メッセージ;
        if (!job.TryValidate(out var validated, out var errors)) {
            throw new InvalidOperationException($"ジョブの結果を記録できません: {string.Join(", ", errors.GetAllMessages())}");
        }

        await using var tran = await BeginTransactionAsync();
        var result = await DbContext.TrySaveAsync(validated);
        if (!result.Success) {
            throw new InvalidOperationException($"ジョブの結果を記録できませんでした: {string.Join(", ", result.Errors.GetAllMessages())}");
        }
        if (job.起動したユーザー != null) {
            var 結果 = 状態 == ジョブの状態.正常終了 ? "正常終了しました" : "異常終了しました";
            await Create通知Async([job.起動したユーザー], $"ジョブ「{job.種類}」が{結果}。", 画面のURL.ジョブ);
        }
        await tran.CommitAsync();
    }

    /// <summary>
    /// このサーバーで実行中のまま残っているジョブの識別子を返す。
    /// サーバーの起動の時点では、このサーバーで実行中のジョブは無いはずなので、前回の停止で中断されたものである。
    /// </summary>
    internal async Task<string[]> Load中断されたジョブAsync() {
        return await DbContext.ジョブDbSet
            .Where(j => j.状態 == ジョブの状態.実行中 && j.実行したサーバー == Environment.MachineName)
            .Select(j => j.ジョブID!)
            .ToArrayAsync();
    }
}

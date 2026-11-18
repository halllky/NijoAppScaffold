using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApp.Core.Authorization;
using MyApp.Core.ジョブ;

namespace MyApp;

partial class OverridedApplicationService {

    /// <summary>
    /// 終了したジョブの記録と作業フォルダの保存期間。これを過ぎたものはクリーンナップで削除する。
    /// </summary>
    public static readonly TimeSpan 終了したジョブの保存期間 = TimeSpan.FromDays(90);

    /// <summary>
    /// ジョブを登録する。登録したジョブは、後からサーバーのプロセス内で登録順に実行される。
    /// 業務データと一緒に確定させるため、トランザクションの中で呼ぶこと。業務の処理がロールバックされれば、ジョブも登録されない。
    /// ログインユーザーがいれば画面から起動したジョブとみなし、そのユーザーを起動したユーザーとして記録する。
    /// </summary>
    /// <param name="種類">引数をとらないジョブの種類</param>
    /// <returns>登録したジョブの ID</returns>
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

        // 登録内容は利用者の入力ではなくプログラムが決めるので、保存できない内容はプログラムの誤りとして例外にする
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
    /// ジョブを、ログインユーザーに閲覧権限があるものに絞り込む。システム管理者は全員のジョブを、それ以外のユーザーは自分が起動したジョブだけを閲覧できる。
    /// 利用者の操作でジョブを参照する処理（一覧、スプールのダウンロード）は、必ずこれで絞り込むこと。
    /// </summary>
    /// <exception cref="InvalidOperationException">ログインしていない場合</exception>
    private IQueryable<ジョブDbEntity> Filter閲覧できるジョブ(IQueryable<ジョブDbEntity> query) {
        var loginUser = LoginUser ?? throw new InvalidOperationException("ログインしていない状態で、ジョブが参照されました。");
        if (loginUser.Isシステム管理者) return query;
        return query.Where(j => j.起動したユーザー == loginUser.従業員番号);
    }

    /// <summary>
    /// この DI スコープ内の処理を、指定したジョブの処理として扱うようにする。
    /// ログインユーザーは、画面から起動したジョブなら起動したユーザーとし、定期実行のジョブや、起動したユーザーが削除済みの場合はなしとする。
    /// 作成者・更新者には、ログインユーザーがいればそのユーザーを、いなければジョブの種類を記録する。
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
    /// 待機中のジョブのうち最も早く登録されたものを、実行中に更新して返す。
    /// ほかのスレッドやサーバーが先に取り出したジョブは楽観排他エラーで更新に失敗するので、その場合は次のジョブを探す。
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
    /// 取り出したジョブの本体を実行する。作業フォルダとスプールフォルダは実行前に作成する。
    /// 本体が例外を投げたら、Error レベルでログを出力し、異常終了とする。
    /// </summary>
    /// <returns>終了時の状態と、利用者向けの結果のメッセージ</returns>
    internal async Task<(ジョブの状態 状態, string? 結果メッセージ)> Executeジョブの本体Async(取り出したジョブ taken, ジョブの種類 種類, CancellationToken cancellationToken) {
        var job = new 実行中のジョブ(taken.ジョブID, taken.引数, Settings.Job.Get作業フォルダ(taken.ジョブID), cancellationToken);
        try {
            Directory.CreateDirectory(job.スプールフォルダ);
            await 種類.ExecuteAsync(this, job);
            return (ジョブの状態.正常終了, job.結果メッセージ);

        } catch (Exception ex) when (ex is OperationCanceledException && cancellationToken.IsCancellationRequested) {
            Log.LogError(ex, "サーバーの停止により、ジョブを中断しました。");
            return (ジョブの状態.異常終了, job.結果メッセージ ?? MSG.JobInterruptedResult());

        } catch (Exception ex) {
            Log.LogError(ex, "ジョブが異常終了しました。");
            return (ジョブの状態.異常終了, job.結果メッセージ ?? MSG.JobFailedResult());
        }
    }

    /// <summary>
    /// 種類が定義されていないジョブを異常終了とする。
    /// ジョブの登録後に、その種類を削除したバージョンのプログラムをリリースした場合などに起きる。
    /// </summary>
    internal (ジョブの状態 状態, string? 結果メッセージ) Fail定義されていない種類のジョブ(string 種類) {
        Log.LogError("ジョブの種類「{Kind}」が定義されていません。", 種類);
        return (ジョブの状態.異常終了, MSG.JobKindUndefinedResult());
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
            var 本文 = 状態 == ジョブの状態.正常終了
                ? MSG.JobSucceededNotification(job.種類!)
                : MSG.JobFailedNotification(job.種類!);
            await Create通知Async([job.起動したユーザー], 本文, PAGES_URL.P004());
        }
        await tran.CommitAsync();
    }

    /// <summary>
    /// このサーバーで実行中のまま残っているジョブの ID を返す。
    /// サーバーの起動時点では、このサーバーで実行中のジョブは無いはずなので、これらは前回の停止で中断されたものである。
    /// </summary>
    internal async Task<string[]> Load中断されたジョブIDAsync() {
        return await DbContext.ジョブDbSet
            .Where(j => j.状態 == ジョブの状態.実行中 && j.実行したサーバー == Environment.MachineName)
            .Select(j => j.ジョブID!)
            .ToArrayAsync();
    }

    /// <summary>
    /// 終了から保存期間が過ぎたジョブの記録を削除する。クリーンナップで呼ぶ。
    /// あわせて、このサーバーの作業フォルダのうち、記録が無くなったジョブのものを削除する。
    /// 作業フォルダはジョブを実行したサーバーにしかないので、ほかのサーバーの作業フォルダは、そのサーバーでクリーンナップを実行したときに削除される。
    /// </summary>
    /// <returns>削除した記録の件数</returns>
    private async Task<int> Delete保存期間を過ぎたジョブAsync() {
        var threshold = CurrentTime - 終了したジョブの保存期間;
        var deletedCount = await DbContext.ジョブDbSet
            .Where(j => (j.状態 == ジョブの状態.正常終了 || j.状態 == ジョブの状態.異常終了) && j.終了日時 < threshold)
            .ExecuteDeleteAsync();

        var workDirectory = new DirectoryInfo(Settings.Job.WorkDirectory);
        if (!workDirectory.Exists) return deletedCount;

        var folderIds = workDirectory.GetDirectories().Select(d => d.Name).ToArray();
        var existingIds = await DbContext.ジョブDbSet
            .Where(j => folderIds.Contains(j.ジョブID))
            .Select(j => j.ジョブID!)
            .ToArrayAsync();
        foreach (var id in folderIds.Except(existingIds)) {
            try {
                Directory.Delete(Settings.Job.Get作業フォルダ(id), recursive: true);
            } catch (IOException ex) {
                // 使用中のファイルがある場合など。次回のクリーンナップで改めて削除を試みる
                Log.LogWarning(ex, "ジョブの作業フォルダを削除できませんでした: {Directory}", id);
            } catch (UnauthorizedAccessException ex) {
                Log.LogWarning(ex, "ジョブの作業フォルダを削除できませんでした: {Directory}", id);
            }
        }
        return deletedCount;
    }
}

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace MyApp.Core.外部システム;

/// <summary>
/// DB更新と外部リソース更新の整合性を保つためのトランザクション管理クラス。
/// </summary>
public class ExternalResourceTransaction : IAsyncDisposable {

    /// <summary>
    /// コンストラクタ。
    /// DBトランザクションの破棄はこのクラスの中で行うため
    /// 引数のトランザクションはusingせずそのまま渡すこと。
    /// </summary>
    public ExternalResourceTransaction(IDbContextTransaction dbTransaction, ILogger logger) {
        _dbTransaction = dbTransaction;
        _logger = logger;
    }

    private readonly IDbContextTransaction _dbTransaction;
    private readonly ILogger _logger;
    /// <summary>
    /// マルチスレッドからの同時アクセスを防止するためのロックオブジェクト。
    /// </summary>
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    /// <summary>
    /// 登録された外部リソース更新処理の一覧。
    /// 実際に処理が行われるのは <see cref="CommitAsync"/> のタイミング。
    /// </summary>
    private readonly List<UpdateAction> _updateActions = new();

    /// <summary>
    /// コミットまたはロールバック済みかどうか。
    /// </summary>
    public bool IsCommittedOrRolledBack { get; private set; } = false;

    /// <summary>
    /// 外部リソースの更新を予約する。
    /// 実際に処理が行われるのは <see cref="CommitAsync"/> のタイミング。
    /// </summary>
    public void Prepare(string resourceNameForLog, Func<CancellationToken, Task> func) {
        _semaphore.Wait();
        try {
            _updateActions.Add(new() {
                ResourceNameForLog = resourceNameForLog,
                UpdateResourceAsync = func,
            });
        } finally {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// <para>
    /// 外部リソースの更新を実行する。
    /// このメソッドが呼ばれずにスコープを抜けた場合は自動的にロールバックされる。
    /// </para>
    /// <para>
    /// DB更新SQL発行はこのメソッドの外で既に行われている想定。
    /// このメソッドの内部ではDBトランザクションのコミットだけを行う。
    /// </para>
    /// <para>
    /// 一度に複数の外部リソースの更新が行われる場合、
    /// いずれかでエラーが発生したときは登録順でエラー以降の外部リソース更新処理はスキップされる。
    /// また、その場合、DB更新はロールバックされる。
    /// </para>
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default) {
        await _semaphore.WaitAsync(cancellationToken);
        try {
            if (IsCommittedOrRolledBack) {
                throw new InvalidOperationException("すでにコミットまたはロールバック済みのトランザクションに対してコミットを行うことはできません。");
            }
            IsCommittedOrRolledBack = true;
        } finally {
            _semaphore.Release();
        }

        // 外部リソース更新処理を順次実行
        var hasError = false;
        var exceptionList = new List<Exception>();
        foreach (var action in _updateActions) {
            // 一度に複数の外部リソースを更新する場合、
            // いずれかでエラーが発生した場合は他の更新処理はスキップする
            if (hasError) {
                _logger.LogWarning("外部リソース '{resourceName}' の更新処理はスキップされました。", action.ResourceNameForLog);
                continue;
            }

            // 外部リソースの更新処理を実行
            try {
                await action.UpdateResourceAsync(cancellationToken);
            } catch (Exception ex) {
                hasError = true;
                exceptionList.Add(ex);
                _logger.LogError(ex, "外部リソース '{resourceName}' の更新処理で例外発生", action.ResourceNameForLog);
            }
        }

        // いずれかの外部リソースの更新でエラーが発生した場合はDBトランザクションをロールバック
        if (hasError) {
            try {
                await _dbTransaction.RollbackAsync(cancellationToken);
            } catch (Exception ex) {
                exceptionList.Add(ex);
                _logger.LogError(ex, "外部リソース更新後のDBロールバックで例外発生");
            }

            throw new AggregateException("外部リソースの更新処理中にエラーが発生しました。", exceptionList);
        }

        // 外部リソースの更新が1個も無いか、すべて成功した場合はDBトランザクションをコミット
        await _dbTransaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// リソース解放処理。
    /// コミットされずにこのクラスのインスタンスが破棄される場合、ロールバックを行う。
    /// </summary>
    async ValueTask IAsyncDisposable.DisposeAsync() {
        bool shouldDispose = true;
        try {
            // コミットまたはロールバック済みかどうかを確認
            await _semaphore.WaitAsync();
            if (IsCommittedOrRolledBack) {
                _semaphore.Release();
                return;
            }
            IsCommittedOrRolledBack = true;

            // ここまで来るということはまだコミットされていないということなのでロールバックを行う
            try {
                await _dbTransaction.RollbackAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "外部リソース更新後のDBロールバックで例外発生");
            }
            _semaphore.Release();

        } catch (ObjectDisposedException) {
            // すでに破棄済みの場合は何もしない
            shouldDispose = false;
            return;

        } finally {
            if (shouldDispose) {
                await _dbTransaction.DisposeAsync();
                _semaphore.Dispose();
            }
        }
    }

    /// <summary>
    /// 外部リソースの更新処理を表すインターフェイス。
    /// </summary>
    private class UpdateAction {

        /// <summary>
        /// エラー発生時のログ出力用の外部リソース名。
        /// 運用担当者がリカバリ作業を行う際に役立つ情報となる。
        /// </summary>
        public required string ResourceNameForLog { get; init; }

        /// <summary>
        /// 外部リソースの更新処理を行う。
        /// <list type="bullet">
        /// <item>例外が発生した場合のリトライはこのメソッドの内部で行うこと。</item>
        /// <item>例外のログ出力は呼び出し元で行うので不要。</item>
        /// <item>エラーが出たとしても他の処理を継続したい場合、この中で例外をキャッチしてハンドリングすること。</item>
        /// </list>
        /// </summary>
        public required Func<CancellationToken, Task> UpdateResourceAsync { get; init; }
    }

}

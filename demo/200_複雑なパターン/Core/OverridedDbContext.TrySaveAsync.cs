using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MyApp;

/// <summary>
/// データの保存処理。
/// 保存時に行うべきことはシステムの要件によって大きく異なるので自動生成せず、ここに記述している。
/// 以下はあくまで既定の実装であり、実際のアプリケーションの要件に応じて自由に書き換えてよい。
/// </summary>
partial class OverridedDbContext {

    /// <summary>
    /// 新規登録または更新を、1件保存します。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 引数のコマンドの内容の変更追跡を開始して
    /// <see cref="ISaveCommand.GetValidationErrors"/> と
    /// <see cref="DbContext.SaveChangesAsync"/> を実行し、
    /// 成否にかかわらず、終了時に変更追跡を終了します。
    /// 更新日時や更新者といった監査項目と、更新後の楽観排他のバージョンの指定はこの中で行われます。
    /// </para>
    /// <para>
    /// トランザクションの開始、コミット、ロールバックは行いません。
    /// 失敗しても同じトランザクションはそのまま使えるので、コミットするか否かは呼び出し側で判断してください。
    /// </para>
    /// <para>
    /// 一度このメソッドの引数に渡したコマンドは、成否にかかわらず再度このメソッドの引数として渡すことはできません。
    /// </para>
    /// <para>
    /// 保存時に発生したエラーは例外ではなく戻り値で失敗を返します。
    /// エラーメッセージは、どの行で起きたかの捕捉が難しいので、項目ごとではなく保存処理全体の単位で返ります。
    /// キー重複を行単位で見せたいなどの場合は、このメソッドを呼ぶ前に独自に検証してください。
    /// </para>
    /// </remarks>
    /// <returns>
    /// 保存の成否、エラーメッセージ、保存後のデータ。
    /// 保存後のデータは引数のコマンドとは別のインスタンスです。
    /// 保存時に採番されたシーケンスの参照や、続けて別の更新を行なうときに使えます。
    /// </returns>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合、および使用済みのコマンドを渡した場合</exception>
    public async Task<SaveResult<TUpdateCommand>> TrySaveAsync<TUpdateCommand>(ISaveCommand<TUpdateCommand> command, CancellationToken ct = default) {
        var result = await TrySaveCoreAsync([command], ct).ConfigureAwait(false);
        return result.Success
            ? SaveResult<TUpdateCommand>.Succeeded(result.Errors, result.Updated[0])
            : SaveResult<TUpdateCommand>.Failed(result.Errors, result.FailureKind);
    }

    /// <summary>
    /// 削除を、1件保存します。
    /// 物理削除か論理削除かはスキーマ定義に従います。
    /// </summary>
    /// <remarks>
    /// 処理内容と、失敗時や例外時の挙動は新規登録・更新の場合と同じです。
    /// <inheritdoc cref="TrySaveAsync{TUpdateCommand}(ISaveCommand{TUpdateCommand}, CancellationToken)" path="/remarks/*"/>
    /// </remarks>
    /// <returns>保存の成否、エラーメッセージ</returns>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合、および使用済みのコマンドを渡した場合</exception>
    public async Task<SaveResult> TrySaveAsync<TUpdateCommand>(IDeleteCommand<TUpdateCommand> command, CancellationToken ct = default) {
        return await TrySaveCoreAsync([command], ct).ConfigureAwait(false);
    }

    /// <summary>
    /// 複数の新規登録・更新・削除を、1回の SaveChangesAsync で一括保存します。
    /// 1件でも失敗すると、このメソッドによる変更はすべて保存されません。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 一部が失敗しても他はコミットさせたい場合は、コマンドの数だけループで回してこのメソッドを呼び、都度コミットしてください。
    /// </para>
    /// <para>
    /// 引数のコマンドの中でキーが重複している場合は <see cref="SaveFailureKind.UniqueConstraint"/> の失敗になります。
    /// 同じキーの削除と新規登録を同時に行なうこともできないので、その場合はこのメソッドを2回に分けて呼んでください。
    /// </para>
    /// 処理内容と、失敗時や例外時の挙動は1件ずつ保存する場合と同じです。
    /// <inheritdoc cref="TrySaveAsync{TUpdateCommand}(ISaveCommand{TUpdateCommand}, CancellationToken)" path="/remarks/*"/>
    /// </remarks>
    /// <returns>
    /// 保存の成否、エラーメッセージ、保存後のデータ。
    /// 保存後のデータの順番は引数の順番と対応します。削除されたものは含まれないので、削除を含む場合は要素の位置がその分ずれます。
    /// </returns>
    /// <exception cref="InvalidOperationException">トランザクションが開始されていない場合、および使用済みのコマンドを渡した場合</exception>
    public Task<SaveResult<IList<TUpdateCommand>>> TrySaveAsync<TUpdateCommand>(IReadOnlyList<ISaveCommand<TUpdateCommand>> commands, CancellationToken ct = default) {
        return TrySaveCoreAsync(commands, ct);
    }

    private async Task<SaveResult<IList<TUpdateCommand>>> TrySaveCoreAsync<TUpdateCommand>(IReadOnlyList<ISaveCommand<TUpdateCommand>> commands, CancellationToken ct) {
        if (Database.CurrentTransaction == null) throw new InvalidOperationException("トランザクションが開始されていません。");

        var errors = new MessageSetter([], new MessageContainer());
        SaveResult<IList<TUpdateCommand>> Fail(SaveFailureKind kind) => SaveResult<IList<TUpdateCommand>>.Failed(errors, kind);

        // このメソッドの外で何らかのエンティティが変更されており、かつまだ SaveChanges が呼ばれていない場合、
        // このメソッドの保存によってそれらも巻き添えで保存されてしまう。そういった変更が無いかを調べる。
        if (ChangeTracker.HasChanges()) {
            errors.AddError(MSG.UnknownUpdateError("保存処理の前に別の変更が行なわれています。"));
            _logger.LogError("保存処理の前に別の変更が行なわれています。");
            return Fail(SaveFailureKind.ProgrammingError);
        }

        // バリデーションエラーチェックのかけ直し。
        // 利用者向けのエラーはこのメソッドより前に確認されているはずなので、ここでエラーになるのはプログラムの誤り
        foreach (var command in commands) {
            var validationErrors = command.GetValidationErrors();
            if (!validationErrors.HasError()) continue;

            foreach (var message in validationErrors.GetAllMessages()) {
                errors.AddError(message);
            }
            _logger.LogError("保存処理の中でバリデーションエラーが発生しました: {command} {errors}",
                ToLogString(command),
                string.Join(", ", validationErrors.GetAllMessages()));
        }
        if (errors.HasError()) return Fail(SaveFailureKind.ProgrammingError);

        // 引数のコマンド内部でキー重複していないかを調べる。
        // 変更追跡を開始するとEFCoreの例外になってしまうので、その前に検出する
        var keys = new HashSet<object>();
        foreach (var command in commands) {
            var key = command.GetKeyForDuplicateCheck();
            if (key == null || keys.Add(key)) continue;

            errors.AddError(MSG.UniqueConstraintError());
            _logger.LogWarning("同時に保存しようとしたデータのキーが重複しています: {command}", ToLogString(command));
            return Fail(SaveFailureKind.UniqueConstraint);
        }

        var tracked = new List<object>();
        try {
            // 変更追跡を開始し、更新日時・更新者・更新後楽観排他バージョンを設定する
            foreach (var command in commands) {
                tracked.AddRange(command.StartTracking());
            }

            // 保存実行
            await SaveChangesAsync(ct).ConfigureAwait(false);

        } catch (DbUpdateConcurrencyException ex) {
            // 楽観排他エラー
            errors.AddError(MSG.ConcurrencyError());
            _logger.LogWarning(ex, "楽観排他エラーが発生しました: {commands}", ToLogString(commands));
            return Fail(SaveFailureKind.Concurrency);

        } catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex)) {
            // 一意制約違反。
            // 他ユーザーとの操作競合の可能性が高いため、ハンドリング内容は楽観排他に準ずる。
            errors.AddError(MSG.UniqueConstraintError());
            _logger.LogWarning(ex, "一意制約違反が発生しました: {commands}", ToLogString(commands));
            return Fail(SaveFailureKind.UniqueConstraint);

        } catch (DbUpdateException ex) {
            // 一般DBエラー。接続が切れた場合など色々
            errors.AddError(MSG.UnknownUpdateError(ex.Message));
            _logger.LogError(ex, "保存処理でエラーが発生しました: {commands}", ToLogString(commands));
            return Fail(SaveFailureKind.DatabaseError);

        } finally {
            // 後続処理に影響が出るのを防ぐためエンティティを解放
            foreach (var entity in tracked) {
                Entry(entity).State = EntityState.Detached;
            }
        }

        _logger.LogInformation("データを保存しました: {commands}", ToLogString(commands));

        // 後続処理で使えるようにするために保存後データを返す
        var updated = commands
            .Select(command => command.ToSavedCommand())
            .OfType<TUpdateCommand>()
            .ToList();
        return SaveResult<IList<TUpdateCommand>>.Succeeded(errors, updated);
    }

    /// <summary>
    /// 一意制約違反か否かを判定します。RDBMSの種類に依存します。
    /// ここでは SQLite にあわせています。
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) {
        const int SQLITE_CONSTRAINT = 19;
        const int SQLITE_CONSTRAINT_PRIMARYKEY = 1555;
        const int SQLITE_CONSTRAINT_UNIQUE = 2067;

        return ex.InnerException is SqliteException {
            SqliteErrorCode: SQLITE_CONSTRAINT,
            SqliteExtendedErrorCode: SQLITE_CONSTRAINT_PRIMARYKEY or SQLITE_CONSTRAINT_UNIQUE,
        };
    }

    private static string ToLogString(ISaveCommand command) {
        return command.ToLogJson().ToJsonString(LOG_JSON_OPTIONS);
    }
    private static string ToLogString(IEnumerable<ISaveCommand> commands) {
        return string.Join(", ", commands.Select(ToLogString));
    }
    private static readonly JsonSerializerOptions LOG_JSON_OPTIONS = new() {
        // Unicode文字のエスケープを抑制
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

/// <summary>
/// 保存処理の結果
/// </summary>
public class SaveResult {
    protected SaveResult(bool success, MessageSetter errors, SaveFailureKind? failureKind) {
        Success = success;
        Errors = errors;
        FailureKind = failureKind;
    }

    /// <summary>保存に成功したか否か</summary>
    public bool Success { get; }
    /// <summary>
    /// エラーメッセージ。
    /// どの行でエラーが起きたかの捕捉が難しいので、項目ごとではなく保存処理全体の単位で返ります。
    /// </summary>
    public MessageSetter Errors { get; }
    /// <summary>失敗の種類。成功した場合は null</summary>
    public SaveFailureKind? FailureKind { get; }
}

/// <summary>
/// 保存処理の結果と保存後のデータ
/// </summary>
public sealed class SaveResult<T> : SaveResult {
    private SaveResult(bool success, MessageSetter errors, SaveFailureKind? failureKind, T? updated) : base(success, errors, failureKind) {
        _updated = updated;
    }
    private readonly T? _updated;

    /// <summary>
    /// 保存後のデータ。引数のコマンドとは別のインスタンスです。
    /// 保存時に採番されたシーケンスの参照や、続けて別の更新を行なうときに使用します。
    /// </summary>
    /// <exception cref="InvalidOperationException">保存に失敗している場合</exception>
    public T Updated => Success
        ? _updated!
        : throw new InvalidOperationException("保存に失敗しているので保存後のデータはありません。");

    internal static SaveResult<T> Succeeded(MessageSetter errors, T updated) => new(true, errors, null, updated);
    internal static SaveResult<T> Failed(MessageSetter errors, SaveFailureKind? failureKind) => new(false, errors, failureKind, default);
}

/// <summary>
/// 保存処理の失敗の種類
/// </summary>
public enum SaveFailureKind {
    /// <summary>プログラムの誤り。バリデーション漏れや、保存処理以外での変更など。</summary>
    ProgrammingError,
    /// <summary>楽観排他エラー</summary>
    Concurrency,
    /// <summary>一意制約違反</summary>
    UniqueConstraint,
    /// <summary>その他のデータベースのエラー</summary>
    DatabaseError,
}

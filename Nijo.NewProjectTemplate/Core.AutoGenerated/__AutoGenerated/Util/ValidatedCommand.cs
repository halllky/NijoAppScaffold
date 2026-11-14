
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : ValidatedCommand.cs
// ※ このファイルを生成したメソッド: Render

using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;

namespace MyApp;

/// <summary>
/// 検証を通過したコマンドの、検証時点の値。
/// 元のコマンドを検証後に書き換えても影響を受けない。
/// 通常の業務ロジックでは保存処理に渡すだけで、メンバーを直接使うことはない。
/// </summary>
public abstract class ValidatedCommand {
    private protected ValidatedCommand(DbContext dbContext, SaveCommandUsage usage) {
        _dbContext = dbContext;
        _usage = usage;
    }
    private readonly DbContext _dbContext;
    private readonly SaveCommandUsage _usage;

    /// <summary>
    /// 監査項目と楽観排他のバージョンを設定し、エンティティの変更追跡を開始します。
    /// 元のコマンドが同じであれば、検証済みコマンドが異なっていても1回しか呼べません。2回目以降は例外を送出します。
    /// </summary>
    /// <returns>変更追跡を開始したエンティティ。保存後にこれらの変更追跡を終了するのは呼び出し側の責務です。</returns>
    public IReadOnlyList<object> StartTracking() {
        _usage.MarkStarted();

        var tracked = new List<object>();
        try {
            StartTrackingCore(tracked);
        } catch {
            foreach (var entity in tracked) {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }
            throw;
        }
        return tracked;
    }

    /// <summary>変更追跡を開始する。変更追跡を開始するエンティティは状態を変更する前に引数のリストに追加すること。</summary>
    private protected abstract void StartTrackingCore(List<object> tracked);

    /// <summary>同一の保存処理の中でのキー重複の検出に使うキーを返します。保存時に採番されるのでキーが未確定の場合は null を返します。</summary>
    public abstract object? GetKeyForDuplicateCheck();

    /// <summary>ログに出力する情報（集約名、操作の種類、キー）を返します。</summary>
    public abstract JsonNode ToLogJson();
}

/// <inheritdoc cref="ValidatedCommand"/>
/// <typeparam name="TUpdateCommand">保存後のデータを表す更新コマンドの型</typeparam>
public abstract class ValidatedCommand<TUpdateCommand> : ValidatedCommand where TUpdateCommand : class {
    private protected ValidatedCommand(DbContext dbContext, SaveCommandUsage usage) : base(dbContext, usage) {
    }

    /// <summary>保存後のデータを表す、このインスタンスとは別の更新コマンドを返します。削除の場合は null を返します。</summary>
    public abstract TUpdateCommand? ToSavedCommand();
}

/// <summary>
/// 検証を通過した削除コマンド。
/// 保存処理の戻り値に保存後のデータが含まれないことを型で表すためにオーバーロードを分けるのに使う。
/// </summary>
/// <typeparam name="TUpdateCommand">削除コマンドと同じ集約の更新コマンドの型</typeparam>
public abstract class ValidatedDeleteCommand<TUpdateCommand> : ValidatedCommand<TUpdateCommand> where TUpdateCommand : class {
    private protected ValidatedDeleteCommand(DbContext dbContext, SaveCommandUsage usage) : base(dbContext, usage) {
    }

    /// <inheritdoc />
    public sealed override TUpdateCommand? ToSavedCommand() => null;
}

/// <summary>
/// コマンドが保存処理にかけられたか否か。
/// 1つのコマンドを複数回検証すると検証済みコマンドが複数できるが、それらを別々に保存すると
/// 保存時に採番されるキーをもつデータでは二重に登録されてしまうので、コマンドの単位で管理する。
/// </summary>
internal sealed class SaveCommandUsage {
    private bool _started;

    /// <summary>保存処理にかけられたことを記録します。既に記録されている場合は例外を送出します。</summary>
    internal void MarkStarted() {
        if (_started) throw new InvalidOperationException("一度保存処理にかけたコマンドは再度使用できません。");
        _started = true;
    }
}

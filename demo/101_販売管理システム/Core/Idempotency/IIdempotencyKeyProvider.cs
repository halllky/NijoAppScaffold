namespace MyApp.Core.Idempotency;

/// <summary>
/// 呼び出し元から渡された冪等キー。
/// 冪等キーは、利用者の1回の操作を表す値で、同じ操作が二重に処理されるのを防ぐために使う。
/// 冪等キーを渡せる呼び出し元（Web）だけが実装を登録する。登録されていなければ、冪等キーは無いものとして扱う。
/// </summary>
public interface IIdempotencyKeyProvider {
    /// <summary>
    /// 呼び出し元から渡された冪等キー。渡されていなければ null。
    /// </summary>
    string? Key { get; }
}

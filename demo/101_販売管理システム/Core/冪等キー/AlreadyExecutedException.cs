namespace MyApp.Core.冪等キー;

/// <summary>
/// 同じ冪等キーの操作がすでに処理済みであることを表す例外。
/// トランザクションの開始時に投げられ、処理はそこで中断される。
/// 呼び出し元は、この例外を受け取ったら、処理済みであることを利用者に伝える。
/// </summary>
public sealed class AlreadyExecutedException() : Exception("この操作は、すでに処理されています。");

namespace MyApp;

/// <summary>
/// 現在実行中の処理。
/// 何の処理かを決められるのは呼び出し元（Web、バッチ、単体テスト）なので、呼び出し元ごとに実装して DI コンテナに登録する。
/// </summary>
public interface ICurrentProcess {
    /// <summary>
    /// 処理名。その処理を人が識別できる名前（LOG-003, DB-002）。
    /// </summary>
    string Name { get; }
}

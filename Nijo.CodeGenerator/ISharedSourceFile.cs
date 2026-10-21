namespace Nijo.CodeGenerator;

/// <summary>
/// 生成処理のあちこちから内容が登録され、それらがすべて出そろった後で1回だけレンダリングされるソースファイル。
/// <see cref="CodeRenderingContext{TSelf}.Use{T}"/> で型ごとに1つだけインスタンスが作られる。
/// 内容の登録は並列に行われることがあるため、このインターフェースを実装するクラスはスレッドセーフに実装される必要がある。
/// </summary>
/// <typeparam name="TContext">コード生成のコンテキストの型</typeparam>
public interface ISharedSourceFile<in TContext> {

    /// <summary>
    /// このソースファイルが必要とする他のソースファイルを登録します。
    /// すべての共有ソースファイルのレンダリングより前に呼ばれます。
    /// </summary>
    void RegisterDependencies(TContext ctx);

    /// <summary>
    /// ソースコードの生成を実行します。
    /// このメソッドの中で新たに共有ソースファイルを使用することはできません。
    /// </summary>
    void Render(TContext ctx);
}

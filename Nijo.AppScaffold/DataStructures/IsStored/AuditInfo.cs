using Nijo.CodeGenerating;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// データの更新を記録するときに使う情報のインターフェース。
/// スキーマ定義にデータ構造が1個も無くてもコンパイルが通るように、常に生成される。
/// </summary>
internal static class AuditInfo {

    internal const string INTERFACE = "IAuditInfo";

    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = $"{INTERFACE}.cs",
            Contents = $$"""
                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// データの更新を記録するときに使う情報。
                /// </summary>
                public interface {{INTERFACE}} {
                    /// <summary>
                    /// <para>
                    /// 現在時刻。データ更新時の更新時刻の記録などに使用。
                    /// <see cref="DateTime.Now"/> を使ってしまうと現在時刻に依存する処理のテストが困難になるので、基本的にはこのプロパティを使うこと。
                    /// </para>
                    /// <para>
                    /// 同一のリクエストの中では、たとえ実時刻に多少のずれがあったとしてもすべて同じ時刻（リクエスト開始時点の時刻）になる。
                    /// 理由は、例えば深夜0時前後の処理で処置の途中で日付が変わることでロジックに影響が出る、などといった事象を防ぐため。
                    /// </para>
                    /// <para>
                    /// ちなみにログ出力の時刻にはこのプロパティが用いられず、正確な現在時刻が出力される。
                    /// </para>
                    /// </summary>
                    DateTime CurrentTime { get; }
                    /// <summary>
                    /// 現在操作中のユーザーの名前。データ更新時の更新者の記録などに使用。
                    /// </summary>
                    string CurrentUser { get; }
                }
                """,
        };
    }
}

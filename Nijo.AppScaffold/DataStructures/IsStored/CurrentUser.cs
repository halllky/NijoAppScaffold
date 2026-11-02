using Nijo.CodeGenerating;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// 現在操作中のユーザーのインターフェース。
/// スキーマ定義にデータ構造が1個も無くてもコンパイルが通るように、常に生成される。
/// </summary>
internal static class CurrentUser {

    internal const string INTERFACE = "ICurrentUser";
    internal const string NAME = "Name";

    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = $"{INTERFACE}.cs",
            Contents = $$"""
                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 現在操作中のユーザー。データ更新時の作成者・更新者の記録などに使用。
                /// </summary>
                public interface {{INTERFACE}} {
                    /// <summary>ユーザーを識別する名前</summary>
                    string {{NAME}} { get; }
                }
                """,
        };
    }
}

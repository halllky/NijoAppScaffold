using Nijo.CodeGenerating;

namespace Nijo.Models.DataModelModules;

internal class DataModelSaveResult {
    internal const string CLASS_NAME = "DataModelSaveResult";

    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = "DataModelSaveResult.cs",
            Contents = $$"""
                using System.Diagnostics.CodeAnalysis;

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 保存処理の結果を表します。
                /// </summary>
                public class {{CLASS_NAME}}<T> {
                    public {{CLASS_NAME}}(bool success, T? dbEntity) {
                        Success = success;
                        DbEntity = dbEntity;
                    }

                    /// <summary>
                    /// 保存処理が成功したかどうかを示します。
                    /// エラーチェックのみの場合や保存処理でエラーが発生した場合はfalseとなります。
                    /// </summary>
                    [MemberNotNullWhen(true, nameof(DbEntity))]
                    public bool Success { get; }

                    /// <summary>
                    /// 保存後のデータを表します。保存処理が成功した場合にのみ設定されます。
                    /// </summary>
                    public T? DbEntity { get; }
                }
                """,
        };
    }
}

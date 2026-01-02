using Nijo.CodeGenerating;

namespace Nijo.Models.DataModelModules;

internal class DataModelSaveResult {
    internal const string CLASS_NAME = "DataModelSaveResult";

    internal static SourceFile Render(CodeRenderingContext ctx) {
        return new SourceFile {
            FileName = "DataModelSaveResult.cs",
            Contents = $$"""

                namespace {{ctx.Config.RootNamespace}};

                /// <summary>
                /// 保存処理の結果を表します。
                /// </summary>
                public class {{CLASS_NAME}}<T> {
                    public {{CLASS_NAME}}(DataModelSaveResultType result, DataModelSaveErrorReason? errorReason, T? dbEntity) {
                        Result = result;
                        ErrorReason = errorReason;
                        DbEntity = dbEntity;
                    }

                    /// <summary>
                    /// 保存処理の終了状態を示します。
                    /// </summary>
                    public DataModelSaveResultType Result { get; }
                    /// <summary>
                    /// エラーが発生した場合、その理由を示します。エラーが発生しなかった場合は null です。
                    /// </summary>
                    public DataModelSaveErrorReason? ErrorReason { get; }
                    /// <summary>
                    /// 保存後のデータを表します。ステータスが <see cref="SaveResultType.Completed"/> の場合にのみ設定されます。
                    /// </summary>
                    public T? DbEntity { get; }
                }

                public enum DataModelSaveResultType {
                    /// <summary>登録が成功したことを表します。</summary>
                    Completed,
                    /// <summary>バリデーションエラーが無いことを確認し、登録は行わなかったことを表します。</summary>
                    ValidationOk,
                    /// <summary>何らかのエラーが発生したことを表します。</summary>
                    Error,
                }

                public enum DataModelSaveErrorReason {
                    /// <summary>バリデーションエラーが発生したことを表します。</summary>
                    ValidationError,
                    /// <summary>同時更新エラー（排他エラー）が発生したことを表します。</summary>
                    ConcurrencyError,
                    /// <summary>登録後の処理でエラーが発生したことを表します。</summary>
                    AfterSaveError,
                }
                """,
        };
    }
}

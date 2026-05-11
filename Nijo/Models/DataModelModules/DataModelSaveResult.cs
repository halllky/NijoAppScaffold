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
                    /// <summary>
                    /// 保存処理がバリデーションのみで終了した場合のコンストラクタ
                    /// </summary>
                    public {{CLASS_NAME}}(bool isValidationOk) {
                        _result = isValidationOk ? DataModelSaveResultType.ValidationOk : DataModelSaveResultType.Error;
                        _errorReason = isValidationOk ? null : DataModelSaveErrorReason.ValidationError;
                        _savedEntity = default;
                    }
                    /// <summary>
                    /// 保存処理が成功し、保存されたデータが存在する場合に使用されます。
                    /// </summary>
                    /// <param name="savedEntity">保存されたデータ</param>
                    public {{CLASS_NAME}}(T savedEntity) {
                        _result = DataModelSaveResultType.Completed;
                        _errorReason = null;
                        _savedEntity = savedEntity;
                    }
                    /// <summary>
                    /// 保存処理がエラーで終了したことを示す場合に使用されます。
                    /// </summary>
                    /// <param name="errorReason">エラーの理由</param>
                    public {{CLASS_NAME}}(DataModelSaveErrorReason? errorReason) {
                        _result = DataModelSaveResultType.Error;
                        _errorReason = errorReason;
                        _savedEntity = default;
                    }

                    /// <summary>
                    /// 保存処理の終了状態を示します。
                    /// </summary>
                    private readonly DataModelSaveResultType _result;
                    /// <summary>
                    /// エラーが発生した場合、その理由を示します。エラーが発生しなかった場合は null です。
                    /// </summary>
                    private readonly DataModelSaveErrorReason? _errorReason;
                    /// <summary>
                    /// 保存後のデータを表します。ステータスが <see cref="DataModelSaveResultType.Completed"/> の場合にのみ設定されます。
                    /// </summary>
                    private readonly T? _savedEntity;

                    /// <summary>
                    /// 保存処理が成功し、保存されたデータが存在する場合に true を返します。
                    /// </summary>
                    public bool IsSaveCompleted() => IsSaveCompleted(out var _);
                    /// <summary>
                    /// 保存処理が成功し、保存されたデータが存在する場合に true を返します。
                    /// 保存されたデータは out パラメータで取得できます。
                    /// バリデーションのみの確認で保存処理が行われなかった場合や、エラーが発生した場合は false を返します。
                    /// </summary>
                    /// <param name="savedEntity">保存されたデータ。保存処理が成功し、保存されたデータが存在する場合に設定されます。</param>
                    public bool IsSaveCompleted([NotNullWhen(true)] out T? savedEntity) {
                        savedEntity = _savedEntity;
                        return _result == DataModelSaveResultType.Completed && _savedEntity != null;
                    }
                    /// <summary>
                    /// 保存処理がバリデーションエラーなしで終了したことを示す場合に true を返します。
                    /// </summary>
                    public bool IsValidationOk() {
                        return _result == DataModelSaveResultType.Completed
                            || _result == DataModelSaveResultType.ValidationOk;
                    }
                    /// <summary>
                    /// 保存処理がエラーで終了したことを示す場合に true を返します。
                    /// </summary>
                    public bool IsError() => IsError(out var _);
                    /// <summary>
                    /// 保存処理がエラーで終了したことを示す場合に true を返します。
                    /// </summary>
                    /// <param name="errorReason">エラーが発生した場合、その理由を示します。エラーが発生しなかった場合は null です。</param>
                    public bool IsError([NotNullWhen(true)] out DataModelSaveErrorReason? errorReason) {
                        errorReason = _errorReason;
                        return _result == DataModelSaveResultType.Error;
                    }

                    private enum DataModelSaveResultType {
                        /// <summary>登録が成功したことを表します。</summary>
                        Completed,
                        /// <summary>バリデーションエラーが無いことを確認し、登録は行わなかったことを表します。</summary>
                        ValidationOk,
                        /// <summary>何らかのエラーが発生したことを表します。</summary>
                        Error,
                    }
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

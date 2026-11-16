using Nijo.CodeGenerating;
using Nijo.Util.DotnetEx;
using System.Collections.Generic;
using System.Linq;

namespace Nijo.DataStructures.IsStored;

/// <summary>
/// チェックの中身を DbContext の abstract メソッド（フック）に任せるバリデーター。
/// 生成されるコマンドは項目ごとにフックを呼ぶだけで、チェックの中身は手書きの DbContext がオーバーライドして実装する。
/// フックは null を返すと妥当、文字列を返すとその文言のエラーとなる。
/// </summary>
internal abstract class DbContextHookValidator : ValidatorBase {

    private const string ERROR_MESSAGE = "errorMessage";

    /// <summary>
    /// DbContext に宣言するフックの abstract メソッドをレンダリングします。
    /// </summary>
    internal abstract IEnumerable<string> RenderHookDeclarations(CodeRenderingContext ctx);

    protected sealed override IEnumerable<string> RenderMethodHead() {
        yield return $$"""
            string? {{ERROR_MESSAGE}};
            """;
    }

    /// <summary>
    /// フックを呼び、エラーメッセージが返ってきたらエラーとするチェックを返します。
    /// 複数の呼び出しを渡した場合は、最初にエラーメッセージを返したものを採用します。
    /// </summary>
    /// <param name="hookCalls">フックの呼び出し。<see cref="RenderHookCall"/> で作る</param>
    protected static ValidateStatement CallHooks(IEnumerable<string> hookCalls) {
        return new ValidateStatement {
            If = $"({ERROR_MESSAGE} = {hookCalls.Join("\r\n?? ")}) is not null",
            RenderErrorMessage = ERROR_MESSAGE,
        };
    }

    /// <summary>
    /// コマンドから DbContext のフックを呼ぶ式をレンダリングします。
    /// </summary>
    protected static string RenderHookCall(string methodName, params IEnumerable<string> args) {
        return $"{SaveCommand.FIELD_DBCONTEXT}.{methodName}({args.Join(", ")})";
    }

    /// <summary>
    /// C# の文字列リテラルをレンダリングします。
    /// </summary>
    protected static string ToLiteral(string value) {
        return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }
}

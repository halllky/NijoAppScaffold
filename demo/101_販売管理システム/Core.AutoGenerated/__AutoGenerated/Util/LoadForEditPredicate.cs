
// このファイルは自動生成されました。このファイルの内容を直接書き換えても、次回の自動生成処理で上書きされるのでご注意ください。
// ※ このファイルを生成したクラス  : LoadForEdit.cs
// ※ このファイルを生成したメソッド: RenderPredicate

using System.Linq.Expressions;

namespace MyApp;

/// <summary>
/// 複数のキーでデータを読み込むときの検索条件を組み立てる。
/// </summary>
internal static class LoadForEditPredicate {
    /// <summary>引数の条件のいずれかに一致することを表す条件を返します。</summary>
    internal static Expression<Func<T, bool>> AnyOf<T>(IEnumerable<Expression<Func<T, bool>>> predicates) {
        var parameter = Expression.Parameter(typeof(T), "e");
        var body = predicates
            .Select(predicate => new ParameterReplacer(predicate.Parameters[0], parameter).Visit(predicate.Body))
            .Aggregate(Expression.OrElse);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}

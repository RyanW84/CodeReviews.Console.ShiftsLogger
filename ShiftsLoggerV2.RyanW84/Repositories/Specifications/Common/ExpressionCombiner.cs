using System.Linq.Expressions;

namespace ShiftsLoggerV2.RyanW84.Repositories.Specifications.Common;

/// <summary>
/// Utility class for combining LINQ expressions
/// </summary>
public static class ExpressionCombiner
{
    /// <summary>
    /// Combines two expressions with an AND operation
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="left">The left expression (can be null)</param>
    /// <param name="right">The right expression</param>
    /// <param name="parameterName">The parameter name to use (e.g., "s" for Shift, "w" for Worker, "l" for Location)</param>
    /// <returns>The combined expression</returns>
    public static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>>? left,
        Expression<Func<T, bool>> right,
        string parameterName = "x")
    {
        if (left == null)
            return right;

        var parameter = Expression.Parameter(typeof(T), parameterName);
        var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
        var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
        var andExpression = Expression.AndAlso(leftBody, rightBody);

        return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
    }

    /// <summary>
    /// Internal helper class for replacing expression parameters
    /// </summary>
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Shop.Shared.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyWhere<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> primaryExpr,
        Expression<Func<T, bool>> fallbackExpr,
        bool usePrimary) =>
        usePrimary ? query.Where(primaryExpr) : query.Where(fallbackExpr);
}
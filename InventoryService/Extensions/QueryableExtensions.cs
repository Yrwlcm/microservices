namespace InventoryService.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> RetrievePage<T>(this IQueryable<T> items, int page, int limit) =>
        items.Skip((page - 1) * limit).Take(limit);
}
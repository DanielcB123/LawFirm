using Microsoft.EntityFrameworkCore;

namespace EnterpriseKnowledgeAssistant.Api.Common.Paging;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class PagingQueryableExtensions
{
    /// <summary>
    /// Pages an already filtered and ordered query. Apply access filtering before calling,
    /// so <see cref="PagedResult{T}.TotalCount"/> never counts rows the caller cannot read.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(page.Skip)
            .Take(page.NormalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page.NormalizedPage, page.NormalizedPageSize, totalCount);
    }
}

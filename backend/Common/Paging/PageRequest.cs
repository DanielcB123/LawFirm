namespace EnterpriseKnowledgeAssistant.Api.Common.Paging;

public sealed record PageRequest(int Page = 1, int PageSize = PageRequest.DefaultPageSize)
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    // Upper bound keeps Skip within int range at the maximum page size.
    private const int MaxPage = int.MaxValue / MaxPageSize;

    public int NormalizedPage => Math.Clamp(Page, 1, MaxPage);

    public int NormalizedPageSize => PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);

    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}

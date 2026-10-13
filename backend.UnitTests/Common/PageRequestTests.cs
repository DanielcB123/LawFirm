using EnterpriseKnowledgeAssistant.Api.Common.Paging;

namespace EnterpriseKnowledgeAssistant.UnitTests.Common;

public sealed class PageRequestTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void Page_is_at_least_one(int page, int expected)
    {
        Assert.Equal(expected, new PageRequest(page).NormalizedPage);
    }

    [Theory]
    [InlineData(0, PageRequest.DefaultPageSize)]
    [InlineData(-1, PageRequest.DefaultPageSize)]
    [InlineData(25, 25)]
    [InlineData(10_000, PageRequest.MaxPageSize)]
    public void Page_size_is_defaulted_and_capped(int pageSize, int expected)
    {
        Assert.Equal(expected, new PageRequest(1, pageSize).NormalizedPageSize);
    }

    [Fact]
    public void Skip_uses_normalized_values()
    {
        Assert.Equal(40, new PageRequest(3, 20).Skip);
        Assert.Equal(0, new PageRequest(-1, -1).Skip);
    }

    [Fact]
    public void Skip_does_not_overflow_for_huge_page_numbers()
    {
        var skip = new PageRequest(int.MaxValue, PageRequest.MaxPageSize).Skip;

        Assert.InRange(skip, 0, int.MaxValue);
    }

    [Fact]
    public void Total_pages_rounds_up()
    {
        Assert.Equal(3, new PagedResult<int>([], 1, 20, 41).TotalPages);
        Assert.Equal(0, new PagedResult<int>([], 1, 20, 0).TotalPages);
    }
}

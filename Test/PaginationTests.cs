using GaesdeApi;
using GaesdeApi.DTOs;
using Xunit;

namespace GaesdeApi.Tests;

public class PaginationTests
{
    [Fact]
    public void Paginate_ReturnsRequestedPageAndMetadata()
    {
        var result = Utils.Paginate(Enumerable.Range(1, 5), new PaginationRequest
        {
            Page = 2,
            PageSize = 2
        });

        Assert.Equal(new[] { 3, 4 }, result.Items);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void Paginate_ClampsInvalidPageSize()
    {
        var result = Utils.Paginate(Enumerable.Range(1, 2), new PaginationRequest
        {
            Page = 0,
            PageSize = 500
        });

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void Paginate_NullCollectionReturnsEmptyPage()
    {
        var result = Utils.Paginate<int>(null!, new PaginationRequest());

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPages);
    }
}
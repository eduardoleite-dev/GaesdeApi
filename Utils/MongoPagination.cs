using GaesdeApi.DTOs;
using MongoDB.Driver;

namespace GaesdeApi;

public static class MongoPagination
{
    public static async Task<PaginatedResponse<TOutput>> ExecuteAsync<TDocument, TOutput>(
        IFindFluent<TDocument, TDocument> query,
        PaginationRequest request,
        Func<TDocument, TOutput> map)
    {
        var page = request.ValidPage;
        var pageSize = request.ValidPageSize;
        var totalItems = await query.CountDocumentsAsync();
        var documents = await query
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PaginatedResponse<TOutput>(
            documents.Select(map).ToArray(),
            page,
            pageSize,
            (int)totalItems,
            totalPages);
    }
}

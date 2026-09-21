using GaesdeApi.DTOs;
using GaesdeApi.Models;
using GaesdeApi.Services.Interfaces;
using MongoDB.Driver;

namespace GaesdeApi.Services;

public class CategoryService : ICategoryService
{
    private readonly IMongoCollection<Category> _categoriesCollection;

    public CategoryService(IMongoDatabase database)
    {
        _categoriesCollection = database.GetCollection<Category>("Categories");
    }

    public async Task<IReadOnlyCollection<CategoryResponseDto>> GetAllAsync()
    {
        var categories = await _categoriesCollection
            .Find(Builders<Category>.Filter.Empty)
            .SortBy(category => category.Name)
            .ToListAsync();

        return categories.Select(ToResponse).ToArray();
    }

    public async Task<PaginatedResponse<CategoryResponseDto>> GetPageAsync(
        PaginationRequest request,
        string? search = null)
    {
        var filter = Builders<Category>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(search))
            filter &= Builders<Category>.Filter.Regex(
                category => category.Name,
                new MongoDB.Bson.BsonRegularExpression(search.Trim(), "i"));

        var query = _categoriesCollection.Find(filter).SortBy(category => category.Name);
        return await MongoPagination.ExecuteAsync(query, request, ToResponse);
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(string id)
    {
        var category = await _categoriesCollection
            .Find(category => category.Id == id)
            .FirstOrDefaultAsync();

        return category is null ? null : ToResponse(category);
    }

    public async Task<CategoryResponseDto?> CreateAsync(CreateCategoryRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || await NameExistsAsync(request.Name))
            return null;

        var now = DateTime.UtcNow;
        var category = new Category
        {
            Name = request.Name.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _categoriesCollection.InsertOneAsync(category);
        return ToResponse(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(string id, UpdateCategoryRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return null;

        var category = await _categoriesCollection
            .Find(existingCategory => existingCategory.Id == id)
            .FirstOrDefaultAsync();

        if (category is null || await NameExistsAsync(request.Name, id))
            return null;

        category.Name = request.Name.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await _categoriesCollection.ReplaceOneAsync(existingCategory => existingCategory.Id == id, category);
        return ToResponse(category);
    }

    public async Task<CategoryResponseDto?> UpdateImageAsync(string id, string imageUrl)
    {
        var category = await _categoriesCollection
            .Find(existingCategory => existingCategory.Id == id)
            .FirstOrDefaultAsync();

        if (category is null)
            return null;

        category.ImageUrl = imageUrl;
        category.UpdatedAt = DateTime.UtcNow;
        await _categoriesCollection.ReplaceOneAsync(existingCategory => existingCategory.Id == id, category);
        return ToResponse(category);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _categoriesCollection.DeleteOneAsync(category => category.Id == id);
        return result.DeletedCount > 0;
    }

    private async Task<bool> NameExistsAsync(string name, string? excludedId = null)
    {
        return await _categoriesCollection.Find(category =>
                category.Name == name.Trim() &&
                (excludedId == null || category.Id != excludedId))
            .Limit(1)
            .AnyAsync();
    }

    private static CategoryResponseDto ToResponse(Category category) => new(
        category.Id,
        category.Name,
        category.ImageUrl,
        category.CreatedAt,
        category.UpdatedAt);
}
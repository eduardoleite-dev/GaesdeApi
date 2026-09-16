using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class CategoriesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedCategory()
    {
        var request = new CreateCategoryRequestDto("Matemática");
        var response = new CategoryResponseDto(
            "category-id", "Matemática", DateTime.UtcNow, DateTime.UtcNow);
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync(response);
        var controller = new CategoriesController(service.Object);

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal("category-id", ((CategoryResponseDto)created.Value!).Id);
    }

    [Fact]
    public async Task Update_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        var request = new UpdateCategoryRequestDto("Matemática");
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.UpdateAsync("missing-id", request)).ReturnsAsync((CategoryResponseDto?)null);
        var controller = new CategoriesController(service.Object);

        var result = await controller.Update("missing-id", request);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}

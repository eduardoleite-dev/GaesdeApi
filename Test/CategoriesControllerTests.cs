using GaesdeApi.Controllers;
using GaesdeApi.DTOs;
using GaesdeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GaesdeApi.Tests;

public class CategoriesControllerTests
{
    private static CategoryResponseDto Response() => new(
        "category-id", "Matemática", DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.GetAllAsync()).ReturnsAsync(new[] { Response() });

        var result = await new CategoriesController(service.Object).GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.GetByIdAsync("category-id")).ReturnsAsync(Response());

        var result = await new CategoriesController(service.Object).GetById("category-id");

        Assert.IsType<OkObjectResult>(result);
    }

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

    [Fact]
    public async Task Create_WhenNameAlreadyExists_ReturnsConflict()
    {
        var request = new CreateCategoryRequestDto("Matemática");
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.CreateAsync(request)).ReturnsAsync((CategoryResponseDto?)null);

        var result = await new CategoriesController(service.Object).Create(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Update_WhenValid_ReturnsOk()
    {
        var request = new UpdateCategoryRequestDto("Matemática avançada");
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.UpdateAsync("category-id", request)).ReturnsAsync(Response());

        var result = await new CategoriesController(service.Object).Update("category-id", request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WhenMissing_ReturnsNotFound()
    {
        var service = new Mock<ICategoryService>();
        service.Setup(value => value.DeleteAsync("missing")).ReturnsAsync(false);

        var result = await new CategoriesController(service.Object).Delete("missing");

        Assert.IsType<NotFoundResult>(result);
    }
}

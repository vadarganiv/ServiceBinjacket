using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Infrastructure.Persistence;
using ServisBinjaket.Infrastructure.Repositories;

namespace ServisBinjaket.Tests.Products;

public sealed class ProductImageRepositoryTests : IDisposable
{
    private readonly AppDbContext _database;
    private readonly ProductRepository _repository;

    public ProductImageRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        _database = new AppDbContext(options);
        _repository = new ProductRepository(_database);
    }

    [Fact]
    public async Task DeleteImage_requires_the_image_to_belong_to_the_route_product()
    {
        var firstProduct = CreateProduct("first");
        var secondProduct = CreateProduct("second");
        _database.Products.AddRange(firstProduct, secondProduct);
        await _database.SaveChangesAsync();

        var secondProductImage = new ProductImage
        {
            ProductId = secondProduct.Id,
            Path = $"/uploads/products/{secondProduct.Id}/second.jpg",
            AltSq = "Second"
        };
        _database.ProductImages.Add(secondProductImage);
        await _database.SaveChangesAsync();

        var wrongProductResult = await _repository.DeleteImageAsync(
            firstProduct.Id,
            secondProductImage.Id);

        Assert.Null(wrongProductResult);
        Assert.True(await _database.ProductImages.AnyAsync(image => image.Id == secondProductImage.Id));

        var correctProductResult = await _repository.DeleteImageAsync(
            secondProduct.Id,
            secondProductImage.Id);

        Assert.Equal(secondProductImage.Path, correctProductResult);
        Assert.False(await _database.ProductImages.AnyAsync(image => image.Id == secondProductImage.Id));
    }

    public void Dispose() => _database.Dispose();

    private static Product CreateProduct(string slug) => new()
    {
        NameSq = slug,
        SlugSq = slug,
        CategoryId = 1,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}

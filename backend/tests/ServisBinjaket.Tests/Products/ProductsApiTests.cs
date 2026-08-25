using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ServisBinjaket.Application.Products.Queries;
using ServisBinjaket.Application.Products.UseCases;
using ServisBinjaket.Domain.Entities;
using ServisBinjaket.Domain.Enums;
using ServisBinjaket.Infrastructure.Persistence;
using ServisBinjaket.Infrastructure.Repositories;

namespace ServisBinjaket.Tests.Products;

public class ProductsUseCaseTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly GetProductsUseCase _getProducts;
    private readonly GetProductBySlugUseCase _getBySlug;
    private readonly GetProductCategoriesUseCase _getCategories;

    public ProductsUseCaseTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var repo = new ProductRepository(_db);
        _getProducts = new GetProductsUseCase(repo);
        _getBySlug = new GetProductBySlugUseCase(repo);
        _getCategories = new GetProductCategoriesUseCase(repo);

        SeedTestData();
    }

    public void Dispose() => _db.Dispose();

    private void SeedTestData()
    {
        var phones = new ProductCategory
        {
            NameSq = "Telefonë",
            NameEn = "Phones",
            SlugSq = "telefone",
            SlugEn = "phones",
            SortOrder = 1,
            IsPublished = true
        };
        var laptops = new ProductCategory
        {
            NameSq = "Laptop",
            NameEn = "Laptops",
            SlugSq = "laptop",
            SlugEn = "laptops",
            SortOrder = 2,
            IsPublished = true
        };
        _db.ProductCategories.AddRange(phones, laptops);
        _db.SaveChanges();

        var now = DateTime.UtcNow;

        _db.Products.AddRange(
            new Product
            {
                NameSq = "iPhone 12",
                NameEn = "iPhone 12",
                SlugSq = "iphone-12",
                SlugEn = "iphone-12",
                ShortDescriptionSq = "Telefon i mirë",
                ShortDescriptionEn = "Great phone",
                DescriptionSq = "Përshkrim i plotë",
                DescriptionEn = "Full description",
                Price = 45000,
                Currency = "ALL",
                Condition = ProductCondition.Used,
                StockQty = 5,
                CategoryId = phones.Id,
                IsPublished = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Product
            {
                NameSq = "Samsung Galaxy A54",
                NameEn = null,
                SlugSq = "samsung-galaxy-a54",
                SlugEn = null,
                ShortDescriptionSq = "Telefon Samsung me çmim të mirë",
                ShortDescriptionEn = null,
                DescriptionSq = "Përshkrim Samsung",
                DescriptionEn = null,
                Price = 30000,
                Currency = "ALL",
                Condition = ProductCondition.New,
                StockQty = 0,
                CategoryId = phones.Id,
                IsPublished = true,
                CreatedAt = now.AddMinutes(-10),
                UpdatedAt = now.AddMinutes(-10)
            },
            new Product
            {
                NameSq = "MacBook Pro",
                NameEn = "MacBook Pro",
                SlugSq = "macbook-pro",
                SlugEn = "macbook-pro",
                ShortDescriptionSq = "Laptop i fuqishëm",
                ShortDescriptionEn = "Powerful laptop",
                DescriptionSq = "Përshkrim MacBook",
                DescriptionEn = "MacBook description",
                Price = 150000,
                Currency = "ALL",
                Condition = ProductCondition.Refurbished,
                StockQty = null,
                CategoryId = laptops.Id,
                IsPublished = true,
                CreatedAt = now.AddMinutes(-20),
                UpdatedAt = now.AddMinutes(-20)
            },
            new Product
            {
                NameSq = "Draft produkt",
                SlugSq = "draft-produkt",
                ShortDescriptionSq = "",
                DescriptionSq = "",
                Price = 1000,
                Currency = "ALL",
                Condition = ProductCondition.New,
                CategoryId = phones.Id,
                IsPublished = false,
                CreatedAt = now,
                UpdatedAt = now
            }
        );
        _db.SaveChanges();
    }

    // ── Localization ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_LocaleSq_ReturnsSqValues()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Locale = "sq" });
        var iphone = result.Items.First(p => p.Slug == "iphone-12");
        iphone.ShortDescription.Should().Be("Telefon i mirë");
    }

    [Fact]
    public async Task GetProducts_LocaleEn_ReturnsEnValues()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Locale = "en" });
        var iphone = result.Items.First(p => p.Slug == "iphone-12");
        iphone.ShortDescription.Should().Be("Great phone");
    }

    [Fact]
    public async Task GetProducts_LocaleEn_FallsBackToSqWhenEnEmpty()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Locale = "en" });
        var samsung = result.Items.First(p => p.Slug == "samsung-galaxy-a54");
        samsung.Name.Should().Be("Samsung Galaxy A54");
        samsung.ShortDescription.Should().Be("Telefon Samsung me çmim të mirë");
    }

    [Fact]
    public async Task GetProductBySlug_LocaleEn_ReturnsEnValues()
    {
        var result = await _getBySlug.ExecuteAsync("iphone-12", "en");
        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Great phone");
        result.Description.Should().Be("Full description");
    }

    [Fact]
    public async Task GetProductBySlug_LocaleEn_FallsBackToSq()
    {
        var result = await _getBySlug.ExecuteAsync("samsung-galaxy-a54", "en");
        result.Should().NotBeNull();
        result!.Name.Should().Be("Samsung Galaxy A54");
    }

    // ── Published filter ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_ExcludesUnpublished()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery());
        result.Items.Should().NotContain(p => p.Slug == "draft-produkt");
        result.TotalCount.Should().Be(3);
    }

    // ── Category filter ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_FilterByCategory_ReturnsOnlyThatCategory()
    {
        var phonesId = _db.ProductCategories.First(c => c.SlugSq == "telefone").Id;
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { CategoryId = phonesId });
        result.Items.Should().NotContain(p => p.Slug == "macbook-pro");
        result.Items.Should().AllSatisfy(p => p.Slug.Should().NotBe("macbook-pro"));
    }

    // ── Price filter ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_FilterByMinPrice_ExcludesCheaper()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { MinPrice = 40000 });
        result.Items.Should().NotContain(p => p.Slug == "samsung-galaxy-a54");
        result.Items.Should().Contain(p => p.Slug == "iphone-12");
        result.Items.Should().Contain(p => p.Slug == "macbook-pro");
    }

    [Fact]
    public async Task GetProducts_FilterByMaxPrice_ExcludesExpensive()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { MaxPrice = 40000 });
        result.Items.Should().NotContain(p => p.Slug == "macbook-pro");
        result.Items.Should().Contain(p => p.Slug == "samsung-galaxy-a54");
    }

    // ── Condition filter ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_FilterByCondition_ReturnsOnlyMatching()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Condition = ProductCondition.New });
        result.Items.Should().AllSatisfy(p => p.Condition.Should().Be(ProductCondition.New));
        result.Items.Should().ContainSingle(p => p.Slug == "samsung-galaxy-a54");
    }

    // ── Stock filter ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_FilterInStock_ExcludesOutOfStock()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { InStock = true });
        result.Items.Should().Contain(p => p.Slug == "iphone-12");
        result.Items.Should().Contain(p => p.Slug == "macbook-pro");
        result.Items.Should().NotContain(p => p.Slug == "samsung-galaxy-a54");
    }

    [Fact]
    public async Task GetProducts_InStock_NullStockQty_CountsAsInStock()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { InStock = true });
        result.Items.Should().Contain(p => p.Slug == "macbook-pro");
    }

    // ── Search filter ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_SearchQ_ReturnsMatchingProducts()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Q = "iphone" });
        result.Items.Should().ContainSingle(p => p.Slug == "iphone-12");
    }

    [Fact]
    public async Task GetProducts_SearchQ_CaseInsensitive()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Q = "IPHONE" });
        result.Items.Should().ContainSingle(p => p.Slug == "iphone-12");
    }

    // ── Sort ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_SortByPriceAsc_ReturnsCheapestFirst()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Sort = "price-asc" });
        var prices = result.Items.Select(p => p.Price).ToList();
        prices.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetProducts_SortByPriceDesc_ReturnsMostExpensiveFirst()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Sort = "price-desc" });
        var prices = result.Items.Select(p => p.Price).ToList();
        prices.Should().BeInDescendingOrder();
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_Pagination_ReturnsCorrectPage()
    {
        var result = await _getProducts.ExecuteAsync(new GetProductsQuery { Page = 1, PageSize = 1 });
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetProducts_PageTwo_ReturnsDifferentItems()
    {
        var page1 = await _getProducts.ExecuteAsync(new GetProductsQuery { Page = 1, PageSize = 1, Sort = "price-asc" });
        var page2 = await _getProducts.ExecuteAsync(new GetProductsQuery { Page = 2, PageSize = 1, Sort = "price-asc" });
        page1.Items[0].Slug.Should().NotBe(page2.Items[0].Slug);
    }

    // ── GetBySlug ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductBySlug_Found_ReturnsDetail()
    {
        var result = await _getBySlug.ExecuteAsync("iphone-12", "sq");
        result.Should().NotBeNull();
        result!.Name.Should().Be("iPhone 12");
        result.Category.Should().NotBeNull();
        result.Category!.Name.Should().Be("Telefonë");
    }

    [Fact]
    public async Task GetProductBySlug_NotFound_ReturnsNull()
    {
        var result = await _getBySlug.ExecuteAsync("nonexistent-slug", "sq");
        result.Should().BeNull();
    }

    // ── Categories ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductCategories_LocaleSq_ReturnsSqNames()
    {
        var result = await _getCategories.ExecuteAsync("sq");
        result.Should().Contain(c => c.Name == "Telefonë" && c.Slug == "telefone");
    }

    [Fact]
    public async Task GetProductCategories_LocaleEn_ReturnsEnNames()
    {
        var result = await _getCategories.ExecuteAsync("en");
        result.Should().Contain(c => c.Name == "Phones" && c.Slug == "phones");
        result.Should().Contain(c => c.Name == "Laptops" && c.Slug == "laptops");
    }

    [Fact]
    public async Task GetProductCategories_OrderedBySortOrder()
    {
        var result = await _getCategories.ExecuteAsync("sq");
        result.Select(c => c.SortOrder).Should().BeInAscendingOrder();
    }
}

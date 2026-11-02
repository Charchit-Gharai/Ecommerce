using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Core.Services;
using Moq;

namespace ECommerce.Tests.Unit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _products = new();

    private ProductService Create() =>
        new(_products.Object);

    [Fact]
    public async Task Create_Throws_WhenNameIsEmpty()
    {
        var request = new CreateProductRequest
        {
            Name = "",
            Price = 10m,
            Category = "Electronics",
            StockQuantity = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().CreateAsync(request));
    }

    [Fact]
    public async Task Create_Throws_WhenCategoryIsEmpty()
    {
        var request = new CreateProductRequest
        {
            Name = "Laptop",
            Price = 10m,
            Category = " ",
            StockQuantity = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().CreateAsync(request));
    }

    [Fact]
    public async Task Create_Throws_WhenPriceIsNotPositive()
    {
        var request = new CreateProductRequest
        {
            Name = "Laptop",
            Price = 0m,
            Category = "Electronics",
            StockQuantity = 1
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().CreateAsync(request));
    }

    [Fact]
    public async Task Create_Throws_WhenStockIsNegative()
    {
        var request = new CreateProductRequest
        {
            Name = "Laptop",
            Price = 10m,
            Category = "Electronics",
            StockQuantity = -1
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().CreateAsync(request));
    }

    [Fact]
    public async Task Create_TrimsFields_AndSavesProduct()
    {
        Product? added = null;

        _products
            .Setup(repository => repository.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>(
                (product, _) => added = product)
            .Returns(Task.CompletedTask);

        var request = new CreateProductRequest
        {
            Name = "  Laptop  ",
            Description = "  Laptop description  ",
            Price = 99.50m,
            Category = "  Electronics  ",
            StockQuantity = 5,
            ImageUrl = "https://example.com/laptop.jpg"
        };

        var result = await Create().CreateAsync(request);

        Assert.NotNull(added);
        Assert.Equal("Laptop", added.Name);
        Assert.Equal("Laptop description", added.Description);
        Assert.Equal("Electronics", added.Category);
        Assert.Equal(5, added.StockQuantity);
        Assert.Equal(99.50m, added.Price);

        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Electronics", result.Category);
        Assert.Equal(5, result.StockQuantity);

        _products.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();

        _products
            .Setup(repository => repository.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await Create().GetByIdAsync(id);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenInStockRequiredAndStockIsZero()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Price = 100m,
            Category = "Electronics",
            StockQuantity = 0
        };

        _products
            .Setup(repository => repository.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await Create().GetByIdAsync(
            product.Id,
            inStockOnly: true);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsProduct_WhenFound()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Price = 100m,
            Category = "Electronics",
            StockQuantity = 3
        };

        _products
            .Setup(repository => repository.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await Create().GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Electronics", result.Category);
        Assert.Equal(3, result.StockQuantity);
    }

    [Fact]
    public async Task GetAll_ReturnsMappedProducts()
    {
        IReadOnlyList<Product> products =
        [
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Laptop",
                Price = 100m,
                Category = "Electronics",
                StockQuantity = 3
            },
            new Product
            {
                Id = Guid.NewGuid(),
                Name = "Book",
                Price = 20m,
                Category = "Books",
                StockQuantity = 5
            }
        ];

        _products
            .Setup(repository => repository.GetAllAsync(
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await Create().GetAllAsync(
            category: null,
            inStockOnly: true);

        Assert.Equal(2, result.Count);
        Assert.Equal("Laptop", result[0].Name);
        Assert.Equal("Book", result[1].Name);
    }

    [Fact]
    public async Task GetAll_PassesCategoryAndStockFilterToRepository()
    {
        _products
            .Setup(repository => repository.GetAllAsync(
                "Electronics",
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Product>)Array.Empty<Product>());

        await Create().GetAllAsync(
            category: "Electronics",
            inStockOnly: true);

        _products.Verify(
            repository => repository.GetAllAsync(
                "Electronics",
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsNull_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();

        _products
            .Setup(repository => repository.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var request = new UpdateProductRequest
        {
            Name = "Updated Laptop",
            Price = 150m,
            Category = "Electronics",
            StockQuantity = 10
        };

        var result = await Create().UpdateAsync(id, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task Update_ChangesProductDetails()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Old Laptop",
            Price = 100m,
            Category = "Electronics",
            StockQuantity = 2
        };

        _products
            .Setup(repository => repository.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var request = new UpdateProductRequest
        {
            Name = "New Laptop",
            Price = 200m,
            Category = "Computers",
            StockQuantity = 8
        };

        var result = await Create().UpdateAsync(product.Id, request);

        Assert.NotNull(result);
        Assert.Equal("New Laptop", product.Name);
        Assert.Equal(200m, product.Price);
        Assert.Equal("Computers", product.Category);
        Assert.Equal(8, product.StockQuantity);

        _products.Verify(
            repository => repository.Update(product),
            Times.Once);

        _products.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsFalse_WhenProductDoesNotExist()
    {
        var id = Guid.NewGuid();

        _products
            .Setup(repository => repository.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await Create().DeleteAsync(id);

        Assert.False(result);
    }

    [Fact]
    public async Task Delete_RemovesProduct_WhenFound()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Price = 100m,
            Category = "Electronics",
            StockQuantity = 3
        };

        _products
            .Setup(repository => repository.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await Create().DeleteAsync(product.Id);

        Assert.True(result);

        _products.Verify(
            repository => repository.Remove(product),
            Times.Once);

        _products.Verify(
            repository => repository.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
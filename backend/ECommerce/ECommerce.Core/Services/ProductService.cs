using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;

namespace ECommerce.Core.Services;

public class ProductService(
    IProductRepository repository) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        string? category,
        bool inStockOnly,
        CancellationToken cancellationToken = default)
    {
        var products = await repository.GetAllAsync(
            category,
            inStockOnly,
            cancellationToken);

        return products.Select(ToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(
        Guid id,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(
            id, cancellationToken);

        if (product is null ||
            (inStockOnly && product.StockQuantity <= 0))
        {
            return null;
        }

        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Category = request.Category.Trim(),
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl
        };

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        var product = await repository.GetByIdAsync(
            id, cancellationToken);

        if (product is null)
            return null;

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Category = request.Category.Trim();
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;

        repository.Update(product);

        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(
            id, cancellationToken);

        if (product is null)
            return false;

        repository.Remove(product);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void Validate(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException(
                "Product name is required.");

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException(
                "Category is required.");

        if (request.Price <= 0)
            throw new ArgumentException(
                "Price must be greater than zero.");

        if (request.StockQuantity < 0)
            throw new ArgumentException(
                "Stock cannot be negative.");
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl
        };
    }
}
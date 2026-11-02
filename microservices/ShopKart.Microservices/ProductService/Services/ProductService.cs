using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class ProductCatalogService(IProductRepository products)
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        string? category,
        bool inStockOnly,
        CancellationToken cancellationToken = default)
    {
        var result = await products.GetAllAsync(
            category, inStockOnly, cancellationToken);

        return result.Select(ToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            Category = request.Category.Trim(),
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl?.Trim()
        };

        await products.AddAsync(product, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);

        if (product is null) return null;

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.Price = request.Price;
        product.Category = request.Category.Trim();
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.UpdatedAt = DateTime.UtcNow;

        products.Update(product);
        await products.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);

        if (product is null) return false;

        products.Remove(product);
        await products.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeductStockAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var product = await products.GetByIdAsync(
            productId, cancellationToken);

        if (product is null || product.StockQuantity < quantity)
            return false;

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;

        products.Update(product);
        await products.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ProductResponse ToResponse(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Category = p.Category,
        StockQuantity = p.StockQuantity,
        ImageUrl = p.ImageUrl,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
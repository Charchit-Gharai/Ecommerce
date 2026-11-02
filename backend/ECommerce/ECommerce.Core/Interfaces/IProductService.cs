using ECommerce.Core.DTOs;

namespace ECommerce.Core.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        string? category,
        bool inStockOnly,
        CancellationToken cancellationToken = default);

    Task<ProductResponse?> GetByIdAsync(
        Guid id,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default);

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductResponse?> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
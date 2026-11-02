
using ECommerce.Core.Models;

namespace ECommerce.Core.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> GetAllAsync(
        string? category = null,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    void Update(Product product);

    void Remove(Product product);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

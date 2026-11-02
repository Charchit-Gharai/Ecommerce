using ProductService.Models;

namespace ProductService.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        string? category = null,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default);

    void Update(Product product);

    void Remove(Product product);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
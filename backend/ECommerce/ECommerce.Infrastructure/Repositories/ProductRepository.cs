
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class ProductRepository(
    ECommerceDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await db.Products
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        string? category = null,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryName = category.Trim();

            query = query.Where(p => p.Category == categoryName);
        }

        if (inStockOnly)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        await db.Products.AddAsync(product, cancellationToken);
    }

    public void Update(Product product)
    {
        db.Products.Update(product);
    }

    public void Remove(Product product)
    {
        db.Products.Remove(product);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}

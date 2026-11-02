using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

public class ProductRepository(
    ProductDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        string? category = null,
        bool inStockOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p =>
                p.Category.ToLower() == category.ToLower());

        if (inStockOnly)
            query = query.Where(p => p.StockQuantity > 0);

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await db.Products.FindAsync(
            new object[] { id }, cancellationToken);
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
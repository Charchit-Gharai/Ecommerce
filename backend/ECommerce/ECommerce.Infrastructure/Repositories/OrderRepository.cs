
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository(
    ECommerceDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(
                o => o.Id == orderId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        await db.Orders.AddAsync(order, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}

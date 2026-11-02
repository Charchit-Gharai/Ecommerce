using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository(
    ECommerceDbContext db) : ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);
    }

    public Task<CartItem?> GetItemByIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        return db.CartItems
            .Include(i => i.Cart)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(
                i => i.Id == itemId,
                cancellationToken);
    }

    public async Task AddCartAsync(
        Cart cart,
        CancellationToken cancellationToken = default)
    {
        await db.Carts.AddAsync(cart, cancellationToken);
    }

    public async Task AddItemDirectAsync(
        CartItem item,
        CancellationToken cancellationToken = default)
    {
        await db.CartItems.AddAsync(item, cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await db.SaveChangesAsync(cancellationToken);
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }

    public void RemoveItem(CartItem item)
    {
        db.CartItems.Remove(item);
    }

    public void Clear(Cart cart)
    {
        db.CartItems.RemoveRange(cart.Items);
    }
}
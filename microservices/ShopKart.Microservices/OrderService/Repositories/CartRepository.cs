using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

public class CartRepository(
    OrderDbContext db) : ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(
                c => c.UserId == userId,
                cancellationToken);
    }

    public async Task<CartItem?> GetItemByIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        return await db.CartItems
            .Include(i => i.Cart)
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

    public void RemoveItem(CartItem item)
    {
        db.CartItems.Remove(item);
    }

    public void Clear(Cart cart)
    {
        db.CartItems.RemoveRange(cart.Items);
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }
}
using ECommerce.Core.Models;

namespace ECommerce.Core.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CartItem?> GetItemByIdAsync(
        Guid itemId,
        CancellationToken cancellationToken = default);

    Task AddCartAsync(
        Cart cart,
        CancellationToken cancellationToken = default);

    Task AddItemDirectAsync(
        CartItem item,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    void RemoveItem(CartItem item);

    void Clear(Cart cart);

    void ClearTracking();
}

using ECommerce.Core.Models;

namespace ECommerce.Core.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Order order,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}

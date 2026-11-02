using ECommerce.Core.DTOs;

namespace ECommerce.Core.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(
        string userId,
        string? shippingAddress = null,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        string userId,
        CancellationToken cancellationToken = default);
}
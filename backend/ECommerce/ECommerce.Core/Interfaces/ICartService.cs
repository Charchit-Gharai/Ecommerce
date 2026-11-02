using ECommerce.Core.DTOs;

namespace ECommerce.Core.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<CartResponse> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default);

    Task<CartResponse> UpdateItemAsync(
        string userId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default);

    Task RemoveItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default);
}
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderManagementService(
    ICartRepository carts,
    IOrderRepository orders,
    ProductServiceClient productClient)
{
    public async Task<OrderResponse> CreateAsync(
        string userId,
        string? shippingAddress,
        string? phoneNumber,
        string serviceToken,
        CancellationToken cancellationToken = default)
    {
        var cart = await carts.GetByUserIdAsync(userId, cancellationToken);

        if (cart is null || !cart.Items.Any())
            throw new ArgumentException("Cart is empty.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            ShippingAddress = shippingAddress,
            PhoneNumber = phoneNumber
        };

        foreach (var item in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });

            order.TotalAmount += item.UnitPrice * item.Quantity;

            // Call ProductService to deduct stock
            await productClient.DeductStockAsync(
                item.ProductId,
                item.Quantity,
                serviceToken,
                cancellationToken);
        }

        await orders.AddAsync(order, cancellationToken);
        carts.Clear(cart);
        await orders.SaveChangesAsync(cancellationToken);

        return ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> GetMyOrdersAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = await orders.GetByUserIdAsync(userId, cancellationToken);
        return result.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(orderId, cancellationToken);
        if (order is null || order.UserId != userId) return null;
        return ToResponse(order);
    }

    private static OrderResponse ToResponse(Order o) => new()
    {
        Id = o.Id,
        UserId = o.UserId,
        OrderDate = o.OrderDate,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        ShippingAddress = o.ShippingAddress,
        PhoneNumber = o.PhoneNumber,
        Items = o.Items.Select(i => new OrderItemResponse
        {
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList()
    };
}
using ECommerce.Core.DTOs;
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;

namespace ECommerce.Core.Services;

public class OrderService(
    ICartRepository carts,
    IProductRepository products,
    IOrderRepository orders,
    IEncryptionService encryption) : IOrderService
{
    public async Task<OrderResponse> CreateAsync(
        string userId,
        string? shippingAddress = null,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        var cart = await carts.GetByUserIdAsync(
            userId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new ArgumentException("The cart is empty.");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,

            ShippingAddress = encryption.Encrypt(shippingAddress),
            PhoneNumber = encryption.Encrypt(phoneNumber)
        };

        foreach (var cartItem in cart.Items)
        {
            var product = await products.GetByIdAsync(
                cartItem.ProductId, cancellationToken);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"Product {cartItem.ProductId} no longer exists.");
            }

            if (cartItem.Quantity < 1 ||
                product.StockQuantity < cartItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}.");
            }

            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Product = product,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price
            });

            order.TotalAmount +=
                product.Price * cartItem.Quantity;

            product.StockQuantity -= cartItem.Quantity;

            products.Update(product);
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
        var result = await orders.GetByUserIdAsync(
            userId, cancellationToken);

        return result.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse?> GetByIdAsync(
        Guid orderId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var order = await orders.GetByIdAsync(
            orderId, cancellationToken);

        if (order is null || order.UserId != userId)
            return null;

        return ToResponse(order);
    }

    private OrderResponse ToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status,

            ShippingAddress =
                encryption.Decrypt(order.ShippingAddress),

            PhoneNumber =
                encryption.Decrypt(order.PhoneNumber),

            Items = order.Items.Select(item =>
                new OrderItemResponse
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),

            PaymentStatus = order.Payment?.Status
        };
    }
}
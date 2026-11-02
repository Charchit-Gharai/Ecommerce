using System.ComponentModel.DataAnnotations;
using ECommerce.Core.Enums;

namespace ECommerce.Core.DTOs;

public class CreateOrderRequest
{
    [StringLength(500, MinimumLength = 5)]
    public string? ShippingAddress { get; set; }

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }
}

public class OrderItemResponse
{
    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => UnitPrice * Quantity;
}

public class OrderResponse
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public string? ShippingAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public List<OrderItemResponse> Items { get; set; } = [];

    public string? PaymentStatus { get; set; }
}
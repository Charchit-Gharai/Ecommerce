using ECommerce.Core.Enums;

namespace ECommerce.Core.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string? ShippingAddress { get; set; }

    public string? PhoneNumber { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public Payment? Payment { get; set; }
}
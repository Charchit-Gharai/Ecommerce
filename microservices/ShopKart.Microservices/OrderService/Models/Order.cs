namespace OrderService.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ShippingAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
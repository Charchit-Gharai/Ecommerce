namespace OrderService.DTOs;

public class CreateOrderRequest
{
    public string? ShippingAddress { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public List<OrderItemResponse> Items { get; set; } = [];
}
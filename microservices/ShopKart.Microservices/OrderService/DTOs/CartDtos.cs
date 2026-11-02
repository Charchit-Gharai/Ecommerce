namespace OrderService.DTOs;

public class AddCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class CartResponse
{
    public Guid Id { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public decimal TotalAmount =>
        Items.Sum(i => i.TotalPrice);
}
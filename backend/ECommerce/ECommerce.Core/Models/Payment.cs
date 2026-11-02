namespace ECommerce.Core.Models;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Method { get; set; } = "Mock";

    public string Status { get; set; } = "Pending";

    public string? TransactionReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
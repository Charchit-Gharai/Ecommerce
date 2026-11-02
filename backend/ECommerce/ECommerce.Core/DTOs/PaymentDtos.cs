
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.DTOs;

public class CreatePaymentRequest
{
    [Required]
    public Guid OrderId { get; set; }

    [Required, RegularExpression("^(Mock|Card|UPI)$")]
    public string Method { get; set; } = "Mock";
}

public class PaymentResponse
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }
}

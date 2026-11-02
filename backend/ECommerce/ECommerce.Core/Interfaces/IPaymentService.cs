
using ECommerce.Core.Models;

namespace ECommerce.Core.Interfaces;

public interface IPaymentService
{
    Task<Payment> ProcessPaymentAsync(
        Guid orderId,
        string userId,
        string method,
        CancellationToken cancellationToken = default);
}

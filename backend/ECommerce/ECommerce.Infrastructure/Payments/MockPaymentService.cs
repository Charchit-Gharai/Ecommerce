
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Payments;

public class MockPaymentService(
    ECommerceDbContext db) : IPaymentService
{
    public async Task<Payment> ProcessPaymentAsync(
        Guid orderId,
        string userId,
        string method,
        CancellationToken cancellationToken = default)
    {
        var order = await db.Orders
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(
                o => o.Id == orderId &&
                     o.UserId == userId,
                cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        if (order.Payment is not null)
            throw new InvalidOperationException(
                "A payment already exists for this order.");

        if (order.Status == ECommerce.Core.Enums.OrderStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot pay for a cancelled order.");

        var payment = new Payment
        {
            OrderId = order.Id,
            Order = order,
            Amount = order.TotalAmount,
            Method = method,
            Status = "Succeeded",
            TransactionReference = $"MOCK-{Guid.NewGuid():N}"
        };

        db.Payments.Add(payment);
        await db.SaveChangesAsync(cancellationToken);

        return payment;
    }
}

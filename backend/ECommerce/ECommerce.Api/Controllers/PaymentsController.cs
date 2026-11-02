
using System.Security.Claims;
using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Customer")]
public class PaymentsController(
    IPaymentService payments) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await payments.ProcessPaymentAsync(
                request.OrderId,
                UserId,
                request.Method,
                cancellationToken);

            return Ok(new PaymentResponse
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                TransactionReference = payment.TransactionReference
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Order not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

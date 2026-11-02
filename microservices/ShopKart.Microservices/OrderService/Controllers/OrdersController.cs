using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Customer")]
public class OrdersController(
    OrderManagementService orders,
    IConfiguration config) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        var serviceToken = config["ServiceToken"] ?? string.Empty;

        var result = await orders.CreateAsync(
            UserId,
            request?.ShippingAddress,
            request?.PhoneNumber,
            serviceToken,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetMyOrders), null, result);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(
        CancellationToken cancellationToken)
    {
        var result = await orders.GetMyOrdersAsync(
            UserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await orders.GetByIdAsync(
            id, UserId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
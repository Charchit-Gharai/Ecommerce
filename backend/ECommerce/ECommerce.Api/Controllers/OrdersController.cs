using System.Security.Claims;
using ECommerce.Core.Constants;
using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = AppRoles.Customer)]
public class OrdersController(
    IOrderService orders) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await orders.CreateAsync(
            UserId,
            request?.ShippingAddress,
            request?.PhoneNumber,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetMyOrders),
            null,
            result);
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
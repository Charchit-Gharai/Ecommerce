using System.Security.Claims;
using ECommerce.Core.Constants;
using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = AppRoles.Customer)]
public class CartController(
    ICartService carts) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await carts.GetAsync(
            UserId, cancellationToken));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await carts.AddItemAsync(
            UserId, request, cancellationToken);

        return Ok(result);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await carts.UpdateItemAsync(
            UserId,
            id,
            request.Quantity,
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        await carts.RemoveItemAsync(
            UserId, id, cancellationToken);

        return NoContent();
    }
}
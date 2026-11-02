using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public class CartController(CartService cart) : ControllerBase
{
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var result = await cart.GetAsync(UserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cart.AddItemAsync(
            UserId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cart.UpdateItemAsync(
            UserId, id, request.Quantity, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        await cart.RemoveItemAsync(UserId, id, cancellationToken);
        return NoContent();
    }
}
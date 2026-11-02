using ECommerce.Core.Constants;
using ECommerce.Core.DTOs;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(
    IProductService products) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var inStockOnly = !User.IsInRole(AppRoles.Admin);

        var result = await products.GetAllAsync(
            category, inStockOnly, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var inStockOnly = !User.IsInRole(AppRoles.Admin);

        var result = await products.GetByIdAsync(
            id, inStockOnly, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await products.CreateAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await products.UpdateAsync(
            id, request, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await products.DeleteAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
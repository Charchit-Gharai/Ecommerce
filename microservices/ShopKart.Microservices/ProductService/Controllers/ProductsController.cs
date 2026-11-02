using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(
    ProductCatalogService products) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] bool inStockOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await products.GetAllAsync(
            category, inStockOnly, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await products.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await products.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById),
            new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await products.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var deleted = await products.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // Called internally by OrderService to deduct stock after order
    [HttpPost("deduct-stock")]
    [Authorize(Roles = "Admin,Service")]
    public async Task<IActionResult> DeductStock(
        [FromBody] UpdateStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var success = await products.DeductStockAsync(
            request.ProductId,
            request.QuantityToDeduct,
            cancellationToken);

        return success ? Ok() : BadRequest(new
        {
            message = "Insufficient stock or product not found."
        });
    }
}
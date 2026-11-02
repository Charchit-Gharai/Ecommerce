
using System.ComponentModel.DataAnnotations;
using ECommerce.Core.Validation;

namespace ECommerce.Core.DTOs;

public class CreateProductRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [HttpOrHttpsUrl]
    [StringLength(2048)]
    public string? ImageUrl { get; set; }
}

public class UpdateProductRequest : CreateProductRequest
{
}

public class ProductResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Category { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }
}


using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [MaxLength(2048)]
    public string? ImageUrl { get; set; }
}

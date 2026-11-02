using System.Net.Http.Json;

namespace OrderService.Services;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class ProductServiceClient(
    HttpClient httpClient)
{
    public async Task<ProductDto?> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ProductDto>(
                $"/api/products/{productId}", cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> DeductStockAsync(
        Guid productId,
        int quantity,
        string serviceToken,
        CancellationToken cancellationToken = default)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", serviceToken);

        var response = await httpClient.PostAsJsonAsync(
            "/api/products/deduct-stock",
            new { ProductId = productId, QuantityToDeduct = quantity },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
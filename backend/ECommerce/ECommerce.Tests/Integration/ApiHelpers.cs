using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerce.Tests.Integration;

internal static class ApiHelpers
{
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Admin123!Abc";

    public static async Task EnsureOkAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        throw new HttpRequestException(
            $"{(int)response.StatusCode} {response.RequestMessage?.Method} " +
            $"{response.RequestMessage?.RequestUri?.AbsolutePath}: " +
            await response.Content.ReadAsStringAsync());
    }

    public static async Task<JsonElement> DataAsync(this HttpResponseMessage response)
    {
        await response.EnsureOkAsync();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.Clone();
    }

    public static async Task<HttpClient> LoginAsync(
        this ApiFactory factory, string email, string password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        var data = await response.DataAsync();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", data.GetProperty("token").GetString());
        return client;
    }

    public static Task<HttpClient> AdminAsync(this ApiFactory factory) =>
        factory.LoginAsync(AdminEmail, AdminPassword);

    public static async Task<HttpClient> NewCustomerAsync(this ApiFactory factory)
    {
        var email = $"c-{Guid.NewGuid():N}@example.com";
        var register = await factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { name = "Test Customer", email, password = "Customer1!Abc" });
        await register.EnsureOkAsync();

        return await factory.LoginAsync(email, "Customer1!Abc");
    }

    public static async Task<Guid> CreateProductAsync(
        this HttpClient admin, decimal price, int stock)
    {
        var response = await admin.PostAsJsonAsync("/api/products", new
        {
            name = $"Prod-{Guid.NewGuid():N}",
            description = "Test product",
            price,
            category = "TestCategory",
            stockQuantity = stock,
            imageUrl = (string?)null
        });

        var product = await response.DataAsync();
        return product.GetProperty("id").GetGuid();
    }

    public static async Task<JsonElement> PlaceOrderAsync(
        this HttpClient customer, Guid productId, int quantity)
    {
        var add = await customer.PostAsJsonAsync("/api/cart/items", new { productId, quantity });
        await add.EnsureOkAsync();

        var orderResponse = await customer.PostAsJsonAsync("/api/orders", new { });
        var order = await orderResponse.DataAsync();

        var payResponse = await customer.PostAsJsonAsync("/api/payments", new
        {
            orderId = order.GetProperty("id").GetGuid(),
            method = "Mock"
        });
        await payResponse.EnsureOkAsync();

        return order;
    }

    public static async Task<int> StockAsync(this HttpClient client, Guid productId) =>
        (await (await client.GetAsync($"/api/products/{productId}")).DataAsync())
            .GetProperty("stockQuantity").GetInt32();
}
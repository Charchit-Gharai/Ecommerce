using System.Net;
using System.Net.Http.Json;

namespace ECommerce.Tests.Integration;

public class ApiFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Checkout_Success_ConfirmsOrder_AndReducesStock()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);
        var customer = await factory.NewCustomerAsync();

        Assert.Equal(10, await factory.CreateClient().StockAsync(productId));

        var order = await customer.PlaceOrderAsync(productId, 2);

        Assert.Equal("Pending", order.GetProperty("status").GetString());
        Assert.Equal(100m, order.GetProperty("totalAmount").GetDecimal());

        Assert.Equal(8, await factory.CreateClient().StockAsync(productId));
    }

    [Fact]
    public async Task Checkout_ClearsTheCart()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);
        var customer = await factory.NewCustomerAsync();

        await customer.PlaceOrderAsync(productId, 1);

        var cart = await customer.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.OK, cart.StatusCode);

        var data = await cart.DataAsync();
        Assert.Equal(0, data.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task AddToCart_MoreThanStock_Returns400()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 2);
        var customer = await factory.NewCustomerAsync();

        var r = await customer.PostAsJsonAsync("/api/cart/items",
            new { productId, quantity = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task PlaceOrder_WithEmptyCart_Returns400()
    {
        var customer = await factory.NewCustomerAsync();

        var r = await customer.PostAsJsonAsync("/api/orders", new { });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Customer_CanViewOwnOrders()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);
        var customer = await factory.NewCustomerAsync();

        await customer.PlaceOrderAsync(productId, 1);

        var r = await customer.GetAsync("/api/orders/my-orders");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        using var doc = System.Text.Json.JsonDocument.Parse(
                               await r.Content.ReadAsStringAsync());
        var orders = doc.RootElement.EnumerateArray().ToList();
        Assert.True(orders.Count >= 1);
    }

    [Fact]
    public async Task Payment_Succeeds_ForValidOrder()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);
        var customer = await factory.NewCustomerAsync();

        await customer.PostAsJsonAsync("/api/cart/items",
            new { productId, quantity = 1 });
        var orderData = await (await customer.PostAsJsonAsync(
            "/api/orders", new { })).DataAsync();
        var orderId = orderData.GetProperty("id").GetGuid();

        var payResponse = await customer.PostAsJsonAsync("/api/payments",
            new { orderId, method = "Mock" });

        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);

        var payment = await payResponse.DataAsync();
        Assert.Equal("Succeeded", payment.GetProperty("status").GetString());
    }

    [Fact]
    public async Task DeletingProduct_RemovesIt_FromCatalog()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);

        var delete = await admin.DeleteAsync($"/api/products/{productId}");
        var get = await factory.CreateClient()
                         .GetAsync($"/api/products/{productId}");

        Assert.True(delete.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
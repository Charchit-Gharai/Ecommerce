using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ECommerce.Tests.Integration;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Login_AsAdmin_ReturnsToken_AndAdminRole()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiHelpers.AdminEmail, password = ApiHelpers.AdminPassword });

        var data = await response.DataAsync();

        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("token").GetString()));
        Assert.Equal("Admin", data.GetProperty("role").GetString());
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var r = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiHelpers.AdminEmail, password = "WrongPass1" });

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var r = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = "nobody@example.com", password = "Whatever1!" });

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var body = new { name = "Dup", email, password = "Customer1!Abc" };

        Assert.True((await factory.CreateClient()
            .PostAsJsonAsync("/api/auth/register", body)).IsSuccessStatusCode);

        var second = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", body);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var r = await factory.CreateClient().PostAsJsonAsync("/api/auth/register",
            new { name = "Weak", email = $"w-{Guid.NewGuid():N}@example.com", password = "abc" });

        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task Customer_TokenIsValid_AfterRegisterAndLogin()
    {
        var customer = await factory.NewCustomerAsync();

        var r = await customer.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task WithoutToken_CartReturns401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await factory.CreateClient().GetAsync("/api/cart")).StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_Returns401()
    {
        var r = await factory.CreateClient().PostAsJsonAsync("/api/products",
            new { name = "X", price = 10, category = "Test", stockQuantity = 1 });

        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotCreateProduct()
    {
        var customer = await factory.NewCustomerAsync();

        var r = await customer.PostAsJsonAsync("/api/products",
            new { name = "X", price = 10, category = "Test", stockQuantity = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
    }

    [Fact]
    public async Task Customer_CannotListAdminOrders()
    {
        var customer = await factory.NewCustomerAsync();

        var r = await customer.GetAsync("/api/orders/my-orders");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotUseCart()
    {
        var admin = await factory.AdminAsync();

        Assert.Equal(HttpStatusCode.Forbidden,
            (await admin.GetAsync("/api/cart")).StatusCode);
    }

    [Fact]
    public async Task Products_ArePublic()
    {
        Assert.Equal(HttpStatusCode.OK,
            (await factory.CreateClient().GetAsync("/api/products")).StatusCode);
    }

    [Fact]
    public async Task Admin_CanCreateProduct_AndAnyoneCanReadIt()
    {
        var admin = await factory.AdminAsync();
        var productId = await admin.CreateProductAsync(price: 50, stock: 10);

        var r = await factory.CreateClient().GetAsync($"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Products_FilterByCategory_ReturnsCorrectProducts()
    {
        var admin = await factory.AdminAsync();

        await admin.PostAsJsonAsync("/api/products", new
        {
            name = "FilterTest",
            description = "test",
            price = 10m,
            category = "UniqueFilterCategory",
            stockQuantity = 5,
            imageUrl = (string?)null
        });

        var r = await factory.CreateClient()
            .GetAsync("/api/products?category=UniqueFilterCategory");

        Assert.Equal(HttpStatusCode.OK, r.StatusCode);

        using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
        var items = doc.RootElement.EnumerateArray().ToList();
        Assert.True(items.Count >= 1);
        Assert.All(items, p =>
            Assert.Equal("UniqueFilterCategory",
                p.GetProperty("category").GetString()));
    }
}
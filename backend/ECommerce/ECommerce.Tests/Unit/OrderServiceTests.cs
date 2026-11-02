
using ECommerce.Core.Enums;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Core.Services;
using Moq;

namespace ECommerce.Tests.Unit;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orders = new();
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IEncryptionService> _encryption = new();

    public OrderServiceTests()
    {
        // Return the original value for encryption and decryption
        // so unit tests do not need actual encryption.
        _encryption
            .Setup(x => x.Encrypt(It.IsAny<string>()))
            .Returns<string>(value => value);

        _encryption
            .Setup(x => x.Decrypt(It.IsAny<string>()))
            .Returns<string>(value => value);
    }

    private OrderService Create() =>
        new(
            _carts.Object,
            _products.Object,
            _orders.Object,
            _encryption.Object
        );

    [Fact]
    public async Task GetById_ReturnsNull_WhenOrderDoesNotExist()
    {
        var orderId = Guid.NewGuid();

        _orders
            .Setup(repository => repository.GetByIdAsync(
                orderId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var result = await Create().GetByIdAsync(
            orderId,
            "u1",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenOrderBelongsToAnotherUser()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = "another-user",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100m,
            Status = OrderStatus.Pending
        };

        _orders
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await Create().GetByIdAsync(
            order.Id,
            "u1",
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetById_ReturnsOrder_WhenFoundForUser()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = "u1",
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100m,
            Status = OrderStatus.Pending
        };

        _orders
            .Setup(repository => repository.GetByIdAsync(
                order.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await Create().GetByIdAsync(
            order.Id,
            "u1",
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal("u1", result.UserId);
        Assert.Equal(100m, result.TotalAmount);
    }
}

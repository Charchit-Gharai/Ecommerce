
using ECommerce.Core.DTOs;
using ECommerce.Core.Exceptions;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using ECommerce.Core.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ECommerce.Tests.Unit;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<IProductRepository> _products = new();

    private CartService Create() =>
        new(_carts.Object, _products.Object);

    private Product SeedProduct(int stock, decimal price = 50m)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop",
            Price = price,
            Category = "Electronics",
            StockQuantity = stock
        };

        _products
            .Setup(p => p.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        return product;
    }

    private void SetupCart(Cart cart)
    {
        _carts
            .Setup(c => c.GetByUserIdAsync(
                cart.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cart);
    }

    [Fact]
    public async Task AddItem_Throws_WhenQuantityIsZero()
    {
        var request = new AddCartItemRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().AddItemAsync("u1", request));
    }

    [Fact]
    public async Task AddItem_Throws_WhenProductDoesNotExist()
    {
        var request = new AddCartItemRequest
        {
            ProductId = Guid.NewGuid(),
            Quantity = 1
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Create().AddItemAsync("u1", request));
    }

    [Fact]
    public async Task AddItem_Throws_WhenStockIsInsufficient()
    {
        var product = SeedProduct(stock: 1);

        var request = new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 5
        };

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => Create().AddItemAsync("u1", request));
    }

    [Fact]
    public async Task AddItem_CreatesCart_WhenUserHasNone()
    {
        var product = SeedProduct(stock: 10);

        Cart? storedCart = null;

        _carts
            .Setup(c => c.GetByUserIdAsync(
                "u1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => storedCart);

        _carts
            .Setup(c => c.AddCartAsync(
                It.IsAny<Cart>(),
                It.IsAny<CancellationToken>()))
            .Callback<Cart, CancellationToken>((cart, _) =>
            {
                storedCart = cart;
            })
            .Returns(Task.CompletedTask);

        // Simulate adding the item to the in-memory cart.
        _carts
            .Setup(c => c.AddItemDirectAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()))
            .Callback<CartItem, CancellationToken>((item, _) =>
            {
                item.Product = product;
                item.Cart = storedCart!;
                storedCart!.Items.Add(item);
            })
            .Returns(Task.CompletedTask);

        var request = new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 2
        };

        var result = await Create().AddItemAsync("u1", request);

        Assert.NotNull(storedCart);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(100m, result.TotalAmount);

        _carts.Verify(c => c.AddCartAsync(
            It.IsAny<Cart>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddItem_MergesQuantity_ForExistingProduct()
    {
        var product = SeedProduct(stock: 10, price: 50m);

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = 50m,
            Cart = cart
        });

        SetupCart(cart);

        var request = new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 2
        };

        var result = await Create().AddItemAsync("u1", request);

        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Quantity);
        Assert.Equal(150m, result.TotalAmount);
    }

    [Fact]
    public async Task AddItem_ReloadsCartAndRetries_WhenItemIsDeletedConcurrently()
    {
        var product = SeedProduct(stock: 10);

        var originalCart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        originalCart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = product.Price,
            Cart = originalCart
        });

        var reloadedCart = new Cart
        {
            Id = originalCart.Id,
            UserId = originalCart.UserId
        };

        var cartReadCount = 0;

        _carts
            .Setup(c => c.GetByUserIdAsync(
                "u1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
                ++cartReadCount == 1
                    ? originalCart
                    : reloadedCart);

        // First save fails due to concurrency.
        // Second save succeeds.
        _carts
            .SetupSequence(c => c.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException())
            .Returns(Task.CompletedTask);

        // Simulate adding the item after the cart is reloaded.
        _carts
            .Setup(c => c.AddItemDirectAsync(
                It.IsAny<CartItem>(),
                It.IsAny<CancellationToken>()))
            .Callback<CartItem, CancellationToken>((item, _) =>
            {
                item.Product = product;
                item.Cart = reloadedCart;
                reloadedCart.Items.Add(item);
            })
            .Returns(Task.CompletedTask);

        var request = new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 1
        };

        var result = await Create().AddItemAsync("u1", request);

        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Quantity);

        _carts.Verify(
            c => c.ClearTracking(),
            Times.AtLeastOnce);

        _carts.Verify(
            c => c.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task AddItem_Throws_WhenCombinedQuantityExceedsStock()
    {
        var product = SeedProduct(stock: 3);

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = 50m,
            Cart = cart
        });

        SetupCart(cart);

        var request = new AddCartItemRequest
        {
            ProductId = product.Id,
            Quantity = 2
        };

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => Create().AddItemAsync("u1", request));
    }

    [Fact]
    public async Task UpdateItem_ChangesQuantity()
    {
        var product = SeedProduct(stock: 10, price: 50m);
        var itemId = Guid.NewGuid();

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        var item = new CartItem
        {
            Id = itemId,
            ProductId = product.Id,
            Product = product,
            Quantity = 2,
            UnitPrice = 50m,
            Cart = cart
        };

        cart.Items.Add(item);

        SetupCart(cart);

        _carts
            .Setup(c => c.GetItemByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await Create().UpdateItemAsync(
            "u1", itemId, 4);

        Assert.Equal(4, result.Items[0].Quantity);
        Assert.Equal(200m, result.TotalAmount);
    }

    [Fact]
    public async Task UpdateItem_Throws_WhenQuantityIsZero()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => Create().UpdateItemAsync(
                "u1", Guid.NewGuid(), 0));
    }

    [Fact]
    public async Task UpdateItem_Throws_WhenItemBelongsToAnotherUser()
    {
        var product = SeedProduct(stock: 10);
        var itemId = Guid.NewGuid();

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "another-user"
        };

        var item = new CartItem
        {
            Id = itemId,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = 50m,
            Cart = cart
        };

        cart.Items.Add(item);

        _carts
            .Setup(c => c.GetItemByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Create().UpdateItemAsync(
                "u1", itemId, 2));
    }

    [Fact]
    public async Task UpdateItem_Throws_WhenStockIsInsufficient()
    {
        var product = SeedProduct(stock: 3);
        var itemId = Guid.NewGuid();

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        var item = new CartItem
        {
            Id = itemId,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = 50m,
            Cart = cart
        };

        cart.Items.Add(item);

        _carts
            .Setup(c => c.GetItemByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        await Assert.ThrowsAsync<InsufficientStockException>(
            () => Create().UpdateItemAsync(
                "u1", itemId, 4));
    }

    [Fact]
    public async Task RemoveItem_Throws_WhenItemDoesNotExist()
    {
        _carts
            .Setup(c => c.GetItemByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => Create().RemoveItemAsync(
                "u1", Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveItem_RemovesItem_WhenOwnedByUser()
    {
        var product = SeedProduct(stock: 10);
        var itemId = Guid.NewGuid();

        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = "u1"
        };

        var item = new CartItem
        {
            Id = itemId,
            ProductId = product.Id,
            Product = product,
            Quantity = 1,
            UnitPrice = 50m,
            Cart = cart
        };

        cart.Items.Add(item);

        _carts
            .Setup(c => c.GetItemByIdAsync(
                itemId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        await Create().RemoveItemAsync("u1", itemId);

        _carts.Verify(
            c => c.RemoveItem(item),
            Times.Once);

        _carts.Verify(
            c => c.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

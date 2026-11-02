using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class CartService(
    ICartRepository carts,
    ProductServiceClient productClient)
{
    public async Task<CartResponse> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        await carts.SaveChangesAsync(cancellationToken);
        return ToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.");

        var product = await productClient.GetProductAsync(
            request.ProductId, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException("Product was not found.");

        var cart = await carts.GetByUserIdAsync(userId, cancellationToken);

        var existingItem = cart?.Items.FirstOrDefault(
            i => i.ProductId == request.ProductId);

        var newQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

        if (newQuantity > product.StockQuantity)
            throw new InvalidOperationException("Insufficient stock.");

        if (cart is null)
        {
            cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
            await carts.AddCartAsync(cart, cancellationToken);
            await carts.SaveChangesAsync(cancellationToken);
            carts.ClearTracking();

            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            };

            await carts.AddItemDirectAsync(newItem, cancellationToken);
            await carts.SaveChangesAsync(cancellationToken);
            carts.ClearTracking();

            cart = await carts.GetByUserIdAsync(userId, cancellationToken)
                   ?? throw new InvalidOperationException("Cart not found.");

            return ToResponse(cart);
        }

        if (existingItem is null)
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            };
            await carts.AddItemDirectAsync(newItem, cancellationToken);
        }
        else
        {
            existingItem.Quantity = newQuantity;
        }

        await carts.SaveChangesAsync(cancellationToken);
        carts.ClearTracking();

        cart = await carts.GetByUserIdAsync(userId, cancellationToken)
               ?? throw new InvalidOperationException("Cart not found.");

        return ToResponse(cart);
    }

    public async Task<CartResponse> UpdateItemAsync(
        string userId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
            throw new ArgumentException("Quantity must be at least 1.");

        var item = await carts.GetItemByIdAsync(itemId, cancellationToken);

        if (item is null || item.Cart.UserId != userId)
            throw new KeyNotFoundException("Cart item not found.");

        item.Quantity = quantity;
        await carts.SaveChangesAsync(cancellationToken);

        var cart = await carts.GetByUserIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException("Cart not found.");

        return ToResponse(cart);
    }

    public async Task RemoveItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await carts.GetItemByIdAsync(itemId, cancellationToken);

        if (item is null || item.Cart.UserId != userId)
            throw new KeyNotFoundException("Cart item not found.");

        carts.RemoveItem(item);
        await carts.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await carts.GetByUserIdAsync(userId, cancellationToken);
        if (cart is null) return;
        carts.Clear(cart);
        await carts.SaveChangesAsync(cancellationToken);
    }

    private async Task<Cart> GetOrCreateAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var cart = await carts.GetByUserIdAsync(userId, cancellationToken);
        if (cart is not null) return cart;

        cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        await carts.AddCartAsync(cart, cancellationToken);
        return cart;
    }

    private static CartResponse ToResponse(Cart cart) => new()
    {
        Id = cart.Id,
        Items = cart.Items.Select(i => new CartItemResponse
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList()
    };
}

using ECommerce.Core.DTOs;
using ECommerce.Core.Exceptions;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Core.Services;

public class CartService(
    ICartRepository carts,
    IProductRepository products) : ICartService
{
    public async Task<CartResponse> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateAsync(
            userId, cancellationToken);

        await carts.SaveChangesAsync(cancellationToken);

        return ToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(
        string userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Quantity < 1)
        {
            throw new ArgumentException(
                "Quantity must be at least 1.");
        }

        var product = await products.GetByIdAsync(
            request.ProductId, cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                "Product was not found.");
        }

        // Retrieve the customer's cart.
        var cart = await carts.GetByUserIdAsync(
            userId, cancellationToken);

        // Check existing quantity before creating a cart.
        var existingItem = cart?.Items.FirstOrDefault(
            item => item.ProductId == request.ProductId);

        var newQuantity =
            request.Quantity + (existingItem?.Quantity ?? 0);

        // Validate stock before performing any database writes.
        if (newQuantity > product.StockQuantity)
        {
            throw new InsufficientStockException(
                "Insufficient stock.");
        }

        // Create a cart only after validation succeeds.
        if (cart is null)
        {
            cart = new Cart
            {
                Id = Guid.NewGuid(),
                UserId = userId
            };

            await carts.AddCartAsync(
                cart, cancellationToken);

            await carts.SaveChangesAsync(cancellationToken);

            carts.ClearTracking();

            cart = await carts.GetByUserIdAsync(
                userId, cancellationToken)
                ?? throw new InvalidOperationException(
                    "Cart could not be created.");
        }

        // Find the item again after cart creation/reloading.
        existingItem = cart.Items.FirstOrDefault(
            item => item.ProductId == request.ProductId);

        newQuantity =
            request.Quantity + (existingItem?.Quantity ?? 0);

        if (newQuantity > product.StockQuantity)
        {
            throw new InsufficientStockException(
                "Insufficient stock.");
        }

        if (existingItem is null)
        {
            var newItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            };

            await carts.AddItemDirectAsync(
                newItem, cancellationToken);
        }
        else
        {
            existingItem.Quantity = newQuantity;
        }

        try
        {
            await carts.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Clear stale EF Core tracking.
            carts.ClearTracking();

            // Reload the latest cart.
            cart = await carts.GetByUserIdAsync(
                userId, cancellationToken)
                ?? throw new InvalidOperationException(
                    "Cart was not found after concurrency conflict.");

            existingItem = cart.Items.FirstOrDefault(
                item => item.ProductId == request.ProductId);

            newQuantity =
                request.Quantity + (existingItem?.Quantity ?? 0);

            if (newQuantity > product.StockQuantity)
            {
                throw new InsufficientStockException(
                    "Insufficient stock.");
            }

            if (existingItem is null)
            {
                var retryItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = product.Id,
                    Quantity = request.Quantity,
                    UnitPrice = product.Price
                };

                await carts.AddItemDirectAsync(
                    retryItem, cancellationToken);
            }
            else
            {
                existingItem.Quantity = newQuantity;
            }

            // Retry only once.
            await carts.SaveChangesAsync(cancellationToken);
        }

        // Reload the final cart state.
        carts.ClearTracking();

        cart = await carts.GetByUserIdAsync(
            userId, cancellationToken)
            ?? throw new InvalidOperationException(
                "Cart could not be reloaded.");

        return ToResponse(cart);
    }

    public async Task<CartResponse> UpdateItemAsync(
        string userId,
        Guid itemId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        if (quantity < 1)
        {
            throw new ArgumentException(
                "Quantity must be at least 1.");
        }

        var item = await carts.GetItemByIdAsync(
            itemId, cancellationToken);

        if (item is null || item.Cart.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Cart item was not found.");
        }

        if (quantity > item.Product.StockQuantity)
        {
            throw new InsufficientStockException(
                "Insufficient stock.");
        }

        item.Quantity = quantity;

        try
        {
            await carts.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new KeyNotFoundException(
                "The cart item no longer exists or was changed " +
                "by another request. Refresh the cart and try again.");
        }

        carts.ClearTracking();

        var updatedCart = await carts.GetByUserIdAsync(
            userId, cancellationToken);

        if (updatedCart is null)
        {
            throw new KeyNotFoundException(
                "Cart was not found.");
        }

        return ToResponse(updatedCart);
    }

    public async Task RemoveItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await carts.GetItemByIdAsync(
            itemId, cancellationToken);

        if (item is null || item.Cart.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Cart item was not found.");
        }

        carts.RemoveItem(item);

        try
        {
            await carts.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new KeyNotFoundException(
                "The cart item was already removed or no longer exists.");
        }
    }

    private async Task<Cart> GetOrCreateAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var cart = await carts.GetByUserIdAsync(
            userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        await carts.AddCartAsync(
            cart, cancellationToken);

        return cart;
    }

    private static CartResponse ToResponse(Cart cart)
    {
        return new CartResponse
        {
            Id = cart.Id,
            Items = cart.Items.Select(item =>
                new CartItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity
                }).ToList()
        };
    }
}

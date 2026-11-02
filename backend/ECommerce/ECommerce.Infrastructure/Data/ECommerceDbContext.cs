using ECommerce.Core.Models;
using ECommerce.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class ECommerceDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ECommerceDbContext(
        DbContextOptions<ECommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(1000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.StockQuantity)
                .IsRequired();

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(2048);

            entity.HasIndex(p => p.Category);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.UserId)
                .IsRequired();

            entity.HasIndex(c => c.UserId)
                .IsUnique();

            entity.HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.UnitPrice)
                .HasPrecision(18, 2)
                .IsRequired();


            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);

            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(o => o.ShippingAddress)
                .HasMaxLength(2000);

            entity.Property(o => o.PhoneNumber)
                .HasMaxLength(256);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(o => new { o.UserId, o.OrderDate });
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(i => i.Id);

            entity.Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Amount)
                .HasPrecision(18, 2);

            entity.Property(p => p.Method)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(p => p.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(p => p.TransactionReference)
                .HasMaxLength(100);

            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
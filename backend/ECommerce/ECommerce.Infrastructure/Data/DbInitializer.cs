using ECommerce.Core.Models;
using ECommerce.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var db = scope.ServiceProvider
            .GetRequiredService<ECommerceDbContext>();

        // ── Roles ────────────────────────────────────────────────────────
        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        $"Could not create role {role}.");
            }
        }

        // ── Admin user ───────────────────────────────────────────────────
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];

        if (!string.IsNullOrWhiteSpace(email) &&
            !string.IsNullOrWhiteSpace(password))
        {
            var existingAdmin = await userManager.FindByEmailAsync(email);

            if (existingAdmin is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    admin, password);

                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        "Admin creation failed: " +
                        string.Join(", ",
                            createResult.Errors.Select(e => e.Description)));

                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
            }
        }

        // ── Products ─────────────────────────────────────────────────────
        if (await db.Products.AnyAsync())
            return; // Already seeded

        var products = new List<Product>
        {
            // ── Electronics (12 products) ────────────────────────────────
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Apple MacBook Pro 14-inch M3",
                Description   = "Apple MacBook Pro with M3 chip, 14-inch Liquid Retina XDR display, 18GB RAM, 512GB SSD. Supercharged by the next-generation M3 chip for pro-level performance.",
                Price         = 1999.00m,
                Category      = "Electronics",
                StockQuantity = 25,
                ImageUrl      = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Samsung 65\" 4K QLED Smart TV",
                Description   = "Samsung 65-inch QLED 4K Smart TV with Quantum Dot technology, 120Hz refresh rate, HDR10+, and built-in Alexa and Google Assistant.",
                Price         = 1299.00m,
                Category      = "Electronics",
                StockQuantity = 15,
                ImageUrl      = "https://images.unsplash.com/photo-1593359677879-a4bb92f4834a?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Sony WH-1000XM5 Headphones",
                Description   = "Industry-leading noise canceling headphones with 30-hour battery life, multipoint connection, and crystal clear hands-free calling.",
                Price         = 349.99m,
                Category      = "Electronics",
                StockQuantity = 40,
                ImageUrl      = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "iPad Pro 12.9-inch M2",
                Description   = "Apple iPad Pro with M2 chip, 12.9-inch Liquid Retina XDR display, Wi-Fi 6E, 256GB storage. The ultimate iPad experience.",
                Price         = 1099.00m,
                Category      = "Electronics",
                StockQuantity = 30,
                ImageUrl      = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Canon EOS R6 Mark II Mirrorless Camera",
                Description   = "Full-frame mirrorless camera with 40fps shooting, 6K RAW video, dual card slots, and advanced subject detection AF.",
                Price         = 2499.00m,
                Category      = "Electronics",
                StockQuantity = 12,
                ImageUrl      = "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Apple iPhone 15 Pro Max",
                Description   = "iPhone 15 Pro Max with A17 Pro chip, 6.7-inch Super Retina XDR display, titanium design, 48MP main camera system, and Action button.",
                Price         = 1199.00m,
                Category      = "Electronics",
                StockQuantity = 50,
                ImageUrl      = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Dell XPS 15 Laptop",
                Description   = "Dell XPS 15 with Intel Core i9, 32GB RAM, 1TB SSD, NVIDIA RTX 4070, and 15.6-inch OLED touch display. Power meets portability.",
                Price         = 2299.00m,
                Category      = "Electronics",
                StockQuantity = 18,
                ImageUrl      = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Sony PlayStation 5 Console",
                Description   = "PS5 console with ultra-high speed SSD, haptic feedback DualSense controller, 4K gaming at up to 120fps, and ray tracing support.",
                Price         = 499.99m,
                Category      = "Electronics",
                StockQuantity = 20,
                ImageUrl      = "https://images.unsplash.com/photo-1607853202273-797f1c22a38e?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Apple Watch Series 9",
                Description   = "Apple Watch Series 9 with S9 chip, 45mm Always-On Retina display, health sensors, crash detection, and 18-hour battery life.",
                Price         = 429.00m,
                Category      = "Electronics",
                StockQuantity = 35,
                ImageUrl      = "https://images.unsplash.com/photo-1551816230-ef5deaed4a26?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Bose SoundLink Max Bluetooth Speaker",
                Description   = "Portable Bluetooth speaker with immersive sound, 20-hour battery, IP67 waterproof rating, and built-in speakerphone.",
                Price         = 399.00m,
                Category      = "Electronics",
                StockQuantity = 28,
                ImageUrl      = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "LG 27\" UltraGear Gaming Monitor",
                Description   = "27-inch QHD IPS gaming monitor with 165Hz refresh rate, 1ms response time, G-Sync compatible, HDR400, and USB-C connectivity.",
                Price         = 449.99m,
                Category      = "Electronics",
                StockQuantity = 22,
                ImageUrl      = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "DJI Mini 4 Pro Drone",
                Description   = "Lightweight foldable drone under 249g with 4K/60fps video, 48MP photos, 34-min flight time, omnidirectional obstacle sensing.",
                Price         = 759.00m,
                Category      = "Electronics",
                StockQuantity = 14,
                ImageUrl      = "https://images.unsplash.com/photo-1579829366248-204fe8413f31?w=600&q=80"
            },

            // ── Clothing (12 products) ───────────────────────────────────
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Levi's 501 Original Fit Jeans",
                Description   = "The original straight leg jean with a button fly. Made with 100% cotton denim, these iconic jeans have defined style for generations.",
                Price         = 69.99m,
                Category      = "Clothing",
                StockQuantity = 80,
                ImageUrl      = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Nike Air Max 270 Sneakers",
                Description   = "Nike Air Max 270 features Nike's biggest heel Air unit yet for a super-soft ride that feels as great as it looks. Available in multiple colorways.",
                Price         = 149.99m,
                Category      = "Clothing",
                StockQuantity = 60,
                ImageUrl      = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "The North Face Puffer Jacket",
                Description   = "700-fill goose down insulation keeps you warm in extreme cold. Water-repellent finish, packable design, and secure zip pockets.",
                Price         = 299.00m,
                Category      = "Clothing",
                StockQuantity = 45,
                ImageUrl      = "https://images.unsplash.com/photo-1544923246-77307dd654cb?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Ralph Lauren Polo Shirt",
                Description   = "Classic fit polo shirt made from soft cotton mesh. Features the iconic embroidered polo player logo, ribbed collar and cuffs.",
                Price         = 89.99m,
                Category      = "Clothing",
                StockQuantity = 100,
                ImageUrl      = "https://images.unsplash.com/photo-1586790170083-2f9ceadc732d?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Adidas Ultraboost 23 Running Shoes",
                Description   = "Experience incredible energy return with BOOST midsole technology. Primeknit+ upper adapts to your foot for a sock-like fit.",
                Price         = 189.99m,
                Category      = "Clothing",
                StockQuantity = 55,
                ImageUrl      = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Zara Oversized Blazer",
                Description   = "Sophisticated oversized blazer in premium fabric blend. Features notch lapels, front welt pockets, and a relaxed silhouette perfect for layering.",
                Price         = 129.00m,
                Category      = "Clothing",
                StockQuantity = 38,
                ImageUrl      = "https://images.unsplash.com/photo-1591047139829-d91aecb6caea?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Champion Reverse Weave Hoodie",
                Description   = "Iconic Reverse Weave hoodie with C logo on left chest. Made with heavyweight fleece that resists shrinkage and maintains shape wash after wash.",
                Price         = 74.99m,
                Category      = "Clothing",
                StockQuantity = 70,
                ImageUrl      = "https://images.unsplash.com/photo-1556821840-3a63f15732ce?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Ray-Ban Aviator Classic Sunglasses",
                Description   = "Timeless aviator-style sunglasses with gold metal frame, crystal green lenses with G-15 tint. 100% UV protection.",
                Price         = 179.00m,
                Category      = "Clothing",
                StockQuantity = 42,
                ImageUrl      = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Lululemon Align Leggings",
                Description   = "Made with our buttery-soft Nulu fabric, these high-rise leggings feel like a second skin. Ideal for yoga, pilates, and everyday wear.",
                Price         = 118.00m,
                Category      = "Clothing",
                StockQuantity = 65,
                ImageUrl      = "https://images.unsplash.com/photo-1506629082955-511b1aa562c8?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Timberland 6-Inch Premium Waterproof Boots",
                Description   = "Iconic waterproof boots with seam-sealed construction, padded collar for comfort, and lug outsole for rugged traction in any terrain.",
                Price         = 219.99m,
                Category      = "Clothing",
                StockQuantity = 33,
                ImageUrl      = "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "H&M Linen Summer Dress",
                Description   = "Flowy V-neck linen blend dress with adjustable straps and a relaxed fit. Perfect for warm weather with a breathable, lightweight feel.",
                Price         = 49.99m,
                Category      = "Clothing",
                StockQuantity = 75,
                ImageUrl      = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Gucci GG Canvas Baseball Cap",
                Description   = "Baseball cap in GG Supreme canvas with Web stripe and embroidered Gucci logo. Adjustable strap closure for a custom fit.",
                Price         = 395.00m,
                Category      = "Clothing",
                StockQuantity = 20,
                ImageUrl      = "https://images.unsplash.com/photo-1588850561407-ed78c282e89b?w=600&q=80"
            },

            // ── Home & Kitchen (10 products) ─────────────────────────────
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Nespresso Vertuo Next Coffee Machine",
                Description   = "Nespresso Vertuo Next brews 5 cup sizes at the touch of a button. Centrifusion technology extracts every drop of flavor for a perfect cup every time.",
                Price         = 179.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 30,
                ImageUrl      = "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "KitchenAid Artisan Stand Mixer",
                Description   = "5-quart tilt-head stand mixer with 10 speeds and over 10 attachments available. The iconic design has been a kitchen staple since 1937.",
                Price         = 449.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 20,
                ImageUrl      = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Dyson V15 Detect Cordless Vacuum",
                Description   = "Most powerful Dyson cordless vacuum with laser dust detection, LCD screen showing real-time particle count, and up to 60 min runtime.",
                Price         = 749.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 18,
                ImageUrl      = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Instant Pot Duo 7-in-1 Electric Pressure Cooker",
                Description   = "7-in-1 multi-use appliance: pressure cooker, slow cooker, rice cooker, steamer, sauté, yogurt maker, and warmer. 6-quart capacity.",
                Price         = 99.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 45,
                ImageUrl      = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Le Creuset Signature Cast Iron Dutch Oven",
                Description   = "5.5-quart enameled cast iron Dutch oven in iconic Flame orange. Superior heat distribution and retention for braising, soups, and bread baking.",
                Price         = 399.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 25,
                ImageUrl      = "https://images.unsplash.com/photo-1547592180-85f173990554?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Philips Hue Smart Bulb Starter Kit",
                Description   = "Smart LED bulbs starter kit with bridge and 4 bulbs. Control 16 million colors and whites via app or voice. Works with Alexa, Google, Apple HomeKit.",
                Price         = 199.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 35,
                ImageUrl      = "https://images.unsplash.com/photo-1586473219010-2ffc57b0d282?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Vitamix 5200 Blender",
                Description   = "Professional-grade blender with variable speed control, aircraft-grade stainless steel blades, and self-cleaning capability. 64 oz container.",
                Price         = 549.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 15,
                ImageUrl      = "https://images.unsplash.com/photo-1570222094114-d054a817e56b?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Roomba j7+ Robot Vacuum",
                Description   = "iRobot Roomba j7+ with Clean Base Automatic Dirt Disposal. Avoids obstacles, empties itself for 60 days, and learns your home over time.",
                Price         = 799.99m,
                Category      = "Home & Kitchen",
                StockQuantity = 12,
                ImageUrl      = "https://images.unsplash.com/photo-1589006053787-b55b89df7374?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Casper Original Foam Mattress Queen",
                Description   = "The original Casper mattress with 4 layers of foam for pressure relief, cool comfort, and durable support. 100-night risk-free trial.",
                Price         = 1095.00m,
                Category      = "Home & Kitchen",
                StockQuantity = 10,
                ImageUrl      = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Weber Spirit II E-310 Gas Grill",
                Description   = "3-burner propane gas grill with GS4 grilling system, iGrill 3 compatible, porcelain-enameled cast iron cooking grates. 529 sq in cooking area.",
                Price         = 599.00m,
                Category      = "Home & Kitchen",
                StockQuantity = 8,
                ImageUrl      = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600&q=80"
            },

            // ── Books (8 products) ───────────────────────────────────────
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Atomic Habits by James Clear",
                Description   = "The #1 New York Times bestseller. An easy and proven way to build good habits and break bad ones. Over 15 million copies sold worldwide.",
                Price         = 16.99m,
                Category      = "Books",
                StockQuantity = 150,
                ImageUrl      = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "The Psychology of Money by Morgan Housel",
                Description   = "Timeless lessons on wealth, greed, and happiness. 19 short stories exploring the strange ways people think about money.",
                Price         = 14.99m,
                Category      = "Books",
                StockQuantity = 120,
                ImageUrl      = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Zero to One by Peter Thiel",
                Description   = "Notes on startups, or how to build the future. Peter Thiel shares the contrarian thinking that builds companies like PayPal and Palantir.",
                Price         = 15.99m,
                Category      = "Books",
                StockQuantity = 90,
                ImageUrl      = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Deep Work by Cal Newport",
                Description   = "Rules for focused success in a distracted world. Cal Newport argues that the ability to perform deep work is becoming rare and increasingly valuable.",
                Price         = 13.99m,
                Category      = "Books",
                StockQuantity = 110,
                ImageUrl      = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Sapiens by Yuval Noah Harari",
                Description   = "A brief history of humankind. From the Stone Age to the AI era, Harari explores how Homo sapiens came to rule the world.",
                Price         = 17.99m,
                Category      = "Books",
                StockQuantity = 130,
                ImageUrl      = "https://images.unsplash.com/photo-1535398089889-dd807df1dfaa?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "The Lean Startup by Eric Ries",
                Description   = "How today's entrepreneurs use continuous innovation to create radically successful businesses. The definitive guide for modern startups.",
                Price         = 15.99m,
                Category      = "Books",
                StockQuantity = 95,
                ImageUrl      = "https://images.unsplash.com/photo-1516979187457-637abb4f9353?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Clean Code by Robert C. Martin",
                Description   = "A handbook of agile software craftsmanship. Learn how to write code that is readable, maintainable, and efficient from Uncle Bob himself.",
                Price         = 39.99m,
                Category      = "Books",
                StockQuantity = 85,
                ImageUrl      = "https://images.unsplash.com/photo-1515378791036-0648a814c963?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Thinking, Fast and Slow by Daniel Kahneman",
                Description   = "Nobel laureate Daniel Kahneman's exploration of the two systems that drive the way we think — fast, intuitive thinking and slow, deliberate thinking.",
                Price         = 16.99m,
                Category      = "Books",
                StockQuantity = 100,
                ImageUrl      = "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=600&q=80"
            },

            // ── Sports & Fitness (8 products) ────────────────────────────
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Peloton Bike+ Indoor Exercise Bike",
                Description   = "Auto-Follow resistance, 24\" rotating HD touchscreen, Apple GymKit integration, and access to thousands of live and on-demand classes.",
                Price         = 2495.00m,
                Category      = "Sports & Fitness",
                StockQuantity = 8,
                ImageUrl      = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Bowflex SelectTech 552 Adjustable Dumbbells",
                Description   = "Replaces 15 sets of weights. Dial system adjusts from 5 to 52.5 lbs in 2.5 lb increments. Space-saving design for home gyms.",
                Price         = 429.00m,
                Category      = "Sports & Fitness",
                StockQuantity = 22,
                ImageUrl      = "https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Manduka PRO Yoga Mat",
                Description   = "6mm thick professional yoga mat with lifetime guarantee. Closed-cell surface prevents bacteria and moisture absorption. Superior cushioning.",
                Price         = 120.00m,
                Category      = "Sports & Fitness",
                StockQuantity = 50,
                ImageUrl      = "https://images.unsplash.com/photo-1601925228843-ad6a8803e897?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Garmin Forerunner 955 GPS Running Watch",
                Description   = "Solar-powered GPS running watch with full-color maps, advanced training metrics, triathlon mode, and up to 20 days battery life.",
                Price         = 499.99m,
                Category      = "Sports & Fitness",
                StockQuantity = 28,
                ImageUrl      = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "TRX All-in-One Suspension Training Kit",
                Description   = "Complete suspension training system used by military, pro athletes, and elite trainers. Includes anchor, door anchor, and workout guide.",
                Price         = 179.95m,
                Category      = "Sports & Fitness",
                StockQuantity = 35,
                ImageUrl      = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Wilson Evolution Basketball",
                Description   = "Official size and weight indoor game basketball with composite leather cover for exceptional feel and consistent dribble. Used in high school games nationwide.",
                Price         = 69.99m,
                Category      = "Sports & Fitness",
                StockQuantity = 60,
                ImageUrl      = "https://images.unsplash.com/photo-1546519638405-a9f83e7da2dd?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Hydro Flask 32oz Wide Mouth Water Bottle",
                Description   = "Double-wall vacuum insulation keeps drinks cold 24 hours and hot 12 hours. 18/8 pro-grade stainless steel, BPA-free, and dishwasher safe.",
                Price         = 44.95m,
                Category      = "Sports & Fitness",
                StockQuantity = 90,
                ImageUrl      = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=600&q=80"
            },
            new()
            {
                Id            = Guid.NewGuid(),
                Name          = "Theragun Pro Percussion Massage Gun",
                Description   = "Professional-grade percussive therapy device with 6 attachments, OLED screen, and QuietForce Technology. 60 lbs of force for deep muscle treatment.",
                Price         = 599.00m,
                Category      = "Sports & Fitness",
                StockQuantity = 18,
                ImageUrl      = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600&q=80"
            }
        };

        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync();
    }
}
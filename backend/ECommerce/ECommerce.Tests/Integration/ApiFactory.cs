using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Tests.Integration;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _db = $"tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Jwt:Key", "MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MDE=");
        builder.UseSetting("Jwt:Issuer", "ECommerce.Api");
        builder.UseSetting("Jwt:Audience", "ECommerce.Client");
        builder.UseSetting("Encryption:Key", "MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MDE=");
        builder.UseSetting("SeedAdmin:Email", ApiHelpers.AdminEmail);
        builder.UseSetting("SeedAdmin:Password", ApiHelpers.AdminPassword);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ECommerceDbContext>>();

            foreach (var d in services.Where(d =>
                         d.ServiceType.IsGenericType &&
                         d.ServiceType.GetGenericTypeDefinition().Name
                             .StartsWith("IDbContextOptionsConfiguration") &&
                         d.ServiceType.GenericTypeArguments[0] ==
                             typeof(ECommerceDbContext)).ToList())
            {
                services.Remove(d);
            }

            services.AddDbContext<ECommerceDbContext>(
                o => o.UseInMemoryDatabase(_db));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        DbInitializer.SeedAsync(host.Services, config).GetAwaiter().GetResult();

        return host;
    }
}
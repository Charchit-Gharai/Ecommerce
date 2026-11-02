
using System.Security.Claims;
using ECommerce.Api.Middleware;
using ECommerce.Core.Constants;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Services;
using ECommerce.Infrastructure.Authentication;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Payments;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());

    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string is missing.");

builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ECommerceDbContext>()
    .AddSignInManager();


var encodedJwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(encodedJwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Key, Issuer and Audience must be configured.");
}

byte[] jwtKeyBytes;

try
{
    jwtKeyBytes = Convert.FromBase64String(encodedJwtKey);
}
catch (FormatException ex)
{
    throw new InvalidOperationException(
        "JWT key must be Base64 encoded.", ex);
}

if (jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException(
        "JWT key must contain at least 32 random bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(jwtKeyBytes),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Admin));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole(AppRoles.Customer));
});


builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


builder.Services.AddScoped<
    IEncryptionService,
    AesEncryptionService>();


builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    ICartRepository,
    CartRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();


builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    ICartService,
    CartService>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();


builder.Services.AddScoped<
    IPaymentService,
    MockPaymentService>();

var app = builder.Build();


app.UseMiddleware<ExceptionHandlingMiddleware>();


app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


await DbInitializer.SeedAsync(
    app.Services,
    app.Configuration);

app.Run();

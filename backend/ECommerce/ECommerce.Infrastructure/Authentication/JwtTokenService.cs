using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Authentication;

public interface IJwtTokenService
{
    string GenerateToken(
        ApplicationUser user,
        IEnumerable<string> roles);

    string GenerateRefreshToken();

    int RefreshTokenLifetimeDays { get; }
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int RefreshTokenLifetimeDays =>
        _configuration.GetValue<int?>(
            "Jwt:RefreshTokenLifetimeDays") ?? 7;

    public string GenerateToken(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var encodedKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var expirationMinutes =
            _configuration.GetValue<int?>(
                "Jwt:ExpirationMinutes") ?? 15;

        if (string.IsNullOrWhiteSpace(encodedKey))
            throw new InvalidOperationException(
                "JWT signing key is missing.");

        byte[] keyBytes;

        try
        {
            keyBytes = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "JWT key must be Base64 encoded.", ex);
        }

        if (keyBytes.Length < 32)
            throw new InvalidOperationException(
                "JWT key must contain at least 32 random bytes.");

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT issuer or audience is missing.");
        }

        if (expirationMinutes < 1 || expirationMinutes > 60)
            throw new InvalidOperationException(
                "JWT expiration must be between 1 and 60 minutes.");

        var signingKey =
            new SymmetricSecurityKey(keyBytes);

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),

            new(ClaimTypes.NameIdentifier, user.Id),

            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(ClaimTypes.Name, user.FullName)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(
                new Claim(ClaimTypes.Email, user.Email));
        }

        foreach (var role in roles.Distinct())
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(bytes);
    }
}
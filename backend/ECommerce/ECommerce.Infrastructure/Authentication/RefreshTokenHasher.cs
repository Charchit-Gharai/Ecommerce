using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Infrastructure.Authentication;

public static class RefreshTokenHasher
{
    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

        byte[] hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }
}
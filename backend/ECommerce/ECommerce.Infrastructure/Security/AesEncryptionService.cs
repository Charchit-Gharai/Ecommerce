using System.Security.Cryptography;
using System.Text;
using ECommerce.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Security;

public sealed class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var encodedKey = configuration["Encryption:Key"];

        if (string.IsNullOrWhiteSpace(encodedKey))
        {
            throw new InvalidOperationException(
                "Encryption key is missing.");
        }

        try
        {
            _key = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "Encryption key must be Base64 encoded.", ex);
        }

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "AES-256 requires exactly 32 key bytes.");
        }
    }

    public string? Encrypt(string? plainText)
    {
        if (plainText is null)
            return null;

        if (plainText.Length == 0)
            return string.Empty;

        byte[] nonce =
            RandomNumberGenerator.GetBytes(12);

        byte[] plainBytes =
            Encoding.UTF8.GetBytes(plainText);

        byte[] cipherBytes =
            new byte[plainBytes.Length];

        byte[] tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);

        aes.Encrypt(
            nonce,
            plainBytes,
            cipherBytes,
            tag);

        return "v1." +
               Convert.ToBase64String(nonce) + "." +
               Convert.ToBase64String(tag) + "." +
               Convert.ToBase64String(cipherBytes);
    }

    public string? Decrypt(string? cipherText)
    {
        if (cipherText is null)
            return null;

        if (cipherText.Length == 0)
            return string.Empty;

        var parts = cipherText.Split('.');

        if (parts.Length != 4 || parts[0] != "v1")
        {
            throw new CryptographicException(
                "Invalid encrypted data.");
        }

        byte[] nonce =
            Convert.FromBase64String(parts[1]);

        byte[] tag =
            Convert.FromBase64String(parts[2]);

        byte[] cipherBytes =
            Convert.FromBase64String(parts[3]);

        if (nonce.Length != 12 || tag.Length != 16)
        {
            throw new CryptographicException(
                "Invalid AES-GCM payload.");
        }

        byte[] plainBytes =
            new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, tag.Length);

        aes.Decrypt(
            nonce,
            cipherBytes,
            tag,
            plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
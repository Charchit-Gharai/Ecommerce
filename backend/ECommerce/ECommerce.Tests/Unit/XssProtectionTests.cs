
using System.Text.Json;
using ECommerce.Core.Validation;

namespace ECommerce.Tests.Unit;

public class XssProtectionTests
{
    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("<svg onload=alert(1)>")]
    public void JsonSerialization_EncodesHtmlSensitiveCharacters(
        string maliciousInput)
    {
        var json = JsonSerializer.Serialize(maliciousInput);

        Assert.DoesNotContain(
            "<script",
            json,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "<img",
            json,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "<svg",
            json,
            StringComparison.OrdinalIgnoreCase);

        // JSON decoding must preserve the original text.
        var decoded = JsonSerializer.Deserialize<string>(json);

        Assert.Equal(maliciousInput, decoded);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("file:///etc/passwd")]
    public void DangerousImageUrls_AreRejected(string url)
    {
        var attribute = new HttpOrHttpsUrlAttribute();

        Assert.False(attribute.IsValid(url));
    }

    [Theory]
    [InlineData("https://example.com/product.jpg")]
    [InlineData("http://example.com/product.jpg")]
    public void HttpAndHttpsImageUrls_AreAccepted(string url)
    {
        var attribute = new HttpOrHttpsUrlAttribute();

        Assert.True(attribute.IsValid(url));
    }

    [Fact]
    public void NullImageUrl_IsAccepted()
    {
        var attribute = new HttpOrHttpsUrlAttribute();

        Assert.True(attribute.IsValid(null));
    }
}

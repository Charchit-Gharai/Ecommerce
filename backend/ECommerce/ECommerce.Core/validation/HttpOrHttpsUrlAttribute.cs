
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpOrHttpsUrlAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not string url ||
            string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(
                   url,
                   UriKind.Absolute,
                   out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp ||
                   uri.Scheme == Uri.UriSchemeHttps);
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid HTTP or HTTPS URL.";
    }
}

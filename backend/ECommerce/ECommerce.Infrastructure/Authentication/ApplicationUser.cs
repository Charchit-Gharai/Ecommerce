using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Infrastructure.Authentication;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }
}
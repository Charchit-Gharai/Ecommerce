using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
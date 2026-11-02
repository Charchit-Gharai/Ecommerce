using UserService.Models;

namespace UserService.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    int RefreshTokenLifetimeDays { get; }
}
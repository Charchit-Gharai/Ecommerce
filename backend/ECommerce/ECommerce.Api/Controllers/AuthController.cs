using System.Security.Claims;
using ECommerce.Core.DTOs;
using ECommerce.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService tokenService,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshCookieName = "shopkart_refresh";

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim();

        var existing =
            await userManager.FindByEmailAsync(email);

        if (existing is not null)
        {
            return BadRequest(new
            {
                message = "Email is already registered."
            });
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.Name.Trim()
        };

        var result = await userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(
                    e => e.Description)
            });
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user, "Customer");

        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);

            return StatusCode(500, new
            {
                message = "Could not assign Customer role."
            });
        }

        return Created(string.Empty, new
        {
            message = "Registration successful.",
            userId = user.Id,
            email = user.Email,
            name = user.FullName,
            role = "Customer"
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var result =
            await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(await IssueTokensAsync(user));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(
                RefreshCookieName, out var rawToken) ||
            string.IsNullOrWhiteSpace(rawToken))
        {
            return Unauthorized(new
            {
                message = "Refresh token is missing."
            });
        }

        var submittedHash =
            RefreshTokenHasher.Hash(rawToken);

        var user = await userManager.Users
            .SingleOrDefaultAsync(
                u => u.RefreshTokenHash == submittedHash);

        if (user is null)
        {
            ClearRefreshCookie();

            return Unauthorized(new
            {
                message = "Invalid refresh token."
            });
        }

        if (user.RefreshTokenExpiresAt is null ||
            user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;

            await userManager.UpdateAsync(user);

            ClearRefreshCookie();

            return Unauthorized(new
            {
                message = "Refresh token has expired."
            });
        }

        // Rotate old refresh token and issue a new token.
        return Ok(await IssueTokensAsync(user));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var user = await userManager.FindByIdAsync(
            userId ?? string.Empty);

        if (user is null)
            return Unauthorized();

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;

        await userManager.UpdateAsync(user);

        ClearRefreshCookie();

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        var user = await userManager.FindByIdAsync(
            userId ?? string.Empty);

        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            userId = user.Id,
            name = user.FullName,
            email = user.Email,
            role = roles.FirstOrDefault() ?? "Customer"
        });
    }

    private async Task<AuthResponse> IssueTokensAsync(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var accessToken =
            tokenService.GenerateToken(user, roles);

        var refreshToken =
            tokenService.GenerateRefreshToken();

        var expiresAt = DateTime.UtcNow.AddDays(
            tokenService.RefreshTokenLifetimeDays);

        user.RefreshTokenHash =
            RefreshTokenHasher.Hash(refreshToken);

        user.RefreshTokenExpiresAt = expiresAt;

        var updateResult =
            await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Could not save refresh token.");
        }

        var expirationMinutes =
            configuration.GetValue<int>(
                "Jwt:ExpirationMinutes", 15);

        Response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/api/auth"
            });

        return new AuthResponse
        {
            Token = accessToken,
            AccessTokenExpiresAt =
                DateTime.UtcNow.AddMinutes(expirationMinutes),
            UserId = user.Id,
            Name = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "Customer"
        };
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            });
    }
}
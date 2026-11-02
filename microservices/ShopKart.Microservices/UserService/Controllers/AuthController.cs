using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var email = request.Email.Trim();

        if (await userManager.FindByEmailAsync(email) is not null)
            return BadRequest(new
            {
                message = "Email is already registered."
            });

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.Name.Trim(),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });

        await userManager.AddToRoleAsync(user, "Customer");

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
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });

        var result = await signInManager
            .CheckPasswordSignInAsync(user, request.Password, true);

        if (!result.Succeeded)
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request)
    {
        var user = userManager.Users
            .FirstOrDefault(u =>
                u.RefreshToken == request.RefreshToken);

        if (user is null)
            return Unauthorized(new
            {
                message = "Invalid refresh token."
            });

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await userManager.UpdateAsync(user);

            return Unauthorized(new
            {
                message = "Refresh token expired. Please log in again."
            });
        }

        return Ok(await BuildAuthResponseAsync(user));
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await userManager.FindByIdAsync(userId ?? "");

        if (user is null) return Unauthorized();

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await userManager.UpdateAsync(user);

        return NoContent();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = tokenService.GenerateToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(
            tokenService.RefreshTokenLifetimeDays);

        for (int attempt = 0; attempt < 3; attempt++)
        {
            var fresh = await userManager.FindByIdAsync(user.Id);
            if (fresh is null) break;

            fresh.RefreshToken = refreshToken;
            fresh.RefreshTokenExpiresAt = expiresAt;

            var updateResult = await userManager.UpdateAsync(fresh);
            if (updateResult.Succeeded) break;
        }

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = expiresAt,
            UserId = user.Id,
            Name = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "Customer"
        };
    }
}
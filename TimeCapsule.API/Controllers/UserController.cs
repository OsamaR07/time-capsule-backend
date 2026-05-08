using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using TimeCapsule.Application.DTOs.Auth;
using TimeCapsule.Application.DTOs.User;
using TimeCapsule.Application.Interfaces;

namespace TimeCapsule.API.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var rawToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(rawToken))
            await _userService.RevokeRefreshTokenAsync(rawToken);

        Response.Cookies.Delete("refresh_token");
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();
        
        var userId = Guid.Parse(userIdStr);
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        var result = await _userService.UpdateProfileAsync(userId, request);
        return Ok(new { message = "Profile updated.", user = result });
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = Guid.Parse(userIdStr);
        await _userService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Password changed. Please log in again." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _userService.SendPasswordResetEmailAsync(request.Email);
        // Always return 200 regardless of whether email exists (anti-enumeration)
        return Ok(new { message = "If this email exists, a reset link has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(request);
        return Ok(new { message = "Password has been reset successfully." });
    }
}

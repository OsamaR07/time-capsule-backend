using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Enums;

namespace TimeCapsule.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ICapsuleService _capsuleService;

    public AdminController(ICapsuleService capsuleService)
    {
        _capsuleService = capsuleService;
    }

    [HttpGet("debug-claims")]
    [Authorize] // Any logged in user can see their own claims
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = claims,
            Roles = User.Claims.Where(c => c.Type == "roles" || c.Type.Contains("role")).Select(c => c.Value).ToList()
        });
    }

    [HttpGet("capsules")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public async Task<IActionResult> GetAllCapsules(
        [FromQuery] CapsuleStatus? status,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10)
    {
        var result = await _capsuleService.GetAllAdminAsync(status, page, size);
        return Ok(result);
    }

    [HttpGet("capsules/stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _capsuleService.GetStatsAsync();
        return Ok(stats);
    }
}

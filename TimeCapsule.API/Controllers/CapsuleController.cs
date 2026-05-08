using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using TimeCapsule.Application.DTOs.Capsule;
using TimeCapsule.Application.Interfaces;
using TimeCapsule.Domain.Enums;

namespace TimeCapsule.API.Controllers;

[ApiController]
[Route("api/capsules")]
[Authorize]
public class CapsuleController : ControllerBase
{
    private readonly ICapsuleService _capsuleService;

    public CapsuleController(ICapsuleService capsuleService)
    {
        _capsuleService = capsuleService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCapsuleRequest request)
    {
        var senderName = User.FindFirstValue("name")!;
        var senderEmail = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? User.FindFirstValue(ClaimTypes.Email)!;
        var result = await _capsuleService.CreateAsync(
            CurrentUserId, senderName, senderEmail, request);
        return StatusCode(201, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetSent(
        [FromQuery] CapsuleStatus? status,
        [FromQuery] int page = 0,
        [FromQuery] int size = 10)
    {
        var result = await _capsuleService.GetSentAsync(CurrentUserId, status, page, size);
        return Ok(result);
    }

    [HttpGet("received")]
    public async Task<IActionResult> GetReceived(
        [FromQuery] int page = 0, [FromQuery] int size = 10)
    {
        var result = await _capsuleService.GetReceivedAsync(CurrentUserId, page, size);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var isAdmin = User.IsInRole("ROLE_ADMIN");
        var result = await _capsuleService.GetByIdAsync(id, CurrentUserId, isAdmin);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCapsuleRequest request)
    {
        var result = await _capsuleService.UpdateAsync(id, CurrentUserId, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _capsuleService.CancelAsync(id, CurrentUserId);
        return Ok(new { message = "Capsule cancelled successfully." });
    }
}

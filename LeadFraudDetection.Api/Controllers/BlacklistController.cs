using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services.Blacklist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeadFraudDetection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BlacklistController : ControllerBase
{
    private readonly IBlacklistService _blacklistService;
    private readonly ILogger<BlacklistController> _logger;

    public BlacklistController(IBlacklistService blacklistService, ILogger<BlacklistController> logger)
    {
        _blacklistService = blacklistService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlacklistEntry>>> GetBlacklist()
    {
        var entries = await _blacklistService.GetAllActiveEntriesAsync();
        return Ok(entries);
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> CheckBlacklist([FromQuery] string type, [FromQuery] string value)
    {
        var isBlacklisted = await _blacklistService.IsBlacklistedAsync(type, value);
        return Ok(new { isBlacklisted, type, value });
    }

    [HttpPost]
    public async Task<ActionResult<BlacklistEntry>> AddEntry(BlacklistEntry entry)
    {
        await _blacklistService.AddBlacklistEntryAsync(entry);
        return CreatedAtAction(nameof(GetBlacklist), entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveEntry(int id)
    {
        await _blacklistService.RemoveBlacklistEntryAsync(id);
        return NoContent();
    }
}

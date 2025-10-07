using LeadFraudDetection.Api.Data;
using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeadFraudDetection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly FraudDetectionDbContext _context;
    private readonly IFraudDetectionService _fraudDetectionService;
    private readonly ILogger<LeadsController> _logger;

    public LeadsController(
        FraudDetectionDbContext context,
        IFraudDetectionService fraudDetectionService,
        ILogger<LeadsController> logger)
    {
        _context = context;
        _fraudDetectionService = fraudDetectionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Lead>>> GetLeads()
    {
        return await _context.Leads.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Lead>> GetLead(int id)
    {
        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
        {
            return NotFound();
        }

        return lead;
    }

    [HttpPost]
    public async Task<ActionResult<Lead>> CreateLead(Lead lead)
    {
        _context.Leads.Add(lead);
        await _context.SaveChangesAsync();

        var fraudResult = await _fraudDetectionService.DetectFraudAsync(lead);

        _logger.LogInformation(
            "Lead {LeadId} created with fraud detection result: {IsFraudulent}",
            lead.Id, fraudResult.IsFraudulent);

        return CreatedAtAction(nameof(GetLead), new { id = lead.Id }, lead);
    }

    [HttpPost("{id}/check-fraud")]
    public async Task<ActionResult<FraudDetectionResult>> CheckFraud(int id)
    {
        var lead = await _context.Leads.FindAsync(id);

        if (lead == null)
        {
            return NotFound();
        }

        var result = await _fraudDetectionService.DetectFraudAsync(lead);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLead(int id)
    {
        var lead = await _context.Leads.FindAsync(id);
        if (lead == null)
        {
            return NotFound();
        }

        _context.Leads.Remove(lead);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

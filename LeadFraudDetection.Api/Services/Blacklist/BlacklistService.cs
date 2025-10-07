using LeadFraudDetection.Api.Data;
using LeadFraudDetection.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeadFraudDetection.Api.Services.Blacklist;

public class BlacklistService : IBlacklistService
{
    private readonly FraudDetectionDbContext _context;
    private readonly ILogger<BlacklistService> _logger;

    public BlacklistService(FraudDetectionDbContext context, ILogger<BlacklistService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsBlacklistedAsync(string type, string value)
    {
        return await _context.BlacklistEntries
            .AnyAsync(e => e.Type == type && e.Value == value && e.IsActive);
    }

    public async Task<BlacklistEntry?> GetBlacklistEntryAsync(string type, string value)
    {
        return await _context.BlacklistEntries
            .FirstOrDefaultAsync(e => e.Type == type && e.Value == value && e.IsActive);
    }

    public async Task<IEnumerable<BlacklistEntry>> GetAllActiveEntriesAsync()
    {
        return await _context.BlacklistEntries
            .Where(e => e.IsActive)
            .ToListAsync();
    }

    public async Task AddBlacklistEntryAsync(BlacklistEntry entry)
    {
        _context.BlacklistEntries.Add(entry);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added blacklist entry: {Type} - {Value}", entry.Type, entry.Value);
    }

    public async Task RemoveBlacklistEntryAsync(int id)
    {
        var entry = await _context.BlacklistEntries.FindAsync(id);
        if (entry != null)
        {
            entry.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Removed blacklist entry: {Id}", id);
        }
    }
}

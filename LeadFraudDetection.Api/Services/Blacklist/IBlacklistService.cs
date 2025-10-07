using LeadFraudDetection.Api.Models;

namespace LeadFraudDetection.Api.Services.Blacklist;

public interface IBlacklistService
{
    Task<bool> IsBlacklistedAsync(string type, string value);
    Task<BlacklistEntry?> GetBlacklistEntryAsync(string type, string value);
    Task<IEnumerable<BlacklistEntry>> GetAllActiveEntriesAsync();
    Task AddBlacklistEntryAsync(BlacklistEntry entry);
    Task RemoveBlacklistEntryAsync(int id);
}

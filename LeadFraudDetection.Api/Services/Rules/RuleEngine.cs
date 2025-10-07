using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services.Blacklist;
using System.Text.RegularExpressions;

namespace LeadFraudDetection.Api.Services.Rules;

public class RuleEngine : IRuleEngine
{
    private readonly IBlacklistService _blacklistService;
    private readonly ILogger<RuleEngine> _logger;

    public RuleEngine(IBlacklistService blacklistService, ILogger<RuleEngine> logger)
    {
        _blacklistService = blacklistService;
        _logger = logger;
    }

    public async Task<RuleResult> EvaluateAsync(Lead lead)
    {
        var result = new RuleResult { Score = 0 };

        await CheckBlacklistRulesAsync(lead, result);
        CheckPatternRules(lead, result);
        CheckBehavioralRules(lead, result);

        result.IsFraudulent = result.Score >= 70;

        _logger.LogInformation(
            "Rule evaluation for lead {LeadId}: Score={Score}, IsFraudulent={IsFraudulent}, Rules={Rules}",
            lead.Id, result.Score, result.IsFraudulent, string.Join(", ", result.TriggeredRules));

        return result;
    }

    private async Task CheckBlacklistRulesAsync(Lead lead, RuleResult result)
    {
        if (await _blacklistService.IsBlacklistedAsync("Email", lead.Email))
        {
            result.Score += 100;
            result.TriggeredRules.Add("Blacklisted Email");
        }

        if (await _blacklistService.IsBlacklistedAsync("IP", lead.IpAddress))
        {
            result.Score += 100;
            result.TriggeredRules.Add("Blacklisted IP");
        }

        if (await _blacklistService.IsBlacklistedAsync("Phone", lead.PhoneNumber))
        {
            result.Score += 100;
            result.TriggeredRules.Add("Blacklisted Phone");
        }
    }

    private void CheckPatternRules(Lead lead, RuleResult result)
    {
        if (IsDisposableEmail(lead.Email))
        {
            result.Score += 50;
            result.TriggeredRules.Add("Disposable Email");
        }

        if (IsSuspiciousName(lead.FirstName) || IsSuspiciousName(lead.LastName))
        {
            result.Score += 30;
            result.TriggeredRules.Add("Suspicious Name Pattern");
        }

        if (string.IsNullOrWhiteSpace(lead.Company))
        {
            result.Score += 10;
            result.TriggeredRules.Add("Missing Company");
        }

        if (HasRepeatingPatterns(lead.Email))
        {
            result.Score += 20;
            result.TriggeredRules.Add("Repeating Pattern in Email");
        }
    }

    private void CheckBehavioralRules(Lead lead, RuleResult result)
    {
        if (string.IsNullOrWhiteSpace(lead.UserAgent))
        {
            result.Score += 15;
            result.TriggeredRules.Add("Missing User Agent");
        }

        if (lead.Email.Contains("+") && lead.Email.Contains("@"))
        {
            result.Score += 10;
            result.TriggeredRules.Add("Email Plus Addressing");
        }
    }

    private bool IsDisposableEmail(string email)
    {
        var disposableDomains = new[] { "tempmail.com", "guerrillamail.com", "10minutemail.com", "throwaway.email" };
        return disposableDomains.Any(domain => email.EndsWith($"@{domain}", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsSuspiciousName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return name.Length < 2 || Regex.IsMatch(name, @"^(.)\1+$") || Regex.IsMatch(name, @"\d{3,}");
    }

    private bool HasRepeatingPatterns(string text)
    {
        return Regex.IsMatch(text, @"(.{2,})\1{2,}");
    }
}

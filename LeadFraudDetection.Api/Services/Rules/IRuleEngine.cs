using LeadFraudDetection.Api.Models;

namespace LeadFraudDetection.Api.Services.Rules;

public interface IRuleEngine
{
    Task<RuleResult> EvaluateAsync(Lead lead);
}

public class RuleResult
{
    public bool IsFraudulent { get; set; }
    public double Score { get; set; }
    public List<string> TriggeredRules { get; set; } = new();
}

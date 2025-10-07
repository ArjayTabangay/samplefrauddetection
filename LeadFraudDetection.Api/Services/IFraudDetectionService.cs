using LeadFraudDetection.Api.Models;

namespace LeadFraudDetection.Api.Services;

public interface IFraudDetectionService
{
    Task<FraudDetectionResult> DetectFraudAsync(Lead lead);
}

public class FraudDetectionResult
{
    public bool IsFraudulent { get; set; }
    public double FinalScore { get; set; }
    public double RuleScore { get; set; }
    public double MLScore { get; set; }
    public List<string> Reasons { get; set; } = new();
}

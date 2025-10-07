using LeadFraudDetection.Api.Data;
using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services.ML;
using LeadFraudDetection.Api.Services.Rules;

namespace LeadFraudDetection.Api.Services;

public class FraudDetectionService : IFraudDetectionService
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IMLFraudDetectionService _mlService;
    private readonly FraudDetectionDbContext _context;
    private readonly ILogger<FraudDetectionService> _logger;

    public FraudDetectionService(
        IRuleEngine ruleEngine,
        IMLFraudDetectionService mlService,
        FraudDetectionDbContext context,
        ILogger<FraudDetectionService> logger)
    {
        _ruleEngine = ruleEngine;
        _mlService = mlService;
        _context = context;
        _logger = logger;
    }

    public async Task<FraudDetectionResult> DetectFraudAsync(Lead lead)
    {
        var ruleResult = await _ruleEngine.EvaluateAsync(lead);

        var mlResult = await _mlService.PredictAsync(lead);

        var finalScore = (ruleResult.Score * 0.6) + (mlResult.Score * 0.4);

        var result = new FraudDetectionResult
        {
            IsFraudulent = finalScore >= 70 || ruleResult.IsFraudulent,
            FinalScore = finalScore,
            RuleScore = ruleResult.Score,
            MLScore = mlResult.Score,
            Reasons = ruleResult.TriggeredRules
        };

        if (mlResult.IsFraud)
        {
            result.Reasons.Add($"ML Model Prediction (Confidence: {mlResult.Probability:P})");
        }

        var fraudScore = new FraudScore
        {
            LeadId = lead.Id,
            Score = finalScore,
            Reason = string.Join("; ", result.Reasons),
            CreatedAt = DateTime.UtcNow
        };

        _context.FraudScores.Add(fraudScore);

        lead.IsFraud = result.IsFraudulent;
        lead.FraudScore = finalScore;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Fraud detection completed for lead {LeadId}: IsFraudulent={IsFraudulent}, FinalScore={FinalScore}",
            lead.Id, result.IsFraudulent, finalScore);

        return result;
    }
}

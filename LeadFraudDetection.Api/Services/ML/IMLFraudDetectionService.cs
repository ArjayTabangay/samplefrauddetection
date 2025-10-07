using LeadFraudDetection.Api.Models;

namespace LeadFraudDetection.Api.Services.ML;

public interface IMLFraudDetectionService
{
    Task<MLPredictionResult> PredictAsync(Lead lead);
    Task TrainModelAsync(IEnumerable<Lead> trainingData);
    Task<bool> IsModelTrainedAsync();
}

public class MLPredictionResult
{
    public bool IsFraud { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}

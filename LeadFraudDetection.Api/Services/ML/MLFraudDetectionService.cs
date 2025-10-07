using LeadFraudDetection.Api.Models;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace LeadFraudDetection.Api.Services.ML;

public class MLFraudDetectionService : IMLFraudDetectionService
{
    private readonly MLContext _mlContext;
    private readonly ILogger<MLFraudDetectionService> _logger;
    private ITransformer? _model;
    private readonly string _modelPath;

    public MLFraudDetectionService(ILogger<MLFraudDetectionService> logger, IWebHostEnvironment env)
    {
        _mlContext = new MLContext(seed: 0);
        _logger = logger;
        _modelPath = Path.Combine(env.ContentRootPath, "MLModels", "fraud_model.zip");
        LoadModelIfExists();
    }

    public async Task<MLPredictionResult> PredictAsync(Lead lead)
    {
        if (_model == null)
        {
            _logger.LogWarning("ML model not trained. Returning default prediction.");
            return new MLPredictionResult { IsFraud = false, Probability = 0, Score = 0 };
        }

        var leadData = new LeadData
        {
            Email = lead.Email,
            PhoneNumber = lead.PhoneNumber,
            IpAddress = lead.IpAddress,
            UserAgent = lead.UserAgent,
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Company = lead.Company
        };

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<LeadData, LeadPrediction>(_model);
        var prediction = await Task.Run(() => predictionEngine.Predict(leadData));

        return new MLPredictionResult
        {
            IsFraud = prediction.Prediction,
            Probability = prediction.Probability,
            Score = prediction.Score
        };
    }

    public async Task TrainModelAsync(IEnumerable<Lead> trainingData)
    {
        _logger.LogInformation("Starting ML model training with {Count} samples", trainingData.Count());

        var data = trainingData.Select(l => new LeadData
        {
            Email = l.Email,
            PhoneNumber = l.PhoneNumber,
            IpAddress = l.IpAddress,
            UserAgent = l.UserAgent,
            FirstName = l.FirstName,
            LastName = l.LastName,
            Company = l.Company,
            Label = l.IsFraud
        });

        var dataView = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Text.FeaturizeText("EmailFeatures", nameof(LeadData.Email))
            .Append(_mlContext.Transforms.Text.FeaturizeText("PhoneFeatures", nameof(LeadData.PhoneNumber)))
            .Append(_mlContext.Transforms.Text.FeaturizeText("IpFeatures", nameof(LeadData.IpAddress)))
            .Append(_mlContext.Transforms.Text.FeaturizeText("UserAgentFeatures", nameof(LeadData.UserAgent)))
            .Append(_mlContext.Transforms.Text.FeaturizeText("FirstNameFeatures", nameof(LeadData.FirstName)))
            .Append(_mlContext.Transforms.Text.FeaturizeText("LastNameFeatures", nameof(LeadData.LastName)))
            .Append(_mlContext.Transforms.Text.FeaturizeText("CompanyFeatures", nameof(LeadData.Company)))
            .Append(_mlContext.Transforms.Concatenate("Features",
                "EmailFeatures", "PhoneFeatures", "IpFeatures", "UserAgentFeatures",
                "FirstNameFeatures", "LastNameFeatures", "CompanyFeatures"))
            .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: "Label",
                featureColumnName: "Features",
                numberOfLeaves: 20,
                numberOfTrees: 100));

        _model = await Task.Run(() => pipeline.Fit(dataView));

        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath)!);
        _mlContext.Model.Save(_model, dataView.Schema, _modelPath);

        _logger.LogInformation("ML model training completed and saved to {ModelPath}", _modelPath);
    }

    public Task<bool> IsModelTrainedAsync()
    {
        return Task.FromResult(_model != null);
    }

    private void LoadModelIfExists()
    {
        if (File.Exists(_modelPath))
        {
            try
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
                _logger.LogInformation("ML model loaded from {ModelPath}", _modelPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load ML model from {ModelPath}", _modelPath);
            }
        }
    }
}

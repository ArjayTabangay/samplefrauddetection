using Microsoft.ML.Data;

namespace LeadFraudDetection.Api.Services.ML;

public class LeadData
{
    [LoadColumn(0)]
    public string Email { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string PhoneNumber { get; set; } = string.Empty;

    [LoadColumn(2)]
    public string IpAddress { get; set; } = string.Empty;

    [LoadColumn(3)]
    public string UserAgent { get; set; } = string.Empty;

    [LoadColumn(4)]
    public string FirstName { get; set; } = string.Empty;

    [LoadColumn(5)]
    public string LastName { get; set; } = string.Empty;

    [LoadColumn(6)]
    public string Company { get; set; } = string.Empty;

    [LoadColumn(7)]
    public bool Label { get; set; }
}

public class LeadPrediction
{
    [ColumnName("PredictedLabel")]
    public bool Prediction { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}

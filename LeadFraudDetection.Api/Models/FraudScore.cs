namespace LeadFraudDetection.Api.Models;

public class FraudScore
{
    public int Id { get; set; }
    public int LeadId { get; set; }
    public double Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Lead Lead { get; set; } = null!;
}

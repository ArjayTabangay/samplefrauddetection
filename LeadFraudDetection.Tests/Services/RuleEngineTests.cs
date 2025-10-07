using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services.Blacklist;
using LeadFraudDetection.Api.Services.Rules;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadFraudDetection.Tests.Services;

public class RuleEngineTests
{
    [Fact]
    public async Task EvaluateAsync_ReturnsHighScore_ForBlacklistedEmail()
    {
        var blacklistService = new Mock<IBlacklistService>();
        blacklistService.Setup(s => s.IsBlacklistedAsync("Email", It.IsAny<string>()))
            .ReturnsAsync(true);
        blacklistService.Setup(s => s.IsBlacklistedAsync("IP", It.IsAny<string>()))
            .ReturnsAsync(false);
        blacklistService.Setup(s => s.IsBlacklistedAsync("Phone", It.IsAny<string>()))
            .ReturnsAsync(false);

        var logger = Mock.Of<ILogger<RuleEngine>>();
        var ruleEngine = new RuleEngine(blacklistService.Object, logger);

        var lead = new Lead
        {
            Email = "fraud@example.com",
            PhoneNumber = "1234567890",
            IpAddress = "192.168.1.1",
            FirstName = "John",
            LastName = "Doe",
            Company = "Test Corp"
        };

        var result = await ruleEngine.EvaluateAsync(lead);

        Assert.True(result.Score >= 70);
        Assert.True(result.IsFraudulent);
        Assert.Contains("Blacklisted Email", result.TriggeredRules);
    }

    [Fact]
    public async Task EvaluateAsync_DetectsDisposableEmail()
    {
        var blacklistService = new Mock<IBlacklistService>();
        blacklistService.Setup(s => s.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var logger = Mock.Of<ILogger<RuleEngine>>();
        var ruleEngine = new RuleEngine(blacklistService.Object, logger);

        var lead = new Lead
        {
            Email = "test@tempmail.com",
            PhoneNumber = "1234567890",
            IpAddress = "192.168.1.1",
            FirstName = "John",
            LastName = "Doe",
            Company = "Test Corp",
            UserAgent = "Mozilla/5.0"
        };

        var result = await ruleEngine.EvaluateAsync(lead);

        Assert.Contains("Disposable Email", result.TriggeredRules);
    }

    [Fact]
    public async Task EvaluateAsync_DetectsMissingCompany()
    {
        var blacklistService = new Mock<IBlacklistService>();
        blacklistService.Setup(s => s.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var logger = Mock.Of<ILogger<RuleEngine>>();
        var ruleEngine = new RuleEngine(blacklistService.Object, logger);

        var lead = new Lead
        {
            Email = "test@example.com",
            PhoneNumber = "1234567890",
            IpAddress = "192.168.1.1",
            FirstName = "John",
            LastName = "Doe",
            Company = "",
            UserAgent = "Mozilla/5.0"
        };

        var result = await ruleEngine.EvaluateAsync(lead);

        Assert.Contains("Missing Company", result.TriggeredRules);
    }

    [Fact]
    public async Task EvaluateAsync_LowScore_ForValidLead()
    {
        var blacklistService = new Mock<IBlacklistService>();
        blacklistService.Setup(s => s.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var logger = Mock.Of<ILogger<RuleEngine>>();
        var ruleEngine = new RuleEngine(blacklistService.Object, logger);

        var lead = new Lead
        {
            Email = "valid@company.com",
            PhoneNumber = "1234567890",
            IpAddress = "192.168.1.1",
            FirstName = "John",
            LastName = "Doe",
            Company = "Valid Company",
            UserAgent = "Mozilla/5.0"
        };

        var result = await ruleEngine.EvaluateAsync(lead);

        Assert.False(result.IsFraudulent);
        Assert.True(result.Score < 70);
    }
}

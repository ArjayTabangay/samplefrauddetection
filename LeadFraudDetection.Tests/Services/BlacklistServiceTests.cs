using LeadFraudDetection.Api.Data;
using LeadFraudDetection.Api.Models;
using LeadFraudDetection.Api.Services.Blacklist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LeadFraudDetection.Tests.Services;

public class BlacklistServiceTests
{
    private FraudDetectionDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FraudDetectionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FraudDetectionDbContext(options);
    }

    [Fact]
    public async Task IsBlacklistedAsync_ReturnsTrue_WhenEntryExists()
    {
        var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<BlacklistService>>();
        var service = new BlacklistService(context, logger);

        await service.AddBlacklistEntryAsync(new BlacklistEntry
        {
            Type = "Email",
            Value = "fraud@example.com",
            Reason = "Known fraudster",
            IsActive = true
        });

        var result = await service.IsBlacklistedAsync("Email", "fraud@example.com");

        Assert.True(result);
    }

    [Fact]
    public async Task IsBlacklistedAsync_ReturnsFalse_WhenEntryDoesNotExist()
    {
        var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<BlacklistService>>();
        var service = new BlacklistService(context, logger);

        var result = await service.IsBlacklistedAsync("Email", "safe@example.com");

        Assert.False(result);
    }

    [Fact]
    public async Task AddBlacklistEntryAsync_AddsEntry()
    {
        var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<BlacklistService>>();
        var service = new BlacklistService(context, logger);

        var entry = new BlacklistEntry
        {
            Type = "IP",
            Value = "192.168.1.1",
            Reason = "Suspicious activity",
            IsActive = true
        };

        await service.AddBlacklistEntryAsync(entry);

        var result = await service.IsBlacklistedAsync("IP", "192.168.1.1");
        Assert.True(result);
    }

    [Fact]
    public async Task RemoveBlacklistEntryAsync_DeactivatesEntry()
    {
        var context = GetInMemoryDbContext();
        var logger = Mock.Of<ILogger<BlacklistService>>();
        var service = new BlacklistService(context, logger);

        var entry = new BlacklistEntry
        {
            Type = "Phone",
            Value = "1234567890",
            Reason = "Test",
            IsActive = true
        };

        context.BlacklistEntries.Add(entry);
        await context.SaveChangesAsync();

        await service.RemoveBlacklistEntryAsync(entry.Id);

        var result = await service.IsBlacklistedAsync("Phone", "1234567890");
        Assert.False(result);
    }
}

using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Models;
using AccessRequestHub.Api.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AccessRequestHub.Api.Tests;

public class AccessRequestServiceTests
{
    private AppDbContext GetInMemoryDbContext()
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // <-- Add this line
        .Options;

    return new AppDbContext(options);
}

    [Fact]
    public async Task CreateRequest_StandardVsHighRisk_RoutesCorrectly()
    {
        using var context = GetInMemoryDbContext();
        var service = new AccessRequestService(context);

        var standardDto = new CreateRequestDto(Guid.NewGuid(), "CRM", "NonProduction", "Read", "Standard Access Test");
        var highRiskDto = new CreateRequestDto(Guid.NewGuid(), "CRM", "Production", "Read", "High Risk Access Test");

        var (stdReq, _) = await service.CreateRequestAsync(standardDto, "alice@example.local");
        var (hrReq, _) = await service.CreateRequestAsync(highRiskDto, "alice@example.local");

        // Pass Version directly into the constructor
        var stdResult = await service.ApproveRequestAsync(stdReq.Id, new ApproveRequestDto(stdReq.Version), "bob@example.local");
        var hrResult = await service.ApproveRequestAsync(hrReq.Id, new ApproveRequestDto(hrReq.Version), "bob@example.local");
        var hrUpdated = await context.AccessRequests.FindAsync(hrReq.Id);

        Assert.Equal("Success", stdResult);
        Assert.Equal("Approved", (await context.AccessRequests.FindAsync(stdReq.Id))!.Status);

        Assert.Equal("Success", hrResult);
        Assert.Equal("PendingSystemOwner", hrUpdated!.Status);
    }

    [Fact]
    public async Task CreateRequest_Idempotency_PreventsDuplicateRows()
    {
        using var context = GetInMemoryDbContext();
        var service = new AccessRequestService(context);
        var sharedClientId = Guid.NewGuid();

        var dto = new CreateRequestDto(sharedClientId, "CRM", "NonProduction", "Read", "Idempotency Test");

        var (req1, isDup1) = await service.CreateRequestAsync(dto, "alice@example.local");
        var (req2, isDup2) = await service.CreateRequestAsync(dto, "alice@example.local");

        Assert.False(isDup1);
        Assert.True(isDup2);
        Assert.Equal(req1.Id, req2.Id);
        Assert.Equal(1, await context.AccessRequests.CountAsync());
    }

    [Fact]
    public async Task ApproveRequest_StaleVersion_ReturnsConflict()
    {
        using var context = GetInMemoryDbContext();
        var service = new AccessRequestService(context);

        var dto = new CreateRequestDto(Guid.NewGuid(), "CRM", "NonProduction", "Read", "Concurrency Test");

        var (request, _) = await service.CreateRequestAsync(dto, "alice@example.local");
        var staleVersion = request.Version;

        // Pass Version directly into the constructor
        await service.ApproveRequestAsync(request.Id, new ApproveRequestDto(staleVersion), "bob@example.local");
        var conflictResult = await service.ApproveRequestAsync(request.Id, new ApproveRequestDto(staleVersion), "bob@example.local");

        Assert.Equal("Conflict", conflictResult);
    }

    [Fact]
    public async Task RejectRequest_WithoutReason_FailsWithReasonRequired()
    {
        using var context = GetInMemoryDbContext();
        var service = new AccessRequestService(context);

        var dto = new CreateRequestDto(Guid.NewGuid(), "CRM", "NonProduction", "Read", "Rejection Test");
        var (request, _) = await service.CreateRequestAsync(dto, "alice@example.local");

        // Pass Version and Reason directly into the constructor (assuming RejectRequestDto(Guid Version, string Reason))
        var result = await service.RejectRequestAsync(request.Id, new RejectRequestDto(request.Version, "   "), "bob@example.local");

        Assert.Equal("ReasonRequired", result);
    }
}
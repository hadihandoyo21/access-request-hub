using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Models; 
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Services;

public class AccessRequestService
{
    private readonly AppDbContext _context;

    public AccessRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(AccessRequest Request, bool IsDuplicate)> CreateRequestAsync(CreateRequestDto dto, string requesterEmail)
    {
    var existing = await _context.AccessRequests
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.ClientRequestId == dto.ClientRequestId
                               && r.Requester == requesterEmail);
    if (existing != null)
        return (existing, true);

    var newRequest = new AccessRequest
    {
        ClientRequestId = dto.ClientRequestId,
        Requester = requesterEmail,
        Application = dto.Application,
        Environment = dto.Environment,
        AccessLevel = dto.AccessLevel,
        Justification = dto.Justification.Trim(),
        Status = "PendingManager",
        PolicyVersion = "v1",
        Version = Guid.NewGuid(),
        CreatedAt = DateTime.UtcNow
    };

    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        _context.AccessRequests.Add(newRequest);
        await _context.SaveChangesAsync();

        _context.AuditLogs.Add(new AuditLog
        {
            RequestId = newRequest.Id,
            ActorEmail = requesterEmail,
            ActionType = "Created",
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return (newRequest, false);
    }
    catch (DbUpdateException)
    {
        await transaction.RollbackAsync();
        _context.ChangeTracker.Clear();  

        var winner = await _context.AccessRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ClientRequestId == dto.ClientRequestId
                                   && r.Requester == requesterEmail);
        if (winner != null)
            return (winner, true);

        throw; 
        }
    }
    public async Task<string> ApproveRequestAsync(int id, ApproveRequestDto dto, string actorEmail)
    {
        var request = await _context.AccessRequests.FindAsync(id);
        if (request == null) return "NotFound";

        if (request.Version != dto.Version) return "Conflict";

        string previousStatus = request.Status;
        string newStatus = previousStatus;

        if (previousStatus == "PendingManager")
        {
            bool isHighRisk = request.Environment == "Production" || request.AccessLevel == "Admin";
            newStatus = isHighRisk ? "PendingSystemOwner" : "Approved";
        }
        else if (previousStatus == "PendingSystemOwner")
        {
            newStatus = "Approved";
        }
        else
        {
            return "InvalidState";
        }

        request.Status = newStatus;
        request.Version = Guid.NewGuid(); 

        var auditLog = new AuditLog
        {
            RequestId = request.Id,
            ActorEmail = actorEmail,
            ActionType = $"Approved (Moved from {previousStatus} to {newStatus})",
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        
        try
        {
            await _context.SaveChangesAsync();
            return "Success";
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Conflict";
        }
    }

    public async Task<string> RejectRequestAsync(int id, RejectRequestDto dto, string actorEmail)
    {
        var request = await _context.AccessRequests.FindAsync(id);
        if (request == null) return "NotFound";

        if (request.Version != dto.Version) return "Conflict";

        if (string.IsNullOrWhiteSpace(dto.Reason)) return "ReasonRequired";

        request.Status = "Rejected";
        request.Version = Guid.NewGuid(); 

        var auditLog = new AuditLog
        {
            RequestId = request.Id,
            ActorEmail = actorEmail,
            ActionType = "Rejected",
            Reason = dto.Reason.Trim(), 
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        
        try
        {
            await _context.SaveChangesAsync();
            return "Success";
        }
        catch (DbUpdateConcurrencyException)
        {
            return "Conflict";
        }
    }
}
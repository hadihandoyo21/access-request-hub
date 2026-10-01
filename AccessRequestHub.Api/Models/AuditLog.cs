namespace AccessRequestHub.Api.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
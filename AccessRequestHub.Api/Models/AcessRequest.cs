using System.ComponentModel.DataAnnotations;

namespace AccessRequestHub.Api.Models;

public class AccessRequest
{
    public int Id { get; set; }
    public Guid ClientRequestId { get; set; }
    public string Requester { get; set; } = string.Empty;
    public string Application { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string Status { get; set; } = "PendingManager";
    public string PolicyVersion { get; set; } = "v1";
    
    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid(); 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
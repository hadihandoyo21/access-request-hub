using AccessRequestHub.Api.Data;
using AccessRequestHub.Api.DTOs;
using AccessRequestHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class AccessRequestsController : ControllerBase
{
    private readonly AccessRequestService _service;
    private readonly AppDbContext _context;

    public AccessRequestsController(AccessRequestService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }

    private string GetCurrentUser() => Request.Headers["X-Mock-User"].FirstOrDefault() ?? "alice@example.local";

    [HttpPost]
public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
{
    if (dto.ClientRequestId == Guid.Empty)
        return BadRequest(new { message = "ClientRequestId is required." });

    var user = GetCurrentUser();
    var (request, isDuplicate) = await _service.CreateRequestAsync(dto, user);

    if (isDuplicate && (request.Application != dto.Application
                     || request.Environment != dto.Environment
                     || request.AccessLevel != dto.AccessLevel
                     || request.Justification != dto.Justification.Trim()))
    {
        return Conflict(new { message = "ClientRequestId was already used with different data." });
    }

    return isDuplicate ? Ok(request) : StatusCode(201, request);
}
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _context.AccessRequests.ToListAsync();
        return Ok(requests);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApproveRequestDto dto)
    {
        string currentUserEmail = GetCurrentUser();

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        if (actor == null)
        {
            return StatusCode(403, new { message = "Unauthorized: Current mock user not found in system." });
        }

        var accessRequest = await _context.AccessRequests.FindAsync(id);
        if (accessRequest == null)
        {
            return NotFound(new { message = "Request not found." });
        }

        // 1. Check Terminal State
        if (accessRequest.Status == "Approved" || accessRequest.Status == "Rejected")
        {
            return BadRequest(new { message = "Cannot modify requests in a terminal state." });
        }

        // 2. Role-Based & State-Based Validation
        if (actor.Role == "Requester")
        {
            return StatusCode(403, new { message = "Authorization Error: Requesters cannot approve access requests." });
        }

        if (actor.Role == "Admin/Auditor")
        {
            return StatusCode(403, new { message = "Authorization Error: Admins/Auditors cannot act as direct approvers." });
        }

        if (actor.Role == "Manager")
        {
            if (accessRequest.Status != "PendingManager")
            {
                return StatusCode(403, new { message = "Authorization Error: Manager approval is only valid for requests in PendingManager state." });
            }
        }
        else if (actor.Role == "SystemOwner")
        {
            if (accessRequest.Status != "PendingSystemOwner")
            {
                return StatusCode(403, new { message = "Authorization Error: System Owner approval is only valid after Manager approval (PendingSystemOwner state)." });
            }

            if (!string.IsNullOrEmpty(actor.ManagedApp))
            {
                if (!accessRequest.Application.Equals(actor.ManagedApp, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(403, new { message = $"Authorization Error: You are only authorized to approve requests for {actor.ManagedApp}." });
                }
            }
        }

        var result = await _service.ApproveRequestAsync(id, dto, currentUserEmail);

        return result switch
        {
            "Success" => Ok(new { message = "Request approved successfully." }),
            "NotFound" => NotFound(new { message = "Request not found." }),
            "Conflict" => Conflict(new { message = "Concurrency conflict: This request has been modified by another user." }),
            "Forbidden" => StatusCode(403, new { message = "Protocol violation: You cannot approve your own request." }),
            "InvalidState" => BadRequest(new { message = "Invalid request state for approval." }),
            _ => BadRequest(new { message = "An error occurred while processing the approval." })
        };
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRequest(int id, [FromBody] RejectRequestDto dto)
    {
        string currentUserEmail = GetCurrentUser();

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
        if (actor == null)
        {
            return StatusCode(403, new { message = "Unauthorized: Current mock user not found in system." });
        }

        var accessRequest = await _context.AccessRequests.FindAsync(id);
        if (accessRequest == null)
        {
            return NotFound(new { message = "Request not found." });
        }

        // 1. Check Terminal State
        if (accessRequest.Status == "Approved" || accessRequest.Status == "Rejected")
        {
            return BadRequest(new { message = "Cannot modify requests in a terminal state." });
        }

        // 2. Role-Based & State-Based Validation
        if (actor.Role == "Requester")
        {
            return StatusCode(403, new { message = "Authorization Error: Requesters cannot reject access requests." });
        }

        if (actor.Role == "Manager")
        {
            if (accessRequest.Status != "PendingManager")
            {
                return StatusCode(403, new { message = "Authorization Error: Manager rejection is only valid for requests in PendingManager state." });
            }
        }
        else if (actor.Role == "SystemOwner")
        {
            if (accessRequest.Status != "PendingSystemOwner")
            {
                return StatusCode(403, new { message = "Authorization Error: System Owner rejection is only valid during the PendingSystemOwner state." });
            }

            if (!string.IsNullOrEmpty(actor.ManagedApp))
            {
                if (!accessRequest.Application.Equals(actor.ManagedApp, StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(403, new { message = $"Authorization Error: You are only authorized to manage requests for {actor.ManagedApp}." });
                }
            }
        }

        var result = await _service.RejectRequestAsync(id, dto, currentUserEmail);

        return result switch
        {
            "Success" => Ok(new { message = "Request rejected successfully." }),
            "NotFound" => NotFound(new { message = "Request not found." }),
            "Conflict" => Conflict(new { message = "Concurrency conflict: This request has been modified by another user." }),
            "ReasonRequired" => BadRequest(new { message = "A rejection reason is required." }),
            _ => BadRequest(new { message = "An error occurred while processing the rejection." })
        };
    }
}
namespace AccessRequestHub.Api.DTOs;

public record CreateRequestDto(
    Guid ClientRequestId,
    string Application,
    string Environment,
    string AccessLevel,
    string Justification
);

public record ApproveRequestDto(
    Guid Version
);

public record RejectRequestDto(
    Guid Version,
    string Reason
);
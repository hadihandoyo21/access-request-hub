# Self-Review & Production Readiness

| Finding | Severity | Action | Status | Evidence |
|---|---|---|---|---|
| Hardcoded requester email parameters in service methods | Medium | Introduce standard ASP.NET Core JWT Authentication middleware to extract actor identity securely from claims. | Deferred | Service methods currently accept `string requesterEmail` as an explicit parameter. |
| In-Memory database used for testing does not validate relational constraints | Low | Add integration tests using `WebApplicationFactory` with Testcontainers or LocalDB if strict relational checks are required. | Deferred / Handled | Current unit tests successfully validate service logic paths and state transitions. |
| Lack of global exception handling middleware | Low | Implement a centralized `IExceptionHandler` middleware to catch unhandled errors and format uniform problem details. | Resolved | Added standard error boundary configuration. |

## Known Limitations
- The project currently relies on an In-Memory database for unit testing, which does not enforce strict database triggers or complex relational constraints present in production SQL Server instances.

## Deferred Work
- End-to-end integration test suite using `WebApplicationFactory`.
- Advanced role-based authorization policies (`[Authorize(Roles = "...")]`).
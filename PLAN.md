# Assessment Plan - AccessRequestHub.Api

## 1. Problem Understanding
The goal is to build an Access Request Hub API that handles secure IT access requests, risk-based routing (Standard vs High Risk/Production environments), idempotency via client request IDs, and optimistic concurrency control using versioning, coupled with robust unit testing.

## 2. Architecture & Data Model
- **API Layer**: Controllers handling HTTP requests for creating, approving, and rejecting access requests.
- **Service Layer (`AccessRequestService`)**: Contains business logic, state transitions, idempotency checks, and audit logging.
- **Data Layer (`AppDbContext`)**: Entity Framework Core context managing two core tables:
  - `AccessRequests`: Tracks metadata, status (`Approved`, `PendingSystemOwner`, etc.), and version numbers.
  - `AuditLogs`: Records actions taken by actors for compliance.
- **DTOs**: Implemented as records (with primary constructors) for immutability and clean data transfer.

## 3. Implementation Order
1. **Setup**: Initialize .NET 10 solution, Web API project, and xUnit test project.
2. **Models & Context**: Define EF Core entities (`AccessRequest`, `AuditLog`) and `AppDbContext`.
3. **Core Service**: Implement `AccessRequestService` with idempotency and state routing logic.
4. **Testing**: Configure xUnit with EF Core In-Memory database, handling transaction warnings, and write tests for idempotency, risk routing, concurrency conflicts, and input validation.

## 4. Test Strategy
- Use xUnit as the test runner framework.
- Utilize `Microsoft.EntityFrameworkCore.InMemory` to mock the database layer independently of external infrastructure.
- Cover key scenarios: Idempotent duplicate creation, risk-based routing, stale-version concurrency conflicts, and missing rejection reasons.

## 5. Key Trade-offs
- **In-Memory DB vs. Relational DB**: In-memory speeds up unit test execution significantly, but lacks full relational foreign key constraint validation of SQL Server. Mitigated by keeping unit tests tightly scoped to service logic.
- **Optimistic Concurrency**: Used explicit version checking instead of database-level row versioning to keep the implementation portable across various database providers.

## 6. Plan Changes During Implementation
- Adjusted the EF Core test configuration to ignore `TransactionIgnoredWarning` because the in-memory store does not support explicit transactions used in service-level transaction scopes.
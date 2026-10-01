# Access Request Hub API & UI

A full-stack application for managing IT access requests, built with a .NET 10 API, an Angular 18 UI, and Entity Framework Core. It features risk-based routing, idempotent request creation, optimistic concurrency versioning, and automated unit tests.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Setup](#2-setup)
3. [Migrate & Seed the Database](#3-migrate--seed-the-database)
4. [Run the Application](#4-run-the-application)
5. [Run Tests & Code Coverage](#5-run-tests--code-coverage)
6. [Demo Users, Roles & Flow](#6-demo-users-roles--flow)
7. [Reproducibility & Compliance](#7-reproducibility--compliance)

---

## 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) and npm (for the Angular UI)
- [Angular CLI](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- Entity Framework Core CLI tools (`dotnet tool install --global dotnet-ef`)

## 2. Setup

1. Clone the repository and open a terminal at the root `accessRequest` folder.
2. Restore the backend dependencies:

   ```bash
   dotnet restore
   ```

3. Restore the frontend dependencies:

   ```bash
   cd access-request-ui
   npm install
   cd ..
   ```

## 3. Migrate & Seed the Database

Apply the Entity Framework migrations and seed data to create the database schema:

```bash
dotnet ef database update --project AccessRequestHub.Api
```

## 4. Run the Application

### Backend API

```bash
cd AccessRequestHub.Api
dotnet run
```

### Frontend UI (Angular)

In a separate terminal:

```bash
cd access-request-ui
ng serve
```

## 5. Run Tests & Code Coverage

Run all automated unit tests with Coverlet code coverage enabled:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 6. Demo Users, Roles & Flow

### Demo Actors & Roles

| User | Email | Role | Responsibility |
|---|---|---|---|
| Alice | `alice@example.local` | Requester | Submits standard and high-risk access requests |
| Bob | `bob@example.local` | Manager | Handles initial management review and standard approvals |
| Carol | `carol@example.local` | System Owner (CRM) | Approves requests for CRM resources |
| Dana | `dana@example.local` | System Owner (Finance Portal) | Approves requests for Finance Portal resources |
| Erin | `erin@example.local` | Admin/Auditor | Oversees audit logs and compliance tracking (cannot approve) |

The UI includes an "Active User" switcher that sends the selected user in the `X-Mock-User` header.

### Demo Flow

1. **Create request.** Alice submits an access request specifying an application, environment, access level and justification (justification is mandatory).
   - Standard request (`NonProduction` environment and `Read` access): after Manager approval, the request is `Approved`.
   - High-risk request (`Production` environment or `Admin` access): after Manager approval, the request moves to `PendingSystemOwner` and needs the relevant System Owner's sign-off.
2. **Idempotency.** Submitting the same `ClientRequestId` does not create a duplicate row. The existing record is returned instead.
3. **Approval / Rejection.** Authorized users review requests using the current `version` value. A stale version triggers a concurrency conflict, and a rejection requires a reason. Each action is recorded in the audit log.

### Request Status Flow

```
PendingManager ──(Manager approves, standard)──────────► Approved
PendingManager ──(Manager approves, high risk)─────────► PendingSystemOwner ──(System Owner approves)──► Approved
PendingManager / PendingSystemOwner ──(reject + reason)─► Rejected
```

`Approved` and `Rejected` are terminal states and cannot be modified.

## 7. Reproducibility & Compliance

- **Database:** fully reproducible from Entity Framework migrations and seed configuration.
- **Automated tests:** run and verified with the standard `dotnet test` command.
- **Security:** no hardcoded production credentials, tokens, or sensitive corporate data.
- **Deferred work:** documented in detail in [`REVIEW.md`](./REVIEW.md).
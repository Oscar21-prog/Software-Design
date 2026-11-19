# Architecture & Technical Decisions — E-Bank (Team España)

**Author:** Óscar Marín (@Oscar21-prog)  
**Date:** 2026-11-20  
**Status:** Confirmed (Baseline for Lab 2)

## 1. Architectural Style: Modular Monolith
Although the E-Bank technical specification outlines 8 independent services, Team España implements the system as a **Modular Monolith** deployed as a single process (`EBank.Host`). All module boundaries and isolation rules from the specification are strictly preserved:

- **Schema Isolation:** PostgreSQL 16 hosts a single database (`ebank`) partitioned into 8 dedicated schemas (`kyc`, `crm`, `cbs`, `cards`, `loans`, `accounting`, `aml`, `notification`). Cross-schema queries or foreign keys are prohibited.
- **Synchronous Communication:** Spec `/internal/<module>/...` REST calls are mapped to C# interfaces exposed exclusively via each module's `*.Contracts` assembly. Modules cannot reference another module's domain or infrastructure assemblies.
- **Asynchronous Communication:** Modules publish integration events via an in-process `IEventBus` backed by a **Transactional Outbox** pattern. Consumers enforce idempotency using an `inbox` table tracking processed `eventId`s.
- **Internal Layering:** Each module follows Clean/Layered architecture (`Api -> Application -> Domain`, with `Infrastructure` implementing `Application` interfaces). The `Domain` layer has zero dependencies on EF Core, ASP.NET, or external modules.

## 2. Confirmed Technology Stack
- **Backend:** C# / .NET 10 (LTS), ASP.NET Core Web API (Controllers).
- **Database & ORM:** PostgreSQL 16, Entity Framework Core 10 (Npgsql provider), independent migrations per module.
- **Frontend:** Single React + TypeScript + Vite SPA serving `/app` (customer, mobile-first ≥ 360px) and `/backoffice` (employees), localized in LT/EN via `i18next`.
- **Testing:** xUnit, FluentAssertions, `WebApplicationFactory`, and `Testcontainers` (PostgreSQL).
- **Local Infrastructure:** Docker Compose orchestrating PostgreSQL 16.

## 3. Shared Infrastructure (`BuildingBlocks`)
`EBank.BuildingBlocks` centralizes cross-cutting primitives:
- `Money`: Immutable value object backed by `decimal` (`numeric(18,2)`), enforcing non-negative balances and serializing to JSON as two-decimal strings (e.g. `"125.00"`).
- `IClock`: Abstraction over `DateTimeOffset.UtcNow` for deterministic time-based testing.
- `ErrorResponse`: Global exception middleware translating `EBankException` into the mandatory `{"error": {"code": "UPPER_SNAKE_CASE", "message": "..."}}` payload.
- `IEventBus`, `OutboxMessage`, `InboxMessage`: Event envelope and reliability primitives.
- `IdempotencyRecord` & `AuditLogEntry`: Shared entities for idempotent mutations and append-only audit trails.

# Implementation Proposal — E-Bank (Team España)

**Author:** Víctor Castañeda (@vicctoor18)
**Date:** 2026-11-28
**Status:** v1.0
**Specification:** *E-Bank Technical Specification* by Fantastic 5 (`E-Bank_Technical_Specification.txt`; diagrams and wireframes in the original PDF)

## 1. Purpose

This document states what Team España will implement from the E-Bank specification, which external systems are replaced by mocks, and where the implementation deviates from the design. It matches the scope we sent to the supervisor (Vasilij Savin): all **Must** and **Should** features of the eight modules, **Could** features if time allows, external systems mocked as the specification allows (§1.4), and every deviation documented.

Related documents:

| Document | Content | Owner |
|---|---|---|
| [`architecture.md`](architecture.md) | Architecture style, stack, shared building blocks | Óscar Marín |
| [`decisions.md`](decisions.md) | Interpretations of unclear or contradictory spec points | Jaime de Luiz |
| [`screens.md`](screens.md) | Which wireframes are built as screens | Pablo Polledo |
| [`evaluation.md`](evaluation.md) | Our evaluation of the specification | Martín Rodríguez |
| `deviations.md` (from 20 Dec) | Final list of deviations | Jaime de Luiz, Óscar Marín |

## 2. Scope

### 2.1 Features

The specification defines 69 features (§2). We implement the 49 Must and 15 Should features (64 in total). The 5 Could features are done only if time allows.

| Module | Spec chapter | Features | Must | Should | Could | Owner |
|---|---|---|---|---|---|---|
| KYC | 6 | KYC-01…09 | 7 | 2 | 0 | Jaime de Luiz |
| CRM | 7 | CRM-01…07 | 5 | 1 | 1 (CRM-07) | Jaime de Luiz |
| CBS + Payments | 8 | CBS-01…06, PAY-01…06 | 9 | 2 | 1 (PAY-05) | Víctor Castañeda |
| Cards | 9 | CARD-01…06 | 3 | 3 | 0 | Pablo Polledo |
| Loans | 10 | LOAN-01…06 | 3 | 2 | 1 (LOAN-06) | Pablo Polledo |
| Accounting | 10 | ACC-01…06 | 3 | 2 | 1 (ACC-06) | Óscar Marín |
| AML | 11 | AML-01…14 | 12 | 2 | 0 | Martín Rodríguez |
| Notification | 12 | NTF-01…09 | 7 | 1 | 1 (NTF-09) | Óscar Marín |
| **Total** | | **69** | **49** | **15** | **5** | |

Each feature is tracked as a GitHub Issue with its ID, owner and target week. A feature is done when it builds, its tests pass, it is on `main` and any interpretation is logged in `decisions.md`.

### 2.2 Out of scope

These items are excluded by the specification itself (§1.4), so we do not implement them:

- interest calculation for credit cards and monthly card bills;
- cash withdrawals;
- card purchases in currencies other than EUR;
- real connections to external providers (see section 4).

## 3. How we implement it

The full description is in [`architecture.md`](architecture.md). In short:

- **Stack:** C# / .NET 10, ASP.NET Core Web API, PostgreSQL 16 with EF Core 10, React + TypeScript + Vite frontend, xUnit + FluentAssertions + Testcontainers.
- **Modular monolith:** one deployable (`EBank.Host`) with eight isolated modules. Each module owns one PostgreSQL schema and never reads another module's tables (spec §3.2).
- **Synchronous calls:** every `/internal/<module>/...` endpoint of the spec becomes a C# interface in that module's `*.Contracts` project, with the same operations and fields (for example `ICbsInternalApi`, `IAmlInternalApi`, `ICrmInternalApi`).
- **Events:** every event of the spec is published with the common envelope (`eventId`, `eventType`, `occurredAt`, `source`, `payload`) through an in-process event bus with a transactional outbox. Consumers are idempotent (inbox table of processed `eventId`s).
- **Public API:** customer and employee endpoints are exposed under `/api/v1/...` with the paths, fields, status codes and error body (`{"error": {"code", "message"}}`) of the specification.
- **Conventions kept from the spec:** UUID ids, money as `Decimal(18,2)` (a two-decimal string in JSON), UTC timestamps, `Idempotency-Key` on payments, holds and postings, optimistic locking (`409 VERSION_CONFLICT`), and an append-only audit log.
- **Security (NFR-1):** 5-minute JWT access tokens with refresh tokens checked against the CBS session, 15-minute inactivity timeout, 6-digit SCA codes (5 minutes, 3 attempts) delivered through the Notification mock, server-side role checks, and hashed passwords read only by CRM.
- **Frontend:** one web app with a customer area (`/app`, mobile first, from 360 px) and a back-office area (`/backoffice`), in Lithuanian and English. The screens are the consolidated set in [`screens.md`](screens.md).

## 4. Mocked external systems

The specification describes all external systems as interfaces that can be mocked during implementation (§1.4). Each mock sits behind an interface in the module that uses it, so it can be replaced by a real provider without changing the business logic. Mocks are deterministic, which makes the end-to-end flows and the automated tests repeatable.

| External system | Used by | Spec reference | Mock behaviour |
|---|---|---|---|
| Identity verification provider (IDV) | KYC | §6.10.5 | Returns `PASS` by default; specific test inputs return a failure or a hit (decision D-006). |
| Company registry | KYC | §6.10.5 | Returns company data and beneficial owners for test registration numbers (behaviour defined by the KYC owner in `decisions.md`). |
| SEPA and SWIFT networks (Payment Gateway Interface) | CBS + Payments | §8.9 | Accepts the formatted payment and confirms it. A test trigger returns a gateway error, so `TRANSACTION_FAILED` (`GATEWAY_ERROR`) can be tested. |
| Exchange-rate provider | CBS + Payments | BR-06 | Rates are loaded from default data and updated once a day by a mock provider, as the spec states. |
| Card network | Cards | §9.9.2 | Cards exposes the mock endpoints of §9.9.2 (`/network/v1/authorizations`, completion, cancellation), so purchases can be simulated. |
| FNTT (financial intelligence unit) | AML | §11.8.4 | Accepts the report and returns a random reference such as `FNTT-2026-000123`, as the spec describes. |
| Push, e-mail and SMS providers | Notification | §12.8.2 | Messages are stored and logged instead of being sent. One-time codes are masked in the stored copy (NTF-06). |

## 5. Deviations from the specification

These deviations were agreed by the team. They are logged in [`decisions.md`](decisions.md) and will be listed in `deviations.md` for the final submission.

| ID | Spec says | We do | Reason |
|---|---|---|---|
| DV-1 | 8 deployable services, each with its own database (§3.3) | Modular monolith with one PostgreSQL schema per module, outbox and in-process event bus | Team size and time. Module boundaries and isolation rules are kept. |
| DV-2 | REST calls between services under `/internal/<module>` (§3.2) | C# interfaces in `*.Contracts` that mirror those endpoints one to one | Follows from DV-1 (decision D-003). |
| DV-3 | Native mobile apps for customers | Responsive web app | Time. The spec gives the web app the same structure. |
| DV-4 | Connections to external providers | Mocks behind interfaces (section 4) | Allowed by spec §1.4 (decision D-002). |
| DV-5 | 99.9 % availability (G5), RPO/RTO, 1,000 requests per second, multi-zone replication | Not measured | These infrastructure targets cannot be verified in a local environment. |
| DV-6 | Encryption keys in a key management service | AES-256 key in configuration or user secrets | No key management service is available. |

Goals G1–G4 are kept as functional requirements. Where possible, their time limits are checked in tests, for example the 2-second AML timeout of BR-14.

## 6. Plan

| Week | Goal |
|---|---|
| 23–29 Nov | Contracts of all modules. First end-to-end flow: registration (CRM-01), login with one-time code (CBS-06), account opening (CBS-01), internal transfer (PAY-01) with AML screening (AML-01/02), `TRANSACTION_COMPLETED` and notification (NTF-01). |
| 30 Nov–6 Dec | Remaining Must features: KYC review with four-eyes approval, legal entities, SEPA (mock), dual signature, debit cards and authorization, loan application to payout, end-of-day ledger and reconciliation, AML queue and cases. |
| 7–13 Dec | Should features, frontend screens for the main flows, tests. |
| 14–20 Dec | Integration, bug fixing, `deviations.md`, final evaluation, demo rehearsal. |

The per-member task list and deadlines are in `AGENTS.md` §11. The dates will move if the supervisor gives a different deadline for Lab 2.

## 7. Risks

| Risk | Effect | Mitigation |
|---|---|---|
| Unclear or contradictory parts of the spec (see `evaluation.md`, for example E-03 and E-04) | Modules interpret the same rule differently | Every interpretation is logged once in `decisions.md` and shared before coding. |
| Dependencies between modules (Cards and Loans need CBS holds and postings; CBS needs CRM authentication and AML screening) | A module waits for another one | Contracts are published first; a module can use a temporary mock of another module's contract until the real one is ready. |
| Thinner spec for Loans and Accounting (chapter 10) | More decisions are needed during implementation | Those decisions are logged early and reviewed by the whole team. |
| Lab 2 deadline and deliverables not yet confirmed by the supervisor | The plan may change | We asked the supervisor and will update section 6 when he answers. |

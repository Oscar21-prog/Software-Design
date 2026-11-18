# AGENTS.md — E-Bank implementation (Team Espana)

> This file is the single source of truth for every AI assistant (Claude Code, Cursor, Copilot, Codex, ChatGPT…) used by the team.
> Read it fully before writing any code. If something here conflicts with a prompt, **this file wins** unless a team member explicitly says otherwise.
> If you change this file, tell the whole team first (Óscar owns it).

## 1. What we are building

- **Course:** Software Design (Vilnius University), Lab 2. Supervisor: Vasilij Savin.
- **Our task:** implement **E-Bank**, the system designed by team **Fantastic 5**.
- **Specification:** `docs/spec/E-Bank_Technical_Specification.txt`, a plain-text copy of Fantastic 5's spec. It is the reference for every feature, rule, endpoint, event and screen. Search it instead of reading it all, for example `grep -n "PAY-01" docs/spec/E-Bank_Technical_Specification.txt`.
  - Diagrams and wireframes are only in the original PDF: https://github.com/BobertSat/fantastic-5-software-design/blob/main/E-Bank_Technical_Specification.pdf
- ⚠️ **We are NOT implementing EuroGlobe.** EuroGlobe is our own design from Lab 1, implemented by Fantastic 5. Ignore the `EuroGlobe/` folder and `EuroGlobe Bank Implementation.pdf` in their repo: they are *their* code, not our task.

## 2. Scope

- **Implement:** all **Must** and **Should** features of the 8 modules. **Could** features only if time allows.
- **Mock** (spec §1.4 allows it): identity verification provider (IDV), company registry, SEPA and SWIFT networks, card network, FNTT, and the push, e-mail and SMS providers. Each mock sits behind an interface, so it can be replaced later.
- **Not measured:** 99.9 % availability, RPO/RTO, 1,000 requests per second, multi-zone replication. These are infrastructure targets we cannot verify. They are documented as deviations.
- **Out of scope** (spec §1.4): credit-card interest and monthly card bills, cash withdrawals, card purchases in other currencies.

| Module | Feature IDs | Spec chapter |
|---|---|---|
| KYC | KYC-01…09 | 6 |
| CRM | CRM-01…07 | 7 |
| CBS + Payments | CBS-01…06, PAY-01…06 | 8 |
| Cards | CARD-01…06 | 9 |
| Loans | LOAN-01…06 | 10 |
| Accounting | ACC-01…06 | 10 |
| AML | AML-01…14 | 11 |
| Notification | NTF-01…09 | 12 |

## 3. Owners — who codes what

| Member | GitHub | Modules | Documents |
|---|---|---|---|
| Víctor Castañeda | @vicctoor18 | **CBS + Payments** | Implementation proposal |
| Jaime de Luiz | @jaimedeluiz | **KYC + CRM** | Questions & decisions log |
| Pablo Polledo | @paabloo9 | **Cards + Loans** | GitHub backlog + screen scope |
| Martín Rodríguez | @martinrdmos | **AML** | Evaluation of the E-Bank spec |
| Óscar Marín | @Oscar21-prog | **Accounting + Notification + shared infrastructure** (`BuildingBlocks`, `Host`) | Architecture & tech decisions, team agreement |

**AI rule:** when working for a member, only modify that member's modules. If another module needs a change, do **not** edit it: write down what is needed (an interface, endpoint or event) and tell the human to ask that module's owner.

## 4. Tech stack (fixed — do not introduce alternatives)

- **Backend:** C# / **.NET 10 (LTS)**, ASP.NET Core Web API with controllers.
- **Database:** **PostgreSQL 16**, **EF Core 10** (Npgsql). **One schema per module** (`kyc`, `crm`, `cbs`, `cards`, `loans`, `accounting`, `aml`, `notification`). EF Core migrations per module.
- **Frontend:** **React + TypeScript + Vite**, one app with two areas: `/app` (customer, responsive, mobile-first, 360 px minimum) and `/backoffice` (employees). LT/EN with i18next.
- **Tests:** xUnit + FluentAssertions; integration tests with `WebApplicationFactory` + Testcontainers (PostgreSQL).
- **Local environment:** `docker compose up` starts PostgreSQL (and the API when needed).
- **No other libraries** (MediatR, AutoMapper, MassTransit, Redux…) unless Óscar adds them to this list.

## 5. Architecture

**Modular monolith.** This is a documented deviation from the spec, which has 8 deployable services. We keep the spec's rules for independence between modules:

1. **One deployable** (`EBank.Host`). Each module is isolated and owns its schema. **A module never reads or writes another module's tables.**
2. **Synchronous calls between modules** (spec §3.1, the `/internal/...` endpoints) are C# interfaces in the target module's `*.Contracts` project, for example `ICbsInternalApi.CreateHoldAsync(...)`. Method names and fields mirror the spec's `/internal/<module>/...` endpoint. A module may reference **only the `Contracts` project** of another module, never its implementation.
3. **Asynchronous communication** uses events through the in-process **event bus** in `BuildingBlocks`, with a **transactional outbox**. The change and the outbox row are saved in one transaction, and a background relay publishes them **at least once**. Consumers are **idempotent**: each module stores processed `eventId`s in an `inbox` table.
4. Other modules keep only **ids and small replicas** fed by events (for example the customer status in CBS and Cards).

### Solution layout

```
/src
  /BuildingBlocks/EBank.BuildingBlocks        # Money, errors, event envelope, outbox/inbox, event bus, idempotency, audit log, clock
  /Host/EBank.Host                            # Program.cs, DI, auth, /api/v1 routing, background jobs
  /Modules/<Module>/EBank.<Module>            # Domain/, Application/, Infrastructure/, Api/ folders
  /Modules/<Module>/EBank.<Module>.Contracts  # public interfaces, DTOs and event payloads ONLY
/tests/EBank.<Module>.Tests
/frontend
/docs                                         # proposal, decisions log, deviations, evaluation
docker-compose.yml
```

Modules are `Kyc`, `Crm`, `Cbs` (includes Payments), `Cards`, `Loans`, `Accounting`, `Aml` and `Notification`.

**Layering inside a module** (spec §3.3): `Api → Application → Domain`, with `Infrastructure` implementing `Application` interfaces. **`Domain` has no dependency on EF Core, ASP.NET or any other module.**

## 6. Conventions from the spec (§3.2, NFRs) — mandatory

- **Routes:** customer and employee API under `/api/v1/...`, exactly as written in the spec's API tables. Do not rename paths, fields or status codes.
- **Ids:** `Guid` (UUID) everywhere.
- **Money:** `decimal` stored as `numeric(18,2)`, never `float`/`double`. In JSON, money is a **string with two decimals** (`"125.00"`). A balance can never become negative.
- **Time:** stored and sent as UTC ISO 8601 (`DateTimeOffset`). The UI shows Europe/Vilnius time. Use the injected `IClock` and never `DateTime.Now`, so tests can control time.
- **Error body**, always: `{"error": {"code": "INSUFFICIENT_FUNDS", "message": "..."}}`. Error codes are the `UPPER_SNAKE_CASE` codes from the spec. Use the status codes 400/401/403/404/409/422 and 500/502/503/504.
- **Event envelope:** `eventId` (UUID), `eventType` (`UPPER_SNAKE_CASE`, exactly as in the spec, e.g. `TRANSACTION_COMPLETED`, `CUSTOMER_STATUS_CHANGED`, `KYC_APPROVED`, `SCREENING_REQUESTED`), `occurredAt`, `source` (module name) and `payload`. Events about one customer use `customerId` as the key. **Schemas only change by adding fields.**
- **Idempotency:** payments, postings and holds accept an `Idempotency-Key` header or a unique id. Repeating a request returns the original result.
- **Concurrency:** balance and hold changes run in one DB transaction with **optimistic locking** (a version column). A conflict returns `409 VERSION_CONFLICT`.
- **Security (NFR-1):**
  - JWT access token valid 5 minutes, with a refresh token checked against the server-side session in CBS. A session ends after 15 minutes of inactivity.
  - SCA one-time code (6 digits, valid 5 minutes, 3 attempts) at login, for payments above EUR 500, for approvals, mandate changes, card activation and card repayment. The OTP is "sent" through the Notification mock.
  - Role-based access control is enforced on the server.
  - Passwords are hashed and only CRM reads them.
- **Audit log:** an append-only table (user, time, action, old value, new value) for payment, mandate, account-status and back-office changes.
- **Lists:** server-side paging with 50 rows by default.
- **Notifications (NTF-06):** never include a full card number, a password, or anything that shows an AML/FNTT report. **AML-11:** the customer is never shown that a report to FNTT was made.

## 7. How the AI must work

1. **Read the spec section first.** Before implementing a feature, read its row in the feature table and the related business rules (BR-xx), flows, API table, events and wireframe. Quote the IDs you implement.
2. **Do not invent.** Do not add endpoints, fields, statuses or events that are not in the spec. If the spec is unclear or contradicts itself:
   - choose the simplest interpretation that is consistent with the rest of the spec,
   - add an entry to `docs/decisions.md` (format below),
   - tell the human. Never make a silent assumption.
3. **Stay in your module** (see §3). Changes to `BuildingBlocks` or `Host` go through Óscar.
4. **Tests are part of the feature.** Each business rule gets at least one unit test, named after its ID, for example `BR15_RefreshToken_RejectedAfterSessionEnds`. Each endpoint gets at least one integration test.
5. **Small changes.** One feature (or one part of a feature) at a time. Do not refactor unrelated code.
6. **The human must understand the code.** At the end, explain briefly what you did and why, so the member can defend it in the demo.
7. **"What do I have to do?"** When a member says who they are and asks for their tasks, first make sure the code is up to date (`git pull`, see §8). Then answer with **that member's full list from §11**, every item with its deadline, and mark which items are **already done in the repo**. Check today's date and point out what is overdue, what is due next, and what they should start now.

### `docs/decisions.md` entry format
```
## D-012 — <short title>
- Spec reference: §8.4, BR-07, PAY-02
- Problem: <what is unclear / contradictory / missing>
- Decision: <what we implement>
- Deviation from spec: yes/no
- Author / date: <name>, 2026-11-25
```

## 8. Git workflow (simple)

We all work directly on `main`. Three rules are enough, because each member works in their own folders.

**1. Before starting — ALWAYS get the latest code**
```bash
git pull
```
If the AI cannot run commands, it must tell the human: "Run `git pull` first and tell me when it is done." Do not start until this is done.

After pulling, look at what already exists: other modules' `Contracts`, `BuildingBlocks`, and your own module. Compare it with your list in §11, then tell the human what is done and what comes next. **Reuse what exists. Never re-create something that is already there** (Money, errors, event bus, DTOs, interfaces…).

**2. While working — only touch your own folders**
- Your module: `src/Modules/<Module>/`, `tests/EBank.<Module>.Tests/` and `frontend/src/modules/<module>/`.
- Shared files (`BuildingBlocks`, `Host`/`Program.cs`, `docker-compose.yml`, the `.sln`, this file) belong to Óscar. Each module registers itself with its own `Add<Module>Module()` extension, so nobody else needs to edit `Program.cs`.
- Never edit, move, reformat or delete another member's files. If you need something from another module, ask its owner.

**3. Before uploading — pull again, then push**
```bash
git pull
# build + run all tests
git add .
git commit -m "PAY-01: internal transfer"
git push
```
- If `git pull` shows a **conflict**: keep **both** changes. Never delete the other member's code. If unsure, stop and ask.
- Push **small and often** (at least once a day), not everything at the end.
- Never use `git push --force`. Never commit secrets or `bin/`, `obj/`, `node_modules/`.
- Commit messages start with the feature ID (`CARD-04: ...`), and each member commits with their own account.

After pushing something others need (a `Contracts` change, a `BuildingBlocks` feature), tell the group chat.

**Definition of done:** it builds, all tests pass, it is pushed to `main`, the Issue is closed, and any decision is logged in `docs/decisions.md`.

## 9. Roadmap

| When | Goal |
|---|---|
| **Fri 20 Nov** | Óscar: solution skeleton (§5 layout), `BuildingBlocks` (Money, errors, event bus + outbox/inbox, idempotency, audit, IClock), Docker Compose, empty modules that build and run. Team agreement updated. |
| **Sun 22 Nov** | Proposal (Víctor) and GitHub Issues for every feature (Pablo) done. Everyone has read their chapters and chapter 3. Team meeting. Each module publishes its `Contracts` (interfaces + event payloads) as defined in the spec. |
| **23–29 Nov** | **First end-to-end flow:** registration (CRM-01) → login with OTP (CBS-06) → open account (CBS-01) → internal transfer (PAY-01) → AML screening (AML-01/02) → `TRANSACTION_COMPLETED` → notification (NTF-01). First versions of the evaluation (Martín) and the decisions log (Jaime). |
| **30 Nov–6 Dec** | Remaining **Must** features: KYC review and four-eyes approval, legal entities, SEPA (mock), dual signature, debit cards and authorization, loans application → payout, end-of-day ledger and reconciliation, AML queue and cases. |
| **7–13 Dec** | **Should** features + frontend screens for the main flows + tests. |
| **14–20 Dec** | Integration, bug fixing, `docs/deviations.md`, final evaluation, demo rehearsal. |

The dates move if the supervisor gives a different deadline. Update this table when that happens.

## 10. Known deviations from the spec (keep updated)

| # | Spec says | We do | Reason |
|---|---|---|---|
| DV-1 | 8 deployable services, each with its own DB | Modular monolith, one PostgreSQL schema per module, outbox + in-process event bus | Team size and time; module boundaries are kept |
| DV-2 | `/internal/<module>` REST calls between services | C# interfaces in `*.Contracts` that mirror those endpoints | Follows from DV-1 |
| DV-3 | Native iOS/Android apps | Responsive web app | Time; the spec says the web has the same structure |
| DV-4 | External providers (IDV, registry, SEPA/SWIFT, card network, FNTT, push/e-mail/SMS) | Mocks behind interfaces | Allowed by spec §1.4 |
| DV-5 | Availability, RPO/RTO, throughput targets | Not measured | Cannot be verified in our environment |
| DV-6 | Keys in a key management service | AES-256 key in configuration / user secrets | No KMS available |

## 11. Tasks per member, with deadlines

Every member, in addition to their own list:
- Read **chapter 3** (architecture) and your module chapters by **Sun 22 Nov**, and attend the **team meeting on Sun 22 Nov**.
- Publish your modules' `Contracts` (interfaces mirroring the spec's `/internal` endpoints + event payloads) by **Sun 22 Nov**.
- Build the frontend screens of your own modules (spec wireframes) in the **7–13 Dec** week.
- Send unclear spec points to Jaime for the decisions log.
- **14–20 Dec:** integration, bug fixing, demo rehearsal.

### Víctor — CBS + Payments (chapter 8)
| Deadline | Task |
|---|---|
| Sun 22 Nov | **Implementation proposal** `docs/proposal.md` (2–3 pages: what we implement, what we mock, deviations) |
| Sun 22 Nov | CBS contracts: `ICbsInternalApi` (accounts, by-iban, sca/validate, holds, capture/cancel, postings credit/debit, transactions) + CBS events |
| Sun 29 Nov | CBS-06 login + SCA one-time code (uses CRM `authenticate`), CBS-01 open account with IBAN, PAY-01 internal transfer with AML screening, CBS-04 holds and postings (Cards and Loans depend on them) |
| Sun 6 Dec | CBS-02 mandates + dual-signature setting, CBS-03 balance/holds/history/statement export, PAY-02 SEPA (mock), PAY-04 dual-signature payments, PAY-06 fee and FX quote |
| Sun 13 Dec | CBS-05 account administration, PAY-03 SWIFT (mock), W-CBS screens. Could: PAY-05 saved recipients |

### Jaime — KYC + CRM (chapters 6 and 7)
| Deadline | Task |
|---|---|
| Sun 22 Nov | Create `docs/decisions.md`. CRM + KYC contracts (`/internal/crm/users/authenticate`, `/internal/crm/customers/{id}`) + events |
| Sun 29 Nov | **Decisions log, first version.** CRM-01 registration, `authenticate` endpoint (Víctor needs it for login), CRM-04 status control + `CUSTOMER_STATUS_CHANGED` |
| Sun 6 Dec | KYC-01…05 (documents, IDV mock, sanctions/PEP via AML `SCREENING_REQUESTED`, risk scoring, review queue + four-eyes), KYC-08 legal entities (registry mock, UBO), KYC-09 audit trail, CRM-02 profile, CRM-03 representatives, CRM-05 GDPR |
| Sun 13 Dec | KYC-06 information requests, KYC-07 periodic review / expiry, CRM-06 Customer 360, KYC + CRM screens. Could: CRM-07 notes |
| Sun 20 Dec | Decisions log final, turned into `docs/deviations.md` together with Óscar |

### Pablo — Cards + Loans (chapters 9 and 10)
| Deadline | Task |
|---|---|
| Sun 22 Nov | **GitHub Issues for all 69 features** (ID, title, owner, week) + project board + `docs/screens.md` (which wireframes we build) |
| Sun 22 Nov | Cards + Loans contracts (`/internal/cards/...`, `/internal/loans/...`) + events |
| Sun 29 Nov | CARD-01 order/activate debit card, LOAN-01 loan application (using CBS contracts; mock CBS if Víctor is not finished) |
| Sun 6 Dec | CARD-03 block/unblock, CARD-04 purchase authorization (AML + CBS holds), LOAN-02 scoring and decision, LOAN-03 offer, e-signing, payout |
| Sun 13 Dec | CARD-02 limits, CARD-05 credit card use and repayment, CARD-06 credit-card application, LOAN-04 installments, LOAN-05 late payments, screens. Could: LOAN-06 calculator |

### Martín — AML (chapter 11)
| Deadline | Task |
|---|---|
| Sun 22 Nov | AML contracts: `/internal/aml/screenings` (ALLOW/HOLD), `SCREENING_REQUESTED` / `SCREENING_COMPLETED` |
| Sun 29 Nov | **Evaluation of the E-Bank spec, first version** (`docs/evaluation.md`, same criteria as Fantastic 5's evaluation of us). AML-01 pre-execution screening, AML-02 score ≥ 70 → HOLD, AML-03 HOLD for flagged customers, AML-12 case audit |
| Sun 6 Dec | AML-05 alerts, AML-06 queue, AML-07 take alert, AML-08 case details, AML-09 release, AML-10 report to FNTT (mock), AML-11 never shown to customer, AML-14 sanctions/PEP matching for KYC |
| Sun 13 Dec | AML-04 post-execution screening, AML-13 queue filters, W-AML screens |
| Sun 20 Dec | **Evaluation, final version** |

### Óscar — Accounting + Notification + shared infrastructure (chapters 10 and 12)
| Deadline | Task |
|---|---|
| Fri 20 Nov | **Architecture & tech decisions** `docs/architecture.md` (confirm or change §4–5 of this file), **solution skeleton** + `BuildingBlocks` + Docker Compose, **team agreement updated** (phase 2 owners, AI rule) |
| Sun 22 Nov | Accounting + Notification contracts. Auth (JWT + refresh) skeleton in `Host` |
| Sun 29 Nov | NTF-01 send on events, NTF-02 language, NTF-03 channels, NTF-04 mandatory notifications, NTF-06 content rules, OTP delivery for Víctor's SCA (all providers mocked) |
| Sun 6 Dec | ACC-01 end-of-day posting, ACC-02 general ledger view, ACC-03 reconciliation, NTF-07 channel settings, NTF-08 push tokens |
| Sun 13 Dec | ACC-04 reports, ACC-05 chart of accounts, NTF-05 fallback channel, screens. Could: ACC-06 monthly statement, NTF-09 language choice |
| Sun 20 Dec | `docs/deviations.md` final, together with Jaime |

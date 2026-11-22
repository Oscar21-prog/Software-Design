# Decisions & Questions Log — E-Bank (Team España)

**Owner / Maintainer:** Jaime de Luiz (@jaimedeluiz)  
**Initial Date:** 2026-11-22  
**Purpose:** Single log of truth for all technical interpretations, clarifications, and design decisions across the 8 modules of E-Bank during Phase 2 (Implementation).

---

## D-001 — Modular Monolith Architecture
- **Spec reference:** §3.1, §3.3
- **Problem:** The specification describes 8 separate deployable services with independent databases, which introduces massive deployment, operational, and network overhead unfeasible for a 1-month university lab.
- **Decision:** Implement E-Bank as a modular monolith in a single ASP.NET Core host (`EBank.Host`) backed by a single PostgreSQL 16 database with 8 strictly isolated schemas (`kyc`, `crm`, `cbs`, `cards`, `loans`, `accounting`, `aml`, `notification`). Cross-schema foreign keys or joins are strictly prohibited. Cross-module calls use C# interfaces (`*.Contracts`) and asynchronous in-process transactional outbox event publishing (`IEventBus`).
- **Deviation from spec:** yes (DV-1, DV-2)
- **Author / date:** Óscar Marín / Jaime de Luiz, 2026-11-20

---

## D-002 — External Provider Mocking
- **Spec reference:** §1.4, §6.10.5, §8.10.5, §9.10.5, §11.10.5, §12.8.2
- **Problem:** The specification references third-party external networks and providers (IDV provider, company registry, SEPA, SWIFT, card payment networks, FNTT reporting, and SMS/Email/Push gateways) that are unavailable in academic local environments.
- **Decision:** All external integrations are implemented as mocks placed behind domain/contract interfaces (`IIdvProviderAdapter`, `ICompanyRegistryAdapter`, `INotificationChannelAdapter`, etc.). Mocks provide deterministic behavior for automated tests and end-to-end flows while preserving exact message formats and timeouts.
- **Deviation from spec:** yes (DV-4, sanctioned by spec §1.4)
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-003 — Internal Synchronous Module Contracts
- **Spec reference:** §3.1, §7.10.3, §8.10.3
- **Problem:** The specification defines REST endpoints under `/internal/<module>/...` for inter-service queries (e.g. `/internal/crm/users/authenticate`, `/internal/crm/customers/{id}`). In a modular monolith, in-process HTTP adds serialization overhead and port management friction.
- **Decision:** Map all `/internal/...` endpoints directly to C# interfaces in each module's `*.Contracts` assembly (e.g. `ICrmInternalApi`, `ICbsInternalApi`). Methods and payloads mirror the spec routes and DTOs 1:1. Modules only reference other modules' `Contracts` projects.
- **Deviation from spec:** yes (DV-2)
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-004 — Password Hashing and CRM Authentication
- **Spec reference:** §6 (NFR-1), §7.9, §7.10.3, C-BR-02
- **Problem:** The specification mandates that passwords must be hashed and only CRM reads them, but does not specify the exact hashing algorithm.
- **Decision:** Use PBKDF2 with HMAC-SHA512 (ASP.NET Core Identity `PasswordHasher<User>`) with a work factor compliant with modern security standards. Passwords are never stored in plaintext and never leaked in logs, audit records, or events.
- **Deviation from spec:** no
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-005 — Indefinite Corporate Representation and Ownership Dates
- **Spec reference:** §7.9, C-BR-09, C-BR-10
- **Problem:** Spec defines `validFrom` and `validTo` dates for company representatives and beneficial owners. However, real-world corporate appointments typically have no predetermined end date upon initial registration.
- **Decision:** Model `validTo` as a nullable date (`DateOnly?`). A `null` value indicates an ongoing, active mandate. Ending a role or selling shares sets `validTo` to the current date.
- **Deviation from spec:** no
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-006 — Identity Verification (IDV) Mock Behavior
- **Spec reference:** §6.4.1, §6.10.5, K-BR-03
- **Problem:** IDV provider must verify identity documents and selfies with liveness checks. Without a live biometric service, how does the mock decide PASS vs FAIL vs HIT?
- **Decision:** The mock adapter `IIdvProviderAdapter` inspects document contents or deterministic triggers (e.g. file names containing "fail" or "hit", or specific test personal codes). By default, valid base64 payload returns `PASS` with simulated extracted personal code and expiry date, allowing the happy path onboarding (G1) to succeed seamlessly.
- **Deviation from spec:** no
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-007 — Sensitive Field Group Detection for KYC Review
- **Spec reference:** §7.10.4, C-BR-07, §6.4.1, K-BR-12
- **Problem:** When an active customer updates their profile in CRM, if a sensitive field changes, CRM must publish `SENSITIVE_DATA_CHANGED`, which triggers KYC to open a `CHANGE_REVIEW` case.
- **Decision:** CRM compares entity snapshot values before and after applying the update. If any field in `NAME`, `PERSONAL_CODE`, `DATE_OF_BIRTH`, `NATIONALITY`, `ADDRESS`, or `STRUCTURE` has mutated, CRM publishes `SENSITIVE_DATA_CHANGED` with the list of changed groups.
- **Deviation from spec:** no
- **Author / date:** Jaime de Luiz, 2026-11-22

---

## D-008 — Optimistic Concurrency and Status Change Transactionality
- **Spec reference:** §6 (Conventions), §7.7, §7.9
- **Problem:** Goal G3 requires status updates to propagate reliably within 5 seconds without race conditions or lost updates.
- **Decision:** Every aggregate (`Customer`, `KycCase`) contains an integer/long `Version` column used for EF Core optimistic locking. State mutation, audit log writing, and transactional outbox event insertion happen within a single database transaction. A concurrency conflict immediately returns HTTP `409 VERSION_CONFLICT`.
- **Deviation from spec:** no
- **Author / date:** Jaime de Luiz, 2026-11-22


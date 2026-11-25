# Evaluation of Fantastic 5's E-Bank Technical Specification

**Author:** Martín Rodríguez Ramos (`@martinrdmos`), Team España  
**Specification Reviewed:** *E-Bank — Technical specification and API contract* ([`E-Bank_Technical_Specification.txt`](../E-Bank_Technical_Specification.txt))  
**Version:** v1.0 — First Version (Nov 29 Milestone)  
**Date:** 2026-11-25  
**Related Team Docs:** [`docs/architecture.md`](architecture.md), [`docs/decisions.md`](decisions.md), [`docs/screens.md`](screens.md) *(Note: `docs/proposal.md` is not yet in the repository and will be cross-checked in the Dec 20 final version.)*

---

## 1. Executive Summary

Fantastic 5's *E-Bank Technical Specification* is a solid, well-structured foundation for implementing an 8-module banking platform (69 MoSCoW features across KYC, CRM, CBS & Payments, Cards, Loans, Accounting, AML, and Notification). Its strongest aspect is the upfront architectural baseline in Chapter 3—clear module boundaries, explicit sync vs. async rules, transactional outbox, idempotency, and uniform JSON/error conventions—paired with detailed operational chapters for **AML (Ch. 11)**, **Notification (Ch. 12)**, **KYC (Ch. 6)**, **CRM (Ch. 7)**, and **CBS (Ch. 8)**.

That said, once you move from reading to designing real C# contracts and database schemas, several concrete friction points appear:
- **Cross-module event and contract gaps**, especially between CBS and AML (`AML_SCREENING_FAILED` and `TRANSACTION_COMPLETED` payloads, `Employee` ownership/roles, and the "shared audit log").
- **Edge-case ambiguities inside AML (Ch. 11)** around rule boundaries (`R1`/`R2`), card vs. payment hold behavior (`A3`/`A6`), and `AmlCustomer` updates.
- **Uneven depth in Chapter 10 (Loans & Accounting)**, which is noticeably thinner than the rest of the document and will need deeper review as those modules are implemented.

**Overall Score:** **3.6 / 5.0 (18 / 25)** — *Strong and largely implementable in the core modules, with specific contract issues that need team decisions before integration.*

---

## 2. Evaluation Criteria Matrix

Scale: `1 = Missing/Unusable`, `2 = Major Gaps`, `3 = Adequate with Notable Gaps`, `4 = Strong with Minor Issues`, `5 = Ready as Written`.

| Criterion | Score | Justification |
| :--- | :---: | :--- |
| **Scope & Completeness** | **4 / 5** | Chapters 1–2 clearly define 5 measurable goals (`G1`–`G5`), 69 prioritized features, explicit mockable external interfaces (§1.4), and per-module *Won't* lists (§11.2, §12.2). Loses a point mainly because Chapter 10 omits key functional rules for Loans and Accounting. |
| **Internal Consistency & Traceability** | **3 / 5** | Feature IDs, business rules, wireframes, and the Glossary (Ch. 13) tie together well within each chapter, but cross-chapter drift shows up in `EmployeeRole` (§3.4 vs. §11.7.4), `User` ownership (§3.4 vs. §12.7), Loan signing OTP vs. CBS SCA (§8.8 vs. §10.8.1), and small editorial slips (e.g., off-by-one step refs in §11.3.2). |
| **Architecture & Internal Contracts** | **4 / 5** | Chapter 3 sets clean rules for `/internal/<module>` calls, outbox/inbox reliability, `customerId` event ordering, and caller retries/fallbacks. Deducted a point for concrete payload and boundary defects in `AML_SCREENING_FAILED`, `TRANSACTION_COMPLETED`, `SCREENING_COMPLETED`, and the "shared audit log". |
| **Data Model & State Machines** | **3 / 5** | AML (§11.7), Notification (§12.7), and Cards (§9.5) give clean text tables with types, nullability (`?`), and seed data. However, KYC, CRM, CBS, and Chapter 10 keep exact field types and state transitions only in PDF diagrams (leading to inferred choices like [`D-005`](decisions.md)), and `AmlTransaction`/`Employee` have schema gaps. |
| **Security & NFRs** | **4 / 5** | Chapter 4 (`NFR-1`–`NFR-7`) and module NFRs cover real banking requirements well: PSD2 SCA, 5-min JWT + 15-min inactivity sessions, GDPR vs. 8-year AML retention, strict tipping-off prevention (`AML-11`, `NTF-06`), four-eyes KYC (`K-BR-07`), and dual signature (`BR-07`). Deducted a point for scattered security details (`Argon2id` buried in `C-BR-02`), Loan signing bypassing CBS SCA, and lab-unverifiable infra targets (`99.9%`, `1,000 req/s`, KMS). |

---

## 3. Key Strengths

- **Clear Scope and Mocking Boundaries (§1.3–§1.4, §2):** Goals `G1`–`G5` attach concrete targets (such as AML screening ≤ 500 ms p95 / 2 s max) to owning modules. Allowing mocks behind interfaces for IDV, registry, SEPA/SWIFT, card network, FNTT, and notification providers—with concrete mock triggers like IDV `"fail"` filenames (§6.10.5) and FNTT reference formats (§11.8.4)—made our adapter design in [`D-002`](decisions.md) and [`D-006`](decisions.md) straightforward.
- **Solid Event-Driven Conventions (§3.1–§3.5):** The spec consistently applies database-per-module isolation, transactional outbox, `eventId` idempotency, additive event evolution, `Decimal(18,2)` money strings, UTC timestamps, and one error envelope (`{"error": {"code", "message"}}`). These map cleanly onto our shared `EBank.BuildingBlocks` primitives in [`docs/architecture.md`](architecture.md).
- **Strong AML Compliance & Resilience Design (Ch. 11, §8.3 `BR-14`, §9.3.3):**
  - *Tipping-off protection (`AML-11`, `NTF-06`, §11.8.3, §12.8.4):* Enforced directly in the contract—`TRANSACTION_REJECTED` omits any reason field so downstream modules cannot leak FNTT reports to the customer.
  - *Failure handling (`AML-NFR-02`, `F1 A4`/`A6`):* Cleanly separates `> 1,000 EUR` (fail-closed `ON_HOLD` + 5 retries for payments; decline for cards) from `≤ 1,000 EUR` (fail-open + async post-execution screening) and exempts AML holds from the 60 s payment-network timeout (`NFR-2`).
  - *Ready-to-seed configuration (§11.7.1, §12.8.4–12.8.5):* Default rules `R1`–`R3`, `AmlConfig`, high-risk countries (`KP`, `IR`), test watchlist rows, and full EN/LT notification templates eliminate guesswork during testing.
- **Practical UI Guidelines (§5.1–§5.3):** Shared components (confirmation dialogs, OTP dialog, inline + banner errors, mobile-first layout) stay consistent across modules and let us consolidate the UI into 6 core screens in [`docs/screens.md`](screens.md) without losing functionality.

---

## 4. Ambiguities, Gaps & Contradictions

This first pass focuses on **platform contracts** and the **AML module (Ch. 11)**, with initial cross-module notes on Chapter 10 and general documentation quality to be expanded in the Dec 20 revision.

| ID | Severity | Spec Reference | Finding & Practical Impact |
| :--- | :---: | :--- | :--- |
| **E-01** | **Critical** | §8.10.3, §11.3.1 `A4`, §11.6.1–11.6.2, §11.8.3 | **`AML_SCREENING_FAILED` is missing transaction fields and `initiatedByUserId`.** CBS sends only `{transactionId, customerId}`. Because all 5 screening attempts failed, AML never saved an `AmlTransaction` row—leaving it with no amount, currency, or counterparty to display in `W-AML-01`/`W-AML-02`, and no `initiatedByUserId` to emit `TRANSACTION_REJECTED` if the officer reports the case to FNTT. |
| **E-02** | **Critical** | §8.3 `BR-11`, §8.5.2, §8.10.3, §11.7.3, §11.8.2–11.8.3 | **`TRANSACTION_COMPLETED` collides on internal transfers and omits `counterpartyIban`.** CBS models an internal transfer as one `Transaction` (`BR-11`) but publishes `TRANSACTION_COMPLETED` twice with the same `transactionId` (`OUTGOING` and `INCOMING`). Since `AmlTransaction.id = referenceId`, ingesting both causes a primary-key collision. The event also omits `counterpartyIban` and `createdAt`, breaking `AML-13` IBAN search and unscreened fallback rows (`A5`). |
| **E-03** | **High** | §3.4, §8.5.6, §8.8, §11.7.2, §11.7.4, §11.8.2 | **Inconsistent `Employee` ownership, `EmployeeRole` enum, and missing employee name.** §3.4 and §11.7 assign `Employee` to AML, while CBS (§8.5.6) seeds employee users from CRM and keeps `EmployeeReplica`. `EmployeeRole` has 5 values in §3.4/§8.8 but 7 in §11.7.4 (adding unused `OPERATIONS` and `AUDITOR`). Also, `GET /api/v1/aml/cases/{caseId}` requires returning `employee name` in `audit`, yet `Employee` (§11.7.2) has no `name` field and no CRM lookup. |
| **E-04** | **High** | §3.2, §3.5, §8.5.5, §8.8, §11.5, §11.7.2, §12.8.3 | **"Shared audit log" contradicts module isolation.** §3.2 forbids shared DBs, yet CBS writes `AuditLogEntry` to AML's `audit` package (§8.8, §11.5) while AML names it `AuditEntry` and defines no write endpoint. Likewise, §3.5 lists `"Audit log"` as a consumer of `RECONCILIATION_DISCREPANCY` and `NOTIFICATION_FAILED` even though it is not a module. Team España resolved this via shared `AuditLogEntry` infrastructure in [`docs/architecture.md`](architecture.md) and [`D-001`](decisions.md). |
| **E-05** | **High** | §11.2 `AML-04`, §11.3.1 `R1`/`R2`, §8.5.4, §11.8.3 | **`R1`/`R2` gap at `15,000.00 EUR` and `R2` card/concurrency ambiguity.** `R1` checks `> 15,000` and `R2` checks transfers `< 15,000`, leaving an exact `15,000.00 EUR` transfer uncovered by both rules. It is also unclear whether `R2` includes `CARD` transactions (AML only receives `CARD_TRANSACTION_AUTHORIZED`, never completion/cancellation, and CBS skips `TRANSACTION_COMPLETED` for debit cards), and counting only *completed* transfers allows concurrent payments to slip past `R2`. |
| **E-06** | **High** | §11.2 `AML-03`, §11.3.1 `A3`/`A6`, §11.7.4 | **Conflict between `F1 A3` and `A6` on `held`, plus missing `AlertReason`.** For a customer with an open case or held alert, `A3` sets `held = true`, whereas `A6` requires `held = false` for all card alerts (since Cards declines immediately). In addition, `AlertReason` only defines `RULE_MATCH` and `SCREENING_FAILED`, leaving no reason code or defined `riskScore` for pure `AML-03` or `A7` (`kycStatus = REJECTED`) holds. |
| **E-07** | **Medium** | §6.10.3, §11.3.1, §11.7.3, §11.8.3–11.8.4 | **Stale `AmlCustomer` data and informal KYC screening schemas.** AML does not consume `CUSTOMER_PROFILE_UPDATED`, so name or ID-code changes never propagate to `AmlCustomer` or FNTT reports, and no fallback risk factor is defined if `kycRiskLevel` is null due to event lag. Meanwhile, `SCREENING_REQUESTED` (`personalCode or registrationNumber`) and `SCREENING_COMPLETED.matchDetails` are described in prose rather than exact JSON fields/enums, requiring us to formalize `WatchlistMatchType` in `EBank.Aml.Contracts`. |
| **E-08** | **High** | §8.8, §10.3–§10.4, §10.8 | **Initial observation — Chapter 10 (Loans & Accounting) gaps and SCA mismatch.** `POST /api/v1/loans/offers/{id}/sign` asks for a raw SMS `code` without a `LOAN_SIGNING` purpose in CBS `OtpChallenge.purpose` or a `/internal/cbs/sca/validate` call. Chapter 10 also lacks a business-rules table, credit-card limit scoring logic (`credit-card-assessments`), auto-approved loan interest rates, and Accounting seed data / report types (`GET /api/v1/admin/reports/{type}`). *(To be expanded in the Dec 20 revision as Loans and Accounting are built.)* |
| **E-09** | **Medium** | §3.1–§3.3, §4 `NFR-1`–`NFR-4`, §6.9, §7.4 `C-BR-02`, §7.9 | **Diagram-only schemas, buried security rule, and lab scope adaptations.** KYC, CRM, CBS, and Chapter 10 place exact field types/nullability only in PDF diagrams (forcing inferences like nullable `validTo` in [`D-005`](decisions.md) and explicit versioning in [`D-008`](decisions.md)). Also, `Argon2id` is specified only in §7.4 `C-BR-02` rather than `NFR-1`, which led our [`D-004`](decisions.md) to treat the algorithm as unspecified and pick PBKDF2—something we should align with CRM. Broader architecture/mocking adaptations are tracked in [`D-001`](decisions.md)–[`D-003`](decisions.md), [`D-006`](decisions.md), and [`D-007`](decisions.md). |

*Minor editorial notes (to revisit in v2.0):* §12.7 attributes `User` to "KYC and CRM" instead of CRM alone (§3.4); §11.3.2 alternate courses `A1`/`A4` have off-by-one step references (`step 5/6` vs. `4/5`); `GET /api/v1/aml/cases/{caseId}` shows full transaction details only for the first alert in multi-alert cases (§11.8.2).

---

## 5. Concrete Recommendations

1. **Fix AML–CBS Event Payloads (`E-01`, `E-02`):** Add transaction details and `initiatedByUserId` to `AML_SCREENING_FAILED`, add `counterpartyIban` and `createdAt` to `TRANSACTION_COMPLETED`, and specify how AML handles paired `OUTGOING`/`INCOMING` internal-transfer events (e.g., ignore `INCOMING` or use a composite key).
2. **Clarify `Employee` and Audit Ownership (`E-03`, `E-04`):** Align `EmployeeRole` to one list across §3.4, §8.8, and §11.7.4, add `displayName` to `Employee` for AML audit responses, and either keep audit logs module-local or define an explicit internal audit contract instead of referencing an unnamed `"Audit log"` consumer.
3. **Tighten AML Rule & Contract Edge Cases (`E-05`–`E-07`):** Change `R1` to `>= 15,000.00 EUR`, clarify `R2` scope for card and `PENDING`/`ON_HOLD` transfers, explicitly keep `held = false` for card alerts under `AML-03` with a dedicated `AlertReason`, subscribe AML to `CUSTOMER_PROFILE_UPDATED`, and specify exact JSON field/enum names for `SCREENING_REQUESTED` and `SCREENING_COMPLETED`.
4. **Align Loan Signing SCA and Complete Chapter 10 (`E-08`):** Add `LOAN_SIGNING` to CBS `OtpChallenge.purpose` (using `X-SCA-Token` like Cards), and add business rules, credit-card assessment criteria, default interest rates, and Accounting seed/report schemas to Chapter 10.
5. **Add Textual Schema Tables & Centralize Key NFRs (`E-09`):** Mirror AML's and Notification's tabular data models in KYC, CRM, CBS, and Chapter 10 so field types and nullability are not diagram-only, and surface password hashing (`Argon2id`) in `NFR-1`.

---

*Planned for v2.0 (Dec 20 milestone): incorporate integration-test findings across all 8 modules, cross-check against `docs/proposal.md` and `docs/deviations.md`, and expand Chapter 10 observations.*

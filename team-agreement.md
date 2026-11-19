# Team Agreement — E-Bank Implementation (Team España)

**Course:** Software Design — Faculty of Mathematics and Informatics, Vilnius University  
**Supervisor:** Vasilij Savin  
**Phase 1 Project:** EuroGlobe Bank (design specification)  
**Phase 2 Project:** E-Bank — implementing the technical specification designed by team Fantastic 5  

This agreement applies to the **whole Software Design project**, from the first lab to the final submission.

## 1. Team members

| No. | Full name | Group | GitHub username |
| --- | --------- | ----- | --------------- |
| 1. | Óscar Marín Castro | Exchange | @Oscar21-prog |
| 2. | Víctor Jesús Castañeda Morales | Exchange | @vicctoor18 |
| 3. | Jaime de Luiz García | Exchange | @jaimedeluiz |
| 4. | Martín Rodríguez Ramos | Exchange | @martinrdmos |
| 5. | Pablo Polledo Blanco | Exchange | @paabloo9 |

## 2. Project phases

| Phase | What we do | Main output |
| ----- | ---------- | ----------- |
| 1. Design | Write the technical specification and API contract of EuroGlobe Bank (first lab). | Shared specification document (`first-lab-assignment.pdf`) |
| 2. Implementation | Implement E-Bank from Fantastic 5's specification (`E-Bank_Technical_Specification.txt`), working only from their documentation. | Source code in GitHub, tests, demo |
| 3. Evaluation | Write an evaluation of Fantastic 5's documentation (`docs/evaluation.md`), and support the team implementing our design when allowed. | Written evaluation |
| 4. Final submission | Finish, test and present the implementation, and close every open item. | Final code, documentation and presentation |

## 3. Team goals

- Deliver a working, tested implementation of E-Bank following the specification, and document unclear or incomplete parts professionally in `docs/decisions.md` and `docs/deviations.md`.
- Write a fair, specific and useful evaluation of the other team's documentation.
- Aim for a high grade (9/10 or higher).
- Make sure everyone participates equally and learns every part of the process: design, implementation and review.

## 4. Roles and responsibilities

Each member owns one area of the work in each phase. Another member reviews that area before it counts as done. Owning an area means being responsible for it, not working on it alone.

### Phase 1 — Design (first lab — Completed)

| Role | Responsibilities | Owner |
| ---- | ---------------- | ----- |
| Product Owner / Requirements | Overview, target audience, MoSCoW feature list, user stories with measurable acceptance criteria. Confirms scope with the lecturer. | Víctor Jesús Castañeda Morales |
| Software Architect / Patterns | High-level architecture, package and class diagrams, design patterns and their justification. | Óscar Marín Castro |
| Process Analyst / Data Model | Business flows (sequence diagrams), data model with typed fields, enums and states. | Jaime de Luiz García |
| UI/UX Designer | Low-fidelity wireframes for all major use cases, screen transitions, a description of every button and panel. | Pablo Polledo Blanco |
| API Contract / Integrator / QA | API contract, events, error codes, traceability matrix, final integration of the document, "implementer's hat" review. | Martín Rodríguez Ramos |

### Phases 2–4 — Implementation, evaluation and final submission (E-Bank)

Work is split by isolated modules and shared deliverables as defined in `AGENTS.md`:

| Member | Modules Owned | Documents & Shared Responsibilities |
| ------ | ------------- | ----------------------------------- |
| Óscar Marín Castro | **Accounting + Notification + Shared Infrastructure** (`BuildingBlocks`, `Host`, `docker-compose.yml`, `.sln`) | Architecture & tech decisions (`docs/architecture.md`), team agreement, final deviations (`docs/deviations.md` with Jaime) |
| Víctor Jesús Castañeda Morales | **CBS + Payments** | Implementation proposal (`docs/proposal.md`) |
| Jaime de Luiz García | **KYC + CRM** | Questions & decisions log (`docs/decisions.md`), final deviations (`docs/deviations.md` with Óscar) |
| Pablo Polledo Blanco | **Cards + Loans** | GitHub Issues backlog (all 69 features), project board, screen scope (`docs/screens.md`) |
| Martín Rodríguez Ramos | **AML** | Evaluation of the E-Bank specification (`docs/evaluation.md`) |

## 5. Communication

- **WhatsApp group:** day-to-day communication, quick questions, urgent notices, and notifying the team whenever shared contracts (`*.Contracts`) or `BuildingBlocks` are updated.
- **Weekly meeting** (20–30 minutes, in person or online): review progress, solve blockers and plan the next week.
- **Response time:** within 24 hours on weekdays.
- If a member cannot attend a meeting or meet a deadline, they tell the team **in advance**.
- Important decisions (scope, architecture, naming, how we interpret unclear parts of the other team's design) are written down in `docs/decisions.md` so nobody misses them.

## 6. Tools & AI Rule

- **Shared repository & docs:** GitHub repository (`main` branch workflow per `AGENTS.md` §8), Markdown docs in `/docs`, and GitHub Issues for tracking features.
- **Tech stack:** .NET 10 (LTS), PostgreSQL 16, EF Core 10, React + TypeScript + Vite, xUnit + FluentAssertions + Testcontainers.
- **AI Assistance Rule:** AI assistants (Claude Code, Cursor, Copilot, Codex, ChatGPT) must read `AGENTS.md` before writing any code. When working for a member, AI must **only modify that member's assigned module folders** (`src/Modules/<Module>/`, `tests/EBank.<Module>.Tests/`, `frontend/src/modules/<module>/`). Changes to `BuildingBlocks`, `Host`, or cross-module contracts must be requested to the corresponding owner. Every member must understand and be able to defend all code they commit.

## 7. Workload and contribution

- We all commit to an equal contribution (about 20% each) in **every** phase, not only overall.
- Contribution is visible through GitHub commits, test coverage, and completed module features tracked via GitHub Issues.
- If someone falls behind, we discuss it openly at the weekly meeting and rebalance tasks. The goal is to finish the work, not to blame anyone.

## 8. Quality rules

### Implementation & Git Workflow (aligned with `AGENTS.md`)

- **Direct `main` workflow with strict folder ownership:** Each member works inside their own module folders (`src/Modules/<Module>/`, `tests/EBank.<Module>.Tests/`, `frontend/src/modules/<module>/`). Never edit, move, or delete another member's files.
- **Pull before working and before pushing:** Always run `git pull` before starting work and before pushing. Push small, frequent commits prefixed with the feature ID (e.g., `ACC-01: ...`, `NTF-02: ...`). Never force-push (`git push --force`).
- **Four-eyes review & Definition of Done:** Code must build cleanly (`dotnet build`), all unit and integration tests must pass (`dotnet test`), and another member reviews the module before final sign-off.
- **No silent assumptions:** When the E-Bank specification is unclear or missing details, choose the simplest consistent interpretation, log it in `docs/decisions.md`, and inform the team.

### Evaluation

- The evaluation of the other team's documentation (`docs/evaluation.md`) is based on concrete examples (section, page, missing detail), not on general opinions.
- It is respectful and constructive, and it also mentions the strengths of their work.

## 9. Deadlines

- Internal deadline: every deliverable (document sections, implementation milestones, evaluation) is finished **at least 2 days before** the official deadline, so there is time for review and integration.
- Final versions are frozen before the official submission deadline. No edits after that.

## 10. Conflict resolution

1. Disagreements are first discussed respectfully between the people involved.
2. If there is no agreement, the whole team discusses it at the next meeting and decides by majority vote.
3. If a problem continues (for example, repeated missed deadlines or lack of participation), the team will contact the lecturer.

## 11. Changes to this agreement

This agreement can be updated at any time during the project if the whole team agrees and the team is notified.

## 12. Acceptance

By adding our names below, we agree to follow this team agreement (updated for Phase 2 on 20/11/2026).

| Name | Date | Accepted |
| ---- | ---- | -------- |
| Óscar Marín Castro | 20/11/2026 | YES |
| Víctor Jesús Castañeda Morales | 20/11/2026 | YES |
| Jaime de Luiz García | 20/11/2026 | YES |
| Martín Rodríguez Ramos | 20/11/2026 | YES |
| Pablo Polledo Blanco | 20/11/2026 | YES |

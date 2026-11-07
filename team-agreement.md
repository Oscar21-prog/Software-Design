# Team Agreement — EuroGlobe Bank

**Course:** Software Design — Faculty of Mathematics and Informatics, Vilnius University
**Supervisor:** Vasilij Savin
**Project:** EuroGlobe Bank — digital banking platform (mortgages, car leasing, multi-signature shared vaults, cards and round-up micro-investments)

This agreement applies to the **whole Software Design project**, from the first lab to the final submission, and not only to the first lab.

## 1. Team members

| No. | Full name | Group | GitHub username |
| --- | --------- | ----- | --------------- |
| 1. | Óscar Marín Castro | Exchange | @Oscar21-prog |
| 2. | Víctor Jesús Castañeda Morales | Exchange | @vicctoor18 |
| 3. | Jaime de Luiz García | Exchange | @jaimedeluiz|
| 4. | Martín Rodríguez Ramos | Exchange | @martinrdmos|
| 5. | Pablo Polledo Blanco | Exchange | @paabloo9|

## 2. Project phases

| Phase | What we do | Main output |
| ----- | ---------- | ----------- |
| 1. Design | Write the technical specification and API contract of EuroGlobe Bank (first lab). | Shared specification document (`first-lab-assignment.pdf`) |
| 2. Implementation | Implement the system designed by another student team, working only from their documentation. | Source code in GitHub, tests, demo |
| 3. Evaluation | Write an evaluation of the other team's documentation, and support the team implementing our design when the course allows it. | Written evaluation |
| 4. Final submission | Finish, test and present the implementation, and close every open item. | Final code, documentation and presentation |

## 3. Team goals

- Deliver a technical specification and API contract that another team can implement **without needing to ask us anything**.
- Deliver a working, tested implementation of the other team's design, and handle unclear or incomplete parts of their documentation in a professional way.
- Write a fair, specific and useful evaluation of the other team's documentation.
- Aim for a high grade (9/10 or higher).
- Make sure everyone participates equally and learns every part of the process: design, implementation and review.

## 4. Roles and responsibilities

Each member owns one area of the work in each phase. Another member reviews that area before it counts as done. All members can (and should) help in other areas when needed. Owning an area means being responsible for it, not working on it alone.

### Phase 1 — Design (first lab)

| Role | Responsibilities | Owner |
| ---- | ---------------- | ----- |
| Product Owner / Requirements | Overview, target audience, MoSCoW feature list, user stories with measurable acceptance criteria. Confirms scope with the lecturer. | Víctor Jesús Castañeda Morales|
| Software Architect / Patterns | High-level architecture, package and class diagrams, design patterns and their justification. | Óscar Marín Castro|
| Process Analyst / Data Model | Business flows (sequence diagrams), data model with typed fields, enums and states. | Jaime de Luiz García|
| UI/UX Designer | Low-fidelity wireframes for all major use cases, screen transitions, a description of every button and panel. | Pablo Polledo Blanco|
| API Contract / Integrator / QA | API contract, events, error codes, traceability matrix, final integration of the document, "implementer's hat" review, evaluation of the other team's document. | Martín Rodríguez Ramos|

### Phases 2–4 — Implementation, evaluation and final submission

When we receive the other team's documentation, we split the implementation work by module or layer (for example: backend/business logic, API, data/persistence, frontend/UI, testing and integration). We record the split in this section or in GitHub Issues **before** we start coding. Every member writes code, writes tests and reviews other members' work.

| Area | Responsibilities | Owner |
| ---- | ---------------- | ----- |
| Team coordinator | Keeps GitHub Issues and the task board up to date, and runs the weekly meeting. This role rotates every two weeks. | Rotating |
| Documentation questions log | Keeps a list of every unclear, missing or contradictory point found in the other team's documentation, with the decision we took for each one. | To be agreed |
| Evaluation of the other team | Collects everyone's input and writes the final evaluation of the other team's documentation. | To be agreed |
| Other implementation areas | Assigned when the other team's design is received. | To be agreed |

## 5. Communication

- **WhatsApp group:** day-to-day communication, quick questions and urgent notices.
- **Weekly meeting** (20–30 minutes, in person or online): review progress, solve blockers and plan the next week.
- **Response time:** within 24 hours on weekdays.
- If a member cannot attend a meeting or meet a deadline, they tell the team **in advance**.
- Important decisions (scope, architecture, naming, how we interpret unclear parts of the other team's design) are written down in GitHub or in the shared document so nobody misses them.

## 6. Tools

- **Shared document:** Word / Google Docs for the specification and the evaluation shared with the lecturer.
- **Diagrams:** draw.io, PlantUML or Mermaid, with one consistent style across the whole document.
- **GitHub:** repository for submissions, this agreement and the implementation code. GitHub Issues and Pull Requests track every task.

## 7. Workload and contribution

- We all commit to an equal contribution (about 20% each) in **every** phase, not only overall.
- Tasks are split at the start of each phase and recorded in writing (`reparto.md` for phase 1, GitHub Issues for the rest).
- Contribution is visible through GitHub commits, Pull Requests, reviews and the shared document history.
- If someone falls behind, we discuss it openly at the weekly meeting and rebalance tasks. The goal is to finish the work, not to blame anyone.

## 8. Quality rules

### Design and documentation

- **Common glossary:** entity, state, module and event names are agreed at the start and used identically in every section.
- **Four-eyes rule:** every section is reviewed by at least one member who did not write it.
- **No vague requirements:** every acceptance criterion has a concrete number or rule (no "fast", "several" or "secure" on its own).
- **Traceability:** every Must requirement can be traced to a pattern, a flow, a screen, an entity and an endpoint/event.
- Before submission, we run an "implementer's hat" review: we read the document as if we had to build it, and fix every open question.

### Implementation

- Nobody pushes directly to `main`. Every change goes through a Pull Request.
- Every Pull Request needs at least one approval from another member before merging.
- Tests must pass before merging.
- Requested changes in a review are addressed within 48 hours.
- We implement what the other team's documentation says. When something is unclear or missing, we do not guess silently: we write it in the documentation questions log, agree on a decision as a team, and record it.

### Evaluation

- The evaluation of the other team's documentation is based on concrete examples (section, page, missing detail), not on general opinions.
- It is respectful and constructive, and it also mentions the strengths of their work.

## 9. Deadlines

- Internal deadline: every deliverable (document sections, implementation milestones, evaluation) is finished **at least 2 days before** the official deadline, so there is time for review and integration.
- Final versions are frozen before the official submission deadline. No edits after that.

## 10. Conflict resolution

1. Disagreements are first discussed respectfully between the people involved.
2. If there is no agreement, the whole team discusses it at the next meeting and decides by majority vote.
3. If a problem continues (for example, repeated missed deadlines or lack of participation), the team will contact the lecturer.

## 11. Changes to this agreement

This agreement can be updated at any time during the project if the whole team agrees. Changes are made through a Pull Request so that everyone can see them.

## 12. Acceptance

By adding our names below, we agree to follow this team agreement.

| Name | Date | Accepted |
| ---- | ---- | -------- |
| Óscar Marín Castro | 28/10/2026 | YES |
| Víctor Jesús Castañeda Morales | 28/10/2026 | YES |
| Jaime de Luiz García | 30/10/2026 | YES |
| Martín Rodríguez Ramos | 3/11/2026 | YES |
| Pablo Polledo Blanco | 4/11/2026 | YES |

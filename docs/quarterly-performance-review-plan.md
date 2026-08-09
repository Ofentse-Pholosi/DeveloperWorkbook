# Quarterly Performance Review — Implementation Plan

## Overview

Extend the Developer Workbook to support recurring quarterly performance reviews.
The feature is a separate module from the existing onboarding workbook — it does not
modify the current workbook flow, entities, or pages.

Both developers and managers can initiate a review for a given quarter. Developers
fill in a structured self-assessment with 1–5 ratings and notes per metric. Managers
then score the developer independently and record a performance trajectory.

---

## Metrics / Self-Assessment Categories

Each rated item has a **1–5 score** (1 = needs improvement, 5 = exceptional) plus
a free-text notes field. Each category also has a general notes box.

### 1. Deliverables & Output
- Goal completion from last quarter
- Features / tickets delivered vs committed
- Bug rate (defects introduced)
- PR turnaround time

### 2. Code Quality
- Code quality (self-rated)
- Peer review participation
- Adherence to team standards
- Test coverage contribution

### 3. Reliability & Professionalism
- Availability and responsiveness
- Estimate accuracy
- Scope adherence

### 4. Collaboration & Communication
- Proactiveness in surfacing blockers
- Knowledge sharing (docs, walkthroughs)
- Standup / async communication quality
- Pair programming participation

### 5. Learning & Growth
- Skills acquired this quarter
- Courses / certifications completed
- Areas of noticeable improvement

### 6. Goals for Next Quarter
- Free-text only: 2–3 concrete goals
- Skills to develop
- What support is needed from manager

### Manager Counter-Assessment
- Overall rating (1–5)
- Strengths callout (free text)
- Improvement areas (free text)
- Next quarter goals set by manager
- Performance trajectory: **On track / Needs support / Exceeding expectations**
- Additional feedback (free text)

---

## Data Model

### `PerformanceReview` (top-level MongoDB document)

| Field | Type | Notes |
|---|---|---|
| `Id` | ObjectId | MongoDB primary key |
| `DeveloperEmail` | string | Links to `Users.Email` |
| `ManagerEmail` | string | Links to manager `Users.Email` |
| `Quarter` | int | 1–4 |
| `Year` | int | e.g. 2026 |
| `Status` | string | `Draft` \| `Submitted` \| `Reviewed` |
| `CreatedAt` | DateTime | When review was first created |
| `SubmittedAt` | DateTime? | When developer submitted to manager |
| `ReviewedAt` | DateTime? | When manager completed their assessment |
| `SelfAssessment` | List\<ReviewSection\> | Developer's rated responses |
| `ManagerAssessment` | ManagerAssessment? | Null until manager acts |

Upsert key: `(DeveloperEmail, Quarter, Year)` — one review per developer per quarter.

### `ReviewSection`

| Field | Type |
|---|---|
| `Category` | string |
| `Items` | List\<RatedItem\> |
| `GeneralNotes` | string |

### `RatedItem`

| Field | Type | Notes |
|---|---|---|
| `Label` | string | e.g. "Goal completion from last quarter" |
| `Rating` | int | 0 = not yet rated, 1–5 = score |
| `Notes` | string | Optional context |

### `ManagerAssessment`

| Field | Type |
|---|---|
| `OverallRating` | int (1–5) |
| `Strengths` | string |
| `ImprovementAreas` | string |
| `NextQuarterGoals` | string |
| `PerformanceTrajectory` | string (`OnTrack` \| `NeedsSupport` \| `ExceedingExpectations`) |
| `AdditionalFeedback` | string |

---

## Implementation Layers

### Layer 1 — `Workbook.Core`

New files:
- `Entities/PerformanceReview.cs`
- `Entities/ReviewSection.cs`
- `Entities/RatedItem.cs`
- `Entities/ManagerAssessment.cs`

### Layer 2 — `Workbook.Application`

New file:
- `Interfaces/IPerformanceReviewRepository.cs`

Methods:
```csharp
Task<PerformanceReview?> GetByDeveloperQuarterAsync(string developerEmail, int quarter, int year);
Task<List<PerformanceReview>> GetByDeveloperEmailAsync(string developerEmail);
Task<List<PerformanceReview>> GetByManagerEmailAsync(string managerEmail);
Task UpsertAsync(PerformanceReview review);
```

### Layer 3 — `Workbook.Infrastructure`

New file:
- `Data/PerformanceReviewRepository.cs`

MongoDB collection: `"PerformanceReviews"`. Upsert keyed on `(DeveloperEmail, Quarter, Year)`.
Register in `MongoDbContext`.

### Layer 4 — `Workbook.WebApp`

**Config:**
- `performanceReviewSections.json` — categories and rated items, mirrors the
  pattern of `workbookSections.json` so metrics can be changed without code edits.

**Provider:**
- `Services/PerformanceReviewSectionProvider.cs` implementing
  `IPerformanceReviewSectionProvider`

**New pages:**

| Page | Who | Purpose |
|---|---|---|
| `Pages/QuarterlyReview.cshtml` | Developer | Self-assessment form; auto-detects current quarter/year; draft autosave; submit to manager |
| `Pages/ManagerPerformanceReview.cshtml` | Manager | Read-only view of developer self-assessment + manager scoring panel; set performance trajectory |

**Updated pages:**

| Page | Change |
|---|---|
| `Pages/ManagerDashboard.cshtml` | Add "Q Review" status column per developer: `Not Started \| Draft \| Submitted \| Reviewed` |

**DI registration** in `Program.cs`:
- Register `IPerformanceReviewRepository` → `PerformanceReviewRepository`
- Register `IPerformanceReviewSectionProvider` → `PerformanceReviewSectionProvider`

---

## Page Flow

```
Developer                                  Manager
    |                                          |
Opens QuarterlyReview (Q/year auto-set)        |
Fills ratings + notes per category            |
Saves draft (autosave)                        |
Submits →  Status = "Submitted"               |
                                    Sees "Submitted" on ManagerDashboard
                                    Opens ManagerPerformanceReview
                                    Views developer self-assessment (read-only)
                                    Fills counter-scores + trajectory
                                    Submits → Status = "Reviewed"
    |                                          |
Sees "Reviewed" badge + manager feedback       |
```

---

## Non-Goals (out of scope for this iteration)

- Historical trend charts across quarters
- Aggregate team-level reporting
- Email notifications on review submission (can reuse existing `EmailService` later)

---

## PDF Export

**Implemented (browser print):** `QuarterlyReview`, `ManagerReview`, and `ManagerPerformanceReview`
each have a "Print / Save PDF" button (`window.print()`) backed by a `@media print` stylesheet in
`site.css`. It forces accordion sections open, strips nav/footer/action buttons (`.no-print`), and
renders notes as plain text. Users generate a PDF via their browser's "Save as PDF" print destination.
No server-side dependency, but output styling depends on the browser's print engine and there's no
file to store, email, or bulk-export.

**Desired future implementation (server-generated PDF):** replace/augment this with a proper
server-side export — e.g. via [QuestPDF](https://www.questpdf.com/) — producing a downloadable
`application/pdf` file from the compiled `PerformanceReview` / `WorkbookAnswer` data directly, so
reviews can be emailed, archived, or exported in bulk for HR without depending on browser print
behavior. Roughly: add the QuestPDF dependency, an `IPerformanceReviewPdfService` (and onboarding
equivalent) that lays out the existing section/rating/feedback structure, and a page handler
(e.g. `OnGetDownloadPdf`) returning a `FileResult`.

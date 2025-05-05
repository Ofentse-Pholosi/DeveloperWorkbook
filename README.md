# Dev Onboarding Workbook 📘

This is a modern ASP.NET Core Web App that helps new developers track their journey and growth within a team. Built using Clean Architecture principles, MediatR, and MongoDB — it provides a simple, extensible structure to capture onboarding insights.

---

## 🚀 Project Purpose

The workbook is designed to:
- Help developers document their onboarding experience.
- Provide structured reflection sections like team integration, skills tracking, and growth.
- Automatically save progress (per section).
- Offer login/register functionality with persistent user-specific drafts using MongoDB.
- Enable easy future sharing of workbook progress with mentors/team leads.

---

## 🧱 Tech Stack

- **ASP.NET Core Razor Pages**
- **MediatR** (CQRS for separation of concerns)
- **MongoDB** (Document-based persistence)
- **Cookie-based Authentication** (no JWT)
- **Clean Architecture** (Domain, Application, Infrastructure, Web layers)

---

## 📁 Folder Structure Overview

```plaintext
📦 DevWorkbook
├── Application        # CQRS Handlers, Commands, Queries
├── Domain             # Core domain models (e.g., User, WorkbookSection)
├── Infrastructure     # MongoDB implementation, Authentication service
├── Web                # Razor UI, Pages, Startup configs, Views
└── SharedKernel       # Common utilities/interfaces (optional)

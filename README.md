# Group Order Manager API

A REST API for coordinating group-buy orders end to end — built as a .NET 10 backend portfolio project, with every design decision made to be defensible in an interview, not just functional.

**The problem it solves:** someone organizing a group order (e.g. a K-pop group buy) needs to track who's paying for what, calculate who owes how much, and let participants claim items without needing an account. This API handles all of that.

## Features

- **Owner authentication** — JWT-based auth for group order organizers (register/login, password hashing via PBKDF2)
- **Ownership checks** — only an order's owner can add items, close it, or mark claims paid; anyone else gets a 404
- **Group orders** — create, view, and close orders with a title, deadline, and description; closed orders reject new items, participants, and claims
- **Items** — add items with price, quantity available, and deadline
- **Anonymous participant claims** — participants claim items by name/contact info only, no account required
- **Automatic amount-owed calculation** — computes what each participant owes across all their claims
- **Payment tracking** — mark claims as paid/unpaid
- **Optimistic concurrency control** — prevents overselling an item when two people claim the last unit at the same time (the loser gets a 409 and can retry)
- **Consistent error responses** — a global exception handler returns RFC 9457 ProblemDetails with the right status code (400 / 401 / 404 / 409)

## Tech Stack

- **ASP.NET Core Web API** (.NET 10, Minimal APIs)
- **Entity Framework Core** + **PostgreSQL**
- **xUnit** — 44 tests: domain rules, plus service tests (auth, ownership, closed orders, claims) using EF Core's in-memory provider
- **Docker & Docker Compose** — containerized API + Postgres, runs with a single `docker compose up`
- **GitHub Actions CI** — restores, builds, and runs all tests on every push
- **JWT Bearer authentication** (HMAC-SHA256)

## Architecture

Clean/Onion architecture — four projects, dependencies only ever pointing inward:

```
GroupOrderManager.Api             → HTTP endpoints, DI wiring, auth config
GroupOrderManager.Infrastructure  → EF Core, PostgreSQL, service implementations, JWT/password hashing
GroupOrderManager.Application     → DTOs and service interfaces
GroupOrderManager.Domain          → Entities and business rules — zero external dependencies
```

Business rules never depend on how they're persisted or exposed. `GroupOrderItem` acts as an aggregate root: claiming an item (`ClaimFor()`) validates the quantity invariant and creates the claim atomically, so it's structurally impossible to update one without the other.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker (for PostgreSQL)

### Setup

1. **Start PostgreSQL:**
   ```bash
   docker run --name gom-postgres -e POSTGRES_PASSWORD=devpassword -e POSTGRES_DB=gom -p 5432:5432 -d postgres
   ```

2. **Configure connection string and JWT settings** in `GroupOrderManager.Api/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "GomDatabase": "Host=localhost;Port=5432;Database=gom;Username=postgres;Password=devpassword"
     },
     "Jwt": {
       "Key": "your-dev-only-secret-key-at-least-32-characters-long",
       "Issuer": "GroupOrderManager",
       "Audience": "GroupOrderManager"
     }
   }
   ```

3. **Apply migrations:**
   ```bash
   dotnet ef database update --project GroupOrderManager.Infrastructure --startup-project GroupOrderManager.Api
   ```

4. **Run the API:**
   ```bash
   dotnet run --project GroupOrderManager.Api
   ```

5. **Run the tests:**
   ```bash
   dotnet test
   ```

## API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | — | Register a new owner account |
| POST | `/auth/login` | — | Log in, returns a JWT |
| POST | `/group-orders` | Logged in | Create a group order (you become its owner) |
| GET | `/group-orders/{id}` | — | Get order details (shareable public link) |
| PATCH | `/group-orders/{id}/close` | Owner of the order | Close a group order |
| POST | `/group-orders/{groupOrderId}/items` | Owner of the order | Add an item to an order |
| POST | `/group-orders/{groupOrderId}/participants` | — | Add a participant |
| POST | `/items/{itemId}/claims` | — | Claim an item (anonymous) |
| GET | `/group-orders/{id}/amount-owed` | — | Get amount owed per participant |
| PATCH | `/claims/{id}/paid` | Owner of the order | Mark a claim as paid |
| PATCH | `/claims/{id}/unpaid` | Owner of the order | Mark a claim as unpaid |

## Design Decisions

Key architectural choices — aggregate root pattern, dependency inversion between Application/Infrastructure, `decimal` for money, optimistic concurrency via Postgres `xmin`, and why services-per-entity over CQRS — are covered in depth in interviews; happy to walk through the reasoning for any of them.

## Roadmap

- [ ] Frontend (React/Next.js)
- [x] Docker Compose for full-stack local dev
- [x] CI/CD via GitHub Actions
- [ ] Deployment to AWS

# WarehouseOps — Practical Assignment (.NET 8, Clean Architecture)

## Overview

You are given a **partially built** multi-tenant warehouse operations system.  
It manages stock levels, stock movements, and customer orders across multiple warehouse tenants.

Your job is to **fix bugs**, **implement missing features**, and **make both applications run end-to-end**.

> ⏱ Expected time: **2–3 hours**  
> 🎯 Experience level: **4–7 years**

---

## Tech Stack

- .NET 8, ASP.NET Core MVC + Web API
- Entity Framework Core 8 (Code First, SQL Server LocalDB)
- LINQ (method syntax, aggregations, grouping)
- jQuery AJAX + JSON
- Clean Architecture with Repository Pattern
- Multi-tenancy via request header

---

## Project Structure

```
WarehouseOps.Domain          → Entities, Enums, Repository & Service Interfaces   (do not modify)
WarehouseOps.Application     → DTOs, Service Interfaces, Service Implementations  ← your work here
WarehouseOps.Infrastructure  → EF DbContext, Repositories, Seed, TenantContext    ← your work here (middleware)
WarehouseOps.Web             → ASP.NET Core MVC — Stock UI, Dashboard             ← your work here
WarehouseOps.Api             → ASP.NET Core Web API — Orders REST API             ← your work here
```

---

## Getting Started

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8 SDK
- SQL Server LocalDB

### Setup
1. Open `WarehouseOps.slnx`
2. Connection string in both `appsettings.json` files uses `(localdb)\mssqllocaldb` — works out of the box
3. The database is **created and seeded automatically** on first run
4. Set `WarehouseOps.Web` as startup project to test the MVC app
5. Set `WarehouseOps.Api` as startup project to test the REST API (Swagger available at `/swagger`)

### Multi-Tenancy
All requests must include the header:
```
X-Tenant-Slug: warehouse-alpha
```
or
```
X-Tenant-Slug: warehouse-beta
```
Both tenants are seeded. Without this header your middleware should return `400 Bad Request`.

---

## Your Tasks

---

### Task 1 — Fix 5 Bugs in `StockService.cs`
**File:** `WarehouseOps.Application/Services/StockService.cs`

There are **5 bugs** marked with `// BUG` comments.

For **each bug**:
1. Fix the code
2. Write a comment above your fix with:
   - What was wrong
   - What the **production impact** would be (data loss? security? wrong results?)
   - Why your fix is correct

> The production impact question is intentional — we want to see that you think beyond "it returns wrong data".

---

### Task 2 — Implement `OrderService.cs`
**File:** `WarehouseOps.Application/Services/OrderService.cs`

Implement all 4 methods marked `// TODO`:

#### TODO 1 — `GetOrdersAsync`
- Filter by tenant, optionally by status
- **Filter first, then paginate** (you will see why this matters from the bug in StockService)
- `TotalValue` = sum of `QuantityOrdered * UnitPriceAtOrder` per line
- `FulfilledLines` = count of lines where `QuantityFulfilled >= QuantityOrdered`

#### TODO 2 — `GetOrderDetailAsync`
- Return `Result.Failure` if order not found or belongs to a different tenant
- Map all lines to `OrderLineDto`

#### TODO 3 — `FulfillOrderAsync` ⭐ (most complex)
Business rules — enforce **all** of them:
- Reject if order status is `Fulfilled` or `Cancelled`
- For each line, fulfill as much as current stock allows (partial is OK)
- For each fulfilled line, record an `Outbound` `StockMovement`
- Deduct fulfilled quantity from `product.CurrentStock`
- If all lines fully fulfilled → `Status = Fulfilled`, set `FulfilledAt`
- If some lines fulfilled → `Status = PartiallyFulfilled`
- If no lines could be fulfilled (all products out of stock) → `Result.Failure`
- **Save everything in a single `SaveChangesAsync` call**

#### TODO 4 — `GetOverdueOrdersAsync`
- `Pending` orders older than 3 days, ordered by `OrderedAt` ascending

---

### Task 3 — Implement `DashboardService.cs`
**File:** `WarehouseOps.Application/Services/DashboardService.cs`

Implement `GetDashboardAsync`. All fields are documented in the file.

Key requirement:
> **Do NOT call `GetAllAsync` more than once per collection.**  
> Load each collection once, reuse it with in-memory LINQ.  
> We will ask you about this in the walkthrough.

`TopMovedProducts` — top 5 products by total `Outbound` quantity in the **last 30 days**.

---

### Task 4 — Implement Tenant Middleware
**File:** Create `WarehouseOps.Infrastructure/Middleware/TenantResolutionMiddleware.cs`

Implement a middleware class that:
1. Reads the `X-Tenant-Slug` request header
2. Returns `400 Bad Request` if the header is missing or empty
3. Looks up the tenant using `ITenantRepository.GetBySlugAsync`
4. Returns `404 Not Found` if the tenant does not exist or `IsActive == false`
5. Populates `TenantContext.TenantId` and `TenantContext.TenantSlug`
6. Calls `next(context)` to continue the pipeline

Also complete the DI registration in both `Program.cs` files:
- Register `TenantContext` as `scoped`
- Register it as both `ITenantContext` and `TenantContext` resolving to the **same instance**
- Register and use the middleware

---

### Task 5 — Implement MVC Controllers
**File:** `WarehouseOps.Web/Controllers/StockController.cs`  
**File:** `WarehouseOps.Web/Controllers/DashboardController.cs`

Implement all actions marked `// TODO` in both controllers.  
The views are already built — your controller output must match what the views expect.

For `StockController.RecordMovement` (AJAX POST):
- Return `Json(new { success = true })` or `Json(new { success = false, error = "..." })`

For `DashboardController.Index`:
- Pass `DashboardDto` as the strongly typed model
- Pass overdue orders via `ViewBag.OverdueOrders`

---

### Task 6 — Implement REST API Endpoints
**File:** `WarehouseOps.Api/Controllers/OrdersController.cs`

Implement all 4 endpoints marked `// TODO`.  
Use correct HTTP status codes:
- `200 OK` — success
- `400 Bad Request` — business rule failure
- `404 Not Found` — entity not found

---

### Bonus (if time permits)

Add a new API endpoint:
```
GET api/stock/low-stock
```
- Returns all products below reorder level for the current tenant
- Grouped by `Category` in the response JSON
- Shape: `[{ category, products: [...] }]`

---

## Rules

- Do **not** modify `WarehouseOps.Domain`
- Do **not** modify `WarehouseOps.Infrastructure` except to add the middleware
- Do **not** use raw SQL — LINQ only
- Do **not** put business logic in controllers or repositories
- All data access must be **tenant-scoped** — a tenant must never see another tenant's data
- Use `Result<T>` for operations that can fail with a business reason
- Save all related changes in a **single** `SaveChangesAsync` call where applicable

---

## What We Are Evaluating

| Area | What we look for |
|---|---|
| Bug fixing | Correct diagnosis, production impact awareness, minimal targeted fix |
| LINQ | Correct grouping, aggregation, no N+1, filter-before-paginate |
| Multi-tenancy | Middleware implementation, consistent tenant scoping across all queries |
| Business logic | FulfillOrder rules all enforced, partial fulfillment handled correctly |
| REST API | Correct status codes, proper use of `Result<T>`, clean controller code |
| Architecture | Right layer for each concern, no leaking of EF into Application layer |
| DI | Correct scoped registration, same-instance resolution for TenantContext |

---

## Seed Data Reference

| Entity | Tenant Alpha | Tenant Beta |
|---|---|---|
| Products | 6 (3 below reorder) | 2 (1 below reorder) |
| Orders | 4 (Pending, Overdue, PartiallyFulfilled, Fulfilled) | 0 |
| Stock Movements | 6 | 0 |

**Overdue order trap:** `ORD-2024-002` is 4 days old and still Pending — it must appear in overdue results.  
**Cross-tenant trap:** Tenant Beta has a product with `SKU = ELEC-001`, same as Tenant Alpha. Queries must not mix them.  
**Zero-stock trap:** `Monitor Stand` (FURN-002) has `CurrentStock = 0`. An Outbound movement against it must be rejected.

---

## Submission

1. Zip the solution folder (exclude `bin/` and `obj/`)
2. Name it: `WarehouseOps_YourName.zip`
3. Email it back

> 💬 After submission you will do a walkthrough.

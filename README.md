# Code-Plus Tasks: Enterprise .NET Architecture Roadmap

This repository contains practical architectural implementations demonstrating the evolution of modern .NET backend systems from Vertical Slice Architecture to Clean Architecture, CQRS, Caching, Materialized Views, and Background Processing.

---

## 📂 Repository Structure

```text
Code-Plus-Tasks/
├── Task1_VerticalSlice_ShopFlow/   # Task 1: Vertical Slice Architecture
└── Orderflow/                     # Task 2: Clean Architecture + CQRS + Caching + Hangfire
```

---

## 🛍️ Task 1: ShopFlow — Vertical Slice Architecture

### Overview
A streamlined e-commerce backend built with **Vertical Slice Architecture**, organizing code by feature/use-case rather than technical layers. Each feature encapsulates its own request, handler, domain logic, and endpoint.

### Features & Structure
```text
Task1_VerticalSlice_ShopFlow/
├── Features/
│   ├── Cart/
│   │   └── AddToCart/             # Add product to shopping cart
│   ├── Order/
│   │   └── CreateOrder/           # Place a new order
│   └── Product/
│       └── GetProduct/            # Retrieve product catalog details
├── Domain/                        # Cart, Order, Product models
├── Data/                          # EF Core AppDbContext & SQL Server migrations
└── Program.cs                     # Minimal API endpoints registration
```

### Key Highlights
- **Feature Encapsulation:** High cohesion within slices; changes to one feature don't impact others.
- **Minimal APIs:** Direct endpoint mapping without bulky controllers.
- **EF Core:** SQL Server transactional storage.

---

## 🚀 Task 2: OrderFlow — Clean Architecture & Advanced Patterns

### Overview
OrderFlow is an enterprise e-commerce backend showcasing how a real application evolves from simple CRUD into **Clean Architecture**, **CQRS**, **Result Pattern**, **FluentValidation**, **Redis Caching**, a **Materialized View**, and **Background Processing with Hangfire**.

### Architecture & Layers

```text
Orderflow/
├── Domain/                        # Enterprise Core (Entities, Enums - Zero dependencies)
│   ├── Models/                    # Order, OrderItem, Product, OrderDashboardReadModel
│   └── Enums/                     # OrderStatus (Pending, Completed, Cancelled)
│
├── APP/                           # Application Layer (CQRS Features & Business Rules)
│   ├── Common/
│   │   ├── Behaviors/             # MediatR ValidationBehavior pipeline
│   │   ├── Interfaces/            # IAppDbContext, ICacheService, IBackgroundJobService
│   │   ├── Models/                # PagedResult<T>
│   │   └── Results/               # Result & Result<T> pattern
│   └── Features/                  # Vertical Slices with MediatR Commands & Queries
│       ├── Orders/
│       │   ├── Commands/CreateOrder/ # Create order, stock deduction, delayed job
│       │   └── Query/                # GetOrderById (Cached) & GetAllOrders (Paginated)
│       └── Dashboard/
│           └── Queries/           # GetDashboardOrders (Materialized View read)
│
├── Infra/                         # Infrastructure Layer (I/O, DB, Caching, Jobs)
│   ├── BackgroudJobs/             # OrderProcessingJob (Hangfire job implementation)
│   ├── Caching/                   # RedisCacheService (IDistributedCache with fallback)
│   ├── Configeration/             # ServiceCollection DI extensions & Hangfire setup
│   ├── Data/                      # AppDbcontext, precision rules, seed data
│   └── Migrations/                # EF Core migrations
│
└── API/                           # Presentation Layer (Composition Root)
    ├── Controllers/               # OrdersController, DashboardController
    └── Program.cs                 # Swagger, Hangfire Dashboard, Middleware pipeline
```

---

## 🛠️ Architectural Patterns & Design Decisions

### 1. CQRS (Command Query Responsibility Segregation)
- **Commands:** Mutate state (e.g. `CreateOrderCommand`), enforce business rules (stock deduction, status transitions).
- **Queries:** Read data without side-effects (e.g. `GetOrderQuery`, `GetAllOrdersQuery`, `GetDashboardOrdersQuery`).
- **MediatR:** Dispatches all commands and queries through a clean mediator pipeline.

### 2. Result Pattern (`Result<T>`)
- Replaces throwing raw exceptions for expected domain failures (e.g. product not found, insufficient stock).
- `Result<T>` explicitly returns `IsSuccess`, `Value`, or `Error`.
- Clean HTTP status mapping in controllers (`Ok`, `BadRequest`, `NotFound`).

### 3. FluentValidation & MediatR Pipeline Behavior
- Automatic input validation before reaching handlers using `ValidationBehavior<TRequest, TResponse>`.
- Failures return a aggregated `Result.Failure(...)` without halting the pipeline with 500 exceptions.

### 4. Read-Optimized Materialized View
- **Problem:** Aggregating dashboard data (order sums, item counts, customer names) across large transactional tables causes expensive SQL `JOIN`s and table locks.
- **Solution:** Maintained a dedicated read table `OrderDashboards` (`OrderDashboardReadModel`).
- **Performance:** `GET /api/dashboard/orders` executes a lightning-fast single-table `SELECT` with **0 JOINs** and `AsNoTracking()`.

### 5. Background Processing with Hangfire
- **Decoupled Job Scheduling:** `CreateOrderHandler` saves the order with `OrderStatus.Pending` and schedules a 10-second delayed background job.
- **Execution:** Hangfire runs `OrderProcessingJob` asynchronously outside the HTTP request to:
  1. Transition the order from `Pending` to `Completed`.
  2. Invalidate the Redis cache for the completed order.
  3. Refresh the Materialized View dashboard table.
- **Visual Dashboard:** Monitor jobs in real-time at `/hangfire`.

### 6. Distributed Caching with Redis (Cache-Aside Pattern)
- **Cache-Aside:** `GET /api/orders/{id}` checks Redis (`orders:{id}`). On cache miss, queries SQL Server, caches result for 5 minutes, and returns.
- **Cache Invalidation:** When an order's status changes in background processing, its cache entry is automatically invalidated.
- **Resilience:** Gracefully falls back to SQL Server if Redis is temporarily unreachable.

---

## 🌐 API Endpoints Specification

| Method | Endpoint | Description | Pattern / Feature |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/orders` | Create an order for a customer | CQRS Command + FluentValidation + Delayed Job |
| `GET` | `/api/orders/{id}` | Get detailed order by ID | CQRS Query + Redis Cache-Aside |
| `GET` | `/api/orders` | List orders with pagination | CQRS Query (`pageNumber`, `pageSize`) |
| `GET` | `/api/dashboard/orders` | Read dashboard summaries | CQRS Query + Materialized View (0 JOINs) |
| `GET` | `/hangfire` | Background job monitor UI | Hangfire Dashboard |
| `GET` | `/swagger` | Interactive API documentation | Swagger UI |

---

## ⚙️ Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (LocalDB, Express, or Developer)
- [Redis](https://redis.io/) (Optional — system runs with fallback if offline)

### Configuration (`Orderflow/API/appsettings.json`)
```json
{
  "ConnectionStrings": {
    "cs": "Server=localhost;Database=OrderFlow;Trusted_Connection=True;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  }
}
```

### Running OrderFlow
1. Navigate to the OrderFlow project:
   ```bash
   cd Orderflow
   ```
2. Apply database migrations & seed test data:
   ```bash
   dotnet ef database update --project Infra --startup-project API
   ```
3. Run the API:
   ```bash
   dotnet run --project API
   ```
4. Access:
   - **Swagger:** `https://localhost:xxxx/swagger`
   - **Hangfire Dashboard:** `https://localhost:xxxx/hangfire`

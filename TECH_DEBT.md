# Concertable.Customer — Technical Debt

When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## HIGH

### `TicketPurchasedEvent` / `TicketRefundedEvent` not published

The inverse-direction event flow (Customer → B2B/Search) from plan §6 is not implemented. Customer's Ticket module processes payments via `TicketPaymentProcessor` but never publishes a `TicketPurchasedEvent`. As a result:

- B2B cannot build `ConcertSalesProjection` (sold-count / gross-revenue for dashboards + settlement math).
- Search cannot show "X tickets left" counts.

**Resolves when:** `TicketEntity.Create` raises a `TicketPurchasedDomainEvent`; an in-process handler bridges it to `TicketPurchasedEvent : IIntegrationEvent` defined in a new `Concertable.Customer.Ticket.Contracts` csproj; registered as `Publishes<TicketPurchasedEvent>()` in `Program.cs`; B2B.Workers and Search.Workers subscribe and handle.

---

### `ConcertReadModel` doesn't own `AvailableTickets`

Plan §4.5 specifies Customer's concert row as a **behavioural entity** — `ConcertEntity` with `DecrementAvailability(int)` / `RestoreAvailability(int)` methods and invariants (`AvailableTickets >= 0`, `AvailableTickets <= TotalTickets`). Currently `Modules/Concert/.../Domain/Entities/ConcertEntity.cs` contains a class named `ConcertReadModel` with no behaviour and no `AvailableTickets` field. Ticket capacity enforcement may be absent today.

**Resolves when:** Class renamed `ConcertEntity`; `AvailableTickets` field added (sourced from `ConcertChangedEvent`); `DecrementAvailability` / `RestoreAvailability` methods added; `TicketService.PurchaseAsync` decrements availability atomically in the same DB transaction as `TicketEntity` insert.

---

## MED

### Concert, Ticket, Preference modules lack `.Contracts` project

Cross-module access into these three modules is not behind an `IXModule` facade (per `api/docs/MODULAR_MONOLITH_RULES.md`). Any intra-Customer cross-module call reaches in directly.

**Resolves when:** Each gains a `Concertable.Customer.<Module>.Contracts` csproj with `I<Module>Module` + summary DTOs; callers switch to the facade; internal types stay `internal`.

---

### Missing test projects for Artist, Venue, Preference

`Concertable.Customer.Artist`, `Concertable.Customer.Venue`, and `Concertable.Customer.Preference` have no Unit or Integration test projects.

**Resolves when:** Each gains at minimum an Integration tests project following the pattern in `Modules/Review/Tests/` or `Modules/Ticket/Tests/`.

---

## LOW

### Read-model filename / class-name mismatch

Files under `Modules/Concert/`, `Modules/Artist/`, `Modules/Venue/` are named `*Entity.cs` but contain classes called `*ReadModel` (e.g. `ConcertEntity.cs` → `class ConcertReadModel`). This misleads anyone reading the file tree.

**Resolves when:** Files renamed to `*ReadModel.cs` to match the class name inside.

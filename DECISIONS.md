# Architectural Decision Log

This file records key design decisions made during the HotelStay implementation.
It is maintained automatically — each entry is added at the point the decision is made.

---

## ADR-001: .NET Minimal API over MVC Controllers
**Date:** 2026-06-14
**Status:** Decided

**Context:** Backend needs 3 endpoints. Choice between Minimal API and traditional MVC Controllers.

**Decision:** Use .NET Minimal API.

**Rationale:** The surface area is small (3 endpoints). Minimal API removes boilerplate — no controller classes, no `[HttpGet]` attributes, no base class inheritance. Endpoints are defined inline in `Program.cs` with explicit route handlers, making the intent immediately readable.

**Trade-offs:** Less structure than MVC, which would matter in a larger API. For 3 endpoints this is a net win.

---

## ADR-002: IHotelProvider abstraction with IEnumerable<IHotelProvider> injection
**Date:** 2026-06-14
**Status:** Decided

**Context:** Need to query multiple hotel providers (PremierStays, BudgetNests, later BoutiqueCollection) from a single search endpoint without hardcoding provider logic in the aggregation layer.

**Decision:** Define `IHotelProvider` with `SearchAsync` and `BookAsync`. Register each concrete provider with DI as `IHotelProvider`. Inject `IEnumerable<IHotelProvider>` into the aggregation service.

**Rationale:** Adding a new provider only requires: (1) a new class implementing `IHotelProvider`, (2) one DI registration line. The aggregation loop over `IEnumerable<IHotelProvider>` never changes. This is the Open/Closed Principle applied directly.

**Trade-offs:** Providers are all called the same way, so provider-specific orchestration logic (e.g. different retry strategies per provider) would need to be handled inside each implementation, not centrally.

---

## ADR-003: In-memory booking store over a database
**Date:** 2026-06-14
**Status:** Decided

**Context:** Bookings need to be stored and retrieved by reference number.

**Decision:** Use a `ConcurrentDictionary<string, Booking>` registered as a singleton in DI.

**Rationale:** This is a challenge submission with stub providers — no persistence across restarts is required or expected. A real database would add setup complexity (migrations, connection strings, seeding) that distracts from the core evaluation criteria.

**Trade-offs:** Data is lost on restart. Not suitable for production.

---

## ADR-004: Parallel provider queries on search
**Date:** 2026-06-14
**Status:** Decided

**Context:** Search must query all registered providers and aggregate results.

**Decision:** Use `Task.WhenAll` to query providers concurrently. Wrap each provider call in a try/catch so a single provider failure returns partial results rather than a 500.

**Rationale:** Providers are independent stubs; there is no reason to query them sequentially. Resilience to individual provider failure is explicitly tested per the challenge spec.

**Trade-offs:** Partial results on provider failure — the response may silently omit one provider's results. A production system would surface warnings per provider.

---

## ADR-005: Document validation enforced both client-side and server-side
**Date:** 2026-06-14
**Status:** Decided

**Context:** Booking requires Passport for international destinations, National ID accepted for domestic. The spec explicitly requires dual validation.

**Decision:** Angular reactive form validator checks document type against destination before the form can be submitted. The backend independently re-validates and returns 422 if mismatched.

**Rationale:** Client-side validation gives immediate feedback without a network round-trip. Server-side validation is the authoritative guard — client-side can be bypassed (e.g. direct API calls, browser dev tools). Both are required by the spec.

**Trade-offs:** Duplicated logic (destination classification exists in both Angular and C#). Kept in sync via the shared hardcoded destination list.

---

## ADR-006: Unified normalisation model before aggregation returns
**Date:** 2026-06-14
**Status:** Decided

**Context:** PremierStays returns PascalCase JSON with amenities and star rating; BudgetNests returns snake_case JSON with an availability boolean and no amenities. The API response must be uniform.

**Decision:** Each provider's raw response is mapped to a shared `HotelResult` record inside the provider implementation (not in the aggregation layer). The aggregation layer only filters and sorts the already-normalised list.

**Rationale:** Keeps provider-specific parsing logic inside the provider. The aggregation service has no knowledge of PascalCase vs snake_case, or what fields each provider includes. Nullable fields (`Amenities`, `StarRating`) express "this provider doesn't supply this" without breaking the contract.

**Trade-offs:** Nullable fields in the unified model mean the frontend must handle absent values gracefully.

---

## ADR-007: Target net10.0 / Angular 22 (installed toolchain)
**Date:** 2026-06-15
**Status:** Decided

**Context:** Plan assumed .NET 8 SDK + Angular 17–19. The environment had no SDK and no Node; the installed versions are .NET SDK 10.0.301, Node 24.16.0, Angular CLI 22.0.1.

**Decision:** Target `net10.0` for both projects and use the Angular 22 CLI defaults, rather than down-pinning to net8.0.

**Rationale:** Building `net10.0` against the matching SDK avoids needing older targeting packs, and uses the toolchain actually present on the machine. Nothing in the challenge depends on a specific framework version — the code is plain Minimal API + xUnit + Angular reactive forms, all stable across these versions.

**Trade-offs:** A reviewer on an older SDK must install .NET 10 to build. Documented in the README setup section.

---

## ADR-008: Abstract only IHotelProvider; inject concrete services
**Date:** 2026-06-15
**Status:** Decided

**Context:** A house style of "interface per service" is common, but the only place with more than one implementation is the provider seam.

**Decision:** Keep `IHotelProvider` as the sole interface. `HotelSearchService`, `BookingService`, and `DocumentValidator` are concrete classes registered and injected directly.

**Rationale:** Interfaces with a single implementation and no mocking need are ceremony. Tests construct these services directly, so an interface buys nothing. `IHotelProvider` genuinely earns its abstraction — multiple implementations plus the Open/Closed requirement for new providers.

**Trade-offs:** If a second implementation of one of these services ever appears, an interface must be extracted then. Cheap to do when actually needed.

---

## ADR-009: Document rule — passport valid everywhere, national ID domestic-only
**Date:** 2026-06-15
**Status:** Decided

**Context:** The brief states "international ⇒ passport required" and "domestic ⇒ national ID accepted" but doesn't say whether a passport is acceptable domestically.

**Decision:** A passport is accepted for any destination; a national ID is accepted only for domestic ones. Equivalently: an international destination requires a passport, and anything else there is a 422.

**Rationale:** Mirrors reality — a passport is a superset travel document. It satisfies both stated rules and avoids the odd outcome of rejecting a passport for a domestic trip.

**Trade-offs:** Domestic + passport is allowed, which a stricter "exact document per destination" reading might reject. The chosen reading is the reasonable one and is unit-tested.

---

## ADR-010: Stub catalogs are destination-independent
**Date:** 2026-06-15
**Status:** Decided

**Context:** Providers must return deterministic, hardcoded responses. The brief doesn't require per-destination catalogs.

**Decision:** Each provider returns the same fixed room catalog regardless of destination. The destination drives document validation and total price (via nights), not the room list.

**Rationale:** Keeps the stubs deterministic and the normalisation logic the focus, without inventing a destination→catalog matrix the brief never asked for.

**Trade-offs:** Searches look identical across destinations. Acceptable for a stubbed challenge; a real integration would key the catalog by destination.

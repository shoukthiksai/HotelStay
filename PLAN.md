# HotelStay — Implementation Plan

Phased build order. Each phase has a clear exit criterion. Do not start a phase
until the previous one's exit criterion is met. Tick boxes as work completes.

Source of truth for requirements: [challange-hotelstay.md](challange-hotelstay.md).
Design rules and constraints: [CLAUDE.md](CLAUDE.md).
Decisions are logged in [DECISIONS.md](submissions/Shoukthik/usecase-002/DECISIONS.md) as they are made.

---

## Phase 0 — Repository scaffolding

- [x] Create `submissions/Shoukthik/usecase-002/` structure
- [x] `dotnet new sln` (→ `.slnx`), `dotnet new web -o HotelStay.Api`, `dotnet new xunit -o HotelStay.Tests`; Tests references Api
- [x] Add `.gitignore` (.NET template + Node/Angular entries)
- [x] Stub `spec.md`, `prompts.md`, `reflection.md` (filled in later phases)
- [x] Cleaned boilerplate: removed Hello World endpoint, deleted sample `UnitTest1.cs`

**Exit:** `dotnet build` succeeds on the empty solution. ✅ (0 warnings, 0 errors)

---

## Phase 1 — Domain model (the contract everything depends on)

Build this first — every later phase consumes these types. Get them right before
writing logic against them.

- [x] `RoomType` enum: `Standard, Deluxe, Suite`
- [x] `CancellationPolicy` enum: `FreeCancellation, Flexible, NonRefundable`
- [x] `DocumentType` enum: `Passport, NationalId`
- [x] `HotelResult` record (unified normalised model — see CLAUDE.md)
- [x] `SearchCriteria` / `BookingRequest` / `BookingConfirmation` records
- [x] `Booking` record (stored in-memory, keyed by reference)

**Exit:** Domain compiles. No logic yet — just shapes. ✅

---

## Phase 2 — Provider abstraction + two stubs

- [x] `IHotelProvider` interface (`SearchAsync`, `BookAsync`) — frozen; must not change
      when BoutiqueCollection is added in Phase 8
- [x] `PremierStaysProvider` — deterministic, all room types available, amenities
      + star rating, FreeCancellation/NonRefundable, higher rates
- [x] `BudgetNestsProvider` — deterministic, Deluxe `available: false`,
      no amenities/star rating, Flexible/NonRefundable, lower rates
- [x] Each provider maps its own raw shape (PascalCase / snake_case values, `JsonPropertyName`
      on the snake_case DTO) to `HotelResult` internally

**Exit:** Each provider returns a hardcoded `IReadOnlyList<HotelResult>`. Unit-tested. ✅

---

## Phase 3 — Aggregation + booking orchestration

- [x] `HotelSearchService` — injects `IEnumerable<IHotelProvider>`, queries all in parallel
      (`Task.WhenAll`), wraps each call so one failure yields partial results, filters
      `Available == false` and optional `roomType`, returns merged list
- [x] `BookingService` — routes booking to the selected provider by name, prices the stay,
      generates the reference, stores in-memory (`ConcurrentDictionary` field)
- [x] Concrete services injected (no redundant interfaces — see ADR-008)

**Exit:** Services work against the two providers with no HTTP layer yet. ✅

---

## Phase 4 — Document validation guard

- [x] Hardcoded destination registry (`Destinations`): domestic (London, Manchester) +
      international (Paris, New York, Tokyo, Dubai, Sydney)
- [x] `DocumentValidator` — passport valid everywhere, national ID domestic-only (ADR-009);
      returns a clear failure reason on mismatch
- [x] Wired into the book endpoint before the provider booking call (422 on failure)

**Exit:** Validator returns pass/fail with message for every destination+document combo. ✅

---

## Phase 5 — Minimal API endpoints

- [x] `GET /hotels/search` — query binding + 400 on missing required params + 400 when
      `checkOut <= checkIn` + optional `roomType`
- [x] `POST /hotels/book` — 422 on document mismatch, 400 on unavailable room, else confirmation
- [x] `GET /hotels/booking/{reference}` — 404 if unknown, else status
- [x] CORS for the Angular dev origin (`http://localhost:4200`); enums serialised as strings
- [x] `{ message }` error body across 400 / 422

**Exit:** All three endpoints smoke-tested via `Invoke-RestMethod` — correct status codes
and bodies (search filters BudgetNests Deluxe; book 422/200; total 780 = 3×260). ✅

---

## Phase 6 — Tests (HotelStay.Tests)

Mandated coverage — write these deliberately, not exhaustively:

- [x] Normalisation: each provider's raw shape → correct `HotelResult`
- [x] BudgetNests filtering: `available: false` rooms excluded (via `HotelSearchService`)
- [x] Document validation: passport everywhere, national ID domestic-only, unknown rejected
- [x] Provider failure: one provider throwing still yields the other's results
- [~] `checkOut <= checkIn`: enforced at the endpoint, verified by smoke test (not unit-tested —
      no HTTP host in the test project by design; would need WebApplicationFactory)

**Exit:** `dotnet test` green — **10 passed, 0 failed.** ✅

---

## Phase 7 — Frontend (hotelstay-ui, Angular)

- [x] `ng new hotelstay-ui` — Angular 22, standalone, **zoneless** (signals), SCSS
- [x] Typed `HotelService` (search, book) — `getBooking` omitted: no UI consumer, endpoint
      still exists server-side; `BookingFlow` signal service carries state between routes
- [x] Search form (reactive): destination dropdown, dates, optional room type;
      cross-field `checkOut > checkIn` validator
- [x] Results list: provider badge, room type, per-night + total price, cancellation
      label, stars/amenities when present; sortable by price asc/desc
- [x] Booking form (reactive) with client-side document/destination validator (mirrors server)
- [x] Confirmation view: reference, provider, total, cancellation policy

**Exit:** `ng build` passes; full search → book → confirm flow verified in-browser against the
live API (Paris booking, passport enforced, ref `HS-…`, total £720 = 4×180). ✅

---

## Phase 8 — Live tweak: BoutiqueCollection

Time-boxed (20–25 min). Proves the abstraction. Touch only new files + DI.

- [x] `BoutiqueCollectionProvider` — Deluxe/Suite only (Standard unavailable),
      base rate + £15/night flat fee, FreeCancellation, boolean availability
- [x] One DI registration line
- [x] Confirmed: `git diff` shows **+1 line in Program.cs** — `IHotelProvider`, aggregation,
      booking, and both existing providers untouched
- [x] 2 tests (fee math, Standard unavailable); live search shows Deluxe 315 / Suite 515

**Exit:** New provider appears in search with zero edits to existing logic. ✅ (12 tests green)

---

## Phase 9 — Documentation & submission polish

- [x] `README.md` — setup (dotnet + ng), run instructions, architecture, API ref, AI-usage summary
- [x] `spec.md` — interpreted spec / assumptions (document rule, versions, deliberate omissions)
- [x] `prompts.md` — where AI was used
- [x] `reflection.md` — what went well, trade-offs, second-pass changes
- [x] Trimmed default Angular README → pointer to main README
- [x] Verify Definition of Done (all met — table in README; see below)
- [x] No-secrets scan — clean
- [~] Prod API URL: **not done by design** — assignment runs locally; the API origin is a single
      documented constant (`API_BASE`). An environment file would be speculative for a non-deploy.

**Exit:** Every Definition of Done box ticks. ✅ — `dotnet test` 12 green, `ng build` passes.

---

## Blocker protocol

If a step in CLAUDE.md cannot be followed as written (ambiguous requirement,
conflicting constraint, tooling failure):

1. Re-read [challange-hotelstay.md](challange-hotelstay.md) — it is the authoritative
   requirement context and resolves most ambiguity.
2. If still blocked, log it under **Blockers** below with the question and the
   assumption taken to keep moving. Do not silently guess.
3. If the resolution is an architectural choice, also add an ADR to DECISIONS.md.

### Blockers
<!-- blockers will be listed here -->
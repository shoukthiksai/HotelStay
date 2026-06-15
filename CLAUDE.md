# HotelStay — Claude Instructions

## Working Documents

- **[challange-hotelstay.md](challange-hotelstay.md)** — the authoritative requirements (verbatim from the challenge PDF). When anything is ambiguous or you hit a blocker, re-read this first; it resolves most questions.
- **[PLAN.md](PLAN.md)** — phased build order. Follow it in sequence; tick boxes as work completes; log blockers in its Blockers section.
- **[DECISIONS.md](submissions/Shoukthik/usecase-002/DECISIONS.md)** — append an ADR whenever a non-trivial design decision is made.

## Code Quality Bar (READ FIRST)

This submission is judged on craftsmanship, not volume. Hold to these:

- **No AI slop.** No filler comments, no restating the obvious, no defensive code for impossible cases, no speculative abstractions "for the future."
- **Minimal surface area.** Write the least code that fully satisfies the requirement. Fewer files, fewer types, fewer lines — each one must earn its place.
- **Comment only the non-obvious.** Explain *why*, never *what*. If the code needs a comment to explain what it does, rewrite the code.
- **No scaffolding noise.** Delete generated boilerplate (sample weather endpoints, default Angular welcome component) rather than leaving it.
- **Consistency over cleverness.** Match patterns across the codebase. Idiomatic .NET and idiomatic Angular — don't invent house styles.
- **Stop when done.** Do not pad with extra endpoints, extra tests beyond the mandated coverage, or extra config. Completeness, not maximalism.

When in doubt, prefer the smaller, clearer solution and note the trade-off in DECISIONS.md.

## Blocker Protocol

If a rule here can't be followed as written (ambiguity, conflict, tooling failure):
1. Re-read [challange-hotelstay.md](challange-hotelstay.md) — it is the requirement source of truth.
2. If still blocked, record it in the Blockers section of [PLAN.md](PLAN.md) with the question and the assumption taken to proceed. Never silently guess.
3. If the resolution is architectural, also log an ADR in [DECISIONS.md](submissions/Shoukthik/usecase-002/DECISIONS.md).

## Project Overview

Hotel search and booking platform (SkyRoute Travel Platform challenge). Travellers search by destination, check-in/check-out dates, and room type. The system queries two provider stubs, normalises results, and presents a unified list. Booking validates document type before confirming.

This is a challenge submission — no starter code provided. Build from scratch.

## Submission Structure

```
submissions/Shoukthik/usecase-002/
├── README.md
├── spec.md
├── HotelStay.Api/
├── HotelStay.Tests/
├── hotelstay-ui/
├── prompts.md
└── reflection.md
```

## Tech Stack

- **Backend**: .NET Minimal API (C#)
- **Frontend**: Angular (with reactive forms)
- **Tests**: xUnit (or NUnit) for .NET

## Architecture Rules

### Provider Abstraction (CRITICAL)
- Define `IHotelProvider` interface with `SearchAsync` and `BookAsync` methods
- Two concrete stubs: `PremierStaysProvider` and `BudgetNestsProvider`
- Third provider `BoutiqueCollectionProvider` added later as live tweak
- **Never modify** aggregation/orchestration logic or `IHotelProvider` when adding a new provider — only add a new class and register it in DI
- Register all `IHotelProvider` implementations via `services.AddSingleton<IHotelProvider, XxxProvider>()` — inject `IEnumerable<IHotelProvider>`

### Unified Data Model
Always normalise provider responses into a single internal model before returning from the API:

```csharp
record HotelResult(
    string Provider,
    RoomType RoomType,
    decimal NightlyRate,
    CancellationPolicy CancellationPolicy,
    string[]? Amenities,      // null for BudgetNests/BoutiqueCollection
    int? StarRating,          // null for BudgetNests/BoutiqueCollection
    bool Available
);

enum RoomType { Standard, Deluxe, Suite }
enum CancellationPolicy { FreeCancellation, Flexible, NonRefundable }
```

## Provider Specifications (Stubs — hardcode deterministic data)

### PremierStays
- PascalCase JSON response fields
- Returns: RoomType, Rate, CancellationPolicy, Amenities[], StarRating
- CancellationPolicy values: `FreeCancellation` | `NonRefundable`
- Always returns availability for all room types (never unavailable)
- Higher rates

### BudgetNests
- snake_case JSON response fields
- Returns: room_type, rate, cancellation_policy, available (boolean)
- CancellationPolicy values: `Flexible` | `NonRefundable`
- May return `"available": false` — **filter these out in the aggregation layer, never return them to the client**
- Lower rates

### BoutiqueCollection (Live Tweak — add last)
- Supports Deluxe and Suite only; Standard always returns unavailable
- Rate = base nightly rate + £15 boutique_fee per night (flat, all room types)
- CancellationPolicy: `FreeCancellation` up to 72h before check-in
- Returns availability as boolean per room type

## API Endpoints

### GET /hotels/search
Query params: `destination` (required), `checkIn` (required), `checkOut` (required), `roomType` (optional)

- Return 400 if `destination`, `checkIn`, or `checkOut` missing
- Return 400 if `checkOut` is not after `checkIn`
- If `roomType` omitted, return all room types
- Query both providers in parallel, normalise, filter unavailable, return unified list
- Response includes per-night rate; frontend computes total (nights × rate + fees)

### POST /hotels/book
Body: `{ providerId, roomType, checkIn, checkOut, destination, passengerName, documentType, documentNumber }`

- Validate document type server-side:
  - International destination → Passport required → return 422 with clear message if National ID provided
  - Domestic destination → National ID accepted
- On success, return `{ referenceNumber, provider, totalPrice, cancellationPolicy }`

### GET /hotels/booking/{reference}
- Return booking status by reference number
- Store bookings in-memory (no DB needed for this challenge)

## Input Validation Rules

| Rule | HTTP Status |
|------|-------------|
| Missing destination/checkIn/checkOut | 400 |
| checkOut not after checkIn | 400 |
| Document type mismatch for destination | 422 |

## Destination List (hardcode these)

**Domestic (UK):**
- London
- Manchester

**International:**
- Paris
- New York
- Tokyo
- Dubai
- Sydney

Use this list for the Angular dropdown AND the server-side document validation guard.

## Frontend (Angular)

### Search Form (Reactive Form)
- Destination: dropdown (from hardcoded list above)
- Check-in date: date picker
- Check-out date: date picker
- Room type: optional dropdown (Standard / Deluxe / Suite / All)

### Results List
- Show per-provider badge (PremierStays / BudgetNests / BoutiqueCollection)
- Show: room type, per-night rate, **total stay price** (nights × nightly rate), cancellation policy label
- Sortable by total price ascending/descending

### Booking Form
- Passenger name (text)
- Document type: dropdown (Passport / National ID)
- Document number (text)
- Validate document type client-side before submit (Angular reactive form validator)

### Booking Confirmation Page
- Reference number
- Provider name
- Total price
- Cancellation policy

## Testing Requirements (HotelStay.Tests)

Must cover:
1. **Normalisation** — provider responses map correctly to unified model
2. **Document validation** — international → Passport, domestic → NationalId, mismatch → 422
3. **Provider failure** — one provider failing should not fail the entire search (return partial results)
4. **BudgetNests filtering** — `available: false` rooms excluded from results

## Definition of Done Checklist

- [ ] Both providers return normalised results; unavailable BudgetNests rooms filtered
- [ ] Per-night and total price displayed correctly on frontend
- [ ] Document validation fires client-side (Angular) AND server-side (backend)
- [ ] Booking flow completes end-to-end with a reference number
- [ ] `dotnet build` passes
- [ ] `ng build` passes
- [ ] Tests cover normalisation, document validation, and provider failure
- [ ] README has full setup instructions
- [ ] No secrets committed

## Key Constraints

- Stubs must be **deterministic** — hardcode representative responses, no randomness
- No database required — use in-memory storage for bookings
- No external HTTP calls — providers are stubs
- BoutiqueCollection must be addable without touching existing code (Open/Closed principle)
- Annotate code or write `prompts.md` documenting where Copilot/AI was used

## File Naming & Conventions

- C# files: PascalCase, one class per file
- Angular: kebab-case filenames, PascalCase component/service class names
- API project namespace: `HotelStay.Api`
- Tests project namespace: `HotelStay.Tests`

## Architectural Decision Log

Every time a non-trivial design decision is made during implementation, append an entry to `DECISIONS.md` (in `submissions/Shoukthik/usecase-002/`) using this format:

```markdown
## ADR-NNN: <short title>
**Date:** YYYY-MM-DD
**Status:** Decided

**Context:** Why this decision was needed.
**Decision:** What was chosen.
**Rationale:** Why this option over alternatives.
**Trade-offs:** What we give up.
```

Trigger this for decisions about: project structure, library choices, API design, data model shape, validation strategy, error handling approach, provider abstraction pattern, testing approach, or any "why not X?" moments.

Do NOT log trivial choices (variable names, formatting, boilerplate).

## What NOT to Do

- Do not add a real database — in-memory is sufficient
- Do not make real HTTP calls to external APIs
- Do not add auth/JWT — out of scope
- Do not modify `IHotelProvider` or aggregation logic when adding BoutiqueCollection
- Do not use `available: false` BudgetNests rooms anywhere in the response chain

# HotelStay: Hotel Search & Booking

## Scenario

Build a hotel search and booking platform for the SkyRoute Travel Platform. Travellers search by destination, check-in/check-out dates, and room type. The system queries two hotel providers, normalises the results, and presents a unified list. The traveller selects a room and books it. Document requirements are validated before booking is confirmed.

No starter code is provided. You are expected to design and implement the solution from the ground up. Please document your architectural decisions and rationale.

---

## Providers

### PremierStays

- Higher rates. Returns full property detail: room type, rate, cancellation policy, amenities list, star rating.
- `CancellationPolicy`: `FreeCancellation` (up to 48h before check-in) or `NonRefundable`.
- Always returns availability for all requested room types.
- Response field naming uses PascalCase JSON.

### BudgetNests

- Lower rates. Returns minimal detail: room type, rate, cancellation policy only. No amenities or star rating.
- `CancellationPolicy`: `Flexible` (up to 24h before check-in) or `NonRefundable`.
- May return `"available": false` for some room types — your aggregation layer must filter these out before returning results.
- Response field naming uses snake_case JSON.

Both providers return rates as per-night amounts. Your frontend must display both per-night and total-stay price.

---

## Room Types

Both providers support: `Standard`, `Deluxe`, `Suite`. Map both providers' room type representations to this unified enum.

---

## Copilot Usage Guidelines

You are expected to leverage GitHub Copilot for code generation, refactoring, and documentation throughout this challenge. Please annotate your code or provide a summary indicating where Copilot was used and how it influenced your solution.

---

## Submission Instructions

- Submit your solution as a GitHub repository or a zip file containing all source code, documentation, and any supporting files.
- Include a README file summarizing your approach, architectural decisions, and Copilot usage.
- Follow any provided naming conventions and ensure your code is well-documented and professional.

---

## Evaluation Criteria

Your submission will be evaluated on the following dimensions:

- Code quality and organization
- Correctness and completeness of the solution
- Effective and ethical use of Copilot
- Documentation and clarity of architectural decisions
- Professionalism and clarity of submission

Each dimension will be scored as pass/partial/fail with written feedback.

---

## Functional Scope

### Backend (.NET Minimal API)

- `GET /hotels/search?destination={city}&checkIn={date}&checkOut={date}&roomType={type}` — queries both providers, filters unavailable results, normalises, returns unified list
- `POST /hotels/book` — validates document requirement, then books with the selected provider
- `GET /hotels/booking/{reference}` — returns booking status
- Provider abstraction: `IHotelProvider` with two concrete stub implementations
- Stubs must be deterministic — hardcode a representative set of responses per provider

### Input Validation (backend)

- `destination`, `checkIn`, `checkOut` are required — return 400 if missing
- `checkOut` must be after `checkIn` — return 400 if not
- `roomType` is optional — if omitted, return all room types

### Document Validation

- International destinations (outside home country): Passport required
- Domestic destinations: National ID accepted
- Destination list must include at least 2 domestic and 3 international cities
- Validated client-side (Angular reactive form) AND server-side (backend guard)
- Return 422 with a clear error message if document type does not match destination

### Frontend (Choose your own)

- Search form: destination dropdown, check-in date, check-out date, room type (optional)
- Results list: per-provider badge, room type, per-night rate, total price, cancellation policy label; sortable by total price (ascending/descending)
- Booking form: passenger name, document type (Passport / National ID), document number
- Booking confirmation: reference number, provider name, total price, cancellation policy

---

## Live Tweak Scenario (Add New Provider)

### Add BoutiqueCollection provider:

- Rate structure: base nightly rate + a `boutique_fee` (flat £15 per night, applied regardless of room type)
- Supports `Deluxe` and `Suite` only — `Standard` returns unavailable
- Cancellation policy: `FreeCancellation` up to 72h before check-in
- Returns availability as a boolean per room type

The new provider must be added without modifying:

- The aggregation or booking orchestration logic
- The `IHotelProvider` interface
- Existing provider implementations

Only a new `IHotelProvider` implementation and DI registration should change.

Time budget for live tweak: 20–25 minutes.

---

## Submission Structure

```
submissions/<your-name>/usecase-002/
├── README.md
├── spec.md
├── HotelStay.Api/
├── HotelStay.Tests/
├── hotelstay-ui/
├── prompts.md
└── reflection.md
```

---

## Definition of Done

- [ ] Both providers return normalised results; unavailable BudgetNests rooms are filtered
- [ ] Per-night and total price displayed correctly
- [ ] Document validation fires both client-side and server-side
- [ ] Booking flow completes end-to-end with a reference number
- [ ] `dotnet build` and `ng build` pass
- [ ] Tests cover normalisation, document validation, and provider failure
- [ ] README has full setup instructions
- [ ] No secrets committed

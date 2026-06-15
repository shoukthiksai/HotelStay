# HotelStay — Hotel Search & Booking

A hotel search and booking platform for the SkyRoute Travel Platform. Travellers search by
destination and dates, the system queries multiple provider stubs, normalises their differing
response shapes into one model, and presents a unified, sortable list. Booking validates the
traveller's document type against the destination before confirming.

Full requirements: [challange-hotelstay.md](../../../challange-hotelstay.md).
Design decisions and rationale: [DECISIONS.md](DECISIONS.md).

## Tech stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 Minimal API (C#) |
| Frontend | Angular 22 (standalone, zoneless, reactive forms) |
| Tests | xUnit |

Built and verified against .NET SDK **10.0.301**, Node **24.16.0**, Angular CLI **22.0.1**.

## Architecture

```
Angular UI ──HTTP──▶ Minimal API ──▶ HotelSearchService ──▶ IEnumerable<IHotelProvider>
                                          (parallel, resilient)      ├─ PremierStaysProvider
                                                                      ├─ BudgetNestsProvider
                                                                      └─ BoutiqueCollectionProvider
```

- **Provider seam (`IHotelProvider`).** The only abstraction in the system, because it's the
  only place with multiple implementations. Each provider owns its own wire format — PremierStays
  speaks PascalCase with amenities and star ratings; BudgetNests and BoutiqueCollection speak
  snake_case with neither — and normalises it to the unified `HotelResult` internally. The
  aggregator never sees provider-specific shapes.
- **Aggregation (`HotelSearchService`).** Queries every registered provider in parallel
  (`Task.WhenAll`). A provider that throws yields partial results rather than failing the whole
  search. Unavailable rooms (e.g. BudgetNests `"available": false`) and the optional room-type
  filter are applied here, in one place.
- **Document validation (`DocumentValidator`).** A passport is accepted anywhere; a national ID
  only for domestic destinations. Enforced server-side (422) and mirrored client-side in the
  Angular form.
- **Booking (`BookingService`).** Routes to the chosen provider by name, prices the stay
  (`nights × nightly rate`), and stores the confirmation in memory.
- **Open/Closed.** BoutiqueCollection was added as a new `IHotelProvider` plus a single DI line —
  no change to the interface, the services, or the existing providers (see commit history).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) 20+ and the Angular CLI (`npm install -g @angular/cli`)

## Running it

Two processes — start the API first, then the UI. All commands run from this
`usecase-002/` directory.

### 1. Backend API (port 5168)

```bash
dotnet run --project HotelStay.Api
```

Serves at `http://localhost:5168`. CORS is open to the Angular dev origin
(`http://localhost:4200`).

### 2. Frontend (port 4200)

```bash
cd hotelstay-ui
npm install
npm start
```

Open `http://localhost:4200`. The API origin is the single constant `API_BASE` in
[hotel.service.ts](hotelstay-ui/src/app/hotel.service.ts) — change it there if your API runs on a
different host or port.

### 3. Tests

```bash
dotnet test
```

## API reference

| Method | Route | Notes |
|--------|-------|-------|
| `GET` | `/hotels/search?destination=&checkIn=&checkOut=&roomType=` | `400` if a required param is missing or `checkOut <= checkIn`. `roomType` optional. Returns the unified, availability-filtered list. |
| `POST` | `/hotels/book` | `422` if the document type doesn't match the destination; `400` if the room is unavailable; otherwise the confirmation. |
| `GET` | `/hotels/booking/{reference}` | `404` if unknown, else the stored booking. |

Dates are `yyyy-MM-dd`. Enums serialise as strings (`"Suite"`, `"FreeCancellation"`).

**Example**

```bash
curl "http://localhost:5168/hotels/search?destination=Paris&checkIn=2026-07-01&checkOut=2026-07-05"
```

## Destinations

Domestic (UK): **London, Manchester**. International: **Paris, New York, Tokyo, Dubai, Sydney**.
The same list drives the Angular dropdown and the server-side document guard.

## Testing

`HotelStay.Tests` (xUnit) covers the mandated areas plus the live tweak:

- **Normalisation** — each provider's wire shape maps to the correct `HotelResult`
- **Filtering** — `available: false` rooms are excluded by the aggregator
- **Document validation** — passport everywhere, national ID domestic-only, unknown rejected
- **Provider failure** — one provider throwing still returns the others' results
- **BoutiqueCollection** — flat fee math and Standard-never-available

## Project structure

```
HotelStay.Api/
  Domain/         enums + records (HotelResult, SearchCriteria, booking models)
  Providers/      IHotelProvider + the three stubs
  HotelSearchService.cs   BookingService.cs   DocumentValidator.cs   Destinations.cs
  Program.cs      DI registration + the three endpoints
HotelStay.Tests/  xUnit coverage
hotelstay-ui/     Angular app (search → booking → confirmation)
```

## AI usage

Built with AI assistance (Claude Code) under human architectural direction. See
[prompts.md](prompts.md) for where and how AI was used, and
[DECISIONS.md](DECISIONS.md) for the decisions behind the design.

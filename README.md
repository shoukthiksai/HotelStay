# HotelStay

Coding challenge from EPAM — a hotel search and booking platform (SkyRoute Travel Platform).
Travellers search by destination and dates; the system queries multiple provider stubs,
normalises their differing responses into one model, and presents a unified, sortable list.
Booking validates the traveller's document type against the destination before confirming.

## The solution

The full submission lives in **[`submissions/Shoukthik/usecase-002/`](submissions/Shoukthik/usecase-002/)** —
start with its **[README](submissions/Shoukthik/usecase-002/README.md)** for architecture, setup,
and run instructions.

- **Backend** — .NET 10 Minimal API ([`HotelStay.Api`](submissions/Shoukthik/usecase-002/HotelStay.Api/))
- **Frontend** — Angular 22 ([`hotelstay-ui`](submissions/Shoukthik/usecase-002/hotelstay-ui/))
- **Tests** — xUnit ([`HotelStay.Tests`](submissions/Shoukthik/usecase-002/HotelStay.Tests/))
- **Decisions** — [DECISIONS.md](submissions/Shoukthik/usecase-002/DECISIONS.md) ·
  **AI usage** — [prompts.md](submissions/Shoukthik/usecase-002/prompts.md) ·
  **Reflection** — [reflection.md](submissions/Shoukthik/usecase-002/reflection.md)

## Working documents

These capture the process behind the build:

- [challange-hotelstay.md](challange-hotelstay.md) — the requirements, transcribed from the brief
- [PLAN.md](PLAN.md) — the phased build order, with exit criteria and blockers
- [CLAUDE.md](CLAUDE.md) — the working agreement and code-quality bar followed throughout

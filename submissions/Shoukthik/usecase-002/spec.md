# HotelStay — Interpreted Specification

How the challenge requirements were interpreted, including assumptions made where the brief
left room for judgement. The authoritative brief is
[challange-hotelstay.md](../../../challange-hotelstay.md); design rationale is in
[DECISIONS.md](DECISIONS.md).

## Assumptions

- **Document rule.** The brief states "international ⇒ passport required" and "domestic ⇒
  national ID accepted" but is silent on whether a passport works domestically. Interpreted as:
  a passport is valid everywhere, a national ID only domestically. So an international destination
  requires a passport; anything else there is a 422. (ADR-009)
- **Home country.** Taken as the UK, giving domestic = London, Manchester and international =
  Paris, New York, Tokyo, Dubai, Sydney (meets the "≥2 domestic, ≥3 international" requirement).
- **Provider catalogs are destination-independent.** Stubs return a fixed, deterministic room
  set regardless of destination; the destination drives document validation and the total price
  (via nights), not the room list. (ADR-010)
- **Total price = nights × nightly rate.** The API returns per-night rates; the frontend computes
  the total. BoutiqueCollection's flat £15/night fee is folded into its nightly rate, so the same
  `nights × rate` formula holds.
- **`providerId` is the provider name.** Bookings reference a provider by its `Name`
  (e.g. `"PremierStays"`), which is what search results already carry.
- **Booking reference** is a generated short code (`HS-XXXXXXXX`); only provider *responses* must
  be deterministic, not references.
- **Framework versions.** Targeted .NET 10 / Angular 22 — the toolchain actually installed —
  rather than the originally-assumed .NET 8 / Angular 17–19. Nothing in the brief depends on a
  specific version. (ADR-007)

## Deliberate omissions

- **`getBooking` is not called from the UI** — the confirmation view uses the booking response
  directly, so no consumer exists. The `GET /hotels/booking/{reference}` endpoint still exists
  and is part of the API surface.
- **No service interfaces beyond `IHotelProvider`** — the other services have a single
  implementation and are injected concretely. (ADR-008)

## Out of scope

- Persistence — bookings are held in memory.
- Authentication / authorisation.
- Real provider HTTP integrations — providers are deterministic stubs.
- Production deployment config — the app runs locally; the API origin is a single documented
  constant.

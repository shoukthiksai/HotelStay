# Reflection

## What went well

- **The provider seam paid off exactly as intended.** Adding BoutiqueCollection touched one line
  of existing code (a DI registration). The Open/Closed requirement wasn't just satisfied — it's
  visible as a self-contained commit in the history.
- **Normalisation stayed at the edges.** Because each provider maps its own wire format to the
  unified model internally, the aggregation, validation, and booking logic never branch on
  provider identity. That kept those services small and the tests focused.
- **Validating both sides from one source.** The domestic/international split lives in one place
  per tier (server `Destinations`, client `DESTINATIONS`), so the 422 guard and the Angular
  validator can't silently drift on the rule itself.
- **Verification beyond green tests.** The endpoints were smoke-tested and the full UI flow driven
  in a browser, which caught the things unit tests don't — CORS, enum serialisation, zoneless
  change detection on async responses.

## Trade-offs taken

- **In-memory bookings.** No persistence; data is lost on restart. Right for the challenge, wrong
  for production.
- **Only `IHotelProvider` is abstracted.** The other services are concrete. This avoids
  ceremony with single-implementation interfaces, at the cost of extracting one later if a
  second implementation ever appears.
- **Destination-independent catalogs.** Searches look the same across destinations. Keeps the
  stubs deterministic and the focus on normalisation, but isn't how a real integration would behave.
- **State held in a signal service, not the URL.** Simple and type-safe, but a hard refresh of
  `/book` or `/confirmation` redirects home rather than restoring state.

## What a second pass would change

- Persist bookings (even SQLite) and have the confirmation view re-fetch by reference via the
  existing `GET /hotels/booking/{reference}` endpoint, so a refresh or shared link survives.
- Add a thin layer of API integration tests (`WebApplicationFactory`) for the 400/422 paths,
  which are currently covered by smoke testing rather than automated tests.
- Surface per-provider failures in the search response (a warning) instead of silently returning
  partial results.
- Move the API origin into an Angular environment file once there's an actual deployment target.

# AI / Copilot Usage

The challenge asks submitters to leverage AI for code generation and to document where and how it
was used. This solution was built with **Claude Code** (an agentic AI assistant) under human
architectural direction.

## How AI was used

- **Requirements capture.** The challenge PDF was read and transcribed into
  [challange-hotelstay.md](../../../challange-hotelstay.md) as the authoritative reference, and
  distilled into a project instruction file and a phased plan.
- **Scaffolding.** Generating the .NET solution and Angular app, then stripping the template
  boilerplate (sample endpoint, default welcome component, placeholder test).
- **Implementation.** Writing the domain model, the three provider stubs and their normalisation,
  the aggregation/validation/booking services, the Minimal API endpoints, and the Angular
  components, service, and reactive forms.
- **Tests.** Authoring the xUnit suite covering normalisation, filtering, document validation,
  provider failure, and the BoutiqueCollection fee logic.
- **Verification.** Running `dotnet build`/`dotnet test`/`ng build`, smoke-testing the endpoints,
  and driving the full search → book → confirm flow in a browser to confirm behaviour.

## Human direction

The architecture and the decisions behind it were human-directed, not accepted blindly:

- The provider abstraction, unified model, and Open/Closed constraint were specified up front.
- Ambiguities (e.g. the document rule) were resolved explicitly and recorded as ADRs rather than
  guessed.
- A code-quality bar — minimal surface area, no speculative abstractions, comment only the
  non-obvious — was enforced throughout, and AI output was trimmed to meet it.
- Each phase was reviewed and verified before moving on; design choices are logged in
  [DECISIONS.md](DECISIONS.md).

## Effectiveness

AI was most useful for the mechanical breadth — scaffolding, the repetitive
per-provider normalisation, form wiring, and test boilerplate — which left more room to focus on
the design seams: the provider contract, the normalisation boundary, and the validation rule.

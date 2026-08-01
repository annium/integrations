# Annium.Integrations

Wrappers around external services, packaged as NuGet libraries: AI providers and orchestration under
`ai/`, social-platform bots under `social/`. Every project targets `net10.0` and is published to
`https://dotnet.pkg.annium.com` — except the two demo hosts and the legacy Telegram project.

This repo holds thin integration layers, not framework code. The framework (DI, logging, HTTP,
serialization, testing) comes from the `Annium.*` packages built in the `base` sub-project.

## Quick Reference

```bash
just                  # list every recipe
just setup            # dotnet tool restore (csharpier, xs, doclint, versioning, docfx)
just format           # csharpier + xs format -sc -ic
just build            # Release build, package version computed from ./version
just test             # dotnet test with TRX logging
just docs-lint        # doclint — XML docs gate, part of CI
just clean            # xs clean + remove stray *.nupkg
just update           # reinstall dotnet tools, xs update all
```

Single test: `dotnet test <path/to/Project.Tests> --filter "FullyQualifiedName~Name"`.

`make` is gone — the makefile was replaced by the `justfile` in `05894a3`.

## Project Structure

```
integrations/
├── justfile                        # every workflow (build, test, docs, CI, release)
├── Annium.Integrations.sln         # one solution; folder nesting mirrors the paths below
├── Directory.Build.props           # net10.0, nullable, WarningsAsErrors, TreatWarningsAsErrors
├── Directory.Packages.props        # central package versions (ManagePackageVersionsCentrally)
├── version                         # "1.1" — base version consumed by `versioning get-version`
├── ai/
│   ├── openai/
│   │   ├── src/Annium.Integrations.AI.OpenAI/           # OpenAIConfig + keyed client registration
│   │   └── tests/…                                      # 3 tests
│   └── semantickernel/
│       ├── src/Annium.Integrations.AI.SemanticKernel/           # kernel builder, plugins, MCP
│       ├── src/Annium.Integrations.AI.SemanticKernel.AspNetCore/# EMPTY — csproj only, no sources
│       └── tests/…                                              # 4 tests
└── social/telegram/
    ├── src/Annium.Integrations.Social.Telegram/          # current Bot API integration
    ├── src/…Telegram.Obsolete/                           # legacy; IsPackable=false, see below
    ├── tests/Annium.Integrations.Social.Telegram.Tests/  # 8 tests
    └── demo/{…Demo, …Demo.Obsolete}/                     # console hosts, IsPackable=false
```

## Key Patterns

- **Keyed registration everywhere.** Each integration instance is registered under a key
  (`AddOpenAI("chat-client", …)`, `AddTelegramBot("demo", …)`), so several clients or bots coexist in
  one container. Factories receive that key and resolve their own configuration with it.
- **Config as a delegate, resolved late.** `GetOpenAIConfig(IServiceProvider) → OpenAIConfig` is
  registered rather than a bound instance, so configuration loaded after registration (or a rotated
  secret) is still picked up. Same shape for `AddTelegramBot(key, sp => …)`.
- **Builder over container.** `AddSemanticKernel()` returns an `ISemanticKernelBuilder` that only
  carries the container; `With*` extensions write registrations into it. `AddTelegramBot(…, opts => …)`
  does the same through `BotOptions`.
- **Plugin discovery needs a scanned assembly.** `WithPluginInstances()` finds
  `ISemanticKernelPlugin` implementations through Annium's type manager. Without `AddRuntime(assembly)`
  **and** `[assembly: AutoScanned]` (an `Asm.cs` file, by convention) it registers nothing and fails
  silently — the kernel simply resolves with zero plugins.
- **Registration may be async.** `ServicePackBase` hooks are `ConfigureAsync` / `RegisterAsync` /
  `SetupAsync` since Annium 1.1.40, and `Entrypoint…SetupAsync()` is awaited. Work that needs I/O at
  startup (e.g. `WithMcpFunctionsFromHttpServerAsync`, which connects to an MCP server and lists its
  tools) belongs there — never behind a blocking `.Result` in a DI factory.
- **Receivers own background loops.** `ITelegramMessageReceiver` implementations publish updates to a
  `Channel<Update>`, are `IAsyncDisposable`, and are registered as singletons. They must complete the
  channel when they stop — the host awaits it and would otherwise hang forever.
- **External API responses are surfaced, never dropped.** Telegram answers a rejected call with
  `ok:false` plus a `description`; that description is the only diagnostic available and must be
  logged or propagated.
- **Provider secrets stay out of logs.** `OpenAIConfig.ToString()` is overridden to redact the API
  key (a record would otherwise print it), and bot tokens must never appear in a log line or an
  exception message.
- **No Result pattern here.** Unlike `base`, these integrations return plain values (`bool`, domain
  records) or throw; don't introduce `IResult<T>` without a reason.

## Domain modelling for external APIs

Telegram's payloads are modelled as records with `[JsonPropertyName]`. **Only mark a member
`required` if the provider documents it as always present.** `System.Text.Json` throws on a missing
required member, and a throw during deserialization of a batch loses every item in it — this was the
cause of the worst bug found in this repo (a single photo message permanently wedged the poll loop).
When in doubt, make it nullable.

## Testing

- xunit.v3 + `Annium.Testing`; fluent assertions `.Is()`, `.IsTrue()`, `.Has()`, `.At()`,
  `.IsEmpty()`, `.NotNull()`, `.IsDefault()`; exceptions via `Wrap.It().Throws<T>()`.
- Naming: `Method_Scenario_ExpectedResult`.
- Test projects are `{Project}.Tests` under `<area>/tests/`, `IsTestProject=true`, `IsPackable=false`,
  `Solutions=Annium.Integrations`, each with its own `.gitignore` for build and test output.
- **Prefer a real local server over mocks.** `Annium.Integrations.Social.Telegram.Tests` starts an
  `Annium.Net.Servers.Web` server standing in for the Bot API and drives the real HTTP stack,
  serializer and domain model against it. Let the listener pick its own port (`ServerBuilder.New(sp)`
  with no port) — binding a hand-picked port races parallel tests.
- `xUnit1051` is an error here: pass `TestContext.Current.CancellationToken` into anything that takes
  one. Where a wait must be bounded, link it with a timeout instead of waiting on the bare token.
- Reaching internals (e.g. `ApiContext`, `MessageApi`) requires `InternalsVisibleTo` in the source
  project's `Asm.cs`.

## Code Quality

- `WarningsAsErrors` + `TreatWarningsAsErrors` + `EnableNETAnalyzers`: **any** warning fails the
  build, including IDE style rules and VSTHRD threading rules.
- `LOG0001` (Annium analyzer) forbids non-constant log message templates — use
  `this.Error<string>("call failed: {description}", value)`, not string interpolation.
- CSharpier, 120 columns, 4 spaces (`.csharpierrc`); `just format` must leave the tree clean, and CI
  fails if it does not (`ensure-no-changes`).
- XML docs are enforced by `just docs-lint` on every project **except**
  `social/telegram/src/Annium.Integrations.Social.Telegram.Obsolete/**`. No `<inheritdoc/>`: write
  explicit summaries, including on interface implementations and test methods.

## Known State

- **`…Social.Telegram.Obsolete` is frozen.** `IsPackable=false`, kept only for existing consumers,
  excluded from review and from the docs gate by explicit decision (2026-08-01). It has a known
  unfixed defect: `TelegramApi.SendAsync` returns `Ok=false` with an empty description for any
  non-empty response body, and its deserialization is commented out — every successful call reads as
  a failure. It is a separate API generation from the current Telegram project; do not merge them.
- **`…SemanticKernel.AspNetCore` contains no source** — a csproj with a `ModelContextProtocol.AspNetCore`
  reference and nothing else. It is still packable, so it publishes an empty package.
- **GitHub workflows are disabled.** `.github/workflows/{merge-request,release}.yml` are commented out
  in full, and their commands still say `make ci-*`. Nothing runs on push or pull request today; the
  `just ci-*` recipes are what actually work, run locally.
- **`docfx.json` has an empty `metadata` array**, so `just docs-metadata` generates no API pages and
  the built site carries only `index.md`.

## Configuration

| Concern | Location | Notes |
|---------|----------|-------|
| Workflows | `justfile` | Single entry point; CI recipes compose the same steps used locally |
| Target framework | `Directory.Build.props` | `net10.0`, `LangVersion=latest`, nullable, warnings-as-errors |
| SDK pin | `global.json` | `10.0.0`, `rollForward=latestMajor`, prerelease allowed |
| Package versions | `Directory.Packages.props` | Central; add new versions here, reference without a version |
| Tool versions | `.config/dotnet-tools.json` | csharpier, xs, doclint, versioning, docfx — pinned, `rollForward=false` |
| Package version | `version` + `versioning get-version` | Computed at build/pack time, not stored in csproj |
| Publish feed | `justfile` `publish` | `dotnet.pkg.annium.com`, API key read from `.xs.credentials` (gitignored) |
| Formatter | `.csharpierrc`, `.editorconfig` | 120 columns, 4 spaces |
| Review catalog | umbrella `kb/reviews/repos/integrations/` | Areas, guides and fix-code reports live in the umbrella repo |

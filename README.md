# Annium.Integrations

Thin wrappers around external services, packaged for use with the [Annium](https://github.com/annium/base)
framework: AI providers and orchestration under `ai/`, social-platform bots under `social/`.

These are integration layers, not framework code — dependency injection, logging, HTTP, serialization
and testing all come from the `Annium.*` packages.

## Packages

| Package | What it gives you |
| --- | --- |
| [`Annium.Integrations.AI.OpenAI`](https://www.nuget.org/packages/Annium.Integrations.AI.OpenAI) | Keyed registration of an OpenAI client and the chat / audio clients derived from it, with configuration resolved from DI at resolution time |
| [`Annium.Integrations.AI.SemanticKernel`](https://www.nuget.org/packages/Annium.Integrations.AI.SemanticKernel) | A builder for Microsoft Semantic Kernel: plugin discovery through Annium's type manager, and MCP tools loaded from an HTTP server |
| [`Annium.Integrations.AI.SemanticKernel.AspNetCore`](https://www.nuget.org/packages/Annium.Integrations.AI.SemanticKernel.AspNetCore) | ASP.NET Core hosting for the above. **Currently empty** — published as a placeholder while the hosting surface is designed |
| [`Annium.Integrations.Social.Telegram`](https://www.nuget.org/packages/Annium.Integrations.Social.Telegram) | Telegram Bot API integration: a bot host, an API client, and polling or webhook receivers |

## Quick start

Register an OpenAI client under a key and resolve the capability clients from it:

```csharp
container.AddOpenAI("assistant", _ => new OpenAIConfig(apiKey, "gpt-5", null));

var chat = provider.ResolveKeyed<ChatClient>("assistant");
```

Register a Telegram bot with a polling receiver and a message handler:

```csharp
container.AddTelegramBot(
    "demo",
    sp => sp.Resolve<Dictionary<string, TelegramBotConfiguration>>()["demo"],
    opts =>
    {
        opts.UsePollingReceiver();
        opts.UseHandler<EchoTelegramMessageHandler>();
    }
);
```

Every integration is registered under a key, so several clients or bots coexist in one container, and
configuration is supplied as a delegate resolved from DI rather than a bound instance — configuration
loaded after registration is still picked up.

**Provider credentials never belong in a committed file.** The Telegram demo reads its bot token from
`TELEGRAM_BOT_TOKEN`; `OpenAIConfig.ToString()` renders the API key redacted so a config object cannot
leak it into a log line.

## Development

```bash
just              # list every recipe
just setup        # restore dotnet tools
just build        # Release build
just test         # run the test suite
just docs-lint    # XML documentation gate
just format       # csharpier + xs format
```

Targets `net10.0`. Tests run on Microsoft.Testing.Platform (xunit.v3). Warnings are errors, XML docs on
the public surface are enforced by [`annium.doclint`](https://github.com/annium/tools), and CI runs the
same `just` recipes you run locally.

## License

MIT — see [LICENSE](LICENSE).

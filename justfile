set shell := ["bash", "-cu"]
set positional-arguments
set allow-duplicate-recipes := true

# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
import 'lib.just'

# overrides
#
# `publish` is NOT overridden: packages go to nuget.org through the shared recipe, using the
# org-level NUGET_API_KEY, same as base / backend / frontend / tools.

# the Obsolete Telegram project is exempt: it is IsPackable=false, kept only for existing consumers
docs-lint:
    @echo "=== $0 ==="
    dotnet tool run doclint lint -w . -i '**/*.cs' -e '**/obj/**/*.cs' -e 'social/telegram/src/Annium.Integrations.Social.Telegram.Obsolete/**/*.cs'

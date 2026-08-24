set shell := ["bash", "-cu"]
set positional-arguments
set allow-duplicate-recipes := true

# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
import 'lib.just'

# overrides

# packages go to the private Annium feed, not nuget.org
publish apiKey:
    @echo "=== $0 ==="
    dotnet nuget push "*.nupkg" --source https://dotnet.pkg.annium.com/v3/index.json --api-key "$1" --skip-duplicate
    find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete

# the Obsolete Telegram project is exempt: it is IsPackable=false, kept only for existing consumers
docs-lint:
    @echo "=== $0 ==="
    dotnet tool run doclint lint -w . -i '**/*.cs' -e '**/obj/**/*.cs' -e 'social/telegram/src/Annium.Integrations.Social.Telegram.Obsolete/**/*.cs'

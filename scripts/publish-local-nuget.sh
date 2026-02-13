#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage: scripts/publish-local-nuget.sh [options]

Packs rUI libraries and publishes them to a local NuGet feed.

Options:
  --version <semver>       Package version to use for all packages.
  --configuration <cfg>    Build configuration (default: Release).
  --feed <path>            Local feed path (default: ~/.nuget/local-feed).
  -h, --help               Show this help.
EOF
}

VERSION=""
CONFIGURATION="Release"
FEED_DIR="$HOME/.nuget/local-feed"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="${2:-}"
      shift 2
      ;;
    --configuration)
      CONFIGURATION="${2:-}"
      shift 2
      ;;
    --feed)
      FEED_DIR="${2:-}"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PACKAGE_OUTPUT_DIR="$REPO_ROOT/artifacts/nuget/$CONFIGURATION"

PROJECTS=(
  "$REPO_ROOT/rUI.Drawing.Core/rUI.Drawing.Core.csproj"
  "$REPO_ROOT/rUI.Avalonia.Desktop/rUI.Avalonia.Desktop.csproj"
  "$REPO_ROOT/rUI.Drawing.Avalonia/rUI.Drawing.Avalonia.csproj"
  "$REPO_ROOT/rUI/rUI.csproj"
)

mkdir -p "$FEED_DIR"
mkdir -p "$PACKAGE_OUTPUT_DIR"

echo "Packing projects to: $PACKAGE_OUTPUT_DIR"
for project in "${PROJECTS[@]}"; do
  echo " - $(basename "$project")"
  PACK_ARGS=(
    dotnet pack "$project"
    --configuration "$CONFIGURATION"
    --output "$PACKAGE_OUTPUT_DIR"
  )

  if [[ -n "$VERSION" ]]; then
    PACK_ARGS+=( -p:PackageVersion="$VERSION" )
  fi

  "${PACK_ARGS[@]}"
done

mapfile -t PACKAGES < <(find "$PACKAGE_OUTPUT_DIR" -maxdepth 1 -type f -name '*.nupkg' ! -name '*.snupkg' | sort)

if [[ ${#PACKAGES[@]} -eq 0 ]]; then
  echo "No .nupkg files were produced." >&2
  exit 1
fi

echo "Publishing packages to local feed: $FEED_DIR"
for package in "${PACKAGES[@]}"; do
  echo " - $(basename "$package")"
  dotnet nuget push "$package" --source "$FEED_DIR" --skip-duplicate
done

echo "Done."

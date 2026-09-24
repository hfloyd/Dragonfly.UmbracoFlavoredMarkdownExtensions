# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

**Dragonfly Umbraco Flavorted Markdown Extensions** — a NuGet package that adds additional UFM Components and tools to Umbraco 17+ sites.

The package major version matches the target Umbraco major version (currently v17 = Umbraco 17, .NET 10).

## Solution Structure

```
CLAUDE_TODO
```

## Commands

### .NET (run from `src/`)

```powershell
# Build the package (always use Debug — Release triggers automatic nuget push via Custom.targets)
dotnet build Dragonfly/Dragonfly.csproj -c Debug

# Run the test site (starts Umbraco at https://localhost:44394 by default)
dotnet run --project UfmExtensions.TestSite/UfmExtensions.TestSite.csproj

# Pack the NuGet package for testing (Debug adds a -prerelease{timestamp} version suffix)
dotnet pack Dragonfly/Dragonfly.csproj -c Debug
```

> **Warning:** Never run `dotnet build` or `dotnet pack` with `-c Release` unless intentionally publishing to NuGet.org. `Custom.targets` runs `nuget.exe push` to `https://www.nuget.org` automatically after every Release build.

### TypeScript Client (run from `src/Dragonfly/Client/`)

```powershell
npm install           # First-time setup
npm run build         # Compile TypeScript + bundle with Vite
npm run watch         # Watch mode for development

# Regenerate the OpenAPI TypeScript client (requires the test site to be running)
npm run generate-client
```

The `generate-client` script fetches the swagger JSON from the running test site at `https://localhost:44394/umbraco/swagger/dragonflysiteauditorui/swagger.json` and runs `@hey-api/openapi-ts`. All files in `src/Dragonfly/Client/src/api/` ending in `.gen.ts` are auto-generated — do not edit them directly.

The TestSite `.csproj` has a `CopyAppPlugins` target that copies:

- 
- `Dragonfly/wwwroot/App_Plugins/` → `UfmExtensions.TestSite/wwwroot/App_Plugins/` (JS bundles, icons, umbraco-package.json)

## Architecture

### Service Layer

CLAUDE_TODO

### API Controllers

CLAUDE_TODO

### NuGet Package File Deployment

CLAUDE_TODO

### NuGet Package Versioning

All Umbraco and Dragonfly dependency versions for the package are centrally managed in `src/Dragonfly/Directory.Packages.props`. Update versions there, not in `Dragonfly.csproj`. The test site (`UfmExtensions.TestSite.csproj`) manages its own versions directly since it is not part of the central package management scope.

### Frontend (TypeScript / Lit)

CLAUDE_TODO



## Configuration

CLAUDE_TODO

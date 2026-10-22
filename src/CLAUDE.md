# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

**Dragonfly Umbraco Flavored Markdown Extensions** — a NuGet package that adds additional UFM Components and tools to Umbraco 17+ sites.

The package major version matches the target Umbraco major version (currently v17 = Umbraco 17, .NET 10).

## Solution Structure

```
src/
├── Dragonfly.UfmExtensions.slnx        Solution (package + tests + test site)
├── Directory.Packages.props            Central package versions for the package project
├── Custom.targets                      Pack helpers, imported by Dragonfly.csproj (see warning below)
├── Dragonfly/                          The package project (Microsoft.NET.Sdk.Razor)
│   ├── UfmExtensions/
│   │   ├── Constants.cs                ApiName used by the Swagger document and API routes
│   │   ├── BlockLabels/                Block label conversion (AngularJS → UFM)
│   │   ├── Composers/                  Swagger/OpenAPI registration for the package's API
│   │   └── Controllers/                Management API controllers
│   ├── Client/                         TypeScript source (Vite + Lit)
│   │   ├── src/bundle.manifests.ts     Collates every manifest; the entry Vite bundles
│   │   ├── src/ufm-components/         The UFM components and their manifests
│   │   ├── src/entrypoints/            Backoffice entrypoint
│   │   ├── src/api/*.gen.ts            Auto-generated OpenAPI client — never edit by hand
│   │   └── public/umbraco-package.json Loads the built bundle; copied into the output folder
│   └── wwwroot/App_Plugins/Dragonfly.UfmComponents/   Vite build output (ships as static web assets)
├── UfmExtensions.Tests/                xUnit tests (BlockLabelUfmConverter)
└── UfmExtensions.TestSite/             Umbraco site for manual testing
```

## Commands

### .NET (run from `src/`)

```powershell
# Build the package
dotnet build Dragonfly/Dragonfly.csproj -c Debug

# Run the unit tests
dotnet test UfmExtensions.Tests/UfmExtensions.Tests.csproj -c Debug

# Run the test site (Umbraco at https://localhost:44365)
dotnet run --project UfmExtensions.TestSite/UfmExtensions.TestSite.csproj

```

`Dragonfly.csproj` sets `GeneratePackageOnBuild` and imports `Custom.targets`, so **every build packs
the package**. A Debug build is versioned `17.0.0--prerelease<timestamp>`, and `Custom.targets` copies
it into the local feed at `~Dragonfly\LocalNuGetPackages` (which NuGet.Config already lists). That
includes builds triggered by `dotnet test` or by building the test site.

> **Warning:** `Custom.targets` also runs `nuget.exe push` to `https://www.nuget.org` after every
> **Release** pack — and since every build packs, that means every Release build publishes publicly.
> Never build, test or pack with `-c Release`; use `-c Debug`.

### TypeScript Client (run from `src/Dragonfly/Client/`)

```powershell
npm install           # First-time setup
npm run build         # tsc + Vite bundle
npm run watch         # Watch mode for development
npx vite build        # Bundle only, skipping the typecheck

# Regenerate the OpenAPI TypeScript client (requires the test site to be running)
npm run generate-client
```

`generate-client` fetches swagger JSON from the running test site at
`https://localhost:44365/umbraco/swagger/dragonfly-ufmextensions/swagger.json` and runs
`@hey-api/openapi-ts`. Every file under `src/Dragonfly/Client/src/api/` ending in `.gen.ts` is
generated — do not edit those directly.

`runtimeConfigPath` in `scripts/generate-openapi.js` is resolved from the `Client/` folder, not from
the output folder, so it is `./src/hey-api`.

The test site serves the bundle through the project reference, as static web assets; nothing copies
it into the site. Its `RemoveCopiedAppPlugins` target deletes any copy left in the site's own
`wwwroot`, which would otherwise fail the build with "Conflicting assets".

The test site runs the Umbraco starter kit with `ModelsMode: InMemoryAuto` (Development only), which
needs `Umbraco.Cms.DevelopmentMode.Backoffice` and `RazorCompileOnBuild=false`.

## Architecture

### UFM Components

Each component lives in `Client/src/ufm-components/` as one file holding two classes, mirroring how
Umbraco ships its own:

- a custom element (`ufm-*` tag, required prefix — UFM's sanitizer strips other prefixes) that
  resolves and renders the value;
- a `UmbUfmComponentBase` subclass whose `render(token)` emits that element, exported as `api`.

`manifest.ts` registers them as `ufmComponent` manifests, and `bundle.manifests.ts` includes that
array. The `meta.alias` is the syntax editors type; with no `meta.marker`, the token needs its colon
(`{dufmLinkUrl: myLink}`, and `{dufmBlockContentTypeName:}` even with no argument).

Components read property values from `UMB_UFM_RENDER_CONTEXT`, whose `value` is an object keyed by
property alias. A component **cannot** be nested inside a `${ }` expression, so anything conditional
has to be a component or an expression, never both.

Block labels render in two separate places, which matters for any component reading block context:

- on the **block card**, inside the block entry element — `UMB_BLOCK_ENTRY_CONTEXT` and
  `UMB_BLOCK_MANAGER_CONTEXT` are available, the workspace context is not;
- in the **block workspace overlay** — `UMB_BLOCK_WORKSPACE_CONTEXT` is available instead.

`block-content-type-name.component.ts` handles both.

### Block Label Conversion (C#)

`UfmExtensions/BlockLabels/` holds two pieces:

- **`BlockLabelUfmConverter`** — a static, dependency-free converter from one AngularJS label to UFM.
  Pure string handling, so it is unit-testable. It reports `Converted`, `NeedsManualReview` or
  `NoChangeNeeded` plus warnings, and deliberately leaves a label untouched when it cannot convert it
  fully, rather than writing something half-broken.
- **`BlockLabelUfmMigrator`** — walks every `Umbraco.BlockList` and `Umbraco.BlockGrid` datatype,
  converting `blocks[].label` and Block Grid `blocks[].areas[].createLabel`. It works on a JSON copy
  of `ConfigurationData` and only writes back when something changed and `DryRun` is false.
  `BlockLabelUfmComposer` registers it as transient, so a consuming site can inject it.
- **`ListViewLabelUfmMigrator`** — the same walk over `Umbraco.ListView` datatypes, converting the
  `nameTemplate` of each `includeProperties` column that has one. Both migrators share
  `LabelUfmMigratorBase` (the datatype loop, dry-run and save logic) and `LabelUfmReport<TChange>`.

Conversion rules worth knowing:

- `{{x}}` → `${ x }`; a standalone expression that a component covers becomes that component.
  `ComponentSubstitutions` in `BlockLabelUfmConverter` maps each such pattern to a Dragonfly component
  alias and a built-in fallback alias (or none); `UseDragonflyUfmComponents` (default `true`) picks
  which. A new Dragonfly UFM component that replaces an AngularJS pattern needs a row there as well
  as its `manifest.ts` entry.
- `!!` is removed — the expression parser (`heximal-expressions`) rejects it, and an empty value is
  already falsy, so the test still behaves the same.
- `$settings.x == 1` → `$settings.x`, because settings toggles are real booleans in v14+.
- `$index` → `$index+1` (or `($index+1)` inside a larger expression), because AngularJS labels
  counted from 1 and UFM counts from 0. `KeepIndexOneBased` (default `true`) controls this; with
  `false`, `$index` is left as it is.
- `| ncRichText` → `| stripHtml`; `value | truncate:true:N` → `truncate(value, N)`, where the value is
  everything before the filter within the same parentheses. The UFM expression parser
  (`heximal-expressions`) cannot pass arguments to a piped filter: `| truncate:N` is a parse error
  inside parentheses and silently ignores its length elsewhere. A tail (`truncate:true:N:'…'`) is
  passed on as `truncate(value, N, '…')`.
- `value | limitTo:N` → `truncate(value, N, '')`: `limitTo` cut text without an ellipsis, and UFM's
  `truncate` adds one unless its tail is `''`. A negative `limitTo` or one with a start index is left
  for manual review.
- Any other piped filter UFM does not have (`date`, `currency`, `number`, `umbCmsTitleCase`, …), or a UFM
  filter still passed an argument (`| truncate:20`), is reported as `NeedsManualReview`, since the label
  would not render. `UfmFilterAliases` in `BlockLabelUfmConverter` lists UFM's filters.
- A chain of fallbacks (`a ? a : (b ? (b | ncRichText | truncate:true:N) : (c | ncMediaName))`)
  becomes `{dufmFirstValue: a, b:N, c}` with `UseDragonflyUfmComponents`; a chain ending in text
  (`… : 'None'`) passes it on as the fallback text (`…, "None"`), and a trailing `''` is dropped.
  A `limitTo` or `truncate` after the whole chain (`a ? a : b | limitTo:100`) applies to every value,
  as AngularJS filters apply to everything before them: `{dufmFirstValue: a:100, b:100}`.
  `dufmFirstValue` ends a shortened value with `…`, which `limitTo` did not.
  Without `UseDragonflyUfmComponents` a chain stays an expression, except one holding a picked
  item's name, which has no built-in equivalent and needs manual review.
- `x | ncNodeName` and `x | ncMediaName` become `{dufmFirstValue: x}` with `UseDragonflyUfmComponents`, and
  `{umbContentName: x}` without. Both show the names of Content, Multinode Tree and Media Picker values as v17
  stores them (a GUID, `{type, unique}` objects, `{mediaKey}` objects), but `umbContentName` lists several items in
  its own order rather than the order they were picked, as `ncNodeName` did, and shows nothing for a udi string.
- `value | umbCmsJoinArray:', ' | ncNodeName` (a List View column showing several picked items) becomes
  `{dufmFirstValue: value}` with `UseDragonflyUfmComponents`: the render context for a List View column
  label is `{ value: <column value> }`, and `dufmFirstValue` resolves every picked item and joins the names
  with `", "`. It is used instead of `umbContentName` because a list of picked items, such as a Contentment
  Data List on the Umbraco Content source, holds udi strings (`umb://document/…`) that `umbContentName`
  ignores, so the column would be blank. Without `UseDragonflyUfmComponents` it becomes
  `{umbContentName: value}`, which only reads `{type, unique}` objects and GUIDs, so it carries a warning
  (and is reported as `NeedsManualReview`, not saved) to check the column shows names. Other separators
  stay manual review.
- `value["contentData"][0]["X"]` (a List View column over a Block List, reading the first block) becomes
  `{dufmFirstBlockValue: value, X}`, as does the link name chain
  `…["X"][0]["nodeName"] ? … : …["X"][0]["name"]`; `…["X"][0]["url"]` becomes `{dufmFirstBlockValue: value, X, Url}`.
  Block values are stored as `contentData[].values[{alias, value}]` in v14+, which an expression cannot search,
  hence the component. Only `[0]` is converted, and there is no built-in equivalent.
- AngularJS read a picked link's properties in several forms; `Link[0].name` and `Link[0]['name']` are
  matched as `Link[0]["name"]`, so every link pattern accepts all three. The rewrite is only used to find a
  component, so a label that stays an expression keeps its original form.
- `Link[0].name ? Link[0].name : Link[0].nodeName` (optionally testing `Link[0].name.length > 0`) becomes
  `{umbLink: Link}` in both modes: umbLink shows the link's title, else the picked item's name.
- A fallback chain step may test `a.length > 0` or `a.length` instead of `a`, which is the same test for text.
- A link's title in a fallback chain (`title ? title : Link[0].name`) becomes a `Link.name` entry:
  `{dufmFirstValue: title, Link.name}`. `dufmFirstValue` reads a plain `Link` entry the way `dufmLinkDisplay`
  does, picked item's name first, so `.name` is what keeps a custom link title such as "Read more".
- `$contentTypeName` has no built-in UFM equivalent: a standalone one becomes
  `{dufmBlockContentTypeName:}` with `UseDragonflyUfmComponents`; otherwise the element type name is
  written into the label.

### UFM Syntax Check (C#)

`UfmSyntaxChecker` (also in `UfmExtensions/BlockLabels/`) checks one UFM label for known mistakes and
backs the `checkUfmSyntax` endpoint. It finds `${ }` expressions with the same pattern UFM's tokenizer
uses, then runs each `ExpressionRules` row (pattern → message) against them. A newly found pitfall is
a new row there plus a test case in `UfmSyntaxCheckerTests`.

### NuGet Package File Deployment

`Dragonfly.csproj` uses `Microsoft.NET.Sdk.Razor` with `StaticWebAssetBasePath=/`, so everything in
`Dragonfly/wwwroot/` ships as static web assets and is served from the consuming site without being
copied to disk. The bundle therefore lands at `/App_Plugins/Dragonfly.UfmComponents/`.

A consuming site must **not** also have a physical `App_Plugins/Dragonfly.UfmComponents` folder —
duplicate static web assets fail the build ("Sequence contains more than one element").

The `Client/` folder is excluded from packing; only `Client/public/umbraco-package.json` is
referenced, and Vite copies it into the output folder during the build.

Umbraco appends the manifest's `version` to the bundle's URL (`?umb__rnd=<version>`) as its cache-buster.
`UmbracoPackageManifest.targets` therefore ships a copy of `Client/public/umbraco-package.json` with its
`version` set to the package version, generated under `obj/`, in place of the copy Vite writes into
`wwwroot`. Without it the version stays `0.0.0` and sites keep a stale bundle after updating.

### NuGet Package Versioning

All Umbraco dependency versions for the package are centrally managed in
`src/Directory.Packages.props`. Update versions there, not in `Dragonfly.csproj`. The test site
manages its own versions directly, since it sits outside that scope.

## Configuration

The package needs no configuration in a consuming site. Installing it registers the UFM components
(via the bundle in `umbraco-package.json`) and `BlockLabelUfmMigrator` in DI.

The package's own Management API is registered by `UfmExtensionsApiComposer` under the Swagger
document named by `Constants.ApiName` (`dragonfly-ufmextensions`), routed at
`ufmextensions/api/v{version}`.

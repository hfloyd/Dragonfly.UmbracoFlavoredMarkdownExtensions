# Dragonfly Umbraco Flavored Markdown Extensions

[![Downloads](https://img.shields.io/nuget/dt/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=cc9900)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions/)
[![NuGet](https://img.shields.io/nuget/vpre/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=0273B3)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions)
[![GitHub license](https://img.shields.io/github/license/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=8AB803)](../LICENSE)

Extra [Umbraco Flavored Markdown](https://docs.umbraco.com/umbraco-cms/model-your-content/property-editors/umbraco-flavored-markdown)
components for Umbraco 17+, plus a tool for converting AngularJS block labels from Umbraco 13 and
earlier into UFM.

Umbraco 14 removed AngularJS, so block labels written as `{{ propertyAlias }}` no longer render.
UFM covers most of what those labels did, but not all of it — this package fills in the gaps that
come up most often when upgrading.

<!--
Including screenshots is a really good idea!

If you put images into /docs/screenshots, then you would reference them in this readme as, for example:

<img alt="..." src="https://github.com/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions/blob/develop/docs/screenshots/screenshot.png">

And don't forget to add the screenshot files to umbraco-marketplace.json too!
-->

## Installation

Add the package to an existing Umbraco website (v17+) from nuget:

`dotnet add package Dragonfly.UmbracoFlavoredMarkdownExtensions`

Nothing else to configure. The components register themselves and are available anywhere UFM is
rendered — block labels, templated labels, property descriptions.

## UFM Components

### `{blockContentTypeName:}`

Renders the name of a block's content element type — the UFM replacement for AngularJS
`{{$contentTypeName}}`, which has no built-in equivalent.

```
{blockContentTypeName:}                 renders e.g. "Rich Text Editor"
{blockContentTypeName: Unknown block}   fallback text when the name cannot be resolved
```

The colon is required even with no fallback text, because the component has no single-character
marker.

It resolves the name on the block card and in the block workspace overlay, which expose different
contexts.

### `{linkDisplay: myLinkPropertyAlias}`

Describes a link picker (Multi URL Picker) value using a cascading approach: the linked
item's name when content or media is picked, otherwise the link title, otherwise the URL.

```
{linkDisplay: myLinkPropertyAlias}
```

Replaces the old pattern:

```
{{ myLinkPropertyAlias[0]["nodeName"] ? myLinkPropertyAlias[0]["nodeName"] : (myLinkPropertyAlias[0]["name"] ? myLinkPropertyAlias[0]["name"] : myLinkPropertyAlias[0]["url"]) }}
```

Umbraco's built-in `{umbLink: myLinkPropertyAlias}` prefers the link's title over the item name; use this one
when you want the content node's name first. Multiple links are joined with commas.

### `{linkUrl: myLinkPropertyAlias}`

Renders the URL of a link picker value.

```
{linkUrl: myLinkPropertyAlias}
```

Document and media links store only a key, so their URLs are looked up; external links use the URL
stored in the value. Any query string on the link is appended.

## Block Label Conversion

`BlockLabelUfmMigrator` rewrites the AngularJS labels on every Block List and Block Grid datatype in
a site, covering both `blocks[].label` and Block Grid area `createLabel`s. It is registered in DI,
so inject it wherever you want to trigger it — a Management API endpoint, a one-off migration, a
console task:

```csharp
public class MyController(BlockLabelUfmMigrator migrator) : Controller
{
    public async Task<IActionResult> Convert(bool testOnly = true)
    {
        BlockLabelUfmReport report = await migrator.RunAsync(
            DryRun: testOnly,
            UserKey: currentUserKey,
            UseContentTypeNameComponent: true);

        return new JsonResult(report);
    }
}
```

**Run it with `DryRun: true` first.** The report lists every label with its original text, the
proposed UFM, and any warnings, without saving anything. `UseContentTypeNameComponent: true` emits
`{blockContentTypeName:}` for `$contentTypeName`; with `false`, the element type's name is written
into the label instead, which needs no package installed at render time.

### What it converts

| AngularJS | UFM |
| --- | --- |
| `{{Title}}` | `${ Title }` |
| `{{$index}}` | `${ $index }` |
| `{{$contentTypeName}}` | `{blockContentTypeName:}` or the element type's name |
| `{{$settings.umbracoNaviHide == 1 ? 'a' : 'b'}}` | `${ $settings.umbracoNaviHide ? 'a' : 'b' }` |
| `{{!!Name ? Name : Other}}` | `${ Name ? Name : Other }` |
| `{{Body \| ncRichText \| truncate:true:150}}` | `${ Body \| stripHtml \| truncate:150 }` |
| `{{Link[0]["name"]}}` | `{umbLink: Link}` |
| `{{Link[0]["url"]}}` | `{linkUrl: Link}` |
| `{{Link[0]["nodeName"] ? … : …}}` | `{linkDisplay: Link}` |
| `{{Layout \| ncNodeName}}` | `{umbContentName: Layout}` |

Two of these are behaviour fixes rather than translations. `!!` is a parse error in UFM's expression
parser, and an empty value is already false, so it is simply dropped. `$settings.x == 1` never
matches in v14+, where settings toggles are real booleans, so the comparison is dropped too.

### What it leaves alone

A UFM component cannot be nested inside a `${ }` expression. So a label that picks *between* a
picked item's name and something else — `{{BlockName ? BlockName : (ResourceNode | ncNodeName)}}` —
has no direct equivalent. The converter reports these as `NeedsManualReview` with a warning and
**leaves the label untouched**, rather than writing something half-converted. Rewrite those by hand,
either dropping the fallback or showing both values.

`BlockLabelUfmConverter` is public and static if you want to convert a single label yourself:

```csharp
BlockLabelConversion result = BlockLabelUfmConverter.Convert(label, contentTypeName, useComponent);
```

## Contributing

Contributions to this package are most welcome! Please read the [Contributing Guidelines](CONTRIBUTING.md).

## Acknowledgments

The UFM components follow the structure of Umbraco's own built-in components (`umbValue`, `umbLink`,
`umbContentName`) in the Umbraco CMS source.

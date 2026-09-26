# Dragonfly Umbraco Flavored Markdown Extensions

[![Downloads](https://img.shields.io/nuget/dt/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=cc9900)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions/)
[![NuGet](https://img.shields.io/nuget/vpre/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=0273B3)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions)
[![GitHub license](https://img.shields.io/github/license/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=8AB803)](../LICENSE.md)

Extra [Umbraco Flavored Markdown](https://docs.umbraco.com/umbraco-cms/model-your-content/property-editors/umbraco-flavored-markdown) components for Umbraco 17+, plus a tool for converting AngularJS block labels from Umbraco 13 and earlier into UFM.

Umbraco 14 removed AngularJS, so block labels written as `{{ propertyAlias }}` no longer render. UFM covers most of what those labels did, but not all of it — this package fills in the gaps that come up most often when upgrading.

*I created this package to solve my own upgrade/migration needs, and will continue to add to it as I come across additional use-cases. Not being up-to-speed on Lit/Vite and the rest, most of the actual code was written by my buddy Claude Code.*

<!--
Including screenshots is a really good idea!

If you put images into /docs/screenshots, then you would reference them in this readme as, for example:

<img alt="..." src="https://github.com/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions/blob/develop/docs/screenshots/screenshot.png">

And don't forget to add the screenshot files to umbraco-marketplace.json too!
-->

## Installation

Add the package to an existing Umbraco website (v17+) from nuget:

`dotnet add package Dragonfly.UmbracoFlavoredMarkdownExtensions`

Nothing else to configure. The components register themselves.

## Contents

This package includes two main features:

1. Custom UFM components that fill in gaps left by Umbraco's built-in components
2. Some API tools for evaluating and converting AngularJS block labels to UFM, with a report of any labels that need manual review. You can use the tools in your own code, or via the Management API if you have access to the Settings section. Additionally, if you only want to run it once (on a website project upgrade/migration) and select FALSE for `UseDragonflyUfmComponents`, only built-in components will be used, and you can remove the package after the migration. (See more about the tool, [below](#ConversionTool))

## <a name="UfmComponents"></a>UFM Components

Based on my own idiosyncratic labels from projects done in v13, I have created custom components that handle my own needs, and I hope they will help you too. If you find a gap that this package doesn't cover, please [open an issue] or start a discussion in the [Discussions] section of this repo, and I'll see if I can add it.



They are all prefixed with `dufm` to avoid conflicts and confusion with Umbraco's built-in components.

### `{dufmBlockContentTypeName:}`

Renders the name of a block's content element type — the UFM replacement for AngularJS `{{$contentTypeName}}`, which has no built-in equivalent.

```
{dufmBlockContentTypeName:}                 renders e.g. "Rich Text Editor"
{dufmBlockContentTypeName: Unknown block}   fallback text when the name cannot be resolved
```

The colon is required even with no fallback text, because the component has no single-character marker.

It resolves the name on the block card and in the block workspace overlay, which expose different contexts.

### `{dufmLinkDisplay: myLinkPropertyAlias}`

Describes a link picker (Multi URL Picker) value using a cascading approach: the linked
item's name when content or media is picked, otherwise the link title, otherwise the URL with its query string or anchor (an anchor-only link shows just the anchor, e.g. `#contact`).

Example:

```
{dufmLinkDisplay: myLinkPropertyAlias}
```

Replaces the old pattern:

```
{{ myLinkPropertyAlias[0]["nodeName"] ? myLinkPropertyAlias[0]["nodeName"] : (myLinkPropertyAlias[0]["name"] ? myLinkPropertyAlias[0]["name"] : myLinkPropertyAlias[0]["url"]) }}
```

Umbraco's built-in `{umbLink: myLinkPropertyAlias}` prefers the link's title over the item name; use this one when you want the content node's name first. Multiple links are joined with commas.

### `{dufmLinkUrl: myLinkPropertyAlias}`

Renders the URL of a link picker value.

Example:

```
{dufmLinkUrl: myLinkPropertyAlias}
```

Document and media links store only a key, so their URLs are looked up; external links use the URL stored in the value. The link's query string or anchor is not included; use
`{dufmLinkUrlWithAnchor: …}` to include that.

### `{dufmLinkUrlWithAnchor: myLinkPropertyAlias}`

Renders the URL of a link picker value, like `{dufmLinkUrl: …}`, with the link's query string or anchor appended (e.g. `/contact-us/#form` or `https://example.com?ref=x`). A link that is only an anchor renders as the anchor itself (`#contact`).

Example:

```
{dufmLinkUrlWithAnchor: myLinkPropertyAlias}
```

### `{dufmFirstValue: alias1, alias2, …}`

Renders the first of several properties that has a value, rendering each one by what it holds:

- a content, media or multinode tree picker shows the picked items' names;
- rich text has its HTML stripped;
- a list of text, such as tags or a Contentment Data List, is comma-separated;
- plain text is shown as it is.

Add `:length` after an alias to truncate that value to that many characters, with an ellipsis. Values without a length are not truncated.

Add a quoted string (`"…"` or `'…'`) as the last item to show fallback text when none of the properties has a value; without one, nothing is shown.

Examples:

```
{dufmFirstValue: BlockName, Header, HtmlText:100}

{dufmFirstValue: BlockName, ContentTitle:150, Image}

{dufmFirstValue: BlockName, Header, HtmlText:100, "No value"}
```

Replaces AngularJS chains of fallbacks, which read more simply this way and, when they end in a picked item's name, cannot be written as a UFM expression at all, because a component cannot be nested inside one:

```
{{ BlockName ? BlockName : (ContentTitle ? (ContentTitle | ncRichText | truncate:true:150) : (Image | ncMediaName)) }}
```

Pickers are recognised by the shape of their value, so a text property holding nothing but a GUID would be treated as a picked item.

## <a name="ConversionTool"></a>Block Label Evaluation / Conversion Tool

`BlockLabelUfmMigrator` rewrites the AngularJS labels on every Block List and Block Grid datatype in a site, covering both `blocks[].label` and Block Grid area `createLabel`s. 

The easiest way to use the tool for users with Settings section access is to call it from the Swagger UI at `/umbraco/swagger` (the `dragonfly-ufmextensions` document), since Management API endpoints need a bearer token:

| Request                                                                                     | Does                                                              |
| ------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| `GET /umbraco/ufmextensions/api/v1/evaluateBlockLabelsToUfm?UseDragonflyUfmComponents=true` | Returns the report of how each label would convert; saves nothing |
| `POST /umbraco/ufmextensions/api/v1/convertBlockLabelsToUfm?UseDragonflyUfmComponents=true` | Converts the labels and saves the changed datatypes               |

Run the GET first to see what will change, then POST to actually convert. The `UseDragonflyUfmComponents` query parameter indicates whether to use this package's UFM components or only Umbraco's built-in components. 

`UseDragonflyUfmComponents`=`true` (default) : Use this package's UFM components wherever they cover an expression, so the package must stay installed for the labels to render. 

`UseDragonflyUfmComponents`=`false`: Only Umbraco's built-in components are used — install the package just to convert your labels, then remove it before going live.

The optional `KeepIndexOneBased` query parameter controls how `$index` is converted. AngularJS labels counted blocks from 1, but UFM counts them from 0.

`KeepIndexOneBased`=`true` (default) : `{{$index}}` becomes `${ $index+1 }`, so blocks are still numbered from 1.

`KeepIndexOneBased`=`false`: `$index` is left as it is, so blocks are numbered from 0.

Additionally, it is registered in DI, so you can inject it wherever you want to trigger it — a Management API endpoint, a one-off migration, a console task:

```csharp
public class MyController(BlockLabelUfmMigrator migrator) : Controller
{
    public async Task<IActionResult> Convert(bool testOnly = true)
    {
        BlockLabelUfmReport report = await migrator.RunAsync(
            DryRun: testOnly,
            UserKey: currentUserKey,
            UseDragonflyUfmComponents: true,
            KeepIndexOneBased: true);

        return new JsonResult(report);
    }
}
```

**Run it with `DryRun: true` first.** The report lists every label with its original text, the
proposed UFM, and any warnings, without saving anything.

### What it converts

| AngularJS                                                                                                 | UFM                                            | UFM with `UseDragonflyUfmComponents: false`                      |
| --------------------------------------------------------------------------------------------------------- | ---------------------------------------------- | ---------------------------------------------------------------- |
| `{{Title}}`                                                                                               | `${ Title }`                                   | same                                                             |
| `{{$index}}`                                                                                              | `${ $index+1 }` (`${ $index }` with `KeepIndexOneBased: false`) | same                                            |
| `{{$contentTypeName}}`                                                                                    | `{dufmBlockContentTypeName:}`                  | the element type's name, as text                                 |
| `{{$settings.umbracoNaviHide == 1 ? 'a' : 'b'}}`                                                          | `${ $settings.umbracoNaviHide ? 'a' : 'b' }`   | same                                                             |
| `{{!!Name ? Name : Other}}`                                                                               | `{dufmFirstValue: Name, Other}`                | `${ Name ? Name : Other }`                                       |
| `{{Layout ? Layout : "None"}}`                                                                            | `{dufmFirstValue: Layout, "None"}`             | `${ Layout ? Layout : "None" }`                                  |
| `{{Body \| ncRichText \| truncate:true:150}}`                                                             | `${ truncate(Body \| stripHtml, 150) }`        | same                                                             |
| `{{Link[0]["name"]}}`                                                                                     | `{umbLink: Link}`                              | same                                                             |
| `{{Link[0]["url"]}}`                                                                                      | `{dufmLinkUrl: Link}`                          | left for manual review — no built-in equivalent                  |
| `{{Link[0]["nodeName"] ? … : …}}`                                                                         | `{dufmLinkDisplay: Link}`                      | `{umbLink: Link}` (title first, then item name; no URL fallback) |
| `{{Layout \| ncNodeName}}`                                                                                | `{umbContentName: Layout}`                     | same                                                             |
| `{{BlockName ? BlockName : (Body ? (Body \| ncRichText \| truncate:true:100) : (Image \| ncMediaName))}}` | `{dufmFirstValue: BlockName, Body:100, Image}` | left for manual review                                           |

Four of these are behavior fixes rather than translations. UFM's expression parser cannot pass arguments to a piped filter, so `truncate` is called as a function instead; `| truncate:150` would be a parse error inside parentheses and would ignore its length elsewhere. `!!` is a parse error in UFM's expression parser, and an empty value is already false, so it is simply dropped. `$settings.x == 1` never matches in v14+, where settings toggles are real booleans, so the comparison is dropped too. `$index` counts from 0 in UFM, so 1 is added to keep the numbering AngularJS labels showed; inside a larger expression it becomes `($index+1)`.

### What it leaves alone

A UFM component cannot be nested inside a `${ }` expression. So a label that picks *between* a picked item's name and something else has no built-in equivalent. With `UseDragonflyUfmComponents=true`, any chain of fallbacks in which each step shows the property it tests — `{{BlockName ? BlockName : (ResourceNode | ncNodeName)}}` — becomes `{dufmFirstValue: …}`. Any other mix of a picked item's name and an expression is reported as `NeedsManualReview` with a warning, and the converter **leaves the label untouched**, rather than writing something half-converted. Rewrite
those by hand, either dropping the fallback or showing both values.

`BlockLabelUfmConverter` is public and static if you want to convert a single label yourself:

```csharp
BlockLabelConversion result = BlockLabelUfmConverter.Convert(label, contentTypeName, useDragonflyUfmComponents, keepIndexOneBased);
```

## <a name="SyntaxCheck"></a>Checking a UFM Label

When a label you have written or edited by hand renders nothing, check it for common mistakes:

| Request | Does |
| --- | --- |
| `GET /umbraco/ufmextensions/api/v1/checkUfmSyntax?Label=…` | Lists the issues found in the label, with how to fix each one; saves nothing |

Paste the label into the `Label` field in the Swagger UI, which encodes it for you. It reports:

- a component written as an expression — `${ dufmFirstValue: … }` instead of `{dufmFirstValue: …}`;
- a component nested inside an expression;
- an argument passed to a piped filter — `| truncate:150`, which is a parse error inside parentheses and ignored elsewhere; call `truncate(value, 150)` instead;
- `!!`, which is a parse error in UFM expressions;
- leftover AngularJS: `{{ }}`, the `ncNodeName`/`ncMediaName`/`ncRichText` filters, `$contentTypeName`, `$settings.x == 1`, and reading into picker values by index;
- a `${` that is never closed.

The checks look for known patterns, so a label with no issues can still fail to render. The same checks are available in code as `UfmSyntaxChecker.Check(label)`.

## Contributing

Contributions to this package are most welcome! Please read the [Contributing Guidelines](CONTRIBUTING.md).

## Acknowledgments

The UFM components follow the structure of Umbraco's own built-in components (`umbValue`, `umbLink`, `umbContentName`) in the Umbraco CMS source.

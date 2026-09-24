# Dragonfly Umbraco Flavored Markdown Extensions

[![Dragonfly Website](https://img.shields.io/badge/Dragonfly-Website-A84492)](https://DragonflyLibraries.com/umbraco-packages/) [![Umbraco Marketplace](https://img.shields.io/badge/Umbraco-Marketplace-3544B1?logo=Umbraco&logoColor=white)](https://marketplace.umbraco.com/package/Dragonfly.UmbracoFlavoredMarkdownExtensions) [![Nuget Downloads](https://img.shields.io/nuget/dt/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=FF5F49)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions/) [![NuGet](https://img.shields.io/nuget/vpre/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=0273B3)](https://www.nuget.org/packages/Dragonfly.UmbracoFlavoredMarkdownExtensions) [![GitHub license](https://img.shields.io/github/license/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions?color=8AB803)](https://github.com/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions/blob/main/LICENSE) [![GitHub Repo](https://img.shields.io/badge/GitHub-Code-yellow?logo=github)](https://github.com/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions)

Extra [Umbraco Flavored Markdown](https://docs.umbraco.com/umbraco-cms/model-your-content/property-editors/umbraco-flavored-markdown)
components, plus a tool for converting AngularJS block labels from Umbraco 13 and earlier into UFM.

## What's in it

**Three UFM components**, usable anywhere UFM renders (block labels, templated labels, property
descriptions):

| Syntax | Renders |
| --- | --- |
| `{blockContentTypeName:}` | The name of a block's content element type — the replacement for AngularJS `{{$contentTypeName}}` |
| `{linkDisplay: myLink}` | A link picker's linked item name, else its title, else its URL |
| `{linkUrl: myLink}` | A link picker's URL, resolving document and media links |

**A block label converter.** `BlockLabelUfmMigrator` is registered in DI and rewrites the AngularJS
labels on every Block List and Block Grid datatype, including Block Grid area create labels. Run it
with `DryRun: true` for a full report of what would change before anything is saved. Labels it
cannot convert completely are reported and left untouched rather than half-rewritten.

## Installation

`dotnet add package Dragonfly.UmbracoFlavoredMarkdownExtensions`

No configuration needed.

## Versions

Install the correct package/version for your Umbraco installation.

| Umbraco Version | Package / Version      |
| --------------- | ---------------------- |
| v 17            | This package - v. 17.x |
| v 16            | not supported          |
| v 15            | not supported          |
| v 14            | not supported          |

Full documentation, including the conversion rules and what needs rewriting by hand, is in the
[GitHub README](https://github.com/hfloyd/Dragonfly.UmbracoFlavoredMarkdownExtensions).

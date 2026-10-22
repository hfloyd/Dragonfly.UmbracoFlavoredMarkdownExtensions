const n = [
  {
    name: "Umbraco Flavored Markdown Extensions Entrypoint",
    alias: "UmbracoFlavoredMarkdownExtensions.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-B8u3Fa5h.js")
  }
], o = [
  {
    name: "Block Content Type Name UFM Component",
    alias: "Dragonfly.UfmComponent.BlockContentTypeName",
    type: "ufmComponent",
    api: () => import("./block-content-type-name.component-gRf5SgMa.js"),
    meta: {
      alias: "dufmBlockContentTypeName"
    }
  },
  {
    name: "Link Display UFM Component",
    alias: "Dragonfly.UfmComponent.LinkDisplay",
    type: "ufmComponent",
    api: () => import("./link-display.component-C7rS78nI.js"),
    meta: {
      alias: "dufmLinkDisplay"
    }
  },
  {
    name: "Link URL UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrl",
    type: "ufmComponent",
    api: () => import("./link-url.component-D3waH2tJ.js"),
    meta: {
      alias: "dufmLinkUrl"
    }
  },
  {
    name: "Link URL With Anchor UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrlWithAnchor",
    type: "ufmComponent",
    api: () => import("./link-url-with-anchor.component-laj3OxAY.js"),
    meta: {
      alias: "dufmLinkUrlWithAnchor"
    }
  },
  {
    name: "First Block Value UFM Component",
    alias: "Dragonfly.UfmComponent.FirstBlockValue",
    type: "ufmComponent",
    api: () => import("./first-block-value.component-DeWUY5U3.js"),
    meta: {
      alias: "dufmFirstBlockValue"
    }
  },
  {
    name: "First Value UFM Component",
    alias: "Dragonfly.UfmComponent.FirstValue",
    type: "ufmComponent",
    api: () => import("./first-value.component-BCbJ3oHx.js"),
    meta: {
      alias: "dufmFirstValue"
    }
  }
], a = [
  ...n,
  ...o
];
export {
  a as manifests
};
//# sourceMappingURL=umbraco-flavored-markdown-extensions.js.map

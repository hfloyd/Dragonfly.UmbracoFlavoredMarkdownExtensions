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
    api: () => import("./link-display.component-BDj3YWks.js"),
    meta: {
      alias: "dufmLinkDisplay"
    }
  },
  {
    name: "Link URL UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrl",
    type: "ufmComponent",
    api: () => import("./link-url.component-CX4j120u.js"),
    meta: {
      alias: "dufmLinkUrl"
    }
  },
  {
    name: "Link URL With Anchor UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrlWithAnchor",
    type: "ufmComponent",
    api: () => import("./link-url-with-anchor.component-Bi2LXN3Q.js"),
    meta: {
      alias: "dufmLinkUrlWithAnchor"
    }
  },
  {
    name: "First Value UFM Component",
    alias: "Dragonfly.UfmComponent.FirstValue",
    type: "ufmComponent",
    api: () => import("./first-value.component-B6x9VB6k.js"),
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

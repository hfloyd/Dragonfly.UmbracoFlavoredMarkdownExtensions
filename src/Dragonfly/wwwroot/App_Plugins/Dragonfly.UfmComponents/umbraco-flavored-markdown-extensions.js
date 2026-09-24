const n = [
  {
    name: "Umbraco Flavored Markdown Extensions Entrypoint",
    alias: "UmbracoFlavoredMarkdownExtensions.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-C6x7o20I.js")
  }
], a = [
  {
    name: "Umbraco Flavored Markdown Extensions Dashboard",
    alias: "UmbracoFlavoredMarkdownExtensions.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element-B6B7BM0_.js"),
    meta: {
      label: "Example Dashboard",
      pathname: "example-dashboard"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content"
      }
    ]
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
    api: () => import("./link-display.component-Bre7xA-2.js"),
    meta: {
      alias: "dufmLinkDisplay"
    }
  },
  {
    name: "Link URL UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrl",
    type: "ufmComponent",
    api: () => import("./link-url.component-B5Gy6lfw.js"),
    meta: {
      alias: "dufmLinkUrl"
    }
  },
  {
    name: "Link URL With Anchor UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrlWithAnchor",
    type: "ufmComponent",
    api: () => import("./link-url-with-anchor.component-DfqUzIfJ.js"),
    meta: {
      alias: "dufmLinkUrlWithAnchor"
    }
  }
], t = [
  ...n,
  ...a,
  ...o
];
export {
  t as manifests
};
//# sourceMappingURL=umbraco-flavored-markdown-extensions.js.map

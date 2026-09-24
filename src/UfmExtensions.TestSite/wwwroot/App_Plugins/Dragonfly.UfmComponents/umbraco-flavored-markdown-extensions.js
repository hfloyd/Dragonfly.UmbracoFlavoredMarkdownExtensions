const a = [
  {
    name: "Umbraco Flavored Markdown Extensions Entrypoint",
    alias: "UmbracoFlavoredMarkdownExtensions.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-C6x7o20I.js")
  }
], n = [
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
      alias: "blockContentTypeName"
    }
  },
  {
    name: "Link Display UFM Component",
    alias: "Dragonfly.UfmComponent.LinkDisplay",
    type: "ufmComponent",
    api: () => import("./link-display.component-CrUXO3Y8.js"),
    meta: {
      alias: "linkDisplay"
    }
  },
  {
    name: "Link URL UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrl",
    type: "ufmComponent",
    api: () => import("./link-url.component-Cwg5dq97.js"),
    meta: {
      alias: "linkUrl"
    }
  }
], t = [
  ...a,
  ...n,
  ...o
];
export {
  t as manifests
};
//# sourceMappingURL=umbraco-flavored-markdown-extensions.js.map

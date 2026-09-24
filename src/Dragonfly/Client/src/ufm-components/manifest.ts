export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Block Content Type Name UFM Component",
    alias: "Dragonfly.UfmComponent.BlockContentTypeName",
    type: "ufmComponent",
    api: () => import("./block-content-type-name.component.js"),
    meta: {
      alias: "blockContentTypeName",
    },
  },
  {
    name: "Link Display UFM Component",
    alias: "Dragonfly.UfmComponent.LinkDisplay",
    type: "ufmComponent",
    api: () => import("./link-display.component.js"),
    meta: {
      alias: "linkDisplay",
    },
  },
  {
    name: "Link URL UFM Component",
    alias: "Dragonfly.UfmComponent.LinkUrl",
    type: "ufmComponent",
    api: () => import("./link-url.component.js"),
    meta: {
      alias: "linkUrl",
    },
  },
];

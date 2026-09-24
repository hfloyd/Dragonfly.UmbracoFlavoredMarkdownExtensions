export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Umbraco Flavored Markdown Extensions Entrypoint",
    alias: "UmbracoFlavoredMarkdownExtensions.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];

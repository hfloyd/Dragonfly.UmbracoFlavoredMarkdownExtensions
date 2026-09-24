import { UmbUfmComponentBase } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import "./link-url.component.js";

/**
 * UFM component: `{dufmLinkUrlWithAnchor: myLink}`
 *
 * Renders the same element as `{dufmLinkUrl: myLink}`, with the link's query string or anchor appended.
 */
export class UfmLinkUrlWithAnchorComponent extends UmbUfmComponentBase {
  render(token: UfmToken): string | undefined {
    return token.text
      ? `<ufm-link-url ${super.getAttributes(token.text)} append-query-string></ufm-link-url>`
      : undefined;
  }
}

export { UfmLinkUrlWithAnchorComponent as api };

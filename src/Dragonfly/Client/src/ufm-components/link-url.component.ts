import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase, UmbUfmElementBase, UMB_UFM_RENDER_CONTEXT } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE, UmbDocumentUrlRepository } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE, UmbMediaUrlRepository } from "@umbraco-cms/backoffice/media";
import { parseLinkValues } from "./link-value.function.js";
import type { UfmLinkValue } from "./link-value.function.js";

/**
 * Renders the URL of a link picker value. Document and media links store only a key, so their URLs
 * are looked up; external links carry their URL in the value itself.
 */
@customElement("ufm-link-url")
export class UfmLinkUrlElement extends UmbUfmElementBase {
  @property()
  alias?: string;

  #documentUrlRepository?: UmbDocumentUrlRepository;
  #mediaUrlRepository?: UmbMediaUrlRepository;

  constructor() {
    super();

    this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
      this.observe(
        context?.value,
        async (value) => {
          const propertyValue = this.alias && typeof value === "object" ? (value as never)[this.alias] : value;
          const links = parseLinkValues(propertyValue);
          if (!links.length) {
            this.value = "";
            return;
          }

          const urls = await Promise.all(links.map((link) => this.#url(link)));

          this.value = urls.filter((url) => url).join(", ");
        },
        "observeValue",
      );
    });
  }

  async #url(link: UfmLinkValue): Promise<string> {
    return (await this.#lookupUrl(link)) || this.#withQueryString(link, link?.url) || "";
  }

  async #lookupUrl(link: UfmLinkValue): Promise<string | undefined> {
    if (!link?.unique) return undefined;

    if (link.type === UMB_MEDIA_ENTITY_TYPE) {
      this.#mediaUrlRepository ??= new UmbMediaUrlRepository(this);

      const { data } = await this.#mediaUrlRepository.requestItems([link.unique]).catch(() => ({ data: undefined }));
      return Array.isArray(data) ? data[0]?.url : undefined;
    }

    if (link.type === UMB_DOCUMENT_ENTITY_TYPE) {
      this.#documentUrlRepository ??= new UmbDocumentUrlRepository(this);

      const { data } = await this.#documentUrlRepository.requestUrls([link.unique]).catch(() => ({ data: undefined }));
      const url = Array.isArray(data) ? data[0]?.urls?.[0]?.url : undefined;

      return this.#withQueryString(link, url);
    }

    return undefined;
  }

  #withQueryString(link: UfmLinkValue, url?: string): string | undefined {
    if (!url) return url;

    return `${url}${link.queryString ?? ""}`;
  }
}

/**
 * UFM component: `{linkUrl: myLink}`
 */
export class UfmLinkUrlComponent extends UmbUfmComponentBase {
  render(token: UfmToken): string | undefined {
    return token.text ? `<ufm-link-url ${super.getAttributes(token.text)}></ufm-link-url>` : undefined;
  }
}

export { UfmLinkUrlComponent as api };

declare global {
  interface HTMLElementTagNameMap {
    "ufm-link-url": UfmLinkUrlElement;
  }
}

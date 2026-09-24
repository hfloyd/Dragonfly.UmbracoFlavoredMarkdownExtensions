import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase, UmbUfmElementBase, UMB_UFM_RENDER_CONTEXT } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE, UmbDocumentItemRepository } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE, UmbMediaItemRepository } from "@umbraco-cms/backoffice/media";
import { parseLinkValues, withQueryString } from "./link-value.function.js";
import type { UfmLinkValue } from "./link-value.function.js";

/**
 * Describes a link picker value the way AngularJS labels did: the linked item's name when one is
 * picked, otherwise the link title, otherwise the URL with its query string or anchor.
 */
@customElement("ufm-link-display")
export class UfmLinkDisplayElement extends UmbUfmElementBase {
  @property()
  alias?: string;

  #documentItemRepository?: UmbDocumentItemRepository;
  #mediaItemRepository?: UmbMediaItemRepository;

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

          const labels = await Promise.all(links.map((link) => this.#describe(link)));

          this.value = labels.filter((label) => label).join(", ");
        },
        "observeValue",
      );
    });
  }

  async #describe(link: UfmLinkValue): Promise<string> {
    return (await this.#itemName(link)) || link?.name || withQueryString(link, link?.url) || "";
  }

  async #itemName(link: UfmLinkValue): Promise<string | undefined> {
    if (!link?.unique) return undefined;

    const repository = this.#repositoryFor(link.type);

    // A lookup failure should not blank the whole label.
    const { data } = await repository.requestItems([link.unique]).catch(() => ({ data: undefined }));

    if (!Array.isArray(data) || data.length === 0) return undefined;

    return data
      .map((item) => item.variants?.[0]?.name ?? (item as { name?: string }).name)
      .filter((name) => name)
      .join(", ");
  }

  #repositoryFor(entityType?: string) {
    switch (entityType) {
      case UMB_MEDIA_ENTITY_TYPE:
        this.#mediaItemRepository ??= new UmbMediaItemRepository(this);
        return this.#mediaItemRepository;

      case UMB_DOCUMENT_ENTITY_TYPE:
      default:
        this.#documentItemRepository ??= new UmbDocumentItemRepository(this);
        return this.#documentItemRepository;
    }
  }
}

/**
 * UFM component: `{dufmLinkDisplay: myLink}`
 */
export class UfmLinkDisplayComponent extends UmbUfmComponentBase {
  render(token: UfmToken): string | undefined {
    return token.text ? `<ufm-link-display ${super.getAttributes(token.text)}></ufm-link-display>` : undefined;
  }
}

export { UfmLinkDisplayComponent as api };

declare global {
  interface HTMLElementTagNameMap {
    "ufm-link-display": UfmLinkDisplayElement;
  }
}

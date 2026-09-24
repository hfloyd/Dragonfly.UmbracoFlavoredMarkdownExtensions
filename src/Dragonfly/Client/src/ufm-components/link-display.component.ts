import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase, UmbUfmElementBase, UMB_UFM_RENDER_CONTEXT } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE, UmbDocumentItemRepository } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE, UmbMediaItemRepository } from "@umbraco-cms/backoffice/media";

type UfmLinkValue = {
  name?: string;
  type?: string;
  unique?: string;
  url?: string;
};

/**
 * Describes a link picker value the way AngularJS labels did: the linked item's name when one is
 * picked, otherwise the link title, otherwise the URL.
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
          if (!propertyValue) {
            this.value = "";
            return;
          }

          const links: Array<UfmLinkValue> = Array.isArray(propertyValue) ? propertyValue : [propertyValue];
          const labels = await Promise.all(links.map((link) => this.#describe(link)));

          this.value = labels.filter((label) => label).join(", ");
        },
        "observeValue",
      );
    });
  }

  async #describe(link: UfmLinkValue): Promise<string> {
    return (await this.#itemName(link)) || link?.name || link?.url || "";
  }

  async #itemName(link: UfmLinkValue): Promise<string | undefined> {
    if (!link?.unique) return undefined;

    const repository = this.#repositoryFor(link.type);
    const { data } = await repository.requestItems([link.unique]);

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
 * UFM component: `{linkDisplay: myLink}`
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

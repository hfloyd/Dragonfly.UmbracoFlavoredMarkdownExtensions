import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_DOCUMENT_ENTITY_TYPE, UmbDocumentItemRepository } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE, UmbMediaItemRepository } from "@umbraco-cms/backoffice/media";

/**
 * Looks up the names of picked documents or media items, for components that show a picked item's
 * name instead of its key.
 */
export class UfmItemNameResolver {
  #host: UmbControllerHost;
  #documentItemRepository?: UmbDocumentItemRepository;
  #mediaItemRepository?: UmbMediaItemRepository;

  constructor(host: UmbControllerHost) {
    this.#host = host;
  }

  /**
   * Returns the names of the given items, comma-separated, or undefined when none can be found.
   */
  async names(entityType: string | undefined, uniques: Array<string>): Promise<string | undefined> {
    if (!uniques.length) return undefined;

    const repository = this.#repositoryFor(entityType);

    // A lookup failure should not blank the whole label.
    const { data } = await repository.requestItems(uniques).catch(() => ({ data: undefined }));

    if (!Array.isArray(data) || data.length === 0) return undefined;

    return data
      .map((item) => item.variants?.[0]?.name ?? (item as { name?: string }).name)
      .filter((name) => name)
      .join(", ");
  }

  #repositoryFor(entityType?: string) {
    switch (entityType) {
      case UMB_MEDIA_ENTITY_TYPE:
        this.#mediaItemRepository ??= new UmbMediaItemRepository(this.#host);
        return this.#mediaItemRepository;

      case UMB_DOCUMENT_ENTITY_TYPE:
      default:
        this.#documentItemRepository ??= new UmbDocumentItemRepository(this.#host);
        return this.#documentItemRepository;
    }
  }
}

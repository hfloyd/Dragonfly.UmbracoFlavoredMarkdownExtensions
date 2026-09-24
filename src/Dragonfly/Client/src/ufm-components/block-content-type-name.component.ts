import { customElement, property, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";
import { UmbUfmComponentBase } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import {
  UMB_BLOCK_ENTRY_CONTEXT,
  UMB_BLOCK_MANAGER_CONTEXT,
  UMB_BLOCK_WORKSPACE_CONTEXT,
} from "@umbraco-cms/backoffice/block";

/**
 * Renders the name of a block's content element type.
 *
 * Block labels render in two places: on the block card, where the entry and manager contexts are
 * available, and in the block workspace overlay, where they are not. Both are handled here.
 */
@customElement("ufm-block-content-type-name")
export class UfmBlockContentTypeNameElement extends UmbLitElement {
  @property()
  fallback?: string;

  @state()
  private _name?: string;

  #manager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;
  #contentElementTypeKey?: string;

  constructor() {
    super();

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
      if (!context) return;

      this.observe(
        context.contentElementTypeKey,
        (key) => {
          this.#contentElementTypeKey = key;
          this.#resolveFromManager();
        },
        "_observeContentElementTypeKey",
      );
    });

    this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (context) => {
      this.#manager = context;
      this.#resolveFromManager();
    });

    // The workspace overlay has the element type loaded already.
    this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (context) => {
      if (!context) return;

      this.observe(
        context.content.structure.ownerContentTypeObservablePart((contentType) => contentType?.name),
        (name) => {
          if (name) this._name = name;
        },
        "_observeOwnerContentTypeName",
      );
    });
  }

  async #resolveFromManager() {
    if (!this.#manager || !this.#contentElementTypeKey) return;

    await this.#manager.contentTypesLoaded;

    const name = this.#manager.getContentTypeNameOf(this.#contentElementTypeKey);
    if (name) this._name = name;
  }

  override render() {
    return this._name ?? this.fallback ?? "";
  }
}

/**
 * UFM component: `{dufmBlockContentTypeName:}`, or `{dufmBlockContentTypeName: Block}` to set fallback text
 * for when the element type name cannot be resolved.
 */
export class UfmBlockContentTypeNameComponent extends UmbUfmComponentBase {
  render(token: UfmToken): string | undefined {
    const fallback = (token.text ?? "")
      .replace(/&/g, "&amp;")
      .replace(/"/g, "&quot;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;");

    return `<ufm-block-content-type-name fallback="${fallback}"></ufm-block-content-type-name>`;
  }
}

export { UfmBlockContentTypeNameComponent as api };

declare global {
  interface HTMLElementTagNameMap {
    "ufm-block-content-type-name": UfmBlockContentTypeNameElement;
  }
}

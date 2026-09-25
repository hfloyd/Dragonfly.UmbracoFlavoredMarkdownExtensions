import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase, UmbUfmElementBase, UMB_UFM_RENDER_CONTEXT } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import { UmbId } from "@umbraco-cms/backoffice/id";
import { UMB_MEDIA_ENTITY_TYPE } from "@umbraco-cms/backoffice/media";
import { UfmItemNameResolver } from "./item-name-resolver.js";
import { parseLegacyUdi } from "./link-value.function.js";

type FirstValueEntry = {
  alias: string;
  length?: number;
};

type FirstValueArguments = {
  entries: Array<FirstValueEntry>;
  fallback: string;
};

type PickedItem = {
  type?: string;
  unique: string;
};

const entryPattern = /^(\w+)(?:\s*:\s*(\d+))?$/;
const fallbackPattern = /,\s*(["'])(.*)\1\s*$/;
const htmlTagPattern = /<\/?[a-z][^>]*>/i;

/**
 * Renders the first of several properties that has a value, the way an AngularJS label's chain of
 * `a ? a : (b ? b : c)` fallbacks did. Each value is rendered by its shape: a picker shows the picked
 * items' names, rich text has its HTML stripped, and plain text is shown as it is.
 *
 * `aliases` is a comma-separated list of property aliases, each optionally followed by `:length` to
 * truncate that value, e.g. `BlockName, ContentTitle:150, Image`. A quoted string as the last item is
 * shown when none of the properties has a value, e.g. `BlockName, Image, "No image"`.
 */
@customElement("ufm-first-value")
export class UfmFirstValueElement extends UmbUfmElementBase {
  @property()
  aliases?: string;

  #itemNames = new UfmItemNameResolver(this);

  constructor() {
    super();

    this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
      this.observe(
        context?.value,
        async (value) => {
          this.value = await this.#firstValue(value);
        },
        "observeValue",
      );
    });
  }

  async #firstValue(values: unknown): Promise<string> {
    const { entries, fallback } = parseArguments(this.aliases);

    if (!values || typeof values !== "object") return fallback;

    for (const entry of entries) {
      const text = await this.#describe((values as Record<string, unknown>)[entry.alias]);
      if (text) return truncate(text, entry.length);
    }

    return fallback;
  }

  async #describe(value: unknown): Promise<string | undefined> {
    const pickedItems = parsePickedItems(value);
    if (pickedItems.length) {
      return this.#itemNames.names(
        pickedItems[0].type,
        pickedItems.map((item) => item.unique),
      );
    }

    // Rich text arrives as { markup, blocks }; an empty editor still sends the object, with empty markup.
    if (value && typeof value === "object" && "markup" in value) {
      return stripHtml(String(value.markup ?? ""));
    }

    if (typeof value === "string") {
      return htmlTagPattern.test(value) ? stripHtml(value) : value.trim();
    }

    // Zero was falsy in the AngularJS fallbacks, so it is skipped here too.
    if (typeof value === "number" && value !== 0) {
      return String(value);
    }

    return undefined;
  }
}

// The fallback is read first, so a comma inside its quotes is not taken as a separator.
function parseArguments(aliases?: string): FirstValueArguments {
  const text = aliases ?? "";
  const fallback = fallbackPattern.exec(text);
  const list = fallback ? text.slice(0, fallback.index) : text;

  const entries = list
    .split(",")
    .map((entry) => entryPattern.exec(entry.trim()))
    .filter((match): match is RegExpExecArray => match !== null)
    .map(([, alias, length]) => ({ alias, length: length ? Number(length) : undefined }));

  return { entries, fallback: fallback?.[2] ?? "" };
}

/**
 * Reads a content, media or multinode tree picker value into the items it picks. Anything else
 * returns no items, so it is rendered as text instead.
 */
function parsePickedItems(value: unknown): Array<PickedItem> {
  const items = typeof value === "string" ? value.split(",") : Array.isArray(value) ? value : [];

  const pickedItems = items.map(toPickedItem);

  return pickedItems.length && pickedItems.every((item) => item) ? (pickedItems as Array<PickedItem>) : [];
}

function toPickedItem(item: unknown): PickedItem | undefined {
  if (typeof item === "string") {
    const key = item.trim();
    return UmbId.validate(key) ? { unique: key } : parseLegacyUdi(key);
  }

  if (!item || typeof item !== "object") return undefined;

  // Media Picker values hold the picked media's key in mediaKey; other pickers use unique and type.
  const { mediaKey, unique, type } = item as { mediaKey?: string; unique?: string; type?: string };

  if (mediaKey) return { type: UMB_MEDIA_ENTITY_TYPE, unique: mediaKey };
  if (unique) return { type, unique };

  return undefined;
}

function stripHtml(markup: string): string {
  const document = new DOMParser().parseFromString(markup, "text/html");

  return (document.body.textContent ?? "").trim();
}

function truncate(text: string, length?: number): string {
  if (!length || text.length <= length) return text;

  return `${text.slice(0, length).trim()}…`;
}

/**
 * UFM component: `{dufmFirstValue: BlockName, ContentTitle:150, Image}`
 */
export class UfmFirstValueComponent extends UmbUfmComponentBase {
  render(token: UfmToken): string | undefined {
    return token.text ? `<ufm-first-value aliases="${escapeAttribute(token.text)}"></ufm-first-value>` : undefined;
  }
}

function escapeAttribute(text: string): string {
  return text.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}

export { UfmFirstValueComponent as api };

declare global {
  interface HTMLElementTagNameMap {
    "ufm-first-value": UfmFirstValueElement;
  }
}

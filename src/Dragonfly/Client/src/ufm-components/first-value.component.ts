import { customElement, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase, UmbUfmElementBase, UMB_UFM_RENDER_CONTEXT } from "@umbraco-cms/backoffice/ufm";
import type { UfmToken } from "@umbraco-cms/backoffice/ufm";
import { describeValue } from "./describe-value.js";
import { UfmItemNameResolver } from "./item-name-resolver.js";
import { parseLinkValues } from "./link-value.function.js";

type FirstValueEntry = {
  alias: string;
  linkName: boolean;
  length?: number;
};

type FirstValueArguments = {
  entries: Array<FirstValueEntry>;
  fallback: string;
};

const entryPattern = /^(\w+)(\.name)?(?:\s*:\s*(\d+))?$/;
const fallbackPattern = /,\s*(["'])(.*)\1\s*$/;

/**
 * Renders the first of several properties that has a value, the way an AngularJS label's chain of
 * `a ? a : (b ? b : c)` fallbacks did. Each value is rendered by its shape: a picker shows the picked
 * items' names, rich text has its HTML stripped, a list of text is comma-separated, and plain text is
 * shown as it is.
 *
 * `aliases` is a comma-separated list of property aliases, each optionally followed by `:length` to
 * truncate that value, e.g. `BlockName, ContentTitle:150, Image`. A quoted string as the last item is
 * shown when none of the properties has a value, e.g. `BlockName, Image, "No image"`.
 *
 * A link picker alias followed by `.name` shows only the first link's title, the way an AngularJS label's
 * `myLink[0].name` did, rather than the picked item's name, e.g. `Title, PrimaryLink.name`.
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
      const value = (values as Record<string, unknown>)[entry.alias];
      const text = entry.linkName ? firstLinkName(value) : await describeValue(value, this.#itemNames);
      if (text) return truncate(text, entry.length);
    }

    return fallback;
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
    .map(([, alias, linkName, length]) => ({ alias, linkName: !!linkName, length: length ? Number(length) : undefined }));

  return { entries, fallback: fallback?.[2] ?? "" };
}

function firstLinkName(value: unknown): string | undefined {
  return parseLinkValues(value)[0]?.name?.trim() || undefined;
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

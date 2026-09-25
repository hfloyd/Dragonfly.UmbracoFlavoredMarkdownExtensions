export type UfmLinkValue = {
  name?: string;
  queryString?: string;
  target?: string;
  type?: string;
  udi?: string;
  unique?: string;
  url?: string;
};

// Links saved before Umbraco 14 identify their target with a udi instead of unique + type.
const legacyUdiPattern = /^umb:\/\/(document|media)\/([0-9a-fA-F]{32})$/;

/**
 * Normalises a link picker property value into link objects.
 *
 * The value reaches UFM either as an array, a single object, or the raw JSON string it is stored
 * as, depending on the editor and on whether the content has been re-saved since an upgrade.
 */
export function parseLinkValues(value: unknown): Array<UfmLinkValue> {
  if (!value) return [];

  let parsed: unknown = value;

  if (typeof parsed === "string") {
    try {
      parsed = JSON.parse(parsed);
    } catch {
      return [];
    }
  }

  const items = Array.isArray(parsed) ? parsed : [parsed];

  return items
    .filter((item): item is UfmLinkValue => !!item && typeof item === "object")
    .map(withResolvedIdentity);
}

/**
 * Appends a link's query string, which holds any anchor, to a URL. An anchor-only link has no URL,
 * so its query string is returned on its own.
 */
export function withQueryString(link: UfmLinkValue, url?: string): string | undefined {
  return `${url ?? ""}${link.queryString ?? ""}` || undefined;
}

/**
 * Reads the entity type and key from a udi saved before Umbraco 14, e.g. `umb://document/{key}`.
 */
export function parseLegacyUdi(udi: string): { type: string; unique: string } | undefined {
  const match = legacyUdiPattern.exec(udi);
  if (!match) return undefined;

  const [, entityType, key] = match;

  return { type: entityType, unique: toGuid(key) };
}

function withResolvedIdentity(link: UfmLinkValue): UfmLinkValue {
  if (link.unique || !link.udi) return link;

  const identity = parseLegacyUdi(link.udi);
  if (!identity) return link;

  return { ...link, type: link.type ?? identity.type, unique: identity.unique };
}

function toGuid(key: string): string {
  return [key.slice(0, 8), key.slice(8, 12), key.slice(12, 16), key.slice(16, 20), key.slice(20)]
    .join("-")
    .toLowerCase();
}

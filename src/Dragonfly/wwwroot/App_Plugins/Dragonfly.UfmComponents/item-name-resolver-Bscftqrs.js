import { UmbDocumentItemRepository as i, UMB_DOCUMENT_ENTITY_TYPE as a } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as m, UmbMediaItemRepository as n } from "@umbraco-cms/backoffice/media";
class d {
  #t;
  #e;
  #r;
  constructor(t) {
    this.#t = t;
  }
  /**
   * Returns the names of the given items, comma-separated, or undefined when none can be found.
   */
  async names(t, o) {
    if (!o.length) return;
    const s = this.#o(t), { data: r } = await s.requestItems(o).catch(() => ({ data: void 0 }));
    if (!(!Array.isArray(r) || r.length === 0))
      return r.map((e) => e.variants?.[0]?.name ?? e.name).filter((e) => e).join(", ");
  }
  #o(t) {
    switch (t) {
      case m:
        return this.#r ??= new n(this.#t), this.#r;
      case a:
      default:
        return this.#e ??= new i(this.#t), this.#e;
    }
  }
}
export {
  d as U
};
//# sourceMappingURL=item-name-resolver-Bscftqrs.js.map

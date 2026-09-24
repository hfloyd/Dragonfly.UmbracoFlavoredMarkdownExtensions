import { property as w, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as T, UmbUfmElementBase as C, UMB_UFM_RENDER_CONTEXT as D } from "@umbraco-cms/backoffice/ufm";
import { UmbDocumentItemRepository as I, UMB_DOCUMENT_ENTITY_TYPE as A } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as P, UmbMediaItemRepository as x } from "@umbraco-cms/backoffice/media";
var N = Object.defineProperty, O = Object.getOwnPropertyDescriptor, v = (t) => {
  throw TypeError(t);
}, y = (t, e, r, s) => {
  for (var a = s > 1 ? void 0 : s ? O(e, r) : e, i = t.length - 1, m; i >= 0; i--)
    (m = t[i]) && (a = (s ? m(e, r, a) : m(a)) || a);
  return s && a && N(e, r, a), a;
}, h = (t, e, r) => e.has(t) || v("Cannot " + r), l = (t, e, r) => (h(t, e, "read from private field"), r ? r.call(t) : e.get(t)), c = (t, e, r) => e.has(t) ? v("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), u = (t, e, r, s) => (h(t, e, "write to private field"), e.set(t, r), r), _ = (t, e, r) => (h(t, e, "access private method"), r), n, o, p, d, E, U;
let f = class extends C {
  constructor() {
    super(), c(this, p), c(this, n), c(this, o), this.consumeContext(D, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          const r = this.alias && typeof e == "object" ? e[this.alias] : e;
          if (!r) {
            this.value = "";
            return;
          }
          const s = Array.isArray(r) ? r : [r], a = await Promise.all(s.map((i) => _(this, p, d).call(this, i)));
          this.value = a.filter((i) => i).join(", ");
        },
        "observeValue"
      );
    });
  }
};
n = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakMap();
p = /* @__PURE__ */ new WeakSet();
d = async function(t) {
  return await _(this, p, E).call(this, t) || t?.name || t?.url || "";
};
E = async function(t) {
  if (!t?.unique) return;
  const e = _(this, p, U).call(this, t.type), { data: r } = await e.requestItems([t.unique]);
  if (!(!Array.isArray(r) || r.length === 0))
    return r.map((s) => s.variants?.[0]?.name ?? s.name).filter((s) => s).join(", ");
};
U = function(t) {
  switch (t) {
    case P:
      return l(this, o) ?? u(this, o, new x(this)), l(this, o);
    case A:
    default:
      return l(this, n) ?? u(this, n, new I(this)), l(this, n);
  }
};
y([
  w()
], f.prototype, "alias", 2);
f = y([
  M("ufm-link-display")
], f);
class b extends T {
  render(e) {
    return e.text ? `<ufm-link-display ${super.getAttributes(e.text)}></ufm-link-display>` : void 0;
  }
}
export {
  b as UfmLinkDisplayComponent,
  f as UfmLinkDisplayElement,
  b as api
};
//# sourceMappingURL=link-display.component-ChWyKkjZ.js.map

import { property as w, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as T, UmbUfmElementBase as C, UMB_UFM_RENDER_CONTEXT as D } from "@umbraco-cms/backoffice/ufm";
import { UmbDocumentItemRepository as I, UMB_DOCUMENT_ENTITY_TYPE as P } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as x, UmbMediaItemRepository as N } from "@umbraco-cms/backoffice/media";
import { p as O } from "./link-value.function-DaOoVRsf.js";
var R = Object.defineProperty, A = Object.getOwnPropertyDescriptor, v = (t) => {
  throw TypeError(t);
}, d = (t, e, s, a) => {
  for (var r = a > 1 ? void 0 : a ? A(e, s) : e, i = t.length - 1, m; i >= 0; i--)
    (m = t[i]) && (r = (a ? m(e, s, r) : m(r)) || r);
  return a && r && R(e, s, r), r;
}, f = (t, e, s) => e.has(t) || v("Cannot " + s), l = (t, e, s) => (f(t, e, "read from private field"), s ? s.call(t) : e.get(t)), c = (t, e, s) => e.has(t) ? v("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, s), _ = (t, e, s, a) => (f(t, e, "write to private field"), e.set(t, s), s), u = (t, e, s) => (f(t, e, "access private method"), s), n, o, p, y, E, U;
let h = class extends C {
  constructor() {
    super(), c(this, p), c(this, n), c(this, o), this.consumeContext(D, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          const s = this.alias && typeof e == "object" ? e[this.alias] : e, a = O(s);
          if (!a.length) {
            this.value = "";
            return;
          }
          const r = await Promise.all(a.map((i) => u(this, p, y).call(this, i)));
          this.value = r.filter((i) => i).join(", ");
        },
        "observeValue"
      );
    });
  }
};
n = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakMap();
p = /* @__PURE__ */ new WeakSet();
y = async function(t) {
  return await u(this, p, E).call(this, t) || t?.name || t?.url || "";
};
E = async function(t) {
  if (!t?.unique) return;
  const e = u(this, p, U).call(this, t.type), { data: s } = await e.requestItems([t.unique]).catch(() => ({ data: void 0 }));
  if (!(!Array.isArray(s) || s.length === 0))
    return s.map((a) => a.variants?.[0]?.name ?? a.name).filter((a) => a).join(", ");
};
U = function(t) {
  switch (t) {
    case x:
      return l(this, o) ?? _(this, o, new N(this)), l(this, o);
    case P:
    default:
      return l(this, n) ?? _(this, n, new I(this)), l(this, n);
  }
};
d([
  w()
], h.prototype, "alias", 2);
h = d([
  M("ufm-link-display")
], h);
class g extends T {
  render(e) {
    return e.text ? `<ufm-link-display ${super.getAttributes(e.text)}></ufm-link-display>` : void 0;
  }
}
export {
  g as UfmLinkDisplayComponent,
  h as UfmLinkDisplayElement,
  g as api
};
//# sourceMappingURL=link-display.component-CrUXO3Y8.js.map

import { property as U, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as T, UmbUfmElementBase as C, UMB_UFM_RENDER_CONTEXT as D } from "@umbraco-cms/backoffice/ufm";
import { UmbDocumentItemRepository as I, UMB_DOCUMENT_ENTITY_TYPE as P } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as x, UmbMediaItemRepository as N } from "@umbraco-cms/backoffice/media";
import { p as O, w as R } from "./link-value.function-BBn2Bd7X.js";
var A = Object.defineProperty, B = Object.getOwnPropertyDescriptor, v = (t) => {
  throw TypeError(t);
}, d = (t, e, r, s) => {
  for (var a = s > 1 ? void 0 : s ? B(e, r) : e, i = t.length - 1, m; i >= 0; i--)
    (m = t[i]) && (a = (s ? m(e, r, a) : m(a)) || a);
  return s && a && A(e, r, a), a;
}, f = (t, e, r) => e.has(t) || v("Cannot " + r), l = (t, e, r) => (f(t, e, "read from private field"), r ? r.call(t) : e.get(t)), c = (t, e, r) => e.has(t) ? v("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), _ = (t, e, r, s) => (f(t, e, "write to private field"), e.set(t, r), r), u = (t, e, r) => (f(t, e, "access private method"), r), n, o, p, y, w, E;
let h = class extends C {
  constructor() {
    super(), c(this, p), c(this, n), c(this, o), this.consumeContext(D, (t) => {
      this.observe(
        t?.value,
        async (e) => {
          const r = this.alias && typeof e == "object" ? e[this.alias] : e, s = O(r);
          if (!s.length) {
            this.value = "";
            return;
          }
          const a = await Promise.all(s.map((i) => u(this, p, y).call(this, i)));
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
y = async function(t) {
  return await u(this, p, w).call(this, t) || t?.name || R(t, t?.url) || "";
};
w = async function(t) {
  if (!t?.unique) return;
  const e = u(this, p, E).call(this, t.type), { data: r } = await e.requestItems([t.unique]).catch(() => ({ data: void 0 }));
  if (!(!Array.isArray(r) || r.length === 0))
    return r.map((s) => s.variants?.[0]?.name ?? s.name).filter((s) => s).join(", ");
};
E = function(t) {
  switch (t) {
    case x:
      return l(this, o) ?? _(this, o, new N(this)), l(this, o);
    case P:
    default:
      return l(this, n) ?? _(this, n, new I(this)), l(this, n);
  }
};
d([
  U()
], h.prototype, "alias", 2);
h = d([
  M("ufm-link-display")
], h);
class b extends T {
  render(e) {
    return e.text ? `<ufm-link-display ${super.getAttributes(e.text)}></ufm-link-display>` : void 0;
  }
}
export {
  b as UfmLinkDisplayComponent,
  h as UfmLinkDisplayElement,
  b as api
};
//# sourceMappingURL=link-display.component-Bre7xA-2.js.map

import { property as w, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as T, UmbUfmElementBase as C, UMB_UFM_RENDER_CONTEXT as A } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE as P, UmbDocumentUrlRepository as g } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as q, UmbMediaUrlRepository as x } from "@umbraco-cms/backoffice/media";
import { p as D } from "./link-value.function-DaOoVRsf.js";
var O = Object.defineProperty, R = Object.getOwnPropertyDescriptor, d = (e) => {
  throw TypeError(e);
}, U = (e, t, r, a) => {
  for (var s = a > 1 ? void 0 : a ? R(t, r) : t, i = e.length - 1, c; i >= 0; i--)
    (c = e[i]) && (s = (a ? c(t, r, s) : c(s)) || s);
  return a && s && O(t, r, s), s;
}, _ = (e, t, r) => t.has(e) || d("Cannot " + r), l = (e, t, r) => (_(e, t, "read from private field"), t.get(e)), h = (e, t, r) => t.has(e) ? d("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, r), v = (e, t, r, a) => (_(e, t, "write to private field"), t.set(e, r), r), p = (e, t, r) => (_(e, t, "access private method"), r), o, u, n, y, E, m;
let f = class extends C {
  constructor() {
    super(), h(this, n), h(this, o), h(this, u), this.consumeContext(A, (e) => {
      this.observe(
        e?.value,
        async (t) => {
          const r = this.alias && typeof t == "object" ? t[this.alias] : t, a = D(r);
          if (!a.length) {
            this.value = "";
            return;
          }
          const s = await Promise.all(a.map((i) => p(this, n, y).call(this, i)));
          this.value = s.filter((i) => i).join(", ");
        },
        "observeValue"
      );
    });
  }
};
o = /* @__PURE__ */ new WeakMap();
u = /* @__PURE__ */ new WeakMap();
n = /* @__PURE__ */ new WeakSet();
y = async function(e) {
  return await p(this, n, E).call(this, e) || p(this, n, m).call(this, e, e?.url) || "";
};
E = async function(e) {
  if (e?.unique) {
    if (e.type === q) {
      l(this, u) ?? v(this, u, new x(this));
      const { data: t } = await l(this, u).requestItems([e.unique]).catch(() => ({ data: void 0 }));
      return Array.isArray(t) ? t[0]?.url : void 0;
    }
    if (e.type === P) {
      l(this, o) ?? v(this, o, new g(this));
      const { data: t } = await l(this, o).requestUrls([e.unique]).catch(() => ({ data: void 0 })), r = Array.isArray(t) ? t[0]?.urls?.[0]?.url : void 0;
      return p(this, n, m).call(this, e, r);
    }
  }
};
m = function(e, t) {
  return t && `${t}${e.queryString ?? ""}`;
};
U([
  w()
], f.prototype, "alias", 2);
f = U([
  M("ufm-link-url")
], f);
class W extends T {
  render(t) {
    return t.text ? `<ufm-link-url ${super.getAttributes(t.text)}></ufm-link-url>` : void 0;
  }
}
export {
  W as UfmLinkUrlComponent,
  f as UfmLinkUrlElement,
  W as api
};
//# sourceMappingURL=link-url.component-Cwg5dq97.js.map

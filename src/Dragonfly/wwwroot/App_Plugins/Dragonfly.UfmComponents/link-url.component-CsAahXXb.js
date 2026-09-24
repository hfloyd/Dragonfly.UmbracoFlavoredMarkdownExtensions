import { property as w, customElement as M } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as T, UmbUfmElementBase as A, UMB_UFM_RENDER_CONTEXT as C } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE as P, UmbDocumentUrlRepository as q } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as x, UmbMediaUrlRepository as D } from "@umbraco-cms/backoffice/media";
var O = Object.defineProperty, R = Object.getOwnPropertyDescriptor, y = (r) => {
  throw TypeError(r);
}, U = (r, t, e, i) => {
  for (var s = i > 1 ? void 0 : i ? R(t, e) : t, a = r.length - 1, c; a >= 0; a--)
    (c = r[a]) && (s = (i ? c(t, e, s) : c(s)) || s);
  return i && s && O(t, e, s), s;
}, _ = (r, t, e) => t.has(r) || y("Cannot " + e), l = (r, t, e) => (_(r, t, "read from private field"), t.get(r)), h = (r, t, e) => t.has(r) ? y("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(r) : t.set(r, e), v = (r, t, e, i) => (_(r, t, "write to private field"), t.set(r, e), e), p = (r, t, e) => (_(r, t, "access private method"), e), o, u, n, d, E, m;
let f = class extends A {
  constructor() {
    super(), h(this, n), h(this, o), h(this, u), this.consumeContext(C, (r) => {
      this.observe(
        r?.value,
        async (t) => {
          const e = this.alias && typeof t == "object" ? t[this.alias] : t;
          if (!e) {
            this.value = "";
            return;
          }
          const i = Array.isArray(e) ? e : [e], s = await Promise.all(i.map((a) => p(this, n, d).call(this, a)));
          this.value = s.filter((a) => a).join(", ");
        },
        "observeValue"
      );
    });
  }
};
o = /* @__PURE__ */ new WeakMap();
u = /* @__PURE__ */ new WeakMap();
n = /* @__PURE__ */ new WeakSet();
d = async function(r) {
  return await p(this, n, E).call(this, r) || p(this, n, m).call(this, r, r?.url) || "";
};
E = async function(r) {
  if (r?.unique) {
    if (r.type === x) {
      l(this, u) ?? v(this, u, new D(this));
      const { data: t } = await l(this, u).requestItems([r.unique]);
      return Array.isArray(t) ? t[0]?.url : void 0;
    }
    if (r.type === P) {
      l(this, o) ?? v(this, o, new q(this));
      const { data: t } = await l(this, o).requestUrls([r.unique]), e = Array.isArray(t) ? t[0]?.urls?.[0]?.url : void 0;
      return p(this, n, m).call(this, r, e);
    }
  }
};
m = function(r, t) {
  return t && `${t}${r.queryString ?? ""}`;
};
U([
  w()
], f.prototype, "alias", 2);
f = U([
  M("ufm-link-url")
], f);
class I extends T {
  render(t) {
    return t.text ? `<ufm-link-url ${super.getAttributes(t.text)}></ufm-link-url>` : void 0;
  }
}
export {
  I as UfmLinkUrlComponent,
  f as UfmLinkUrlElement,
  I as api
};
//# sourceMappingURL=link-url.component-CsAahXXb.js.map

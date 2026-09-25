import { property as d, customElement as E } from "@umbraco-cms/backoffice/external/lit";
import { UmbUfmComponentBase as M, UmbUfmElementBase as T, UMB_UFM_RENDER_CONTEXT as g } from "@umbraco-cms/backoffice/ufm";
import { UMB_DOCUMENT_ENTITY_TYPE as C, UmbDocumentUrlRepository as A } from "@umbraco-cms/backoffice/document";
import { UMB_MEDIA_ENTITY_TYPE as P, UmbMediaUrlRepository as S } from "@umbraco-cms/backoffice/media";
import { p as q, w as x } from "./link-value.function-D3PQjIif.js";
var B = Object.defineProperty, D = Object.getOwnPropertyDescriptor, v = (e) => {
  throw TypeError(e);
}, f = (e, t, r, s) => {
  for (var a = s > 1 ? void 0 : s ? D(t, r) : t, i = e.length - 1, c; i >= 0; i--)
    (c = e[i]) && (a = (s ? c(t, r, a) : c(a)) || a);
  return s && a && B(t, r, a), a;
}, _ = (e, t, r) => t.has(e) || v("Cannot " + r), p = (e, t, r) => (_(e, t, "read from private field"), t.get(e)), h = (e, t, r) => t.has(e) ? v("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, r), m = (e, t, r, s) => (_(e, t, "write to private field"), t.set(e, r), r), y = (e, t, r) => (_(e, t, "access private method"), r), n, o, u, U, w;
let l = class extends T {
  constructor() {
    super(), h(this, u), this.appendQueryString = !1, h(this, n), h(this, o), this.consumeContext(g, (e) => {
      this.observe(
        e?.value,
        async (t) => {
          const r = this.alias && typeof t == "object" ? t[this.alias] : t, s = q(r);
          if (!s.length) {
            this.value = "";
            return;
          }
          const a = await Promise.all(s.map((i) => y(this, u, U).call(this, i)));
          this.value = a.filter((i) => i).join(", ");
        },
        "observeValue"
      );
    });
  }
};
n = /* @__PURE__ */ new WeakMap();
o = /* @__PURE__ */ new WeakMap();
u = /* @__PURE__ */ new WeakSet();
U = async function(e) {
  const t = await y(this, u, w).call(this, e) || e?.url;
  return (this.appendQueryString ? x(e, t) : t) || "";
};
w = async function(e) {
  if (e?.unique) {
    if (e.type === P) {
      p(this, o) ?? m(this, o, new S(this));
      const { data: t } = await p(this, o).requestItems([e.unique]).catch(() => ({ data: void 0 }));
      return Array.isArray(t) ? t[0]?.url : void 0;
    }
    if (e.type === C) {
      p(this, n) ?? m(this, n, new A(this));
      const { data: t } = await p(this, n).requestUrls([e.unique]).catch(() => ({ data: void 0 }));
      return Array.isArray(t) ? t[0]?.urls?.[0]?.url : void 0;
    }
  }
};
f([
  d()
], l.prototype, "alias", 2);
f([
  d({ type: Boolean, attribute: "append-query-string" })
], l.prototype, "appendQueryString", 2);
l = f([
  E("ufm-link-url")
], l);
class Q extends M {
  render(t) {
    return t.text ? `<ufm-link-url ${super.getAttributes(t.text)}></ufm-link-url>` : void 0;
  }
}
export {
  Q as UfmLinkUrlComponent,
  l as UfmLinkUrlElement,
  Q as api
};
//# sourceMappingURL=link-url.component-CX4j120u.js.map
